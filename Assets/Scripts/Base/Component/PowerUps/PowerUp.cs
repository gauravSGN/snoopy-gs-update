using Sequence;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GameTween;

namespace PowerUps
{
    public class PowerUp : MonoBehaviour
    {
        public const float DEFAULT_TRANSITION_TIME = 0.2f;
        private const float FULL_SILHOUETTE = 1.0f;

        // Magic numbers to fake the visual fill so it is more clear when the power up is not ready
        private const float MAGIC_LIMIT = 0.9999f;
        private const float MAGIC_MAX = 0.9f;

        [SerializeField]
        private Button button;

        [SerializeField]
        private GameObject glow;

        [SerializeField]
        private GameObject filledBackground;

        [SerializeField]
        private GameObject filledIcon;

        [SerializeField]
        private Image fillImage;

        [SerializeField]
        private GameObject fillLine;

        [SerializeField]
        private Animator characterAnimator;

        [SerializeField]
        private float secondsToFill;

        [SerializeField]
        private float max;

        [SerializeField]
        private float current;

        [SerializeField]
        private float progress;

        [SerializeField]
        private int lastBubbleCount;

        [SerializeField]
        private AnimationCurve hideCurve = AnimationCurve.Linear(0, 1, 1, 0);

        [SerializeField]
        private AnimationCurve showCurve = AnimationCurve.Linear(0, 0, 1, 1);

        private float currentFillTime;

        private PowerUpDefinition definition;
        private PowerUpController controller;
        private Animator ownAnimator;
        private ScaleTween activeTween;

        public void Setup(float setMax, PowerUpController setController, Level setLevel, GameObject character)
        {
            max = setMax;
            controller = setController;
            setLevel.levelState.AddListener(UpdateState);
            character.SetActive(true);
            characterAnimator = character.GetComponent<Animator>();
            ownAnimator = GetComponent<Animator>();

            var eventService = GlobalState.EventService;

            eventService.AddEventHandler<InputToggleEvent>(OnInputToggle);
            eventService.AddEventHandler<AddShotModifierEvent>(OnAddShotModifier);
            eventService.AddEventHandler<PrepareForBubblePartyEvent>(OnPrepareForBubbleParty);

            activeTween = GetComponent<ScaleTween>();
        }

        public void SetDefinition(PowerUpDefinition setDefinition)
        {
            if (definition == null)
            {
                definition = setDefinition;
            }
        }

        public void AddPowerUp()
        {
            if (button.interactable && (progress >= 1.0f))
            {
                Sound.PlaySoundEvent.Dispatch(Sound.SoundType.PowerUpCast);
                GlobalState.EventService.Dispatch<InputToggleEvent>(new InputToggleEvent(false));
                GlobalState.EventService.Dispatch(new FTUE.PowerUpUsedEvent(definition.Type));

                if (definition.LaunchSound != null)
                {
                    controller.OverrideLaunchSound(definition.LaunchSound);
                }

                controller.AddPowerUp(definition.Type);
                GlobalState.EventService.AddEventHandler<PowerUpPrepareForReturnEvent>(OnItemReturn);
                Reset();
                StartCoroutine(ShowCharacter());
            }
        }

        public void Hide(float transitionTime)
        {
            StartCoroutine(HideShow(hideCurve, transitionTime));
        }

        public void Show(float transitionTime)
        {
            StartCoroutine(HideShow(showCurve, transitionTime));
        }

        private void OnItemReturn()
        {
            characterAnimator.SetTrigger("Finish");
            characterAnimator.ResetTrigger("AddPowerUp");
            Show(DEFAULT_TRANSITION_TIME);
        }

        private void OnInputToggle(InputToggleEvent gameEvent)
        {
            button.interactable = gameEvent.enabled;
        }

        private void OnAddShotModifier(AddShotModifierEvent gameEvent)
        {
            button.interactable = (gameEvent.type == ShotModifierType.PowerUp);
        }

        private void UpdateState(Observable levelState)
        {
            if ((current < max) && (glow != null))
            {
                var currentBubbleCount = (levelState as LevelState).typeTotals[definition.BubbleType];

                if (currentBubbleCount < lastBubbleCount)
                {
                    var fillRate = ((max - current) - progress) / Mathf.Max(1, currentBubbleCount);
                    progress += (lastBubbleCount - currentBubbleCount) * fillRate;
                    StartCoroutine(UpdateFillImage());
                }

                lastBubbleCount = currentBubbleCount;

                if (!glow.activeSelf && (progress >= 1.0f))
                {
                    GlobalState.EventService.Dispatch(new FTUE.PowerUpFilledEvent(definition.Type));
                    ownAnimator.SetTrigger("Charged");
                    glow.SetActive(true);
                    filledBackground.SetActive(false);
                    Sound.PlaySoundEvent.Dispatch(Sound.SoundType.PowerUpFill);
                    if (activeTween != null)
                    {
                        activeTween.ScaleTo();
                    }
                }
            }
        }

        private void Reset()
        {
            ownAnimator.SetTrigger("Fired");
            filledIcon.SetActive(false);
            glow.SetActive(false);
            filledBackground.SetActive(true);

            var fillLineTransform = (RectTransform)fillLine.transform;
            fillLineTransform.localPosition = new Vector3(fillLineTransform.localPosition.x, 0);

            fillImage.fillAmount = FULL_SILHOUETTE;
            progress = 0.0f;
            current += 1.0f;
        }

        private IEnumerator UpdateFillImage()
        {
            if (currentFillTime <= 0.01f)
            {
                var fillLineTransform = (RectTransform)fillLine.transform;

                filledIcon.SetActive(true);

                while (currentFillTime < secondsToFill)
                {
                    currentFillTime += Time.deltaTime;

                    var magicMultiplier = (progress <= MAGIC_LIMIT) ? MAGIC_MAX : 1.0f;

                    fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount,
                                                      FULL_SILHOUETTE - (magicMultiplier * progress),
                                                      (currentFillTime / secondsToFill));

                    var newY = (FULL_SILHOUETTE - fillImage.fillAmount) * fillLineTransform.rect.height;
                    fillLineTransform.localPosition = new Vector3(fillLineTransform.localPosition.x, newY);

                    yield return null;
                }

                currentFillTime = 0.0f;

                if (progress < 1.0f)
                {
                    filledIcon.SetActive(false);
                }
            }
        }

        private IEnumerator HideShow(AnimationCurve curve, float transitionTime)
        {
            float time = 0f;
            float newValue;

            while (time <= transitionTime)
            {
                time += Time.deltaTime;
                newValue = curve.Evaluate(time / transitionTime);
                transform.localScale = new Vector3(newValue, newValue, 1);
                yield return null;
            }

            newValue = curve.Evaluate(1f);
            transform.localScale = new Vector3(newValue, newValue, 1);
        }

        private IEnumerator ShowCharacter()
        {
            yield return StartCoroutine(HideShow(hideCurve, DEFAULT_TRANSITION_TIME));
            characterAnimator.SetTrigger("AddPowerUp");
        }

        private void OnPrepareForBubbleParty()
        {
            Hide(DEFAULT_TRANSITION_TIME);
            ownAnimator.SetTrigger("Fired");
        }
    }
}
