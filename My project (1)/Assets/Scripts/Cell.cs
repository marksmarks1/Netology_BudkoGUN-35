using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(BoxCollider))]
public class Cell : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public Unit Occupant;

    [SerializeField] MeshRenderer mr;
    [SerializeField] Material whiteMat, blackMat, highlightMat, mustAttackMat;
    Material baseMat;

    public void Init(int x, int y, bool isBlack)
    {
        X = x; Y = y;
        baseMat = isBlack ? blackMat : whiteMat;
        mr.material = baseMat;
    }
    public void SetHighlight(bool on, bool mandatory = false)
    {
        if (!on) { mr.material = baseMat; return; }

        mr.material = mandatory ? mustAttackMat : highlightMat;
    }

    public void OnPointerEnter(PointerEventData e) { }
    public void OnPointerExit(PointerEventData e) { }
    public void OnPointerClick(PointerEventData e) { }
}
