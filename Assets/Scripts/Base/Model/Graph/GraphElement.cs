﻿using System;
using UnityEngine;
using System.Collections.Generic;

namespace Graph
{
    abstract public class GraphElement<T> : GraphNode where T : GraphElement<T>
    {
        public Action<T> OnRemoveNeighbor;

        protected readonly List<T> neighbors = new List<T>();

        [SerializeField]
        private bool isRoot;

        [SerializeField]
        private uint distanceFromRoot;

        public bool IsRoot
        {
            get { return isRoot; }
            set { isRoot = value; }
        }

        public uint DistanceFromRoot
        {
            get { return distanceFromRoot; }
            set { distanceFromRoot = value; }
        }

        public IEnumerable<GraphNode> Neighbors
        {
            get
            {
                foreach (var neighbor in neighbors)
                {
                    yield return neighbor;
                }
            }
        }

        public int NumberOfNeighbors { get { return neighbors.Count; } }

        public GraphElement()
        {
            ResetDistanceFromRoot();
        }

        public void Connect(T node)
        {
            if (!neighbors.Contains(node))
            {
                neighbors.Add(node);
                node.Connect((T)this);
            }
        }

        public void Disconnect(T node)
        {
            if (neighbors.Contains(node))
            {
                neighbors.Remove(node);
                node.Disconnect((T)this);

                if (OnRemoveNeighbor != null)
                {
                    OnRemoveNeighbor(node);
                }
            }
        }

        public void DisconnectAll()
        {
            while (neighbors.Count > 0)
            {
                Disconnect(neighbors[0]);
            }
        }

        virtual public void RemoveFromGraph()
        {
            DisconnectAll();
        }

        public void SortNeighbors()
        {
            neighbors.Sort((a, b) => a.distanceFromRoot.CompareTo(b.distanceFromRoot));
        }

        public void MinimizeDistanceFromRoot()
        {
            if (IsRoot)
            {
                DistanceFromRoot = 0;
                return;
            }

            foreach (var neighbor in neighbors)
            {
                if (neighbor.DistanceFromRoot < DistanceFromRoot)
                {
                    DistanceFromRoot = neighbor.DistanceFromRoot + 1;
                }
            }
        }

        public void PropagateRootDistance()
        {
            var nextDistance = DistanceFromRoot + 1;

            foreach (var neighbor in neighbors)
            {
                if (neighbor.DistanceFromRoot > nextDistance)
                {
                    neighbor.DistanceFromRoot = nextDistance;
                    neighbor.PropagateRootDistance();
                }
            }
        }

        public void ResetDistanceFromRoot()
        {
            distanceFromRoot = IsRoot ? 0u : int.MaxValue;
        }
    }
}
