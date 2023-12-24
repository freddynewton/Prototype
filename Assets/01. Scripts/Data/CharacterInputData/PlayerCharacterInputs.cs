using UnityEngine;

namespace Game.Data.CharacterInputData
{
    public struct PlayerCharacterInputs
    {
        public float MoveAxisForward;
        public float MoveAxisRight;
        public Transform CameraHolderTransform;
        public float LookInputX;
        public bool JumpDown;
        public bool CrouchDown;
        public bool CrouchUp;
    }
}