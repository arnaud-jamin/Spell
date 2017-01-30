using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class RemoveBuff : Action
    {
        public Expression<Buff> Buff = null;
        public Expression<GameObject> Target = new GameObjectValue();

        public override void Execute()
        {
        }
    }
}
