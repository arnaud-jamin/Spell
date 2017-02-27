﻿using UnityEngine;

namespace Spell.Graph
{
    public abstract class Action : Node
    {
        protected Spell.Unit m_owner;

        public abstract void Execute();
    }
}
