using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    public class Field : ObjectNode
    {
        public Shape Shape;
        public Expression<float> Duration = new FloatValue(0.0f);
        public Expression<float> Tick = new FloatValue(0.5f);
        public Action OnEnter = null;
        public Action OnStay = null;
        public Action OnTick = null;
        public Action OnExit = null;
        public Expression<IteratorType> Iterator = new ExpressionValue<IteratorType>();
    }
}
