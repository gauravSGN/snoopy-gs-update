﻿using UnityEngine;

namespace LevelEditor.Manipulator
{
    public interface ManipulatorAction
    {
        Sprite ButtonSprite { get; }

        void Perform(LevelManipulator manipulator, int x, int y);
    }
}
