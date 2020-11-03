using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class CreatureAttackSMB : SceneLinkedSMB<Creature>
    {
        public override void OnSLStatePostEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Set layer priority for animating this attack
            animator.SetLayerWeight(layerIndex, 1);
        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            AnimationClip clip = animator.GetCurrentAnimatorClipInfo(layerIndex)[0].clip;
            // Get current frame of the current animation clip
            int currentFrame = Mathf.RoundToInt(clip.length * (stateInfo.normalizedTime % 1) * clip.frameRate);
        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.EndAttack();
            // Unset layer priority for animation this attack
            animator.SetLayerWeight(layerIndex, 0);
        }
    }
}
