using Planetbase;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AutoRotateBuildings
{
    public class AutoRotateBuildings : IMod
    {
        private int connectionCount;

        public void Init()
        {
            Debug.Log("[MOD] AutoRotateBuildings activated");
            connectionCount = 0;
        }

        public void Update()
        {
            if (GameManager.getInstance().mState != GameManager.State.Updating)
                return;

            GameStateGame gameState = GameManager.getInstance().getGameState() as GameStateGame;
            if (gameState == null || gameState.mMode != GameStateGame.Mode.PlacingModule || gameState.mActiveModule == null)
                return;

            Module activeModule = gameState.mActiveModule;
            List<Vector3> connectionPositions = new List<Vector3>();
            for (int i = 0; i < Construction.mConstructions.Count; ++i)
            {
                Module module = Construction.mConstructions[i] as Module;
                if (module != null && module != activeModule && Connection.canLink(activeModule, module))
                {
                    connectionPositions.Add(module.getPosition());
                }
            }

            if (connectionPositions.Count == 0)
                return;

            connectionCount = Math.Min(connectionCount, connectionPositions.Count-1);
            if (Input.GetKeyUp(KeyCode.R))
                connectionCount = ++connectionCount % connectionPositions.Count;

            activeModule.mObject.transform.localRotation = Quaternion.LookRotation((connectionPositions[connectionCount] - activeModule.getPosition()).normalized);
        }
    }
}
