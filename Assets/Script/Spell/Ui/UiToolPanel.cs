using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Spell.Ui
{
    public class UiToolPanel : MonoBehaviour
    {
        //-----------------------------------------------------------------------------------------
        [Serializable]
        public class ElementSettings
        {
            [AutoFind]
            public Button spawnButton;

            [AutoFind]
            public Dropdown spawnDropDown;

            [AutoFind]
            public Button removeSpawnsButton;

            [AutoFind]
            public Toggle freeSpellsToggle;

            [AutoFind]
            public Button levelUpButton;
        }

        //-----------------------------------------------------------------------------------------
        [SerializeField]
        private ElementSettings m_elements = null;

        //-----------------------------------------------------------------------------------------
        public void Start()
        {
            m_elements.spawnDropDown.ClearOptions();
            var options = Settings.General.Units.Array.Where(u => u.Root != null)
                                                      .Select(u => new Dropdown.OptionData { text = u.Root.Name })
                                                      .ToList();
            m_elements.spawnDropDown.AddOptions(options);
        }

        //-----------------------------------------------------------------------------------------
        public void OnEnable()
        {
            m_elements.spawnButton.onClick.AddListener(OnSpawnButtonClicked);
            m_elements.removeSpawnsButton.onClick.AddListener(OnRemoveSpawnsButtonClicked);
            m_elements.freeSpellsToggle.onValueChanged.AddListener(OnFreeSpellsButtonClicked);
            m_elements.levelUpButton.onClick.AddListener(OnLevelUpButtonClicked);
        }

        //-----------------------------------------------------------------------------------------
        public void OnDisable()
        {
            m_elements.spawnButton.onClick.RemoveListener(OnSpawnButtonClicked);
            m_elements.removeSpawnsButton.onClick.RemoveListener(OnRemoveSpawnsButtonClicked);
            m_elements.freeSpellsToggle.onValueChanged.RemoveListener(OnFreeSpellsButtonClicked);
            m_elements.levelUpButton.onClick.RemoveListener(OnLevelUpButtonClicked);
        }

        //-----------------------------------------------------------------------------------------
        private void OnSpawnButtonClicked()
        {
            var value = m_elements.spawnDropDown.value;
            var unitGraph = Settings.General.Units[value];
            GameManager.Instance.CreateUnit(unitGraph.Root, Vector3.zero, 0);
        }

        //-----------------------------------------------------------------------------------------
        private void OnRemoveSpawnsButtonClicked()
        {
            GameManager.Instance.DestroyAllUnits();
        }

        //-----------------------------------------------------------------------------------------
        private void OnFreeSpellsButtonClicked(bool arg)
        {
        }

        //-----------------------------------------------------------------------------------------
        private void OnLevelUpButtonClicked()
        {
        }
    }
}