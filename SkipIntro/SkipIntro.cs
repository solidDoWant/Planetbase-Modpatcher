using Planetbase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SkipIntro
{
    public class SkipIntro : IMod
    {
        IntroCinemetic m_intro;

        public void Init()
        {
            m_intro = null;

            Debug.Log("[MOD] SkipIntro activated");
        }

        public void Update()
        {
            if (m_intro == null)
            {
                m_intro = CameraManager.getInstance().getCinematic() as IntroCinemetic;
                if (m_intro == null)
                    return;
            }

            if (m_intro.mColonyShip.isDone())
            {
                m_intro = null;
                return;
            }

            // Disable menu
            GameStateGame gameState = GameManager.getInstance().getGameState() as GameStateGame;
            if (gameState.mGameGui.getWindow() is GuiGameMenu)
                gameState.mGameGui.setWindow(null);

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (CameraManager.getInstance().getCinematic() != null)
                {
                    Vector3 shipLandingPosition;
                    PhysicsUtil.findFloor(m_intro.mColonyShip.getPosition(), out shipLandingPosition, 256);
                    shipLandingPosition.y += CameraManager.DefaultHeight;

                    Transform transform = CameraManager.getInstance().getTransform();
                    transform.position = shipLandingPosition + m_intro.mColonyShip.getDirection().flatDirection() * 50f;
                    transform.LookAt(shipLandingPosition);

                    Vector3 euler = transform.eulerAngles;
                    euler.x = CameraManager.VerticalAngle;
                    transform.rotation = Quaternion.Euler(euler);

                    m_intro.mBlackBars = 0f;
                    CameraManager.getInstance().setCinematic(null);
                }
            }
        }
    }
}
