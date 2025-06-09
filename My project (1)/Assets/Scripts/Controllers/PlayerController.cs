using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Battlefield bf;
    private Team active = Team.White;
    private Unit selected;

    public void Cancel()
    {
        if (selected == null) return;
        bf.ClearHighlights();
        selected = null;
    }

    public void Confirm()
    {
        if (selected == null) return;

        bf.ClearHighlights();
        selected = null;
        active = active == Team.White ? Team.Black : Team.White;
    }

    public void HandleUnitClick(Unit u)
    {
        if (u.Team != active) return;
        selected = u;
        Highlight(u);
    }
    public void HandleCellClick(Cell c)
    {
        if (selected == null) return;
        if (!selected.GetAvailableMoves().Contains(c)) return;
        StartCoroutine(Move(selected, c));
    }

    IEnumerator Move(Unit u, Cell dest)
    {
        int dx = dest.X - u.Cell.X;
        int dy = dest.Y - u.Cell.Y;

        if (Mathf.Abs(dx) == 2 && Mathf.Abs(dy) == 2)         
        {
            int midX = u.Cell.X + dx / 2;
            int midY = u.Cell.Y + dy / 2;
            var midCell = bf[midX, midY];

            if (midCell.Occupant != null && midCell.Occupant.Team != u.Team)
            {
                Destroy(midCell.Occupant.gameObject);          
                midCell.Occupant = null;
            }
        }

        u.Cell.Occupant = null;

        yield return u.AnimateTo(dest);

        bf.ClearHighlights();
        selected = null;
        active = active == Team.White ? Team.Black : Team.White;
    }

    void Highlight(Unit u) => bf.HighlightMoves(u);
}
