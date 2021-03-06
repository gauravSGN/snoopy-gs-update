using Util;
using Service;
using Animation;
using UnityEngine;
using System.Linq;
using ScanFunction = Util.CastingUtil.ScanFunction;
using System.Collections.Generic;

namespace PowerUps
{
    public class PowerUpController : MonoBehaviour
    {
        public const int THREE_COMBO_ROWS = 3;
        public const int FOUR_COMBO_ROWS = 5;

        [SerializeField]
        private PowerUpFactory powerUpFactory;

        [SerializeField]
        private BubbleLauncher launcher;

        [SerializeField]
        private BubbleModifierController modifiers;

        [SerializeField]
        private AnimationType shooterType;

        [SerializeField]
        private AnimationType deathEffectType;

        [SerializeField]
        private AnimationType explosionEffectType;

        [SerializeField]
        private PowerUpScanMap scanMap;

        [SerializeField]
        private BubbleDefinition shooterDefinition;

        [SerializeField]
        private AimLine aimline;

        [SerializeField]
        private GameObject fillCharacter;

        private Transform[] anchors;
        private List<PowerUp> powerUps;
        private PowerUpType powerUpType;
        private int totalPowerUpsInUse;
        private AnimationService animationService;

        void Awake()
        {
            var transforms = gameObject.GetComponentsInChildren<Transform>();
            anchors = transforms.Where(child => child != gameObject.transform).ToArray();

            powerUps = new List<PowerUp>();
        }

        public void Setup(float[] fillData)
        {
            animationService = GlobalState.AnimationService;
            scanMap.Load();
            var level = gameObject.GetComponentInParent<Level>();
            var anchorLength = anchors.Length;
            var anchorIndex = 0;

            for (int index = 0, length = fillData.Length; index < length; index++)
            {
                if (anchorIndex < anchorLength)
                {
                    if (fillData[index] > 0.0f)
                    {
                        var powerUp = powerUpFactory.CreateByType((PowerUpType)(1 << index));
                        var powerUpComponent = powerUp.GetComponent<PowerUp>();
                        powerUpComponent.Setup(fillData[index], this, level, fillCharacter);
                        powerUp.transform.SetParent(anchors[anchorIndex]);
                        powerUp.transform.localPosition = Vector3.zero;
                        anchorIndex++;
                        powerUps.Add(powerUpComponent);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        public void AddPowerUp(PowerUpType type)
        {
            if (powerUpType == PowerUpType.Empty)
            {
                modifiers.Add(AddScan, ShotModifierType.PowerUp);
                aimline.ModifyAimline(GlobalState.Instance.Config.aimline.extended);
            }

            modifiers.SetAnimation(animationService.CreateByType(shooterType));

            powerUpType |= type;
            totalPowerUpsInUse += 1;

            if (totalPowerUpsInUse > 2)
            {
                powerUpType = totalPowerUpsInUse == 3 ? PowerUpType.ThreeCombo : PowerUpType.FourCombo;
            }

            GlobalState.EventService.Dispatch<PowerUpAppliedEvent>(new PowerUpAppliedEvent(powerUpType));
        }

        public void AddScan(GameObject bubble)
        {
            // Make bubble unmatchable
            var model = bubble.GetComponent<BubbleModelBehaviour>().Model;
            model.type = BubbleType.Colorless;
            model.definition = shooterDefinition;

            var explosion = bubble.AddComponent<BubbleExplode>();
            explosion.Setup(GetScanFunction(bubble), deathEffectType, explosionEffectType);

            bubble.AddComponent<BreakOnDeactivate>();

            powerUpType = 0;
            totalPowerUpsInUse = 0;
        }

        public void OverrideLaunchSound(AudioClip soundOverride)
        {
            launcher.SetLaunchSoundOverride(soundOverride);
        }

        // Just set objects active/inactive until we have an animation
        public void HidePowerUps(float transitionTime)
        {
            for (int i = 0, count = powerUps.Count; i < count; ++i)
            {
                powerUps[i].Hide(transitionTime);
            }
        }

        public void ShowPowerUps(float transitionTime)
        {
            for (int i = 0, count = powerUps.Count; i < count; ++i)
            {
                powerUps[i].Show(transitionTime);
            }
        }

        private ScanFunction GetScanFunction(GameObject bubble)
        {
            ScanFunction scanFunction;

            if (powerUpType == PowerUpType.ThreeCombo)
            {
                scanFunction = () =>
                {
                    return CastingUtil.FullRowBubbleCast(bubble, THREE_COMBO_ROWS, THREE_COMBO_ROWS);
                };
            }
            else if (powerUpType == PowerUpType.FourCombo)
            {
                scanFunction = () =>
                {
                    return CastingUtil.FullRowBubbleCast(bubble, FOUR_COMBO_ROWS, FOUR_COMBO_ROWS);
                };
            }
            else
            {
                var type = powerUpType;
                scanFunction = () => { return CastingUtil.RelativeBubbleCast(bubble, scanMap.Map[type]); };
            }

            return scanFunction;
        }
    }
}
