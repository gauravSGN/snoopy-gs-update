﻿using UnityEngine;
using Util;

namespace PowerUps
{
    public class PowerUpFactory : ScriptableFactory<PowerUpType, PowerUpDefinition>
    {
        override public GameObject CreateByType(PowerUpType type)
        {
            var definition = GetDefinitionByType(type);
            var instance = Instantiate(definition.Prefab);
            instance.GetComponent<PowerUp>().SetDefinition(definition);
            return instance;
        }
    }
}
