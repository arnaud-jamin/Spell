using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    public class Ability : Node
    {
        public InValue<string> Name;
        public InValue<Sprite> Icon;
        public InValue<float> ManaCost;
        public InValue<float> Cooldown;
        public InValue<CastTargetType> CastTargetType;
        public OutAction Action;

        public override Color Color { get { return Graph.AbilityColor; } }

        public Ability()
        {
            Name = AddInValue<string>("Name", "Ability");
            Icon = AddInValue<Sprite>("DamageType", null);
            ManaCost = AddInValue<float>("ManaCost", 100);
            Cooldown = AddInValue<float>("Cooldown", 10);
            Action = AddOutAction("Action");
        }
    }
}
