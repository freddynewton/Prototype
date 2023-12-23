using Game.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Character
{
    public class CharacterCoreController : MonoBehaviour
    {
        [SerializeField]
        private PlayerInputController playerInputController;

        [HideInInspector]
        public Vector3 OldEulerAngles;

        public Vector3 GetLookPosition()
        {
            if (playerInputController == null)
            {
                return transform.forward;
            }
            else
            {
                return playerInputController.CharacterCamera.transform.position + playerInputController.CharacterCamera.transform.forward * 100;
            }
        }

        public float DesiredRotationAngle()
        {
            return Vector3.SignedAngle(transform.forward, GetLookPosition() - transform.position, transform.up);
        }
    }
}
