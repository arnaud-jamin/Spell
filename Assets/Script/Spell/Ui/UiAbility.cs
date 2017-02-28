using System;
using UnityEngine;
using UnityEngine.UI;

namespace Spell.Ui
{
    public class UiAbility : MonoBehaviour
    {
        //-----------------------------------------------------------------------------------------
        [Serializable]
        public class ReferenceSettings
        {
            [AutoFind]
            public Image icon;

            [AutoFind]
            public Button button;

            [AutoFind]
            public RectTransform cooldownParent;

            [AutoFind]
            public Image cooldownFill;

            [AutoFind]
            public Text cooldownText;

            [AutoFind]
            public RectTransform manaCostParent;

            [AutoFind]
            public Text manaCostText;
        }

        //-----------------------------------------------------------------------------------------
        private Ability m_ability;

        //-----------------------------------------------------------------------------------------
        [SerializeField]
        private ReferenceSettings m_references = null;

        //-----------------------------------------------------------------------------------------
        public ReferenceSettings Reference { get { return m_references; } }
        public Ability Ability { get { return m_ability; } set { SetAbility(value); } }

        //-----------------------------------------------------------------------------------------
        private void SetAbility(Ability value)
        {
            m_ability = value;
            Refresh();
        }

        //-----------------------------------------------------------------------------------------
        public void Refresh()
        {
            m_references.icon.sprite = m_ability.Archetype.Icon;
            m_references.cooldownParent.gameObject.SetActive(m_ability.Cooldown > 0);
            m_references.cooldownFill.fillAmount = m_ability.CooldownRatio;
            m_references.cooldownText.text = ((int)m_ability.Cooldown).ToString();
            m_references.manaCostParent.gameObject.SetActive(true);
            m_references.manaCostText.text = ((int)m_ability.Archetype.ManaCost).ToString();
        }
    }
}