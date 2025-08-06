using UnityEngine;

public class IdleState : StateMachineBehaviour
{
    private static readonly int GoSearchHash = Animator.StringToHash("GoSearch");
    [SerializeField] private float idleDuration = 5f;
    private float _enterTime;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _enterTime = Time.time;
        var ai = animator.GetComponent<CharacterAI>();
        if (ai) ai.Agent.isStopped = true;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Time.time - _enterTime >= idleDuration)
            animator.SetTrigger(GoSearchHash);
    }
}
