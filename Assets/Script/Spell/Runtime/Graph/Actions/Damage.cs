using System;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class Damage : Action
    {
        public Expression<float> Amount = new FloatValue();
        public Expression<DamageType> DamageType = new DamageTypeValue();
        public Expression<TargetType> Target = new ExpressionValue<TargetType>();

        public override void Execute()
        {
            var targetObject = GameManager.GetTarget(Target.Evaluate());
            if (targetObject == null)
                return;

            var health = targetObject.GetComponent<Health>();
            if (health != null)
                return;

            var amount = Amount.Evaluate();
            var damageType = DamageType.Evaluate();
            health.Modify(new Health.Modifier { amount = -amount, source = m_owner, canResurrect = false, damageType = damageType });
        }
    }
}
