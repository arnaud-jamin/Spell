using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Spell
{
    [Serializable]
    public class Stat
    {
        //---------------------------------------------------------------------------------------------
        public class ValueChangedEvent : UnityEvent { }

        //---------------------------------------------------------------------------------------------
        private float m_value;
        private float m_baseValue;
        private float m_bonusValue;
        private float m_multiplierValue;
        private List<StatModifier> m_baseModifiers = new List<StatModifier>();
        private HashSet<StatModifier> m_bonusModifiers = new HashSet<StatModifier>();
        private HashSet<StatModifier> m_multiplierModifiers = new HashSet<StatModifier>();

        //---------------------------------------------------------------------------------------------
        [SerializeField]
        private float m_initialValue;

        [SerializeField]
        private float m_minValue;

        [SerializeField]
        private float m_maxValue;

        //---------------------------------------------------------------------------------------------
        public float InitialValue { get { return m_initialValue; } }

        public float BaseValue { get { return m_baseValue; }  }

        public float BonusValue  { get { return m_bonusValue; }  }

        public float MultiplierValue { get { return m_multiplierValue; }  }

        public float Value { get { return m_value; }  }

        public float MinValue { get { return m_minValue; }  set  { m_minValue = value; RefreshFinalValue(); }  }

        public float MaxValue { get { return m_maxValue; }  set  { m_maxValue = value; RefreshFinalValue(); }  }

        //---------------------------------------------------------------------------------------------
        public ValueChangedEvent ValueChanged = new ValueChangedEvent();

        //---------------------------------------------------------------------------------------------
        public Stat()
        {
            Refresh();
        }

        //---------------------------------------------------------------------------------------------
        public Stat(float initialValue)
        {
            SetInitialValue(initialValue);
            Refresh();
        }

        //---------------------------------------------------------------------------------------------
        public void Refresh()
        {
            RefreshBaseValue();
            RefreshBonusValue();
            RefreshMultiplierValue();
            RefreshFinalValue();
        }

        //---------------------------------------------------------------------------------------------
        public void SetInitialValue(float value)
        {
            m_initialValue = value; 
            RefreshBaseValue();
            RefreshFinalValue();
        }

        //---------------------------------------------------------------------------------------------
        public void AddBase(StatModifier modifier)
        {
            modifier.ValueChanged.AddListener(OnBaseValueChanged);
            m_baseModifiers.Add(modifier);
            RefreshBaseValue();
            RefreshFinalValue();
        }

        //-------------------------------------------------------------------------------------------------
        public void RemoveBase(StatModifier modifier) 
        {
            modifier.ValueChanged.RemoveListener(OnBaseValueChanged);
            m_baseModifiers.Remove(modifier);
            RefreshBaseValue();
            RefreshFinalValue();
        }

        //-------------------------------------------------------------------------------------------------
        public bool HasBase(StatModifier modifier)
        {
            return m_baseModifiers.Contains(modifier);
        }

        //-------------------------------------------------------------------------------------------------
        public void AddBonus(StatModifier modifier)
        {
            modifier.ValueChanged.AddListener(OnBonusValueChanged);
            m_bonusModifiers.Add(modifier);
            RefreshBonusValue();
            RefreshFinalValue();
        }

        //-------------------------------------------------------------------------------------------------
        public void RemoveBonus(StatModifier modifier)
        {
            modifier.ValueChanged.RemoveListener(OnBonusValueChanged);
            m_bonusModifiers.Remove(modifier);
            RefreshBonusValue();
            RefreshFinalValue();
        }

        //-------------------------------------------------------------------------------------------------
        public bool HasBonus(StatModifier modifier)
        {
            return m_bonusModifiers.Contains(modifier);
        }

        //-------------------------------------------------------------------------------------------------
        public void AddMultiplier(StatModifier modifier)
        {
            modifier.ValueChanged.AddListener(OnMultiplierValueChanged);
            m_multiplierModifiers.Add(modifier);
            RefreshMultiplierValue();
            RefreshFinalValue();
        }

        //-------------------------------------------------------------------------------------------------
        public void RemoveMultiplier(StatModifier modifier)
        {
            modifier.ValueChanged.RemoveListener(OnMultiplierValueChanged);
            m_multiplierModifiers.Remove(modifier);
            RefreshMultiplierValue();
            RefreshFinalValue();
        }

        //-------------------------------------------------------------------------------------------------
        public bool HasMultiplier(StatModifier modifier)
        {
            return m_multiplierModifiers.Contains(modifier);
        }

        //-------------------------------------------------------------------------------------------------
        private void RefreshBaseValue()
        {
            if (m_baseModifiers.Count > 0)
            {
                m_baseValue = m_baseModifiers[m_baseModifiers.Count - 1].Value;
            }
            else
            {
                m_baseValue = m_initialValue;
            }
        }

        //-------------------------------------------------------------------------------------------------
        private static float ComputeModifiersValue(HashSet<StatModifier> modifiers)
        {
            float value = 0;
            float overrideNegativeValue = 0;
            float overridePositiveValue = 0;
            foreach (var modifier in modifiers)
            {
                if (modifier.IsAdditive)
                {
                    if (modifier.Value < overrideNegativeValue)
                    {
                        overrideNegativeValue = modifier.Value;
                    }
                    else if (modifier.Value > overridePositiveValue)
                    {
                        overridePositiveValue = modifier.Value;
                    }
                }
                else
                {
                    value += modifier.Value;
                }
            }

            value += overrideNegativeValue + overridePositiveValue;
            return value;
        }
        
        //-------------------------------------------------------------------------------------------------
        private void RefreshBonusValue()
        {
            m_bonusValue = ComputeModifiersValue(m_bonusModifiers);
        }

        //-------------------------------------------------------------------------------------------------
        private void RefreshMultiplierValue()
        {
            m_multiplierValue = ComputeModifiersValue(m_multiplierModifiers);
        }

        //-------------------------------------------------------------------------------------------------
        private void RefreshFinalValue()
        {
            m_value = Mathf.Clamp((m_baseValue + m_bonusValue) * (1 + m_multiplierValue), m_minValue, m_maxValue);
            ValueChanged.Invoke();
        }

        //-------------------------------------------------------------------------------------------------
        private void OnBaseValueChanged()
        {
            RefreshBaseValue();
            RefreshFinalValue();
        }

        //-------------------------------------------------------------------------------------------------
        private void OnBonusValueChanged()
        {
            RefreshBonusValue();
            RefreshFinalValue();
        }

        //-------------------------------------------------------------------------------------------------
        private void OnMultiplierValueChanged()
        {
            RefreshMultiplierValue();
            RefreshFinalValue();
        }
    }
}
