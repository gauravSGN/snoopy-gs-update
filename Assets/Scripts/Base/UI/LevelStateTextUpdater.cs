﻿using UnityEngine;
using UnityEngine.UI;

public class LevelStateTextUpdater : MonoBehaviour
{
    public Level level;
    public string format;
    public string fieldName;

    private Text text;

    void Start()
    {
        text = GetComponent<Text>();
        level.levelState.AddListener(UpdateState);
    }

    private void UpdateState(LevelState levelState)
    {
        if (text != null)
        {
            var type = levelState.GetType();
            var field = type.GetField(fieldName);

            if (field != null)
            {
                var value = field.GetValue(levelState).ToString();

                text.text = string.Format(format, value);
            }
        }
    }
}
