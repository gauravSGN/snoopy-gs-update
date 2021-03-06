﻿using Model;
using UnityEngine;

namespace LevelEditor.Manipulator
{
    [ManipulatorAction(ManipulatorActionType.DeleteBubble)]
    public class DeleteBubbleAction : ManipulatorAction
    {
        public void Perform(LevelManipulator manipulator, int x, int y)
        {
            var key = BubbleData.GetKey(x, y);

            manipulator.Models.Remove(key);

            if (manipulator.Views.ContainsKey(key))
            {
                GameObject.Destroy(manipulator.Views[key]);
                manipulator.Views.Remove(key);
            }

            GlobalState.EventService.Dispatch(new LevelModifiedEvent());
            manipulator.RecomputeScores();
        }

        public void PerformAlternate(LevelManipulator manipulator, int x, int y)
        {
            Perform(manipulator, x, y);
        }
    }
}
