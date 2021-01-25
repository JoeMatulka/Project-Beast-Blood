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

    private float movement = 0;
    private bool jump = false;

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
            // Move towards target to get into attack range
            float input = distToTarget >= (ATTACK_RANGE * WALK_RANGE) ? RUN_INPUT : WALK_INPUT;
            // Adjust input based off of cripple percentage
            if (GetCripplePercent(CreaturePartsType.Ground) <= .5f) input = WALK_INPUT;

            movement = transform.position.x > Target.position.x ? -input : input;
        }
        else
        {
            // TODO move to own method, this is for testing
            Attack();
        }
        UpdateBaseAnimationKeys();
    }

    void FixedUpdate()
    {
        GroundMove(movement, jump);
        movement = 0;
    }
}
