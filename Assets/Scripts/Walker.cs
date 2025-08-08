using UnityEngine;
using DG.Tweening;
using System.Linq;

public class Walker : MonoBehaviour
{
    [Header("Путь")]
    [SerializeField] private Transform[] waypoints;

    [Header("Движение")]
    [SerializeField, Min(0.1f)] private float speed = 1.5f;
    [SerializeField, Range(0f, 0.2f)] private float lookAhead = 0.05f;
    [SerializeField, Min(0f)] private float turnTime = 0.35f;

    [Header("FX")]
    [SerializeField, Min(1f)] private float checkpointScale = 1.25f;
    [SerializeField, Min(0.1f)] private float fxDuration = 1f;
    [SerializeField] private Renderer skinnedMesh;
    [SerializeField] private ParticleSystem dustTrail;

    Vector3 _origScale;
    Tween _scalePulse;
    Sequence _seq;

    void Start()
    {
        _origScale = transform.localScale;

        // прямой и обратный массивы точек
        Vector3[] fwd = waypoints.Select(w => w.position).ToArray();
        Vector3[] back = fwd.Reverse().ToArray();

        transform.position = fwd[0];
        transform.rotation = Quaternion.LookRotation(fwd[1] - fwd[0], Vector3.up);

        // твины
        Tween moveFwd = CreatePathTween(fwd);
        Tween moveBack = CreatePathTween(back);

        // развороты
        Quaternion rotToBack = Quaternion.LookRotation(back[1] - back[0], Vector3.up);
        Quaternion rotToFwd = Quaternion.LookRotation(fwd[1] - fwd[0], Vector3.up);
        Tween turnToBack = transform.DORotateQuaternion(rotToBack, turnTime).SetEase(Ease.Linear);
        Tween turnToFwd = transform.DORotateQuaternion(rotToFwd, turnTime).SetEase(Ease.Linear);

        // последовательность
        _seq = DOTween.Sequence()
                      .Append(moveFwd)
                      .Append(turnToBack)
                      .Append(moveBack)
                      .Append(turnToFwd)
                      .SetLoops(-1);
    }

    Tween CreatePathTween(Vector3[] path)
    {
        float length = 0f;
        for (int i = 1; i < path.Length; i++)
            length += Vector3.Distance(path[i - 1], path[i]);

        float duration = length / Mathf.Max(0.0001f, speed);

        return transform.DOPath(path, duration, PathType.CatmullRom, PathMode.Full3D)
                        .SetEase(Ease.Linear)
                        .SetLookAt(lookAhead)
                        .OnWaypointChange(OnCheckpoint);
    }

    void OnCheckpoint(int _)
    {
        // пыль
        if (dustTrail)
        {
            dustTrail.transform.position = transform.position;
            dustTrail.Emit(10);
        }

        // масштаб-пульс
        _scalePulse?.Kill();
        transform.localScale = _origScale;                                   
        _scalePulse = transform.DOScale(_origScale * checkpointScale,
                                        fxDuration / 2)
                               .SetLoops(2, LoopType.Yoyo);

        // случайный цвет
        if (skinnedMesh)
        {
            var mat = skinnedMesh.material;
            string p = mat.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
            mat.DOColor(Random.ColorHSV(), p, fxDuration);
        }
    }

    void OnDisable() => _seq?.Kill();
}