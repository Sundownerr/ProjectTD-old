﻿using System.Collections;
using System.Collections.Generic;
using Game.Creep;
using UnityEngine;
using Game.Systems;
using Game.Tower;
using Game.Data;

namespace Game.Systems
{
	public static class CreepControlSystem 
	{	
        public static void MoveToNextWaypoint(CreepSystem creep)
        {
            if(creep != null)
            {
                if(creep.HealthSystem != null)
                    creep.HealthSystem.Update();
                if(creep.IsOn)
                {
                    var waypointTransform = GM.I.WaypointList[creep.WaypointIndex].transform;
                    var creepTransform = creep.gameObject.transform;
                    var waypointReached = QoL.CalcDistance(creepTransform.position, waypointTransform.position) < 70;

                    if (creep.WaypointIndex < GM.I.WaypointList.Length - 1)
                        if (!waypointReached)                    
                            MoveAndRotateCreep();                                                     
                        else
                            creep.WaypointIndex++;                    
                    else
                        DestroyCreep(creep);
                }
            }

            void MoveAndRotateCreep()
            {           
                var creepTransform = creep.gameObject.transform;
                creepTransform.Translate(Vector3.forward * Time.deltaTime * creep.Stats.MoveSpeed, Space.Self);

                var clampPos = new Vector3(creepTransform.position.x, creepTransform.lossyScale.y, creepTransform.position.z);
                creepTransform.position = clampPos;

                RotateCreep();

                void RotateCreep()
                {            
                    var lookRotation = Quaternion.LookRotation(GM.I.WaypointList[creep.WaypointIndex].transform.position - creepTransform.position);
                    var rotation = Quaternion.Lerp(creepTransform.rotation, lookRotation, Time.deltaTime * 10f);
                    rotation.z = 0;
                    rotation.x = 0;

                    creepTransform.localRotation = rotation;
                }
            }
        }

        public static void DestroyCreep(CreepSystem creep)
        {
            Object.Destroy(creep.Stats);
            GM.I.CreepList.Remove(creep.gameObject);
            Object.Destroy(creep.gameObject);
        }    
	}
}