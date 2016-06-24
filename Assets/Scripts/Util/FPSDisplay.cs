﻿using UnityEngine;
using System.Collections;

public class FPSDisplay : MonoBehaviour
{
    private const float updateDelay = 0.1f;
	private float deltaTime;
    private float delayTimer;

	void Update()
	{
        delayTimer += Time.deltaTime;
        if(delayTimer >=  updateDelay)
        {
            deltaTime = Time.deltaTime;
            delayTimer = 0;
        }
	}

	void OnGUI()
	{
		int w = Screen.width, h = Screen.height;

		GUIStyle style = new GUIStyle();

		Rect rect = new Rect(0, 0, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 100;
		style.normal.textColor = new Color (1.0f, 1.0f, 1.0f, 1.0f);
		float msec = deltaTime * 1000.0f;
		float fps = 1f / deltaTime;
		string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		GUI.Label(rect, text, style);
	}
}