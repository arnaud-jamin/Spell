using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class ForeachSummoned : Node
    {
        public ForeachSummoned()
        {
            var inSummoner = AddInValue<GameObject>("Summoner", null);
            var outLoop = AddOutAction("Loop");
            var outFinished = AddOutAction("Finished");
            var outIterator = AddOutValue<GameObject>("Iterator", null);

            AddInAction("In", () =>
            {
                var summoner = inSummoner.Value;
                if (summoner == null)
                    return;

                var caster = summoner.GetComponent<Spell.Caster>();
                for (var i = 0; i < caster.Summoned.Count; ++i)
                {
                    var summoned = caster.Summoned[i];
                    outIterator.Value = summoned;
                    outLoop.Execute();
                }

                outFinished.Execute();
            });
        }
    }
}
