using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Unit : MonoBehaviour
{
    public Team Team { get; set; }
    public bool IsKing { get; protected set; }
    public Cell Cell { get; set; }

    protected Battlefield bf;

    public void Init(Battlefield field) => bf = field;

    public abstract List<Cell> GetAvailableMoves();

    public IEnumerator AnimateTo(Cell dest)
    {
        var a = transform.position;
        var b = dest.transform.position + Vector3.up * 0.1f;
        for (float t = 0; t < 1f; t += Time.deltaTime * 4f)
        {
            transform.position = Vector3.Lerp(a, b, t);
            yield return null;
        }
        transform.position = b;
        Cell = dest;
        dest.Occupant = this;
    }

    public virtual List<Cell> GetCaptureMoves()
    {
        var all = GetAvailableMoves();
        var caps = new List<Cell>();
        foreach (var c in all)
            if (Mathf.Abs(c.X - Cell.X) > 1) caps.Add(c);
        return caps;
    }
}
