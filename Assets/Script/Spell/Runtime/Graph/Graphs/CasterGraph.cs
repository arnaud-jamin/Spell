using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    [CreateAssetMenu(fileName = "Caster", menuName = "Spell/Caster")]
    public class CasterGraph : Graph<CasterGraph>
    {
        public Caster Caster = new Caster();

        protected override void OnDeserialized(CasterGraph graph)
        {
            Caster = graph.Caster;
        }
    }
}
