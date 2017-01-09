using System;
using UnityEngine;

namespace Spell.Graph
{
    public class Ability : Node
    {
        public Expression<float> ManaCost = new FloatValue();
        public Expression<float> Cooldown = new FloatValue();
        public Expression<CastTargetType> CastTargetType = new CastTargetTypeValue();
        public Action Action;
    }
}
