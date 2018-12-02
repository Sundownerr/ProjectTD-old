﻿using UnityEngine;
using Game.Creep.Data;
using System.Collections.Generic;
using Game.Data;
using NaughtyAttributes;
using Game.Systems;
using System.Text;

namespace Game.Creep
{  
    public class CreepData : Entity
    {

        public float MoveSpeed  { get => moveSpeed; set => moveSpeed = value >= 0 ? value : 0; }
        public int Exp          { get => exp; set => exp = value; }
        public int Gold         { get => gold; set => gold = value >= 0 ? value : 0; }      
        public float Health     { get => health; set => health = value >= 0 ? value : 0; }
        public float DefaultMoveSpeed       { get => defaultMoveSpeed; set => defaultMoveSpeed = value; }
        public float ArmorValue { get => armorValue; set => armorValue = value; }
        public Armor.ArmorType ArmorType    { get => armorType; set => armorType = value; }
        public bool IsInstanced { get => isInstanced; set => isInstanced = value; }
        public float HealthRegen { get => healthRegen; set => healthRegen = value; }

        [ShowAssetPreview(125, 125)]
        public GameObject Prefab;
        public int WaveLevel;
  
        public RaceType Race;
        public List<Ability> AbilityList;

        protected float healthRegen;
        protected Armor.ArmorType armorType;
        protected float armorValue;
        protected int gold, exp, numberInList;
        protected float defaultMoveSpeed, moveSpeed, health;    
        protected bool isInstanced;
        

        protected virtual void Awake() 
        {
         //   AddToDataBase();                          
        }

        public void SetData(CreepSystem ownerSystem)
        {
            owner = ownerSystem;
        }

#if UNITY_EDITOR
        [Button]
        public void AddToDataBase()
        {
            if (!IsInstanced)      
                if (DataLoadingSystem.Load<CreepDataBase>() is CreepDataBase dataBase)     
                {            
                    var raceList = dataBase.AllCreepList;
                    for (int i = 0; i < raceList.Count; i++)
                        if (i == (int)Race)                    
                        {
                            for (int j = 0; j < raceList[i].CreepList.Count; j++)                             
                                if (CompareId(raceList[i].CreepList[j].Id) || Name == raceList[i].CreepList[j].Name)
                                    return;
                            
                            raceList[i].CreepList.Add(this);
                            numberInList = raceList[i].CreepList.Count - 1;    

                            SetId();
                            SetName();      
                            DataLoadingSystem.Save<CreepDataBase>(dataBase);
                            return;
                        }
                }                   
        }

        private void RemoveFromDataBase()
        {
            if (!IsInstanced)        
                if (DataLoadingSystem.Load<CreepDataBase>() is CreepDataBase dataBase)             
                {               
                    dataBase.AllCreepList[(int)Race].CreepList.RemoveAt(numberInList);       
                    DataLoadingSystem.Save<CreepDataBase>(dataBase);       
                }                    
        }

        protected override void SetName()
        {
            var tempName = new StringBuilder();
        
            tempName.Append(Race.ToString());
            tempName.Append((int)Race);
            tempName.Append(numberInList);

            Name = tempName.ToString();
        }

        private void OnDestroy() => RemoveFromDataBase();

        public void OnValuesChanged() => AddToDataBase();
#endif
        public override void SetId() 
        {
            id = new List<int>
            {
                (int)Race,
                numberInList
            };
        }
    }
}
