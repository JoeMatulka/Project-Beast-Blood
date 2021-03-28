using CreatureSystems;
using UnityEngine;

namespace Gamekit2D
{
    public class CreatureTransitionSMB : SceneLinkedSMB<Creature>
    {
        public override void OnSLStatePostEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.isInAnimationTransition = true;
        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.isInAnimationTransition = false;
        }
    }

}
