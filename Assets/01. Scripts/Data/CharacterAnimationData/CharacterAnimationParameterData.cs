using UnityEngine;

namespace Game.Data.CharacterAnimationData
{
    public class CharacterAnimationParameterData
    {
        public int SpeedHash { get; private set; }

        public int HorizontalHash { get; private set; }

        public int VerticalHash { get; private set; }

        public int IdleTurnHash { get; private set; }

        public int MovingTurnHash { get; private set; }

        public CharacterAnimationParameterData()
        {
            SpeedHash = Animator.StringToHash("Speed");
            HorizontalHash = Animator.StringToHash("Horizontal");
            VerticalHash = Animator.StringToHash("Vertical");
            IdleTurnHash = Animator.StringToHash("IdleTurn");
            MovingTurnHash = Animator.StringToHash("MovingTurn");
        }
    }
}
