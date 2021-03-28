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

    void OnTriggerEnter2D(Collider2D col)
    {
        Creature creature = col.GetComponentInParent<Creature>();
        if (creature != null) {
            // TODO Only set if creature is facing collider
            creature.jumpEvent = this;
        }
    }

    public Vector3 Destination
    {
        get { return destination; }
    }
}
