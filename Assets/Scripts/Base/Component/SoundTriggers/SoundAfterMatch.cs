using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundAfterMatch : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] sounds;

    [SerializeField]
    private ThreshholdCondition condition;

    [SerializeField]
    private int bubbleMatchThreshold;

    [SerializeField]
    private float chanceToPlay;

    private enum ThreshholdCondition
    {
        EqualTo,
        GreaterThanOrEqualTo
    }

    private AudioSource audioSource;
    private readonly System.Random rnd = new System.Random();
    private int counter;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        GlobalState.EventService.AddEventHandler<BubbleReactionEvent>(OnBubbleReactionEvent);
        GlobalState.EventService.AddEventHandler<ReadyForNextBubbleEvent>(OnReadyForNextBubbleEvent);
    }

    void OnBubbleReactionEvent()
    {
        counter++;
    }

    void OnReadyForNextBubbleEvent()
    {
        if (ConditionMet() && (UnityEngine.Random.value <= chanceToPlay))
        {
            audioSource.PlayOneShot(sounds[rnd.Next(sounds.Length)], 0.7f);
        }

        counter = 0;
    }

    bool ConditionMet()
    {
        bool equalToConditionMet = ((condition == ThreshholdCondition.EqualTo) && (counter == bubbleMatchThreshold));
        bool greaterThanOrEqualToConditionMet = ((condition == ThreshholdCondition.GreaterThanOrEqualTo) &&
                                                 (counter >= bubbleMatchThreshold));

        return equalToConditionMet || greaterThanOrEqualToConditionMet;
    }
}
