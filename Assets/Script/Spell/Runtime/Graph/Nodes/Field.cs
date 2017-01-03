using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    public abstract class Field : ObjectNode
    {
        public Expression<float> Duration = new FixedFloat(0.0f);
        public Expression<float> Tick = new FixedFloat(0.5f);
        public Action OnEnter = null;
        public Action OnStay = null;
        public Action OnTick = null;
        public Action OnExit = null;
        public FixedGameObject Selection = new FixedGameObject();
    }
}
