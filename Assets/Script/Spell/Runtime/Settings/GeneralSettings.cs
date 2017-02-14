using System;
using UnityEngine;

namespace Spell
{
    [CreateAssetMenu(fileName = "GeneralSettings", menuName = "Spell/General Settings")]
    public class GeneralSettings : ScriptableObject
    {
        public Unit UnitPrefab;
        public Graph.UnitGraph[] Spawn;
    }
}
