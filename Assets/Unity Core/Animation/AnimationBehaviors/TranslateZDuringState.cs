using UnityEngine;

public class TranslateZDuringState : StateMachineBehaviour
{
    [SerializeField]
    private float z;

    private Vector3 originalPosition;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        originalPosition = animator.gameObject.transform.localPosition;

        animator.gameObject.transform.localPosition = new Vector3(originalPosition.x,
                                                                  originalPosition.y,
                                                                  originalPosition.z + z);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.gameObject.transform.localPosition = originalPosition;
    }
}
