﻿using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Game.System;

namespace Game.Data.Effect
{
    [CreateAssetMenu(fileName = "Stun", menuName = "Data/Tower/Effect/Stun")]
    public class Stun : Effect
    {
        public GameObject EffectPrefab;

        private GameObject effectPrefab;    

        public IEnumerator SetEffect(float delay)
        {
            yield return new WaitForSeconds(delay);
       
            End();
        }

        public override void Start()
        {
            if (target != null)
            {
                effectPrefab = Instantiate(EffectPrefab, target.gameObject.transform.position, Quaternion.identity, target.gameObject.transform);

                target.GetStunned(Duration);
            }

            base.Start();
            EffectCoroutine = GM.Instance.StartCoroutine(SetEffect(Duration));
        }

        public override void End()
        {      
            Destroy(effectPrefab);

            base.End();
        }
    }
}
