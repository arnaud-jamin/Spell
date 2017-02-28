using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    public class Ability : Expression<Ability>
    {
        public string Name = string.Empty;
        public Sprite Icon = null;
        public float ManaCost = 0;
        public float Cooldown = 0;
        public CastTargetType TargetType = CastTargetType.Ground;
        public List<Action> Actions = new List<Action>();

        public override Ability Evaluate()
        {
            return this;
        }
    }
}
