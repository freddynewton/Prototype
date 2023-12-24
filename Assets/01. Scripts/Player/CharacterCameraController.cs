using Cinemachine;
using System;
using UnityEngine;

namespace Game.Character.Camera
{
    public class CharacterCameraController : MonoBehaviour
    {
        public float LookSpeedVertical { get => lookSpeedVertical; }

        [Header("References")]
        [SerializeField]
        private CinemachineVirtualCamera firstPersonCamera;

        [Header("Settings")]
        [SerializeField]
        private float lookSpeedHorizontal = 1f;

        private float lookSpeedVertical = 1f;

        [SerializeField]
        private bool invertX = false;

        [SerializeField]
        private bool invertY = false;

        [SerializeField]
        [Range(-90f, 90f)]
        private float verticalAngleClamp = 80f;

        public void HandleLookInputVertically(float verticalInput)
        {
            float verticalLookDelta = verticalInput * lookSpeedVertical * Time.deltaTime;

            if (invertY)
            {
                verticalLookDelta *= -1f;
            }

            float newVerticalAngle = firstPersonCamera.transform.localEulerAngles.x + verticalLookDelta;

            if (newVerticalAngle > 180f)
            {
                newVerticalAngle -= 360f;
            }

            newVerticalAngle = Mathf.Clamp(newVerticalAngle, -verticalAngleClamp, verticalAngleClamp);

            transform.localEulerAngles = new Vector3(newVerticalAngle, 0f, 0f);
        }
    }
}
