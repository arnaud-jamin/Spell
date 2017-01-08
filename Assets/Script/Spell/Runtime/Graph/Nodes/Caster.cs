using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Object")]
    public class Caster : ObjectNode
    {
        public List<Spell.Ability> Abilities = new List<Spell.Ability>();
    }
}
