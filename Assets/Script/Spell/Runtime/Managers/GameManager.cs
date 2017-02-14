using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Spell.Graph;

namespace Spell
{
    public class GameManager : MonoBehaviour
    {
        //-----------------------------------------------------------------------------------------
        public static GameManager Instance = null;

        //-----------------------------------------------------------------------------------------
        private Dictionary<int, GameObject> m_prefabMap = new Dictionary<int, GameObject>();

        //private List<Player> m_players = new List<Player>();
        private Player m_activePlayer = null;

        //-----------------------------------------------------------------------------------------
        [SerializeField]
        private Settings m_settings = null;

        [SerializeField]
        private CombatLogManager m_combatLogManager = null;

        [SerializeField]
        private CameraManager m_cameraManager = null;

        //[SerializeField]
        //private Player m_playerPrefab = null;

        [SerializeField]
        private Transform m_instancesRoot = null;

        [SerializeField]
        private Transform m_playersRoot = null;

        [SerializeField]
        private Transform m_unitsRoot = null;

        //-----------------------------------------------------------------------------------------
        public static CombatLogManager CombatLogManager { get { return Instance.m_combatLogManager; } }
        public static CameraManager CameraManager { get { return Instance.m_cameraManager; } }
        public static Settings Settings { get { return Instance.m_settings; } }
        public static Transform InstancesRoot { get { return Instance.m_instancesRoot; } }
        public static Transform PlayersRoot { get { return Instance.m_playersRoot; } }
        public static Transform UnitsRoot { get { return Instance.m_unitsRoot; } }
        public static Player ActivePlayer { get { return Instance.m_activePlayer; } }

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
        }

        //-----------------------------------------------------------------------------------------
        void OnDisable()
        {
            Instance = null;
        }

        //-----------------------------------------------------------------------------------------
        void Start()
        {
            for (var i = 0; i < GlobalSettings.General.Spawn.Length; ++i)
            {
                var unit = GlobalSettings.General.Spawn[i];
                if (unit != null)
                {
                    CreateUnit(unit.Root as Graph.Unit, Vector3.zero, 0);
                }
            }
        }

        //-----------------------------------------------------------------------------------------
        public Unit CreateUnit(Graph.Unit archetype, Vector3 position, float rotation)
        {
            var unit = GameplayHelper.Instantiate(archetype.Name, GlobalSettings.General.UnitPrefab, m_unitsRoot, position, Quaternion.AngleAxis(rotation, Vector3.up));
            unit.Initialize(archetype);
            return unit;
        }

        //-----------------------------------------------------------------------------------------
        void Update()
        {
            //InputManager.Update();
        }

        //-----------------------------------------------------------------------------------------
        void LateUpdate()
        {
            if (CameraManager != null)
            {
                CameraManager.ManagerUpdate();
            }
        }

        //-----------------------------------------------------------------------------------------
        public void BuildPrefabRegistery()
        {
            m_prefabMap.Clear();

            foreach (var gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                m_prefabMap[gameObject.GetInstanceID()] = gameObject;
            }
        }

        //-----------------------------------------------------------------------------------------
        public GameObject GetPrefabFromInstanceId(int instanceID)
        {
            GameObject gameObject = null;
            m_prefabMap.TryGetValue(instanceID, out gameObject);
            return gameObject;
        }

        //-----------------------------------------------------------------------------------------
        public GameObject GetTarget(TargetType target)
        {
            return null;
        }
    }
}
