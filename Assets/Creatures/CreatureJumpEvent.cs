using CreatureSystems;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CreatureJumpEvent : MonoBehaviour
{
    private readonly string CREATURE_JUMP_TRIGGER_LAYER_NAME = "Creature Jump Trigger";

    private BoxCollider2D jumpTrigger;

    private Vector3 destination;

    void Awake()
    {
        // Set jump trigger collider to be trigger
        jumpTrigger = GetComponent<BoxCollider2D>();
        jumpTrigger.isTrigger = true;
        // Add object to Creature Jump Trigger layer
        gameObject.layer = LayerMask.NameToLayer(CREATURE_JUMP_TRIGGER_LAYER_NAME);
        // Set Jump Destination from transform child
        destination = transform.GetChild(0).position;
    }

    void OnTriggerStay2D(Collider2D col)
    {
        Creature creature = col.GetComponentInParent<Creature>();
        if (creature != null)
        {
            if (
                // Only set the event if the creature is facing towards the event trigger
                 (creature.IsFacingRight && this.transform.position.x > creature.transform.position.x) ||
                 (!creature.IsFacingRight && this.transform.position.x < creature.transform.position.x)
                // Do not set event if the creature already has one set
                 && creature.jumpEvent == null
                // Do not set event if creature is pursuing a target and the target is behind them (give creature a chance to turn instead of jumping)
                // TODO there is definitely a better way to do this
                 && (creature.AiStateMachine.CurrentAiState.GetType().Equals(typeof(CreatureGroundPursueBehvior)) && 
                     (
                      (creature.Target != null && creature.IsFacingRight && creature.Target.transform.position.x > creature.transform.position.x) ||
                      (creature.Target != null && !creature.IsFacingRight && creature.Target.transform.position.x < creature.transform.position.x)
                     )
                    )
                // Do not set event if creature is currently in attack behavior
                && !creature.AiStateMachine.CurrentAiState.GetType().Equals(typeof(CreatureAttackBehavior))
                )
            {
                creature.jumpEvent = this;
            }
        }
    }

    public Vector3 Destination
    {
        get { return destination; }
    }
}
