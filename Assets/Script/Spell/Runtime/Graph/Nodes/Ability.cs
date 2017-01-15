using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    public class Ability : Node
    {
        public Expression<string> Name = new TextValue();
        public Expression<Sprite> Icon = new SpriteValue();
        public Expression<float> ManaCost = new FloatValue();
        public Expression<float> Cooldown = new FloatValue();
        public Expression<CastTargetType> CastTargetType = new CastTargetTypeValue();
        public List<Action> Actions = new List<Action>();
    }
}
