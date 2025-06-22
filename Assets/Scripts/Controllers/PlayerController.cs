using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Battlefield bf;
    [SerializeField] TurnUI turnUI;

    enum SelState { Idle, PieceSelected, MovePending }
    SelState state = SelState.Idle;

    Team active = Team.White;
    Unit selected;
    Cell pendingCell;
    Vector3 originalPos;

    List<(Unit unit, Cell dest)> mustHit = new();

    void Start()
    {
        turnUI.SetTurn(active);
        ScanForMandatoryHits();

        if (mustHit.Count > 0)
        {
            turnUI.ShowMandatory();
            bf.HighlightMandatoryMoves(mustHit);
        }
    }

    /* ========== PUBLIC: вызовы из BattleController ========== */
    public bool HasSelection => state != SelState.Idle;
    public Unit SelectedUnit => selected;

    public void Cancel() => Rollback();
    public void Confirm() => ApplyMove();

    public Cell SingleMandatoryCell
    => (mustHit.Count == 1) ? mustHit[0].dest : null;

    /* ========== КЛИК ПО ФИГУРЕ ========== */
    public void HandleUnitClick(Unit u)
    {
        //Debug.Log($"CLICK UNIT {u.name}   state={state}  active={active}");
        if (state != SelState.Idle) return;                 // уже что-то выбрано
        if (u.Team != active) return;                     // чужая команда
        if (mustHit.Count > 0 && !IsMandatoryUnit(u)) return;

        selected = u;
        originalPos = u.transform.position;
        state = SelState.PieceSelected;

        turnUI.HideHint();
        bf.HighlightMoves(u, mustHit.Count > 0);
    }

    /* ========== КЛИК ПО КЛЕТКЕ ========== */
    public void HandleCellClick(Cell c)
    {
        //Debug.Log($"CLICK CELL {c.X},{c.Y}  state={state}");
        if (state != SelState.PieceSelected) return;

        List<Cell> legalMoves =           
            (mustHit.Count > 0) ? selected.GetCaptureMoves()
                                : selected.GetAvailableMoves();

        if (!legalMoves.Contains(c)) return;

        pendingCell = c;
        state = SelState.MovePending;

        selected.transform.position = c.transform.position + Vector3.up * 0.1f;
        turnUI.ShowHint("<SPACE> – подтвердить   |   \n<ESC> – отменить");
    }

    /* ==========  ESC  ========== */
    void Rollback()
    {
        switch (state)
        {
            case SelState.MovePending:
                selected.transform.position = originalPos;     // вернули
                pendingCell = null;
                state = SelState.PieceSelected;
                turnUI.HideHint();
                break;

            case SelState.PieceSelected:
                bf.ClearHighlights();
                selected = null;
                state = SelState.Idle;
                break;
        }
    }

    /* ==========  SPACE  ========== */
    void ApplyMove()
    {
        if (state != SelState.MovePending) return;

        bool captured = CaptureIfAny(selected.Cell, pendingCell);  

        selected.Cell.Occupant = null;
        selected.Cell = pendingCell;
        pendingCell.Occupant = selected;

        TryPromote(selected);

        if (captured)
        {
            var moreHits = selected.GetCaptureMoves();
            if (moreHits.Count > 0)
            {
                state = SelState.PieceSelected;
                pendingCell = null;

                bf.HighlightMoves(selected, true);
                mustHit.Clear();
                foreach (var cell in moreHits)
                    mustHit.Add((selected, cell));

                turnUI.ShowHint("продолжай атаку!  \n<ESC> – отмена");
                return;                                    
            }
        }

        EndTurn();                                         
    }

    void EndTurn()
    {
        bf.ClearHighlights();
        turnUI.HideHint();

        selected = null;
        pendingCell = null;
        state = SelState.Idle;

        active = active == Team.White ? Team.Black : Team.White;

        ScanForMandatoryHits();

        turnUI.SetTurn(active);

        if (mustHit.Count > 0)
        {
            turnUI.ShowMandatory();
            bf.HighlightMandatoryMoves(mustHit);
        }
    }

    bool CaptureIfAny(Cell from, Cell to)
    {
        int steps = Mathf.Abs(to.X - from.X);

        // короткий прыжок (обычная шашка)
        if (steps == 2)
        {
            int mx = (from.X + to.X) / 2;
            int my = (from.Y + to.Y) / 2;
            var mid = bf[mx, my];
            if (mid.Occupant)
            {
                Destroy(mid.Occupant.gameObject);
                mid.Occupant = null;
                return true;                     
            }
            return false;                        
        }

        // длинное взятие дамкой
        if (selected.IsKing && steps > 2 && steps == Mathf.Abs(to.Y - from.Y))
        {
            int sx = System.Math.Sign(to.X - from.X);
            int sy = System.Math.Sign(to.Y - from.Y);
            int cx = from.X + sx, cy = from.Y + sy;
            while (cx != to.X && cy != to.Y)
            {
                var cell = bf[cx, cy];
                if (cell.Occupant && cell.Occupant.Team != selected.Team)
                {
                    Destroy(cell.Occupant.gameObject);
                    cell.Occupant = null;
                    return true;                 
                }
                cx += sx; cy += sy;
            }
        }

        return false;                            
    }

    void TryPromote(Unit u)
    {
        if (u.IsKing) return;
        bool backRank = (u.Team == Team.White && u.Cell.Y == 7) ||
                        (u.Team == Team.Black && u.Cell.Y == 0);
        if (backRank && u is CheckerUnit cu) cu.PromoteToKing();
    }

    /* ---------- обязательная атака ---------- */
    void ScanForMandatoryHits()
    {
        mustHit.Clear();
        foreach (var cell in bf.AllCells)
            if (cell.Occupant && cell.Occupant.Team == active)
                foreach (var c in cell.Occupant.GetCaptureMoves())
                    mustHit.Add((cell.Occupant, c));

        //Debug.Log($"[{active}] mandatory hits = {mustHit.Count}");
    }
    bool IsMandatoryUnit(Unit u) => mustHit.Exists(p => p.unit == u);
    bool IsMandatoryCell(Unit u, Cell c) => mustHit.Exists(p => p.unit == u && p.dest == c);
}