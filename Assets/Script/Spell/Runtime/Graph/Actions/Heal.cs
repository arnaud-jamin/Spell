using System;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class Heal : Action
    {
        public Expression<float> Amount = new FloatValue();
        public Expression<TargetType> Target = new ExpressionValue<TargetType>();

        public override void Execute()
        {
            var target = GameManager.GetTarget(Target.Evaluate());
            if (target == null)
                return;

            var health = target.GetComponent<Health>();
            if (health == null)
                return;

            var amount = Amount.Evaluate();
            health.Modify(new Health.Modifier { amount = amount, source = m_owner, canResurrect = false });
        }
    }
}
