﻿using UnityEngine;

public class SnoopySoundHandler : MonoBehaviour
{
    [SerializeField]
    private AudioClip levelStartSound;

    [SerializeField]
    private AudioClip winSound;

    [SerializeField]
    private AudioClip loseSound;

    [SerializeField]
    private AudioClip[] launchSounds;

    private System.Random randomGenerator;

    public void Start()
    {
        randomGenerator = new System.Random();

        var eventService = GlobalState.EventService;
        eventService.AddEventHandler<LevelCompleteEvent>(OnLevelComplete);
        eventService.AddEventHandler<BubbleFiredEvent>(OnBubbleFired);
        eventService.AddEventHandler<IntroScrollCompleteEvent>(OnLevelStart);
    }

    private void OnLevelComplete(LevelCompleteEvent gameEvent)
    {
        Sound.PlaySoundEvent.Dispatch(gameEvent.Won ? winSound : loseSound);
    }

    private void OnBubbleFired(BubbleFiredEvent firedEvent)
    {
        Sound.PlaySoundEvent.Dispatch(launchSounds[randomGenerator.Next(launchSounds.Length)]);
    }

    private void OnLevelStart(IntroScrollCompleteEvent scrollEvent)
    {
        Sound.PlaySoundEvent.Dispatch(levelStartSound);
    }
}
