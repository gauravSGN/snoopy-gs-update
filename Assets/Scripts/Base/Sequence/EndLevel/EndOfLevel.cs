using Event;
using Service;
using Registry;
using UnityEngine;

namespace Sequence
{
    public class EndOfLevel : MonoBehaviour, Blockade
    {
        [SerializeField]
        private Level level;

        [SerializeField]
        private WinLevel winLevelSequence;

        [SerializeField]
        private LoseLevel loseLevelSequence;

        private BlockadeService blockade;
        private bool readyToContinue = false;

        public BlockadeType BlockadeType { get { return BlockadeType.AllNonReaction; } }

        protected void Start()
        {
            blockade = GlobalState.Instance.Services.Get<BlockadeService>();

            var eventService = GlobalState.EventService;
            eventService.AddEventHandler<ReactionsFinishedEvent>(OnReactionsFinished);
            eventService.AddEventHandler<FiringAnimationCompleteEvent>(ContinueLevel);
            eventService.AddEventHandler<PurchasedExtraMovesEvent>(OnPurchasedExtraMoves);
            eventService.AddEventHandler<BubbleFiringEvent>(OnBubbleFiring);

            Util.FrameUtil.AtEndOfFrame(() =>
            {
                GlobalState.SoundService.PreloadMusic(Sound.MusicType.WinLevel);
            });
        }

        private void OnReactionsFinished()
        {
            if (level.AllGoalsCompleted)
            {
                BeginNextSequence(winLevelSequence);
            }
            else if (level.levelState.remainingBubbles <= 0)
            {
                BeginNextSequence(loseLevelSequence);
            }
            else
            {
                ContinueLevel();
            }
        }

        // Only continue with the level if both the reaction queue and the launcher
        // character's animations are complete.
        private void ContinueLevel()
        {
            if (readyToContinue)
            {
                GlobalState.EventService.Dispatch(new ReadyForNextBubbleEvent());
                GlobalState.EventService.Dispatch(new InputToggleEvent(true));
                readyToContinue = false;
                blockade.Remove(this);
            }
            else
            {
                readyToContinue = true;
            }
        }

        private void BeginNextSequence(BaseSequence<LevelState> sequence)
        {
            blockade.Remove(this);
            GlobalState.EventService.RemoveEventHandler<ReactionsFinishedEvent>(OnReactionsFinished);
            sequence.Begin(level.levelState);
        }

        private void OnPurchasedExtraMoves()
        {
            GlobalState.EventService.AddEventHandler<ReactionsFinishedEvent>(OnReactionsFinished);
        }

        private void OnBubbleFiring()
        {
            blockade.Add(this);
        }
    }
}
