using Util;
using Config;
using Service;
using UnityEngine;
using System.Collections;

// The GlobalState prefab needs to be in every scene that uses it for the
// scene editor to work without coming from a different scene.
public class GlobalState : SingletonBehaviour<GlobalState>
{
    public static EventService EventService { get { return Instance.Services.Get<EventService>(); } }
    public static UserStateService User { get { return Instance.Services.Get<UserStateService>(); } }
    public static PopupService PopupService { get { return Instance.Services.Get<PopupService>(); } }
    public static SceneService SceneService { get { return Instance.Services.Get<SceneService>(); } }
    public static UpdateService UpdateService { get { return Instance.Services.Get<UpdateService>(); } }
    public static AssetService AssetService { get { return Instance.Services.Get<AssetService>(); } }
    public static AnimationService AnimationService { get { return Instance.Services.Get<AnimationService>(); } }
    public static InitializerService InitializerService { get { return Instance.Services.Get<InitializerService>(); } }
    public static SoundService SoundService { get { return Instance.Services.Get<SoundService>(); } }

    public GameConfig Config { get { return config; } }
    public ServiceRepository Services { get { return services; } }

    [SerializeField]
    private GameConfig config;

    [SerializeField]
    private TextAsset gsDescriptorJSON;

    [SerializeField]
    private TextAsset servicesJSON;

    private readonly ServiceRepository services = new ServiceRepository();

    override protected void Awake()
    {
        base.Awake();

        if (Instance == this)
        {
            Services.RegisterFromJson(servicesJSON.text);

            GSDescriptorFactory.CreateByPlatform(Application.platform, gsDescriptorJSON).Initialize();
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Input.multiTouchEnabled = false;
        }
    }

    public void RunCoroutine(IEnumerator enumerator)
    {
        if (Instance == this)
        {
            StartCoroutine(enumerator);
        }
    }
}
