﻿using System.Collections.Generic;
using UnityEngine;
using Game.Systems;
using Game.Tower.Data;
using Game.Tower.System;

namespace Game.Tower
{
    public class TowerSystem : EntitySystem
    {
        public Transform RangeTransform { get => rangeTransform; set => rangeTransform = value; }
        public Transform MovingPartTransform { get => movingPartTransform; set => movingPartTransform = value; }
        public Transform StaticPartTransform { get => staticPartTransform; set => staticPartTransform = value; }
        public Transform ShootPointTransform { get => shootPointTransform; set => shootPointTransform = value; }
        public GameObject OcuppiedCell { get => ocuppiedCell; set => ocuppiedCell = value; }
        public GameObject Bullet { get => bullet; set => bullet = value; }
        public GameObject Range { get => range; set => range = value; }
        public Range RangeSystem { private get => rangeSystem; set => rangeSystem = value; }
        public Special SpecialSystem { get => specialSystem; set => specialSystem = value; }
        public Combat CombatSystem { private get => combatSystem; set => combatSystem = value; }
        public AbilitySystem AbilitySystem { private get => abilitySystem; set => abilitySystem = value; }
        public Stats StatsSystem { private get => statsSystem; set => statsSystem = value; }
        public TowerData Stats { get => StatsSystem.CurrentStats; set => StatsSystem.CurrentStats = value; }

        private Transform rangeTransform, movingPartTransform, staticPartTransform, shootPointTransform;
        private GameObject ocuppiedCell, bullet, target, range;    
        private Renderer[] rendererList;
        private System.Range rangeSystem; 
        private System.Special specialSystem;
        private System.Combat combatSystem;
        private System.AbilitySystem abilitySystem;
        private System.Stats statsSystem;
        private StateMachine state;
        private bool isTowerPlaced;
        private TowerData stats;

        protected override void Awake()
        {
            base.Awake();         

            MovingPartTransform = transform.GetChild(0);
            StaticPartTransform = transform.GetChild(1);
            ShootPointTransform = MovingPartTransform.GetChild(0).GetChild(0);
            Bullet = transform.GetChild(2).gameObject;

            StatsSystem     = new System.Stats(this);
            SpecialSystem   = new System.Special(this);
            CombatSystem    = new System.Combat(this);
            AbilitySystem   = new System.AbilitySystem(this);

            IsVulnerable = false;
            state = new StateMachine();
            state.ChangeState(new SpawnState(this));
        }

        public void SetSystem()
        {
            StatsSystem.Set();
            SpecialSystem.Set();
            CombatSystem.Set();
            AbilitySystem.Set();

            Range                       = Instantiate(GM.Instance.RangePrefab, transform);
            RangeSystem                 = Range.GetComponent<System.Range>();
            Range.transform.localScale  = new Vector3(Stats.Range, 0.001f, Stats.Range);
            RangeSystem.SetShow();

            rendererList = GetComponentsInChildren<Renderer>();

            Bullet.SetActive(false);
            StatsSystem.UpdateUI();
        }

        private void Update()
        {
            if (IsOn)
            {
                state.Update();

                if (isTowerPlaced)
                    AbilitySystem.State.Update();
            }
            
            RangeSystem.SetShow();
        }
       
        private void SetTowerColor(Color color)
        {
            for (int i = 0; i < rendererList.Length; i++)
                rendererList[i].material.color = color;
        }

        private void StartPlacing()
        {
            SetTowerColor(GM.Instance.TowerPlaceSystem.GhostedTowerColor);
            transform.position = GM.Instance.TowerPlaceSystem.GhostedTowerPos;
        }

        private void EndPlacing()
        {          
            transform.position = OcuppiedCell.transform.position;
            
            SetTowerColor(Color.white - new Color(0.2f, 0.2f, 0.2f));

            var placeEffect = Instantiate(GM.Instance.ElementPlaceEffectList[(int)Stats.Element], transform.position + Vector3.up * 5, Quaternion.identity);
            Destroy(placeEffect, placeEffect.GetComponent<ParticleSystem>().main.duration);

            gameObject.layer = 14;
            RangeSystem.SetShow(false);
          
            GM.Instance.PlayerInputSystem.NewTowerData = null;           

            isTowerPlaced = true;
        }

