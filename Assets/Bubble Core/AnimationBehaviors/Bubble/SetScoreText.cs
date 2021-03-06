using Service;
using Registry;
using UnityEngine;
using UnityEngine.UI;

public class SetScoreText : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var gameObject = animator.gameObject;
        var parent = gameObject.transform.parent;
        var bubbleScore = parent.GetComponent<BubbleScore>();

        gameObject.GetComponent<BitmapFontRenderer>().Text = bubbleScore.Score.ToString();
        gameObject.transform.SetParent(null, true);
    }
}
