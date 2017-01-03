using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    [CreateAssetMenu(fileName = "Field", menuName = "Spell/Field")]
    public class FieldGraph : Graph<FieldGraph>
    {
        public Field Field = new CircleField();

        protected override void OnDeserialized(FieldGraph graph)
        {
            Field = graph.Field;
        }
    }
}
