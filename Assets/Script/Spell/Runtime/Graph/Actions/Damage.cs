using System;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class Damage : Action
    {
        public Expression<float> Amount = new FloatValue();
        public Expression<DamageType> DamageType = new DamageTypeValue();
        public Expression<GameObject> Target = new GameObjectValue();

        public override void Execute()
        {
            var target = Target.Evaluate();
            var amount = Amount.Evaluate();
            var damageType = DamageType.Evaluate();

            var health = target.GetComponent<Spell.Health>();
            if (health != null)
            {
                health.Modify(new Spell.Health.Modifier { amount = -amount, source = m_owner, canResurrect = false, damageType = damageType });
            }
        }
    }
}
