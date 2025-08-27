using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TargetHighlighter : MonoBehaviour
{
    public static readonly HashSet<TargetHighlighter> All = new HashSet<TargetHighlighter>();

    public Vector3 offset = new Vector3(0, 2f, 0);
    [Range(0.05f, 1f)] public float size = 0.40f;
    public Color color = Color.yellow;
    public bool billboardToCamera = true;

    Transform marker;
    Camera cam;
    bool isOn;

    void OnEnable()
    {
        All.Add(this);
        cam = Camera.main;
        EnsureMarker();
        SetHighlight(false);
    }

    void OnDisable()
    {
        All.Remove(this);
        SetHighlight(false);
    }

    void EnsureMarker()
    {
        if (marker) return;

        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = "Marker";
        var mc = go.GetComponent<MeshCollider>(); if (mc) Destroy(mc);

        var mr = go.GetComponent<MeshRenderer>();
        var mat = new Material(Shader.Find("Unlit/Color")) { color = color };
        mat.renderQueue = 5000;               
        mr.sharedMaterial = mat;
        mr.shadowCastingMode = ShadowCastingMode.Off;
        mr.receiveShadows = false;

        go.transform.SetParent(transform, false);
        go.transform.localScale = Vector3.one * size;
        go.transform.localPosition = offset;

        marker = go.transform;
    }

    public void SetHighlight(bool on)
    {
        isOn = on;
        EnsureMarker();
        if (marker) marker.gameObject.SetActive(on);
        if (marker) marker.localPosition = offset;
    }

    void LateUpdate()
    {
        if (!isOn || !marker) return;

        marker.localPosition = offset;

        if (billboardToCamera)
        {
            if (!cam) cam = Camera.main;
            if (cam) marker.rotation = Quaternion.LookRotation(cam.transform.forward, cam.transform.up);
        }
    }
}