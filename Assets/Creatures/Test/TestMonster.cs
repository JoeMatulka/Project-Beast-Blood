using UnityEngine;

public class TestMonster : Creature
{
    private const float HEALTH = 1000;
    private const float SPEED = 8;
    private const float JUMP_FORCE = 25;
    private const float ATTACK_RANGE = 3;
    private const float WALK_RANGE = 2.25f;

    private float movement = 0;
    private bool jump = false;

    void Awake()
    {
        InitialSetUp(HEALTH, SPEED, JUMP_FORCE, ATTACK_RANGE);
    }

    private void Start()
    {
        // TODO This is for testing, remove later for better way to get creature target
        Target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        float distToTarget = Vector2.Distance(Target.position, transform.position);
        if (Target != null && distToTarget > attackRange)
        {
            // Move towards target to get into attack range
            float input = distToTarget >= (attackRange * WALK_RANGE) ? RUN_INPUT : WALK_INPUT;
            movement = transform.position.x > Target.position.x ? -input : input;
        }
        else
        {
            // TODO move to own method, this is for testing
            // Attack();
        }
        UpdateBaseAnimationKeys();
    }

    void FixedUpdate()
    {
        Move(movement, jump);
        movement = 0;
    }
}
