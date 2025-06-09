using System.Collections.Generic;
using UnityEngine;

public class CheckerUnit : Unit
{
    [SerializeField] Material whiteMat;
    [SerializeField] Material blackMat;

    static readonly (int dx, int dy)[] DIR = { (1, 1), (-1, 1), (1, -1), (-1, -1) };

    public void ApplyTeamMaterial()
    {
        var mr = GetComponent<MeshRenderer>();
        mr.material = Team == Team.White ? whiteMat : blackMat;
    }

    public override List<Cell> GetAvailableMoves()
    {
        var moves = new List<Cell>();
        foreach (var (dx, dy) in DIR)
        {
            if (!IsKing && ((Team == Team.White && dy < 0) || (Team == Team.Black && dy > 0)))
                continue;                      

            TryAdd(Cell.X + dx, Cell.Y + dy);
            TryJump(dx, dy);
        }
        return moves;

        void TryAdd(int x, int y)
        {
            if (bf.Inside(x, y) && bf[x, y].Occupant == null)
                moves.Add(bf[x, y]);
        }
        void TryJump(int dx, int dy)
        {
            int mx = Cell.X + dx, my = Cell.Y + dy;
            int tx = Cell.X + dx * 2, ty = Cell.Y + dy * 2;
            if (!bf.Inside(tx, ty)) return;
            var mid = bf[mx, my];
            var trg = bf[tx, ty];
            if (mid.Occupant != null && mid.Occupant.Team != Team && trg.Occupant == null)
                moves.Add(trg);
        }
    }
}
