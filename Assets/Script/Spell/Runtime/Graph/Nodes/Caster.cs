using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Object")]
    public class Caster : ObjectNode
    {
        public List<Ability> Abilities = new List<Ability>();
    }
}
