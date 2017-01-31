using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class RemoveBuff : Action
    {
        public Expression<Buff> Buff = null;
        public Expression<TargetType> Target = new ExpressionValue<TargetType>();

        public override void Execute()
        {
        }
    }
}
