using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Spell.Ui
{
    public class UiAbilityPanel : MonoBehaviour
    {
        //-----------------------------------------------------------------------------------------
        [Serializable]
        public class ReferenceSettings
        {
            [AutoFind]
            public RectTransform container;

            public UiAbility uiAbilityPrefab;
        }

        //-----------------------------------------------------------------------------------------
        private Unit m_unit;
        private List<UiAbility> m_uiAbilities = new List<UiAbility>();

        //-----------------------------------------------------------------------------------------
        [SerializeField]
        private ReferenceSettings m_references = null;

        //-----------------------------------------------------------------------------------------
        public ReferenceSettings Reference { get { return m_references; } }
        public Unit Unit { get { return m_unit; } set { SetUnit(value); } }

        //-----------------------------------------------------------------------------------------
        protected void OnEnable()
        {
            GameManager.Instance.UnitSelected.AddListener(OnUnitSelected);
        }

        //-----------------------------------------------------------------------------------------
        protected void OnDisable()
        {
            GameManager.Instance.UnitSelected.RemoveListener(OnUnitSelected);
        }

        //-----------------------------------------------------------------------------------------
        private void OnUnitSelected(Unit unit)
        {
            SetUnit(unit);
        }
        
        //-----------------------------------------------------------------------------------------
        private void SetUnit(Unit unit)
        {
            m_unit = unit;
            CreateAbilities();
        }

        //-----------------------------------------------------------------------------------------
        private void CreateAbilities()
        {
            m_uiAbilities.Clear();
            m_references.container.DestroyChildren();
            for (int i = 0; i < m_unit.Abilities.Count; ++i)
            {
                var uiAbility = Instantiate(m_references.uiAbilityPrefab, Vector3.zero, Quaternion.identity, m_references.container);
                uiAbility.Ability = m_unit.Abilities[i];
                m_uiAbilities.Add(uiAbility);
            }
        }

        //-----------------------------------------------------------------------------------------
        private void Update()
        {
            for (int i = 0; i < m_uiAbilities.Count; ++i)
            {
                var uiAbility = m_uiAbilities[i];
                uiAbility.Refresh();
            }
        }
    }
}