using Planetbase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AutoAlerts
{
    public class AutoAlerts : IMod
    {
        private bool m_autoActivated;
        private AlertState m_activatedState;

        public void Init()
        {
            m_activatedState = AlertState.NoAlert;
            m_autoActivated = false;

            Debug.Log("[MOD] AutoAlerts activated");
        }

        public void Update()
        {
            if (ConstructionComponent.findOperational(TypeList<ComponentType, ComponentTypeList>.find<SecurityConsole>()) == null)
                return;

            AlertState state = SecurityManager.getInstance().getAlertState();

            // if the state has been changed manually, don't do anything else. Will be activated again if the player sets NoAlert
            if (state != m_activatedState)
            {
                m_activatedState = AlertState.NoAlert;
                m_autoActivated = false;
                return;
            }

            List<Character> intruders = Character.getSpecializationCharacters(SpecializationList.IntruderInstance);
            if (intruders != null)
            {
                foreach (Character intruder in intruders)
                {
                    if (intruder.hasStatusFlag(Character.StatusFlagDetected))
                    {
                        // check number of guards vs intruders - want to keep on yellow while ratio guards/intruders is high enough
                        float numIntruders = intruders.Count;
                        float numGuards = Character.getCountOfSpecialization(TypeList<Specialization, SpecializationList>.find<Guard>());

                        float ratio = numGuards / numIntruders;
                        AlertState newState = ratio < 0.75f ? AlertState.RedAlert : AlertState.YellowAlert;

                        if (newState != m_activatedState)
                        {
                            SecurityManager.getInstance().setAlertState(newState);
                            m_activatedState = newState;
                            m_autoActivated = true;
                        }

                        return;
                    }
                }
            }

            if (DisasterManager.getInstance().anyInProgress())
            {
                if (state != AlertState.YellowAlert)
                {
                    SecurityManager.getInstance().setAlertState(AlertState.YellowAlert);
                    m_activatedState = AlertState.YellowAlert;
                    m_autoActivated = true;
                }

                return;
            }

            if (m_autoActivated)
            {
                // Only disable alert if it's the same one we set
                if (state == m_activatedState)
                    SecurityManager.getInstance().setAlertState(AlertState.NoAlert);

                m_activatedState = AlertState.NoAlert;
                m_autoActivated = false;
            }
        }
    }
}
