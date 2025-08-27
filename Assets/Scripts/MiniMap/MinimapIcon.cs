using UnityEngine;
using UnityEngine.UI;

public class MinimapIcon : MonoBehaviour
{
    public Transform target;                 
    public Sprite sprite;                   
    public Color color = Color.white;
    [Range(6, 64)] public float size = 22;
    public bool rotateWithTarget = false;

    RectTransform ui;
    Image img;
    float lastSize = -1f;
    Color lastColor;

    void Awake()
    {
        if (!target) target = transform;
    }

    void OnEnable()
    {
        ui = null; img = null;              
        TryEnsureUI();
    }

    void OnDisable()
    {
        if (ui) Destroy(ui.gameObject);
        ui = null; img = null;
    }

    void LateUpdate()
    {
        TryEnsureUI();
        if (!ui) return;

        ui.anchoredPosition = Minimap.Instance.WorldToMap(target.position);
        if (rotateWithTarget)
            ui.localEulerAngles = new Vector3(0f, 0f, -target.eulerAngles.y);

        if (Mathf.Abs(size - lastSize) > 0.01f) { ui.sizeDelta = new Vector2(size, size); lastSize = size; }
        if (img && color != lastColor) { img.color = color; lastColor = color; }
        if (img && img.sprite != sprite) img.sprite = sprite;
    }

    void TryEnsureUI()
    {
        if (ui || Minimap.Instance == null || Minimap.Instance.blipsRoot == null) return;

        var go = new GameObject("Blip_" + (target ? target.name : name),
                                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(Minimap.Instance.blipsRoot, false);

        ui = go.GetComponent<RectTransform>();
        img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        ui.pivot = new Vector2(0.5f, 0.5f);
        ui.sizeDelta = new Vector2(size, size);
        ui.SetAsLastSibling();
        lastSize = size; lastColor = color;
    }
}