using UnityEngine;
using Slideout;
using FTUE;

[RequireComponent(typeof(AimLineEventTrigger))]
public class SpecialAimHandler : MonoBehaviour
{
    private AimLineEventTrigger eventTrigger;
    private bool aiming;
    private RectTransform rectTransform;
    private readonly Vector3[] corners = new Vector3[4];
    private uint blockers;

    protected void Start()
    {
        rectTransform = (transform as RectTransform);
        eventTrigger = GetComponent<AimLineEventTrigger>();

        var eventService = GlobalState.EventService;
        eventService.AddEventHandler<PopupDisplayedEvent>(Disable);
        eventService.AddEventHandler<PopupClosedEvent>(Enable);
        eventService.AddEventHandler<SlideoutStartEvent>(Disable);
        eventService.AddEventHandler<SlideoutCompleteEvent>(Enable);
        eventService.AddEventHandler<InputToggleEvent>(StopAiming);
        eventService.AddEventHandler<TutorialActiveEvent>(TutorialActive);
        blockers = 0;
    }

    protected void OnDestroy()
    {
        var eventService = GlobalState.EventService;
        eventService.RemoveEventHandler<PopupDisplayedEvent>(Disable);
        eventService.RemoveEventHandler<PopupClosedEvent>(Enable);
        eventService.RemoveEventHandler<SlideoutStartEvent>(Disable);
        eventService.RemoveEventHandler<SlideoutCompleteEvent>(Enable);
        eventService.RemoveEventHandler<InputToggleEvent>(OnInputToggle);
        eventService.RemoveEventHandler<TutorialActiveEvent>(TutorialActive);
    }

    protected void LateUpdate()
    {
        bool touching = Input.GetKey(KeyCode.Mouse0);

        // Only do special aiming if standard aiming isn't active
        if (!eventTrigger.Aiming && touching)
        {
            Vector3 position;

            if (PointInsideAimPanel(out position))
            {
                if (!aiming)
                {
                    StartAiming();
                }

                eventTrigger.FakeDrag(position);
            }
            else if (aiming)
            {
                StopAiming();
            }

        }
        else if (aiming && !touching)
        {
            eventTrigger.DoFire();
            aiming = false;
        }
    }

    private bool PointInsideAimPanel(out Vector3 position)
    {
        rectTransform.GetWorldCorners(corners);
        position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Check if point is inside the aim panel rect
        return (position.x >= corners[0].x) &&
               (position.x <= corners[2].x) &&
               (position.y >= corners[0].y) &&
               (position.y <= corners[2].y);
        }

    private void StartAiming()
    {
        aiming = true;
        eventTrigger.StartAiming();
    }

    private void StopAiming()
    {
        aiming = false;
        eventTrigger.StopAiming();
    }

    private void Disable()
    {
        blockers++;
        enabled = false;
        StopAiming();
    }

    private void Enable()
    {
        blockers--;
        enabled = (blockers <= 0);
    }

    private void TutorialActive(TutorialActiveEvent activeEvent)
    {
        if (activeEvent.active)
        {
            Disable();
        }
        else
        {
            Enable();
        }
    }

    private void OnInputToggle(InputToggleEvent gameEvent)
    {
        if (gameEvent.enabled)
        {
            Enable();
        }
        else
        {
            Disable();
        }
    }
}
