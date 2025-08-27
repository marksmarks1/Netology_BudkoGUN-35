using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
[RequireComponent(typeof(FootstepAudio))]
public class FootstepByMotion : MonoBehaviour
{
    [Header("Bones")]
    public Transform leftFoot;
    public Transform rightFoot;

    [Header("Gating")]
    public float minInterval = 0.12f;     // минимальный интервал между шагами одной ноги
    public float minMoveSpeed = 0.10f;    // не играть шаги, когда почти стоим
    public float runThreshold = 3.5f;     // скорость, выше которой считаем «бег»
    public float downVelThreshold = 0.04f;// нога должна заметно двигаться вниз (м/с)

    FootstepAudio audioFx;
    Animator anim;
    NavMeshAgent agent;
    CharacterController cc;
    Rigidbody rb;

    struct FootState { public float prevY, velY, lastStepTime; }
    FootState L, R;

    void Awake()
    {
        audioFx = GetComponent<FootstepAudio>();
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        cc = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();

        // Автоподстановка костей для Humanoid
        if (anim && anim.isHuman)
        {
            if (!leftFoot) leftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
            if (!rightFoot) rightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
        }

        if (leftFoot) L.prevY = leftFoot.position.y;
        if (rightFoot) R.prevY = rightFoot.position.y;
    }

    void Update()
    {
        float dt = Mathf.Max(0.0001f, Time.deltaTime);
        float speed = GetSpeed();

        HandleFoot(leftFoot, ref L, speed, dt);
        HandleFoot(rightFoot, ref R, speed, dt);
    }

    void HandleFoot(Transform foot, ref FootState s, float speed, float dt)
    {
        if (!foot || !audioFx) return;

        float y = foot.position.y;
        float velY = (y - s.prevY) / dt;     // вертикальная скорость стопы

        bool justHit = (s.velY < -downVelThreshold) && (velY >= 0f) && (speed >= minMoveSpeed);

        if (justHit && Time.time >= s.lastStepTime + minInterval)
        {
            float loud = (speed >= runThreshold) ? 1.2f : 1.0f;
            audioFx.PlayStepAt(foot.position, loud);
            s.lastStepTime = Time.time;
        }

        s.velY = velY;
        s.prevY = y;
    }

    float GetSpeed()
    {
        if (agent && agent.isOnNavMesh) return agent.velocity.magnitude;
        if (cc) return cc.velocity.magnitude;
        if (rb) return new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;

        if (anim)
        {
            foreach (var p in anim.parameters)
                if (p.type == AnimatorControllerParameterType.Float && p.name == "speed")
                    return anim.GetFloat("speed");
        }
        return 0f;
    }
}