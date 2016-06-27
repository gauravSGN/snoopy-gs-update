﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FakeSplashLoad : MonoBehaviour
{
    [SerializeField]
    private Slider fillBar;

    [SerializeField]
    private Button playButton;

    [SerializeField]
    private float fillPerSecond;

    private float progress;

    private readonly float delay = 0.1f;

    void Start()
    {
        playButton.gameObject.SetActive(false);
        StartCoroutine(FakeLoad());
    }

    private IEnumerator FakeLoad()
    {
        while (progress < 1)
        {
            yield return new WaitForSeconds(delay);
            progress += (fillPerSecond * delay);
            fillBar.value = progress;
        }
        fillBar.gameObject.SetActive(false);
        playButton.gameObject.SetActive(true);
    }
}