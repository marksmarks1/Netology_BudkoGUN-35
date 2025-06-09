using UnityEngine;
using System.Collections.Generic;

public class Battlefield : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] Cell cellPrefab;
    [SerializeField] CheckerUnit checkerPrefab;

    Cell[,] grid = new Cell[8, 8];
    public Cell this[int x, int y] => grid[x, y];
    public bool Inside(int x, int y) => x >= 0 && x < 8 && y >= 0 && y < 8;

    void Awake()
    {
        BuildGrid();
        SpawnPieces();
    }

    /* ---------- генерация сетки ---------- */

    void BuildGrid()
    {
        const float s = 1f;
        var root = new GameObject("Cells").transform;
        root.SetParent(transform);
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                bool black = (x + y) % 2 == 1;
                var c = Instantiate(cellPrefab,
                         new Vector3(x * s, 0, y * s),
                         Quaternion.Euler(90, 0, 0), root);
                c.name = $"Cell {x},{y}";
                c.Init(x, y, black);
                grid[x, y] = c;
            }
    }

    /* ---------- расстановка шашек ---------- */

    void SpawnPieces()
    {
        for (int y = 0; y < 3; y++) SpawnRow(y, Team.White);
        for (int y = 5; y < 8; y++) SpawnRow(y, Team.Black);
    }
    void SpawnRow(int y, Team team)
    {
        for (int x = 0; x < 8; x++)
        {
            if ((x + y) % 2 == 0) continue;

            var u = Instantiate(checkerPrefab,
                     grid[x, y].transform.position + Vector3.up * 0.1f,
                     Quaternion.identity, transform);

            u.Team = team;
            u.Cell = grid[x, y];
            grid[x, y].Occupant = u;
            u.Init(this);

            u.ApplyTeamMaterial();
        }
    }

    // === хранение выделенных клеток ===
    private readonly List<Cell> _lit = new();

    public void HighlightMoves(Unit unit)
    {
        ClearHighlights();
        foreach (var c in unit.GetAvailableMoves())
        {
            c.SetHighlight(true);
            _lit.Add(c);
        }
    }

    public void ClearHighlights()
    {
        foreach (var c in _lit)
            c.SetHighlight(false);
        _lit.Clear();
    }
}
