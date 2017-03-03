using System;
using UnityEngine;

namespace Spell
{
    [CreateAssetMenu(fileName = "GeneralSettings", menuName = "Spell/General Settings")]
    public class GeneralSettings : ScriptableObject
    {
        [Serializable]
        public class UnitList : ReorderableArray<Graph.UnitGraph> { }

        public Unit UnitPrefab;

        [Reorderable]
        public UnitList Units;

        [Header("Selection")]
        public LayerMask selectionRaycastMask = 0;
        public float SelectionRaycastLength = 20;


        [Header("Unit Conversion")]
        public float DotaDistanceToMeter = 5.0f / 300.0f;
        public float DotaTurnRateToDegreePerSeconds = 2000.0f;
    }
}
