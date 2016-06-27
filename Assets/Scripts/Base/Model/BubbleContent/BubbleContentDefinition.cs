﻿using UnityEngine;
using Model;

namespace BubbleContent
{
    sealed public class BubbleContentDefinition : ScriptableObject, GameObjectDefinition<BubbleContentType>
    {
        public BubbleContentType Type { get { return type; } }
        public GameObject Prefab { get { return prefab; } }

        public BubbleContentType type;
        public GameObject prefab;
    }
}