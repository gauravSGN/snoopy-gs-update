using UnityEngine;

public class BubbleAttachments : MonoBehaviour
{
    public Bubble Model { get { return model; } }

    [SerializeField]
    private Bubble model;

    public void Attach(GameObject other)
    {
        Model.Connect(other.GetComponent<BubbleAttachments>().Model);
    }

    public void SetModel(Bubble model)
    {
        this.model = model;

        model.OnPopped += PoppedHandler;
        model.OnDisconnected += DisconnectedHandler;
    }

    protected void Start()
    {
        if (Model.type != BubbleType.Ceiling)
        {
            GlobalState.EventService.AddEventHandler<CullAllBubblesEvent>(OnCullAllBubbles);
        }
    }

    protected void OnDestroy()
    {
        RemoveHandlers();
    }

    private void RemoveHandlers()
    {
        GlobalState.EventService.RemoveEventHandler<CullAllBubblesEvent>(OnCullAllBubbles);
        Model.OnPopped -= PoppedHandler;
        Model.OnDisconnected -= DisconnectedHandler;
    }

    private void PoppedHandler(Bubble bubble)
    {
        RemoveHandlers();

        gameObject.layer = (int)Layers.IgnoreRayCast;
        var death = gameObject.GetComponent<BubbleDeath>();
        if (death != null)
        {
            StartCoroutine(death.TriggerDeathEffects(BubbleDeathType.Pop));
        }
    }

    private void DisconnectedHandler(Bubble bubble)
    {
        RemoveHandlers();

        var rigidBody = GetComponent<Rigidbody2D>();
        gameObject.layer = (int)Layers.FallingObjects;

        var reactionsConfig = GlobalState.Instance.Config.reactions;
        rigidBody.isKinematic = false;
        rigidBody.AddForce(new Vector3(Random.Range(reactionsConfig.cullMinXForce, reactionsConfig.cullMaxXForce),
                                       Random.Range(reactionsConfig.cullMinYForce, reactionsConfig.cullMaxYForce)));
    }

    private void OnCullAllBubbles(CullAllBubblesEvent gameEvent)
    {
        if (gameObject.layer == (int)Layers.GameObjects)
        {
            Model.RemoveFromGraph();
        }
    }
}
