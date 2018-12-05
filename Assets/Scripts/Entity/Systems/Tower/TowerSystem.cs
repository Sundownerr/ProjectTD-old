using System;
using System.Collections.Generic;
using Game.Creep;
using Game.Systems;
using Game.Tower.Data;
using Game.Tower.System;
using UnityEngine;
using U = UnityEngine.Object;

namespace Game.Tower
{
    public class TowerSystem : EntitySystem
    {
        public Transform RangeTransform     { get => rangeTransform;        private set => rangeTransform = value; }
        public Transform MovingPart         { get => movingPartTransform;   private set => movingPartTransform = value; }
        public Transform StaticPart         { get => staticPartTransform;   private set => staticPartTransform = value; }
        public Transform ShootPoint         { get => shootPointTransform;   private set => shootPointTransform = value; }
        public GameObject OcuppiedCell      { get => ocuppiedCell;          set => ocuppiedCell = value; }
        public GameObject Bullet            { get => bullet;                private set => bullet = value; }
        public GameObject Range             { get => range;                 private set => range = value; }
        public Range RangeSystem            { get => rangeSystem;           private set => rangeSystem = value; }      
        public Combat CombatSystem          { get => combatSystem;          private set => combatSystem = value; }      
        public Stats StatsSystem            { get => statsSystem;           private set => statsSystem = value; }
        public TowerData Stats              { get => StatsSystem.CurrentStats; set => StatsSystem.CurrentStats = value; }
        public Renderer[] RendererList      { get => rendererList;          private set => rendererList = value; }    
        public AbilityControlSystem AbilityControlSystem    { get => abilityControlSystem;  private set => abilityControlSystem = value; }
        public TraitControlSystem TraitControlSystem        { get => traitControlSystem;    private set => traitControlSystem = value; }
        public List<EntitySystem> CreepInRangeList          => rangeSystem.EntitySystemList;

        public List<AbilitySystem> AbilitySystemList { get => abilitySystemList; set => abilitySystemList = value; }
        public List<TraitSystem> TraitSystemList { get => traitSystemList; set => traitSystemList = value; }

        private Transform rangeTransform, movingPartTransform, staticPartTransform, shootPointTransform;
        private GameObject ocuppiedCell, bullet, range;
        private Renderer[] rendererList;
        private Range rangeSystem;
        private TraitControlSystem traitControlSystem;
        private Combat combatSystem;
        private System.AbilityControlSystem abilityControlSystem;
        private Stats statsSystem;
        private List<AbilitySystem> abilitySystemList;
        private List<TraitSystem> traitSystemList;

        public TowerSystem(GameObject ownerPrefab)
        {         
            prefab = ownerPrefab;
            movingPartTransform = ownerPrefab.transform.GetChild(0);
            staticPartTransform = ownerPrefab.transform.GetChild(1);
            shootPointTransform = MovingPart.GetChild(0).GetChild(0);
            bullet = ownerPrefab.transform.GetChild(2).gameObject;

            statsSystem             = new Stats(this);
            traitControlSystem      = new TraitControlSystem(this);
            combatSystem            = new Combat(this);
            abilityControlSystem    = new AbilityControlSystem(this);
            appliedEffectSystem     = new AppliedEffectSystem();         
            abilitySystemList       = new List<AbilitySystem>();
            traitSystemList         = new List<TraitSystem>();
         
            bullet.SetActive(false);   
            isVulnerable = false;                           
        }

        public void SetSystem()
        {               
            for (int i = 0; i < Stats.AbilityList.Length; i++)          
                abilitySystemList.Add(new AbilitySystem(Stats.AbilityList[i], this));   

            for (int i = 0; i < Stats.TraitList.Length; i++)
                traitSystemList.Add(Stats.TraitList[i].GetTraitSystem(this));
                
            if (!Stats.IsGradeTower)            
                statsSystem.Set();            
                       
            combatSystem.Set();
            abilityControlSystem.Set();
            traitControlSystem.Set();

            range = U.Instantiate(GM.I.RangePrefab, prefab.transform);           
            range.transform.localScale = new Vector3(Stats.Range, 0.001f, Stats.Range);
            rangeSystem = range.GetComponent<Range>();
            rangeSystem.Owner = this;    
            rangeSystem.CollideType = CollideWith.Creeps;

            RendererList = prefab.GetComponentsInChildren<Renderer>();                       
        }
        
        public void AddExp(int amount) => StatsSystem.AddExp(amount);      
    }
}