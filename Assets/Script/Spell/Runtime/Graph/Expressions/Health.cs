using System;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Object")]
    public class Health : Expression<float>
    {
        public Expression<GameObject> Target = new GameObjectValue();

        public override float Evaluate()
        {
            var target = Target.Evaluate();
            var health = target.GetComponent<Spell.Health>();
            if (health == null)
                return 0;

            return health.CurrentHealth;
        }
    }
}
