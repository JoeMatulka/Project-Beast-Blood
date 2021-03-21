using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CreatureJumpEvent : MonoBehaviour
{
    public readonly static string CREATURE_JUMP_TRIGGER_LAYER_NAME = "Creature Jump Trigger";

    private BoxCollider2D jumpTrigger;

    private Vector3 jumpDestination;

    private Vector3 jumpStartPosition;

    void Awake()
    {
        // Set jump trigger collider to be trigger
        jumpTrigger = GetComponent<BoxCollider2D>();
        jumpTrigger.isTrigger = true;
        // Add object to Creature Jump Trigger layer
        gameObject.layer = LayerMask.NameToLayer(CREATURE_JUMP_TRIGGER_LAYER_NAME);
        // Set Jump Destination from transform child
        jumpDestination = transform.TransformPoint(transform.GetChild(0).position);
    }

    public Vector3 JumpDestination
    {
        get { return jumpDestination; }
    }
}
