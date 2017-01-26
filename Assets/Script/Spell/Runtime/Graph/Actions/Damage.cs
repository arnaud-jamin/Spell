using System;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class Damage : Node
    {
        public Damage()
        {
            var amount = AddInValue<float>("amount", 0);
            var damageType = AddInValue<DamageType>("DamageType", DamageType.Physical);
            var target = AddInValue<GameObject>("Target", null);

            AddInAction("In", () =>
            {
                if (target.Value == null)
                    return;

                var health = target.Value.GetComponent<Spell.Health>();
                if (health != null)
                {
                    health.Modify(new Spell.Health.Modifier { amount = -amount.Value, source = m_owner, canResurrect = false, damageType = damageType.Value });
                }
            });

            AddOutAction("Out");
        }
    }
}
