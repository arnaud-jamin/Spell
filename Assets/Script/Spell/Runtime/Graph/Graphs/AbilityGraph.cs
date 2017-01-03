using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    [CreateAssetMenu(fileName = "Ability", menuName = "Spell/Ability")]
    public class AbilityGraph : Graph<AbilityGraph>
    {
        public Spell.Ability Ability = new Spell.Ability();

        protected override void OnDeserialized(AbilityGraph graph)
        {
            Ability = graph.Ability;
        }
    }
}
