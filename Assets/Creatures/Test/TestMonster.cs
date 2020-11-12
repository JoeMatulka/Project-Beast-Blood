using UnityEngine;

public class TestMonster : Creature
{
    private const float HEALTH = 1000;
    private const float SPEED = 10;
    private const float JUMP_FORCE = 25;
    private const float ATTACK_RANGE = 3;


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
        UpdateBaseAnimationKeys();
        if (Target != null && Vector2.Distance(Target.position, transform.position) > attackRange)
        {
            // Move towards target to get into attack range
            Vector2 targetRelative = Target.InverseTransformPoint(transform.position);
            movement = targetRelative.x > 0 ? -WALK_INPUT : WALK_INPUT;
        }
        else {
            // TODO move to own method, this is for testing
            Attack();
        }
    }

    void FixedUpdate()
    {
        Move(movement, jump);
    }
}
