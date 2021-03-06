using UnityEngine;

public class PowerUpCharacterEvents : MonoBehaviour
{
    [SerializeField]
    private float zTranslation;

    private Vector3 origin;

    // Event names need to match exactly what is in the spine animation data
    public void MoveToFront()
    {
        origin = gameObject.transform.localPosition;
        gameObject.transform.localPosition = new Vector3(origin.x, origin.y, origin.z + zTranslation);
        GlobalState.EventService.Dispatch<PowerUpChargeEvent>(new PowerUpChargeEvent());
    }

    public void ReturnToBack()
    {
        gameObject.transform.localPosition = origin;
    }

    public void ReturnToKid()
    {
        GlobalState.EventService.Dispatch(new PowerUpReturningEvent());
    }

    public void CatchKite()
    {
        GlobalState.EventService.Dispatch(new PowerUpReturnedEvent());
    }
}
