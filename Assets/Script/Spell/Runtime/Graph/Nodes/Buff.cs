using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    public class Buff : Node
    {
        public Expression<string> Name = new TextValue();
        public Expression<Sprite> Icon = new SpriteValue();
        public Expression<float> Duration = new FloatValue();
        public List<BuffAction> Actions = new List<BuffAction>();
    }
}
