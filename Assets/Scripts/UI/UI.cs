﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

namespace Game.System
{
    public class UI : MonoBehaviour
    {
        public Button StartWaveButton, BuildModeButton, ReadyButton;
        public bool IsBuildModeActive, IsWaveStarted, IsPlayerReady;


        void Start()
        {
            Cursor.lockState = CursorLockMode.Confined;

            StartWaveButton.onClick.AddListener(StartWave);
            BuildModeButton.onClick.AddListener(BuildTower);
            ReadyButton.onClick.AddListener(CheckReady);

        }

        private void StartWave()
        {
            if (GameManager.Instance.CreepList.Count == 0)
            {
                IsWaveStarted = true;
            }
        }

        private void BuildTower()
        {
            if (GameManager.Instance.GridSystem.IsGridBuilded)
            {
                if (!IsBuildModeActive)
                {
                    IsBuildModeActive = true;
                }
            }
        }

        private void CheckReady()
        {
            IsPlayerReady = true;
            Destroy(ReadyButton.gameObject);
        }

        private IEnumerator Test()
        {
            var t = 0;

            while (t < 5)
            {
                yield return new WaitForSeconds(0.2f);
                t++;
                Debug.Log(t);
            }
        }

        private void Update()
        {

            if (GameManager.Instance.GridSystem.IsGridBuilded)
            {
                if (IsBuildModeActive)
                {
                    if (!GameManager.Instance.TowerCellList[GameManager.Instance.TowerCellList.Count - 1].activeSelf)
                    {
                        for (int i = 0; i < GameManager.Instance.TowerCellList.Count; i++)
                        {
                            GameManager.Instance.TowerCellList[i].SetActive(true);
                        }
                    }
                }

                if (!IsBuildModeActive)
                {
                    if (GameManager.Instance.TowerCellList[GameManager.Instance.TowerCellList.Count - 1].activeSelf)
                    {
                        for (int i = 0; i < GameManager.Instance.TowerCellList.Count; i++)
                        {
                            GameManager.Instance.TowerCellList[i].SetActive(false);
                        }
                    }
                }
            }
        }
    }
}
