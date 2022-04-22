﻿using UnityEngine;

namespace Gamekit2D
{
    // Used for attacks that involve a player equipped weapon
    public class PlayerWeaponAttackSMB : SceneLinkedSMB<Player>
    {
        private Vector2 direction;
        public override void OnSLStatePostEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Set layer priority for animating this attack
            animator.SetLayerWeight(layerIndex, 1);
            // Get aim at the beginning of attack
            direction = m_MonoBehaviour.Aim.ToVector;
            // Generate damage used for attack frames
            m_MonoBehaviour.ActionController.GenerateAttackDamage();
        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            AnimationClip clip = animator.GetCurrentAnimatorClipInfo(layerIndex)[0].clip;
            // Get current frame of the current animation clip
            int currentFrame = Mathf.RoundToInt(clip.length * (stateInfo.normalizedTime % 1) * clip.frameRate);
            m_MonoBehaviour.ActionController.ActivateWeaponAttackFrame(direction, currentFrame);
        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.EndAttack();
            // Unset layer priority for animation this attack
            animator.SetLayerWeight(layerIndex, 0);
        }
    }
}