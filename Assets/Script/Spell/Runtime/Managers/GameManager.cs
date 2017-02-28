using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Spell.Graph;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Spell
{
    public class GameManager : MonoBehaviour
    {
        //-----------------------------------------------------------------------------------------
        [Serializable]
        public class ReferenceSettings
        {
            [AutoFind]
            public CombatLogManager combatLogManager = null;

            [AutoFind]
            public CameraManager cameraManager = null;

            public Transform unitsRoot = null;
        }

        //-----------------------------------------------------------------------------------------
        public class UnitSelectionEvent : UnityEvent<Unit> { }

        //-----------------------------------------------------------------------------------------
        public static GameManager Instance = null;

        //-----------------------------------------------------------------------------------------
        [SerializeField]
        ReferenceSettings m_references = null;

        //-----------------------------------------------------------------------------------------
        private List<Unit> m_units = new List<Unit>();
        private Unit m_currentUnit = null;

        //-----------------------------------------------------------------------------------------
        public CombatLogManager CombatLogManager { get { return m_references.combatLogManager; } }
        public CameraManager CameraManager { get { return m_references.cameraManager; } }
        public Transform UnitsRoot { get { return m_references.unitsRoot; } }
        public Unit CurrentUnit { get { return m_currentUnit; } }

        //-----------------------------------------------------------------------------------------
        public UnitSelectionEvent UnitSelected = new UnitSelectionEvent();

        //-----------------------------------------------------------------------------------------
        void Awake()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }

        //-----------------------------------------------------------------------------------------
        void OnEnable()
        {
            if (Instance != null)
                return;

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        //-----------------------------------------------------------------------------------------
        void OnDisable()
        {
            Instance = null;
        }

        //-----------------------------------------------------------------------------------------
        void Initialize()
        {
            //m_references.cameraManager.Initialize(this);
        }

        //-----------------------------------------------------------------------------------------
        public Unit CreateUnit(Graph.Unit unit, Vector3 position, float rotation = 0)
        {
            var instance = GameplayHelper.Instantiate(unit.Name, Settings.General.UnitPrefab, m_references.unitsRoot, position, Quaternion.AngleAxis(rotation, Vector3.up));
            instance.Initialize(this, unit);
            m_units.Add(instance);
            return instance;
        }

        //-----------------------------------------------------------------------------------------
        public void DestroyAllUnits()
        {
            for (int i = 0; i < m_units.Count; ++i)
            {
                Destroy(m_units[i].gameObject);
            }
            m_units.Clear();
        }

        //-----------------------------------------------------------------------------------------
        public void OnPointerDown(PointerEventData eventData)
        {
            Unit selection = null;
            var ray = m_references.cameraManager.MainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Settings.General.SelectionRaycastLength, Settings.General.selectionRaycastMask))
            {
                selection = hit.collider.GetComponent<Unit>();
            }

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                SelectUnit(selection);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (m_currentUnit != null)
                {
                    if (selection == null)
                    {
                        m_currentUnit.MoveTo(hit.point);
                    }
                    else 
                    {
                        m_currentUnit.Attack(selection);
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------
        void SelectUnit(Unit selection)
        {
            if (m_currentUnit != null)
            {
                m_currentUnit.IsSelected = false;
            }

            m_currentUnit = selection;

            if (m_currentUnit != null)
            {
                m_currentUnit.IsSelected = true;
            }

            UnitSelected.Invoke(selection);
        }

        //-----------------------------------------------------------------------------------------
        void Update()
        {
        }

        //-----------------------------------------------------------------------------------------
        void FixedUpdate()
        {
            for (int i = 0; i < m_units.Count; ++i)
            {
                m_units[i].OnFixedUpdate();
            }
        }

        //-----------------------------------------------------------------------------------------
        void LateUpdate()
        {
            if (CameraManager != null)
            {
                CameraManager.OnLateUpdate();
            }
        }

        //-----------------------------------------------------------------------------------------
        public Unit GetTarget(TargetType target)
        {
            return null;
        }
    }
}
