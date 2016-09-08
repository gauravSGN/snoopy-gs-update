﻿using System;
using UnityEngine;
using System.Collections;

namespace Util
{
    static public class FrameUtil
    {
        static public void AtEndOfFrame(Action action)
        {
            GlobalState.Instance.RunCoroutine(RunAtEndOfFrame(action));
        }

        static public void OnNextFrame(Action action)
        {
            GlobalState.Instance.RunCoroutine(RunOnNextFrame(action));
        }

        static private IEnumerator RunAtEndOfFrame(Action action)
        {
            yield return new WaitForEndOfFrame();

            action();
        }

        static private IEnumerator RunOnNextFrame(Action action)
        {
            yield return null;

            action();
        }
    }
}
