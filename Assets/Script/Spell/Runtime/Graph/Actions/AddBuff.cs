using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class AddBuff : Action
    {
        public Buff Buff = null;
        public Expression<GameObject> Target = new GameObjectValue();

        public override void Execute()
        {
        }
    }
}
