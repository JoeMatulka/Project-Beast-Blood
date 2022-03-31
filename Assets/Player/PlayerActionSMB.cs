using UnityEngine;

namespace Gamekit2D
{
    // Used for player attacks not involving equipped weapons
    public class PlayerActionSMB : SceneLinkedSMB<Player>
    {
        public override void OnSLStatePostEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Set layer priority for animating this action
            animator.SetLayerWeight(layerIndex, 1);
            m_MonoBehaviour.stopInput = true;
        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            AnimationClip clip = animator.GetCurrentAnimatorClipInfo(layerIndex)[0].clip;
            // Get current frame of the current animation clip
            int currentFrame = Mathf.RoundToInt(clip.length * (stateInfo.normalizedTime % 1) * clip.frameRate);
            m_MonoBehaviour.ActionController.ActiveActionFrame(currentFrame);
        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.stopInput = false;
            m_MonoBehaviour.ActionController.EndAttackOrAction();
            // Unset layer priority for animation this action
            animator.SetLayerWeight(layerIndex, 0);
        }
    }
}