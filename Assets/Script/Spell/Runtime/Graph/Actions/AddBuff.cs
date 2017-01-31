using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class AddBuff : Action
    {
        public Buff Buff = null;
        public Expression<TargetType> Target = new ExpressionValue<TargetType>();

        public override void Execute()
        {
        }
    }
}
