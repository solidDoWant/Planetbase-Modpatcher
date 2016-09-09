using Planetbase;
using Redirection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CharacterCam
{
    public class CharacterCam : IMod
    {
        public void Init()
        {
            Redirector.PerformRedirections();
            Debug.Log("[MOD] CharacterCam activated");
        }

        public void Update() {}
    }

    public abstract class CustomCharacter : Character
    {
        [RedirectFrom(typeof(Character))]
        public override bool isCloseCameraAvailable()
        {
            return true;
        }
    }

    public abstract class CustomCloseCameraCinematic : CloseCameraCinematic
    {
        private CustomCloseCameraCinematic(Selectable s, bool b) : base(s, b) { }

        [RedirectFrom(typeof(CloseCameraCinematic))]
        public new void updateCharacter(Character character, float timeStep)
        {
            Transform cameraTransform = CameraManager.getInstance().getTransform();
            Transform characterTransform = character.getTransform();

            float yAngle = characterTransform.eulerAngles.y;
            float horizontalBobbing = Mathf.Clamp((mLastRotation - yAngle) * 0.25f, -0.5f, 0.5f);
            Vector3 newPos = characterTransform.position + Vector3.up * character.getHeight() + characterTransform.forward * 0.7f + horizontalBobbing * characterTransform.right;

            if (mFirstUpdate)
            {
                cameraTransform.position = newPos;
                cameraTransform.rotation = characterTransform.rotation;
                mLastRotation = yAngle;
            }

            cameraTransform.position = Vector3.Lerp(cameraTransform.position, newPos, 0.1f);
            Vector3 lookAtDir = (characterTransform.position + characterTransform.forward * 1.4f + Vector3.up * (character.getHeight() * 0.85f) - cameraTransform.position).normalized;
            cameraTransform.rotation = Quaternion.RotateTowards(cameraTransform.rotation, Quaternion.LookRotation(lookAtDir), timeStep * 120f);
            mLastRotation = yAngle;
        }
    }
}
