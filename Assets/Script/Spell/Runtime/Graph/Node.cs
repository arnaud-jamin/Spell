using FullSerializer;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Spell.Graph
{
    public abstract class Node 
    {
        [fsIgnore]
        private GameManager m_gameManager;

        [fsIgnore]
        private Graph m_graph;

        public GameManager GameManager { get { return m_gameManager; } }
        public Graph Graph { get { return m_graph; } }
        public Vector2 GraphPosition { get; set; }
        public string VariableName { get; set; }
        public virtual Type ValueType { get { return null; } }
        public virtual object BoxedValue { get { return null; }  set { } }
        public virtual Type BoxedValueType { get { return null; } }

        public void Initialize(GameManager gameManager, Graph graph)
        {
            m_gameManager = gameManager;
            m_graph = graph;
        }
    }
}
