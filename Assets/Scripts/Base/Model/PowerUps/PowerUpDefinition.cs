﻿using UnityEngine;
using Model;

namespace PowerUps
{
    public class PowerUpDefinition : ScriptableObject, GameObjectDefinition<PowerUpType>
    {
        public PowerUpType Type { get { return type; } }
        public GameObject Prefab { get { return prefab; } }
        public BubbleType BubbleType { get { return bubbleType; } }
        public AudioClip LaunchSound { get { return launchSound; } }

        [SerializeField]
        private PowerUpType type;

        [SerializeField]
        private BubbleType bubbleType;

        [SerializeField]
        private GameObject prefab;

        [SerializeField]
        private AudioClip launchSound;
    }
}
