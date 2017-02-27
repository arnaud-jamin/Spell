using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    public class AbilityReference : Expression<Ability>
    {
        public AbilityGraph Ability;

        public override Ability Evaluate()
        {
            return Ability.Root as Ability;
        }
    }
}
