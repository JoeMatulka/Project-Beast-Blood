using UnityEngine;

public class TestMonster : Creature
{
    private const float HEALTH = 1000;
    private const float SPEED = 10;
    private const float JUMP_FORCE = 25;
    private const float ATTACK_RANGE = 4;


    void Awake()
    {
        InitialSetUp(HEALTH, SPEED, JUMP_FORCE, ATTACK_RANGE);
    }

    private void Start()
    {
        // TODO This is for testing, remove later for better way to get creature target
        Target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        if (Target != null && Vector2.Distance(Target.position, transform.position) > attackRange) {
            // Move towards target to get into attack range
            float input = Target.position.x < transform.position.y ? -WALK_INPUT : WALK_INPUT;
            Move(input, false);
        } else if (Target == null) { 
            // Find new target
        }
    }

    private void Update()
    {
        UpdateBaseAnimationKeys();
    }
}
