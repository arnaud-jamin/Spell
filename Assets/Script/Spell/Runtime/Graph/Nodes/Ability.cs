using System;
using UnityEngine;

namespace Spell.Graph
{
    public class Ability : Node
    {
        public Expression<float> ManaCost = new FixedFloat();
        public Expression<float> Cooldown = new FixedFloat();
        public Expression<CastTargetType> CastTargetType = new FixedCastTargetType();
        public Action Action;
    }
}
