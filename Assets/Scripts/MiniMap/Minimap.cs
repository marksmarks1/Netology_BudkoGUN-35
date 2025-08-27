using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    public static Minimap Instance { get; private set; }

    [Header("Refs")]
    public Camera miniCam;                 
    public RectTransform mapRect;          
    public RectTransform blipsRoot;        
    public Transform follow;              

    [Header("Follow")]
    public float height = 50f;             
    public bool northUp = true;            

    void Awake() { Instance = this; }

    void LateUpdate()
    {
        if (!miniCam || !follow) return;

        // камера висит над follow
        var p = follow.position;
        miniCam.transform.position = new Vector3(p.x, height, p.z);

        float yaw = northUp ? 0f : follow.eulerAngles.y;
        miniCam.transform.rotation = Quaternion.Euler(90f, yaw, 0f);
    }

    public Vector2 WorldToMap(Vector3 world)
    {
        if (!miniCam || !mapRect) return Vector2.zero;

        // границы видимой области в МИРОВЫХ единицах
        float halfH = miniCam.orthographicSize;           
        float halfW = halfH * miniCam.aspect;             

        Vector3 camPos = miniCam.transform.position;
        Vector3 delta = new Vector3(world.x - camPos.x, 0f, world.z - camPos.z);

        float yaw = miniCam.transform.eulerAngles.y;
        if (Mathf.Abs(yaw) > 0.001f)
            delta = Quaternion.Euler(0f, -yaw, 0f) * delta;

        float px = Mathf.Clamp(delta.x, -halfW, halfW) / halfW * (mapRect.rect.width * 0.5f);
        float py = Mathf.Clamp(delta.z, -halfH, halfH) / halfH * (mapRect.rect.height * 0.5f);
        return new Vector2(px, py);
    }
}