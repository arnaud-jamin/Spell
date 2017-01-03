﻿using FullSerializer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    public abstract class Graph : ScriptableObject
    {
        // ----------------------------------------------------------------------------------------
        [fsIgnore]
        private fsSerializer m_serializer = new fsSerializer();

        [SerializeField]
        [fsIgnore]
        protected string m_json;

        [SerializeField]
        protected List<Node> m_nodes = new List<Node>();

        [SerializeField]
        private Vector2 m_viewOffset = new Vector2(0, 0);

        [SerializeField]
        private float m_viewZoom = 1.0f;

        // ----------------------------------------------------------------------------------------
        public List<Node> Nodes { get { return m_nodes; } }
        public Vector2 ViewOffset { get { return m_viewOffset; } set { m_viewOffset = value; } }
        public float ViewZoom { get { return m_viewZoom; } set { m_viewZoom = value; } }

        // ----------------------------------------------------------------------------------------
        public Node CreateNode(Type type)
        {
            var node = Activator.CreateInstance(type) as Node;
            node.rectPosition = m_viewOffset + new Vector2(200, 200);
            m_nodes.Add(node);
            return node;
        }

        // ----------------------------------------------------------------------------------------
        public void Clear()
        {
            m_nodes.Clear();
            m_viewOffset = Vector2.zero;
            m_viewZoom = 1.0f;
        }

        // ----------------------------------------------------------------------------------------
        public void Save()
        {
            fsData data;
            m_serializer.TrySerialize(GetType(), this, out data).AssertSuccessWithoutWarnings();
            m_json = fsJsonPrinter.PrettyJson(data);
        }

        // ----------------------------------------------------------------------------------------
        public void Load()
        {
            var data = fsJsonParser.Parse(m_json);
            var obj = (object)this;
            m_serializer.TryDeserialize(data, GetType(), ref obj).AssertSuccessWithoutWarnings();
        }

        // ----------------------------------------------------------------------------------------
        protected abstract void OnDeserialized(object deserialized);
    } 

    public abstract class Graph<T> : Graph where T : class
    {
        // ----------------------------------------------------------------------------------------
        protected override void OnDeserialized(object deserialized)
        {
            OnDeserialized(deserialized as T);
        }

        // ----------------------------------------------------------------------------------------
        protected abstract void OnDeserialized(T deserialized);
    }
}