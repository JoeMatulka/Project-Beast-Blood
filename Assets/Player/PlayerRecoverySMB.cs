using UnityEngine;

namespace Gamekit2D
{
    // Used for recovery actions to ensure the animation is played
    public class PlayerRecoverySMB : SceneLinkedSMB<Player>
    {
        public override void OnSLStatePostEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Set layer priority for animating this recovery
            animator.SetLayerWeight(layerIndex, 1);
        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.stopInput = true;
        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.stopInput = false;
            // Unset layer priority for animation this attack
            animator.SetLayerWeight(layerIndex, 0);
        }
    }
}