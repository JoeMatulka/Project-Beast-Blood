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
            m_MonoBehaviour.stopInput = true;
            m_MonoBehaviour.ActionController.WeaponController.HideHolsteredWeapon();
        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.stopInput = false;
            m_MonoBehaviour.ActionController.WeaponController.SetHolsteredWeaponSprite();
            // Unset layer priority for animation this attack
            animator.SetLayerWeight(layerIndex, 0);
        }
    }
}