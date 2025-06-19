using TMPro;
using UnityEngine;

public class TurnUI : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] TMP_Text hint;
    [SerializeField] Color whiteTurnColor = Color.white;
    [SerializeField] Color blackTurnColor = Color.black;

    public void SetTurn(Team team)
    {
        label.text = team == Team.White ? "Белые ходят" : "Чёрные ходят";
        label.color = team == Team.White ? whiteTurnColor : blackTurnColor;
        HideHint();
    }

    public void ShowHint(string msg) { if (hint) { hint.text = msg; hint.gameObject.SetActive(true); } }
    public void HideHint() { if (hint) hint.gameObject.SetActive(false); }

    public void ShowMandatory() => ShowHint("Сыграйте обязательный ход!");
}
