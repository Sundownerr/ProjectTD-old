﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Tower
{

    public class RangeCollider : MonoBehaviour
    {

        public bool IsCreepInRange;
        public List<GameObject> CreepInRangeList;

        private IEnumerator DeleteMissing()
        {
            while (true)
            {
                
                if(CreepInRangeList.Count > 0)
                {
                    for (int i = 0; i < CreepInRangeList.Count; i++)
                    {
                        if(CreepInRangeList[i] == null)
                        {
                            CreepInRangeList.RemoveAt(i);
                        }
                    }
                }
                yield return new WaitForFixedUpdate();
            }
        }

        private void Start()
        {
            CreepInRangeList = new List<GameObject>();
            StartCoroutine(DeleteMissing());
        }

        private void OnTriggerEnter(Collider other)
        {
            CreepInRangeList.Add(other.gameObject);
            IsCreepInRange = true;
        }
  
        private void OnTriggerExit(Collider other)
        {
            if (CreepInRangeList.Count > 0)
            {
                CreepInRangeList.RemoveAt(0);
            }

            if (CreepInRangeList.Count == 0)
            {
                IsCreepInRange = false;
            }
        }

    }
}
