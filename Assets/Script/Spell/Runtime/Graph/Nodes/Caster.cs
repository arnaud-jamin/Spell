using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Object")]
    public class Caster : Node
    {
        public override Color Color { get { return Graph.ShapeColor; } }

        private InValue<List<Ability>> m_abilities;

        public Caster()
        {
            m_abilities = AddInValue<List<Ability>>("Abilities", new List<Ability>());

            AddOutValue<Caster>("Caster", () =>
            {
                return this;
            });
        }
    }
}
