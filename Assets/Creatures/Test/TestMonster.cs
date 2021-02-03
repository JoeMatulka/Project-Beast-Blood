using CreatureSystems;
using System.Collections.Generic;
using UnityEngine;

public class TestMonster : Creature
{
    private readonly Creature.CreatureStats STATS = new Creature.CreatureStats
    {
        BaseHealth = 1000,
        TripThreshold = 250,
        KOThreshold = 500,
        Speed = 5,
        JumpForce = 25,
        BaseSize = 10,
        SizeModifier = 1,
        CreatureType = CreatureType.Bipedal,
        ResistedElements = new Dictionary<DamageType, float> { { DamageType.RAW, 1.5f }, { DamageType.FIRE, -1.5f } }
    };

    private const float ATTACK_RANGE = 3;
    private const float WALK_RANGE = 2.25f;

    void Awake()
    {
        InitialSetUp(STATS);
    }

    private void Start()
    {
        // TODO This is for testing, remove later for better way to get creature target
        Target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        float distToTarget = Vector2.Distance(Target.position, transform.position);
        if (Target != null && distToTarget > ATTACK_RANGE)
        {
            stateMachine.ChangeState(new CreatureGroundFollow(this, Target, WALK_RANGE, WALK_RANGE * ATTACK_RANGE));
        }
        else
        {
            // TODO Change to attack state
        }

        UpdateBaseAnimationKeys();
    }

    void FixedUpdate()
    {
        stateMachine.Update();
    }
}
