﻿using PowerUps;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Util;

public class LevelIntroScroll : MonoBehaviour
{
    [SerializeField]
    private float scrollSpeed;

    [SerializeField]
    private float scrollDelay;

    [SerializeField]
    private Transform scrollBound;

    [SerializeField]
    private Transform gameView;

    [SerializeField]
    private GameObject launcherGroup;

    [SerializeField]
    private PowerUpController powerUpController;

    [SerializeField]
    private List<GameObject> disableOnScroll;

    [SerializeField]
    private Level level;

    public void Start()
    {
        GlobalState.EventService.AddEventHandler<LevelLoadedEvent>(OnLevelLoaded);
    }

    private void OnLevelLoaded()
    {
        GlobalState.EventService.RemoveEventHandler<LevelLoadedEvent>(OnLevelLoaded);

        var maxY = level.Loader.LevelData.Bubbles.Aggregate(1, (acc, b) => Mathf.Max(acc, b.Y));
        var targetY = -(maxY - 11) * GlobalState.Instance.Config.bubbles.size * MathUtil.COS_30_DEGREES;

        GameObjectUtil.SetActive(disableOnScroll, false);
        StartCoroutine(DoScroll(Mathf.Min(targetY, scrollBound.position.y)));
    }

    private IEnumerator DoScroll(float targetY)
    {
        powerUpController.HidePowerUps(0.01f);

        var finalLauncherPosition = launcherGroup.transform.localPosition + new Vector3(0f, targetY, 0f);
        launcherGroup.transform.position = finalLauncherPosition;

        var position = gameView.position;
        var y = position.y;

        yield return new WaitForSeconds(scrollDelay);

        while (y > targetY)
        {
            y = Mathf.Max(targetY, y - scrollSpeed * Time.deltaTime);
            gameView.position = new Vector3(position.x, y, position.z);
            launcherGroup.transform.position = finalLauncherPosition;
            yield return null;
        }

        GameObjectUtil.SetActive(disableOnScroll, true);
        powerUpController.ShowPowerUps(PowerUp.DEFAULT_TRANSITION_TIME);

        GlobalState.EventService.Dispatch(new LevelIntroCompleteEvent());

        // Input starts disabled from a callback in the PreLevelPopup
        GlobalState.EventService.Dispatch(new InputToggleEvent(true));
    }
}
