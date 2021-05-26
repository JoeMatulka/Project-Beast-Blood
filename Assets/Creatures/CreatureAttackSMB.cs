using CreatureSystems;
using UnityEngine;

namespace Gamekit2D
{
    public class CreatureAttackSMB : SceneLinkedSMB<Creature>
    {
        // Used to ensure attack frames are only activated once per frame
        private int previousFrame;

        public override void OnSLStatePostEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Set layer priority for animating this attack
            animator.SetLayerWeight(layerIndex, 1);
            // Intialize last animation frame to a number that can't be attainable by animation
            previousFrame = -1;
        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            AnimationClip clip = animator.GetCurrentAnimatorClipInfo(layerIndex)[0].clip;
            // Get current frame of the current animation clip
            int currentFrame = Mathf.RoundToInt(clip.length * (stateInfo.normalizedTime % 1) * clip.frameRate);
            // Compare last frame to current to ensure that attack frames are not activated more than once per frame
            if (previousFrame != currentFrame) {
                m_MonoBehaviour.ActivateAttackFrame(currentFrame);
                previousFrame = currentFrame;
            }
        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.EndAttack();
            // Unset layer priority for animation this attack
            animator.SetLayerWeight(layerIndex, 0);
        }
    }
}
