using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell
{
    public enum LogType
    {
        Cast,
        Damage,
        Heal,
        Death,
        BuffAdded,
        BuffRemoved,
    }

    //-----------------------------------------------------------------------------------------
    public struct CombatLog
    {
        public LogType logType;
        public float time;
        public GameObject source;
        public GameObject target;
        public Ability ability;
        public float oldValue;
        public float newValue;

        public static CombatLog Damage(Health.HealthEvent healthEvent)
        {
            CombatLog log;
            log.time = Time.time;
            log.logType = LogType.Damage;
            log.source = healthEvent.source;
            log.target = healthEvent.target;
            log.ability = healthEvent.ability;
            log.oldValue = healthEvent.oldHealth;
            log.newValue = healthEvent.newHealth;
            return log;
        }

        public static CombatLog Cast(Ability.CastEvent castEvent)
        {
            CombatLog log;
            log.time = Time.time;
            log.logType = LogType.Cast;
            log.source = castEvent.source;
            log.target = castEvent.target;
            log.ability = castEvent.ability;
            log.oldValue = 0;
            log.newValue = 0;
            return log;
        }

        public static CombatLog Heal(Health.HealthEvent healthEvent)
        {
            CombatLog log;
            log.time = Time.time;
            log.logType = LogType.Heal;
            log.source = healthEvent.source;
            log.target = healthEvent.target;
            log.ability = healthEvent.ability;
            log.oldValue = healthEvent.oldHealth;
            log.newValue = healthEvent.newHealth;
            return log;
        }

        public static CombatLog Death(Health.HealthEvent deathEvent)
        {
            CombatLog log;
            log.time = Time.time;
            log.logType = LogType.Death;
            log.source = deathEvent.source;
            log.target = deathEvent.target;
            log.ability = deathEvent.ability;
            log.oldValue = deathEvent.oldHealth;
            log.newValue = deathEvent.newHealth;
            return log;
        }
    }

    public class CombatLogManager : MonoBehaviour
    {
        //-----------------------------------------------------------------------------------------
        private Queue<CombatLog> m_logQueue = new Queue<CombatLog>();

        //-----------------------------------------------------------------------------------------
        [SerializeField]
        private bool m_logOnConsole = true;

        [SerializeField]
        private int m_queueSize = 100;

        //-----------------------------------------------------------------------------------------
        public void Log(CombatLog log)
        {
            m_logQueue.Enqueue(log);

            if (m_logQueue.Count > m_queueSize)
            {
                m_logQueue.Dequeue();
            }

            if (m_logOnConsole)
            {
                Print(ref log);
            }
        }

        //-----------------------------------------------------------------------------------------
        private void Print(ref CombatLog log)
        {
            var sourceName = log.source != null ? log.source.name : null;
            var targetName = log.target != null ? log.target.name : null;
            var effectName = log.ability.ToString();
            var timeText = Utils.FormatTime(log.time);

            switch (log.logType)
            {
                case LogType.Cast:
                    if (targetName == null)
                    {
                        Debug.Log(string.Format("[{0}] {1} cast ability {2}.", timeText, sourceName, effectName));
                    }
                    else
                    {
                        Debug.Log(string.Format("[{0}] {1} cast ability {2} on {3}.", timeText, sourceName, effectName, targetName));
                    }
                    break;

                case LogType.Damage:
                    Debug.Log(string.Format("[{0}] {1}'s {2} hits {3} for {4} damage ({5} -> {6}).", timeText, sourceName, effectName, targetName, log.oldValue - log.newValue, log.oldValue, log.newValue));
                    break;

                case LogType.Heal:
                    Debug.Log(string.Format("[{0}] {1}'s {2} heals {3} for {4} health ({5} -> {6}).", timeText, sourceName, effectName, targetName, log.newValue - log.oldValue, log.oldValue, log.newValue));
                    break;

                case LogType.BuffAdded:
                    Debug.Log(string.Format("[{0}] {1} receives {2} buff from {3}.", timeText, targetName, effectName, sourceName));
                    break;

                case LogType.BuffRemoved:
                    Debug.Log(string.Format("[{0}] {1} loses {2} buff.", timeText, targetName, effectName));
                    break;

                case LogType.Death:
                    Debug.Log(string.Format("[{0}] {1} is killed by {2}.", timeText, targetName, sourceName));
                    break;
            }
        }
    }
}
