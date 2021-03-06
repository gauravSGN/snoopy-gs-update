﻿using UnityEngine;
using System.Collections.Generic;
using System;

namespace Model
{
    public class ChainedRandomizer<T> where T : struct
    {
        public enum SelectionMethod { Once, Each }

        private readonly SelectionMethod method;
        private readonly System.Random rng;
        private readonly float[] weights;
        private readonly IList<T> items;

        private float[] activeWeights;
        private List<ChainedRandomizer<T>> exclusions;
        private T? selectedItem;

        public ChainedRandomizer(System.Random rng, SelectionMethod method, IList<T> items, IEnumerable<float> weights)
        {
            this.rng = rng;
            this.method = method;
            this.items = items;
            this.weights = new float[items.Count];

            var index = 0;

            foreach (var weight in weights)
            {
                this.weights[index++] = weight;

                if (index >= items.Count)
                {
                    break;
                }
            }
        }

        public void Reset()
        {
            selectedItem = null;
        }

        public T GetValue()
        {
            if ((method == SelectionMethod.Each) || !selectedItem.HasValue)
            {
                selectedItem = GenerateValue();
            }

            return selectedItem.Value;
        }

        public void AddExclusion(ChainedRandomizer<T> other)
        {
            if (other.method == SelectionMethod.Each)
            {
                throw new ArgumentException(
                    "Only randomizers with the Once selection method can be used as exclusions."
                );
            }

            exclusions = exclusions ?? new List<ChainedRandomizer<T>>();
            exclusions.Add(other);
            Reset();
        }

        private T GenerateValue()
        {
            float result = (float)rng.NextDouble() * GetTotalWeight();
            var count = items.Count;
            T item = default(T);

            for (var index = 0; index < count; index++)
            {
                if (result < activeWeights[index])
                {
                    item = items[index];
                    break;
                }

                result -= activeWeights[index];
            }

            return item;
        }

        private float GetTotalWeight()
        {
            activeWeights = activeWeights ?? ComputeActiveWeights();

            var count = items.Count;
            float totalWeight = 0.0f;

            for (var index = 0; index < count; index++)
            {
                totalWeight += activeWeights[index];
            }

            return totalWeight;
        }

        private float[] ComputeActiveWeights()
        {
            var count = weights.Length;
            activeWeights = new float[count];

            for (var index = 0; index < count; index++)
            {
                activeWeights[index] = weights[index];
            }

            if (exclusions != null)
            {
                count = exclusions.Count;

                for (var index = 0; index < count; index++)
                {
                    activeWeights[items.IndexOf(exclusions[index].GetValue())] = 0.0f;
                }
            }

            return activeWeights;
        }
    }
}
