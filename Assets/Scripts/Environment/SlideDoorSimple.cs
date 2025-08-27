using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class SlideDoorSimple : MonoBehaviour
{
    [Header("Movement")]
    public Transform door;                 // если пусто Ч двигаем сам объект
    public float openOffsetX = 2.0f;       // на сколько уехать по локальной оси X
    public float openTime = 0.5f;
    public float closeTime = 0.5f;
    public float closeDelay = 0.2f;        // подождать перед закрытием после ухода

    [Header("NavMesh (опц.)")]
    public NavMeshObstacle obstacle;      

    Transform _leaf;
    Vector3 _closedPos, _openPos;
    bool _isOpen;
    Coroutine _anim;
    readonly HashSet<Collider> _inside = new HashSet<Collider>();

    void Awake()
    {
        _leaf = door ? door : transform;
        _closedPos = _leaf.localPosition;
        _openPos = _closedPos + new Vector3(openOffsetX, 0f, 0f);

        bool hasTrigger = false;
        foreach (var c in GetComponents<Collider>()) if (c && c.isTrigger) { hasTrigger = true; break; }
        if (!hasTrigger) return;

        SetCarve(true); 
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        _inside.Add(other);
        Open();
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        _inside.Remove(other);
        if (_inside.Count == 0) StartCoroutine(CloseDelayed());
    }

    bool IsPlayer(Collider other)
    {
        if (!other) return false;
        if (other.CompareTag("Player")) return true;
        return other.GetComponentInParent<PlayerHealth>() != null;
    }

    IEnumerator CloseDelayed()
    {
        yield return new WaitForSeconds(closeDelay);
        if (_inside.Count == 0) Close();
    }

    public void Open()
    {
        if (_isOpen) return;
        _isOpen = true;
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(Animate(open: true));
        SetCarve(false);
    }

    public void Close()
    {
        if (!_isOpen) return;
        _isOpen = false;
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(Animate(open: false));
        SetCarve(true);
    }

    IEnumerator Animate(bool open)
    {
        float t = 0f, dur = open ? openTime : closeTime;
        Vector3 from = _leaf.localPosition;
        Vector3 to = open ? _openPos : _closedPos;

        while (t < dur)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / dur));
            _leaf.localPosition = Vector3.LerpUnclamped(from, to, k);
            yield return null;
        }
        _leaf.localPosition = to;
    }

    void SetCarve(bool closed)
    {
        if (obstacle) obstacle.carving = closed;
    }
}