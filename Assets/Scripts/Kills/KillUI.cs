using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KillUI : MonoBehaviour
{
    public TextMeshProUGUI tmp;     

    [SerializeField] string prefix = "Kills: ";

    void OnEnable()
    {
        UpdateLabel();
        if (KillCounter.Instance) KillCounter.Instance.OnChanged += OnChanged;
    }

    void OnDisable()
    {
        if (KillCounter.Instance) KillCounter.Instance.OnChanged -= OnChanged;
    }

    void OnChanged(int value) => UpdateLabel();

    void UpdateLabel()
    {
        int v = KillCounter.Instance ? KillCounter.Instance.Kills : 0;
        string s = prefix + v;
        if (tmp) tmp.text = s;
    }
}
