using System;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action/Heal")]
    public class Heal : Action
    {
        public Expression<float> Amount = new FixedFloat();
        public Expression<GameObject> Target = new FixedGameObject();

        public override void Execute()
        {
            var target = Target.Evaluate();
            var amount = Amount.Evaluate();

            var health = target.GetComponent<Health>();
            if (health != null)
            {
                health.Modify(new Health.Modifier { amount = amount, source = m_owner, canResurrect = false });
            }
        }
    }
}
