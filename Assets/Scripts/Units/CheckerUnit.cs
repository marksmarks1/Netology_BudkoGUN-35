using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CheckerUnit : Unit
{
    [Header("Normal materials")]
    [SerializeField] Material whiteMat;  
    [SerializeField] Material blackMat;

    [Header("King materials")]
    [SerializeField] Material kingWhiteMat;   
    [SerializeField] Material kingBlackMat;   

    static readonly (int dx, int dy)[] DIR = { (1, 1), (-1, 1), (1, -1), (-1, -1) };

    public void ApplyTeamMaterial()
    {
        var mr = GetComponent<MeshRenderer>();
        mr.material = IsKing
            ? (Team == Team.White ? kingWhiteMat : kingBlackMat)
            : (Team == Team.White ? whiteMat : blackMat);
    }

    public void PromoteToKing()
    {
        IsKing = true;
        ApplyTeamMaterial();
    }

    public override List<Cell> GetAvailableMoves()
    {
        var moves = new List<Cell>();

        foreach (var (dx, dy) in DIR)
        {
            if (IsKing)
            {
                bool opponentMet = false;

                int tx = Cell.X + dx;
                int ty = Cell.Y + dy;

                while (bf.Inside(tx, ty))
                {
                    var next = bf[tx, ty];

                    if (next.Occupant == null)
                    {
                        
                        if (!opponentMet || (opponentMet && moves.LastOrDefault() != next))
                            moves.Add(next);
                    }
                    else
                    {
                        if (next.Occupant.Team == Team) break;      
                        if (opponentMet) break;                     

                        opponentMet = true;                         
                    }

                    tx += dx; ty += dy;
                }
            }
            else          
            {
                bool forward = (Team == Team.White && dy > 0) ||
                               (Team == Team.Black && dy < 0);

                if (forward)
                    TryAdd(Cell.X + dx, Cell.Y + dy);

                TryJump(dx, dy);
            }
        }
        return moves;

        void TryAdd(int x, int y)
        {
            if (bf.Inside(x, y) && bf[x, y].Occupant == null)
                moves.Add(bf[x, y]);
        }

        void TryJump(int dx, int dy)
        {
            int mx = Cell.X + dx, my = Cell.Y + dy;     // клетка с соперником
            int tx = Cell.X + dx * 2, ty = Cell.Y + dy * 2; // клетка приземления
            if (!bf.Inside(tx, ty)) return;

            var mid = bf[mx, my];
            var trg = bf[tx, ty];

            if (mid.Occupant != null && mid.Occupant.Team != Team && trg.Occupant == null)
                moves.Add(trg);
        }
    }
    public override List<Cell> GetCaptureMoves()
    {
        if (!IsKing)
            return base.GetCaptureMoves();

        var caps = new List<Cell>();

        foreach (var (dx, dy) in DIR)
        {
            bool enemyFound = false;
            int x = Cell.X + dx, y = Cell.Y + dy;

            while (bf.Inside(x, y))
            {
                var cur = bf[x, y];

                if (cur.Occupant == null)
                {
                    if (enemyFound) caps.Add(cur);          
                }
                else
                {
                    if (cur.Occupant.Team == Team) break;   
                    if (enemyFound) break;                  
                    enemyFound = true;                      
                }
                x += dx; y += dy;
            }
        }
        return caps;
    }
}
