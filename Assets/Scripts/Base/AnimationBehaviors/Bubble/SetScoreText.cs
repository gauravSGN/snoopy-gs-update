using UnityEngine;

public class SetScoreText : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var gameObject = animator.gameObject;
        var parent = gameObject.transform.parent;

        if (parent != null)
        {
            var textMesh = gameObject.GetComponent<TextMesh>();

            textMesh.text = parent.GetComponent<BubbleScore>().Score.ToString();
            textMesh.color = parent.GetComponent<BubbleAttachments>().Model.definition.BaseColor;

            gameObject.transform.SetParent(null, true);
        }
    }
}
