using UnityEngine;

public class SearchState : StateMachineBehaviour
{
    private static readonly int FoundItemHash = Animator.StringToHash("FoundItem");
    private CharacterAI _ai;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _ai = animator.GetComponent<CharacterAI>();
        if (_ai == null) return;

        _ai.Agent.isStopped = false;
        _ai.Agent.stoppingDistance = 0f;
        _ai.SetRandomDestination();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_ai == null) return;

        if (_ai.TryFindTarget())
        {
            animator.SetTrigger(FoundItemHash);
            return;
        }

        if (_ai.ReachedDestination())
            _ai.SetRandomDestination();
    }
}
