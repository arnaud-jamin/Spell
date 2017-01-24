using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class ForeachSummoned : Node
    {
        public InAction In = new InAction();
        public InValue<GameObject> Summoner = new InValue<GameObject>();

        public OutAction Loop = new OutAction();
        public OutAction Finished = new OutAction();
        public OutValue<GameObject> Iterator = new OutValue<GameObject>();

        public ForeachSummoned()
        {
            In.Action = Execute;
        }

        public void Execute()
        {
            var summoner = Summoner.Value;
            if (summoner == null)
                return;

            var caster = summoner.GetComponent<Spell.Caster>();
            for (var i = 0; i < caster.Summoned.Count; ++i)
            {
                var summoned = caster.Summoned[i];
                Iterator.Value = summoned;
                Loop.Execute();
            }

            Finished.Execute();
        }
    }
}
