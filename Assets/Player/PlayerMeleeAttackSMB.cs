using UnityEngine;

namespace Gamekit2D
{
    public class PlayerMeleeAttackSMB : SceneLinkedSMB<Player>
    {
        private AimDirection direction;
        public override void OnSLStatePostEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Set layer priority for animating this attack
            animator.SetLayerWeight(layerIndex, 1);
            // Get aim at the beginning of attack
            direction = m_MonoBehaviour.Aim.AimDirection;
            // Generate damage used for attack frames
            m_MonoBehaviour.WeaponController.GenerateAttackDamage();
        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            AnimationClip clip = animator.GetCurrentAnimatorClipInfo(layerIndex)[0].clip;
            // Get current frame of the current animation clip
            int currentFrame = Mathf.RoundToInt(clip.length * (stateInfo.normalizedTime % 1) * clip.frameRate);
            m_MonoBehaviour.WeaponController.ActivateWeaponAttackFrame(direction, currentFrame);
        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.EndAttack();
            // Unset layer priority for animation this attack
            animator.SetLayerWeight(layerIndex, 0);
        }
    }
}