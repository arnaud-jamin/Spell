using FullSerializer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Object")]
    public class Unit : ObjectNode
    {
        [Header("General")]
        public string Name;
        public Sprite Icon;
        public Model Model;

        public GameObject Test;

        public List<Ability> Abilities = new List<Ability>();

        [Header("Stats")]
        public PrimaryAttribute PrimaryAttribute;
        public float BaseStrength = 20;
        public float StrengthGain = 2;
        public float BaseHealth = 200;
        public float BaseHealthRegen = 0.25f;
        public float BaseAgility = 20;
        public float AgilityGain = 2;
        public float BaseArmor = 5;
        public float BaseIntelligence = 20;
        public float IntelligenceGain = 2;
        public float BaseManaRegen = 0.01f;
        public float BaseAttackDamageMin = 50;
        public float BaseAttackDamageMax = 60;
        public float AttackRange = 500;
        public float BaseAttackTime = 1.5f;
        public float AttackPoint = 0.5f;
        public float AttackBackswing = 0.5f;
        public float MoveSpeed = 300;
        public float TurnRate = 0.5f;
        public float VisionRange = 1500;
        public float CollisionSize = 20;
        public float ProjectileSpeed = 20;
        public float MagicResistance = 0.25f;
    }
}
