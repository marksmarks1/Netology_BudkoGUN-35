using UnityEngine;

public class CollectState : StateMachineBehaviour
{
    private static readonly int DoneCollectHash = Animator.StringToHash("DoneCollect");
    [SerializeField] private float collectDuration = 1.5f;

    private CharacterAI _ai;
    private float _collectStart = -1f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _ai = animator.GetComponent<CharacterAI>();
        if (_ai == null) return;

        _ai.Agent.isStopped = false;
        _ai.Agent.stoppingDistance = _ai.StopDistance;
        _collectStart = -1f;
        _ai.MoveToTarget();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_ai == null) { animator.SetTrigger(DoneCollectHash); return; }
        if (_ai.CurrentTarget == null) { animator.SetTrigger(DoneCollectHash); return; }

        _ai.MoveToTarget();

        float dist = Vector3.Distance(_ai.transform.position, _ai.CurrentTarget.position);
        if (dist <= _ai.Agent.stoppingDistance + 0.05f)
        {
            _ai.Agent.isStopped = true;

            if (_collectStart < 0f) _collectStart = Time.time;

            if (Time.time - _collectStart >= collectDuration)
            {
                _ai.CollectTarget();
                _ai.Agent.isStopped = false;
                animator.SetTrigger(DoneCollectHash);
            }
        }
    }
}
