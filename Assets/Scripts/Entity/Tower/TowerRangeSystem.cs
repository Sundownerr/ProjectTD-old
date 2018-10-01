﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.System;
#pragma warning disable CS1591 
namespace Game.Tower
{

    public class TowerRangeSystem : ExtendedMonoBehaviour
    {
        public List<GameObject> CreepInRangeList;

        private Renderer rend;

        private void Start()
        {
            CreepInRangeList = new List<GameObject>();
            transform.position += new Vector3(0, -5, 0);
            rend = GetComponent<Renderer>();
        }

        private void OnTriggerEnter(Collider other)
        {
            for (int i = 0; i < GameManager.Instance.CreepList.Count; i++)
            {
                if (other.gameObject == GameManager.Instance.CreepList[i])
                {
                    CreepInRangeList.Add(other.gameObject);
                }
            }                     
        }

        private void OnTriggerStay(Collider other)
        {
            if (CreepInRangeList.Count > 0)
            {
                for (int i = 0; i < CreepInRangeList.Count; i++)
                {
                    if (CreepInRangeList[i] == null)
                    {
                        CreepInRangeList.RemoveAt(i);                     
                    }                
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (CreepInRangeList.Count > 0)
            {
                CreepInRangeList.RemoveAt(0);
            }
        }
   
        public void Show(bool show)
        {
            if (show)
            {
                if (rend.material.color != new Color(0, 0.5f, 0, 0.2f))
                {
                    rend.material.color = new Color(0, 0.5f, 0, 0.2f);
                }
            }
            else
            {
                if (rend.material.color != new Color(0f, 0f, 0f, 0f))
                {
                    rend.material.color = new Color(0f, 0f, 0f, 0f);
                }
            }
        }            
    }
}
