﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.System;
using System;
using Game.Creep;
using Game.Tower;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "New Ability", menuName = "Data/Tower/Ability")]

    [Serializable]
    public class Ability : Entity
    {       
        [HideInInspector]
        public bool IsNeedStack, IsOnCooldown;

        public string AbilityName, AbilityDescription;
        public float Cooldown, TriggerChance;
        public int ManaCost;

        [Expandable]
        public List<Effect> EffectList;

        private bool isStackable, isStacked;
        private EntitySystem target;
        private StateMachine state;
        private int effectCount;
        private float timer;

        private void OnEnable()
        {
            state = new StateMachine();
            state.ChangeState(new SetEffectState(this));
        }

        protected override void SetId() 
        {
            var tempId = new List<int>();

            if (owner is Creep.CreepSystem ownerCreep)
            {
                tempId.AddRange(ownerCreep.GetStats().Id);              
            }
            else if(owner is Tower.TowerSystem ownerTower)
            {
                tempId.AddRange(ownerTower.GetStats().Id);   
                tempId.Add(ownerTower.GetStats().AbilityList.IndexOf(this));
            }

            Id = tempId;
        }

        public void Init() => state.Update();

        public EntitySystem GetTarget() => target;
        
        public override void SetOwner(EntitySystem owner)
        {
            this.owner = owner;

            for (int i = 0; i < EffectList.Count; i++)
                EffectList[i].SetOwner(owner);

            EffectList[EffectList.Count - 1].NextInterval = 0.01f;
            CheckStackable();           
        }

        public void SetTarget(EntitySystem target)
        {
            this.target = target;
            SetEffectsTarget(target);
        }       

        private void SetEffectsTarget(EntitySystem target)
        {
            for (int i = 0; i < EffectList.Count; i++)
                if(EffectList[i].IsStackable)
                    EffectList[i].SetTarget(target, true);
                else 
                    EffectList[i].SetTarget(target, false);
        }

        public void StackReset()
        {
            isStacked = true;

            for (int i = 0; i < EffectList.Count; i++)
                if(EffectList[i].IsStackable)
                    EffectList[i] = Instantiate(EffectList[i]);                    
        }

        public void Reset()
        {
            timer = 0;
            effectCount = 0;          

            for (int i = 0; i < EffectList.Count; i++)
                EffectList[i].ApplyReset();
        }

        private void CheckStackable()
        {
            var allEffectsInterval = 0f;
            var allEffectsDuration = 0f;

            for (int i = 0; i < EffectList.Count; i++)
            {
                allEffectsInterval += EffectList[i].NextInterval;
                allEffectsDuration += EffectList[i].Duration;
            }

            isStackable = 
                allEffectsInterval >= Cooldown ? true : 
                allEffectsDuration >= Cooldown ? true : false;
        }

        public bool CheckAllEffectsEnded()
        {
            for (int i = 0; i < EffectList.Count; i++)
                if (!EffectList[i].IsEnded)
                    return false;
            
            return true;
        }

        public bool CheckAllEffectsSet() => 
            effectCount >= EffectList.Count - 1 ? true : false;         
        
        public bool CheckNeedStack()
        {
            if (isStackable)
                for (int i = 0; i < EffectList.Count; i++)
                    if (EffectList[i].IsStackable)
                        if (!EffectList[i].IsEnded)
                            return true;
            return false;         
        }

        private IEnumerator StartCooldown(float delay)
        {
            IsOnCooldown = true;

            yield return new WaitForSeconds(delay);
           
            IsNeedStack = CheckNeedStack();
            Reset();
            IsOnCooldown = false;
        }
     
        protected class SetEffectState : IState
        {
            private readonly Ability o;

            public SetEffectState(Ability o) => this.o = o; 

            public void Enter() {  }

            public void Execute()
            {
                o.timer += Time.deltaTime;

                for (int i = 0; i <= o.effectCount; i++)
                    o.EffectList[i].Init();  

                if(!o.isStacked)
                    if(!o.IsOnCooldown)
                        GM.Instance.StartCoroutine(o.StartCooldown(o.Cooldown)); 

                if (!o.CheckAllEffectsSet())
                    if (o.timer > o.EffectList[o.effectCount].NextInterval)
                    {
                        o.effectCount++;
                        o.timer = 0;
                    }
            }

            public void Exit() { }
        }        
    }
}