        private void RotateAtCreep(GameObject target)
        {
            var offset = target.transform.position - transform.position;
            offset.y = 0;

            var towerRotation = Quaternion.LookRotation(offset);

            MovingPartTransform.rotation = Quaternion.Lerp(MovingPartTransform.rotation, towerRotation, Time.deltaTime * 9f);
        }

        public List<Creep.CreepSystem> GetCreepInRangeList() => RangeSystem.CreepSystemList;

        public void AddExp(int amount) => StatsSystem.AddExp(amount);
        
        public void Upgrade()
        {
            var isGradeCountOk =
                Stats.GradeList.Count > 0 &&
                Stats.GradeCount < Stats.GradeList.Count;

            if (isGradeCountOk)
            {
                var upgradedTowerPrefab = Instantiate(Stats.GradeList[0].Prefab, transform.position, Quaternion.identity, GM.Instance.TowerParent);
                var upgradedTowerSystem = upgradedTowerPrefab.GetComponent<TowerSystem>();
               
                upgradedTowerSystem.StatsSystem.Upgrade(Stats, Stats.GradeList[0]);
                upgradedTowerSystem.OcuppiedCell = OcuppiedCell;
                upgradedTowerSystem.SetSystem();

                GM.Instance.PlayerInputSystem.ChoosedTower = upgradedTowerPrefab;
                
                Destroy(gameObject);
            }
        }

        public void Sell()
        {
            GM.Instance.ResourceSystem.AddTowerLimit(-Stats.TowerLimit);
            GM.Instance.ResourceSystem.AddGold(Stats.GoldCost);

            OcuppiedCell.GetComponent<Cells.Cell>().IsBusy = false;
            GM.Instance.PlacedTowerList.Remove(gameObject);
            Destroy(gameObject);
        }

        protected class SpawnState : IState
        {
            private readonly TowerSystem o;

            public SpawnState(TowerSystem o) => this.o = o;

            public void Enter() { }

            public void Execute()
            {
                if (GM.PlayerState == GM.State.PlacingTower)
                    o.StartPlacing();
                else
                    o.state.ChangeState(new LookForCreepState(o));               
            }

            public void Exit()
            {
                o.EndPlacing();
                o.StatsSystem.UpdateUI();
            }
        }

        protected class LookForCreepState : IState
        {
            private readonly TowerSystem o;

            public LookForCreepState(TowerSystem o) => this.o = o;

            public void Enter() { }

            public void Execute()
            {
                if (o.RangeSystem.CreepList.Count > 0)
                    o.state.ChangeState(new CombatState(o));                  
            }

            public void Exit() { }
        }

        protected class CombatState : IState
        {
            private readonly TowerSystem o;

            public CombatState(TowerSystem o) => this.o = o;

            public void Enter() { }

            public void Execute()
            {
                o.CombatSystem.State.Update();

                for (int i = 0; i < o.GetCreepInRangeList().Count; i++)
                    if (o.GetCreepInRangeList()[i] == null)
                    {
                        o.RangeSystem.CreepList.RemoveAt(i);
                        o.RangeSystem.CreepSystemList.RemoveAt(i);
                    }

                if (o.GetCreepInRangeList().Count < 1)
                    o.state.ChangeState(new MoveRemainingBulletState(o));
                else
                    o.target = o.GetCreepInRangeList()[0].gameObject;         
                              
                if (o.target != null)
                    o.RotateAtCreep(o.target);         
            }

            public void Exit() { }
        }

        protected class MoveRemainingBulletState : IState
        {
            private readonly TowerSystem o;

            public MoveRemainingBulletState(TowerSystem o) => this.o = o;

            public void Enter() { }

            public void Execute()
            {
                if (o.RangeSystem.CreepList.Count > 0)
                    o.state.ChangeState(new CombatState(o));
                else 
                if(!o.CombatSystem.CheckAllBulletInactive())
                    o.CombatSystem.MoveBullet();
                else
                    o.state.ChangeState(new LookForCreepState(o));
            }         

            public void Exit() { }
        }
    }
}
