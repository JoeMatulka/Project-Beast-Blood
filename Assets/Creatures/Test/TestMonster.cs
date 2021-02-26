using CreatuePartSystems;
using CreatureAttackLibrary;
using CreatureSystems;
using System.Collections.Generic;
using UnityEngine;

public class TestMonster : Creature
{
    private readonly Creature.CreatureStats STATS = new Creature.CreatureStats
    {
        BaseHealth = 1000,
        TripThreshold = 200,
        KOThreshold = 400,
        Speed = 5,
        JumpForce = 25,
        BaseSize = 10,
        SizeModifier = 1,
        CreatureType = CreatureType.Bipedal,
        ResistedElements = new Dictionary<DamageType, float> { { DamageType.RAW, 1.5f }, { DamageType.FIRE, -1.5f } },
    };

    private const float ATTACK_RANGE = 3;
    private const float WALK_RANGE = 2.25f;

    public CreaturePart ArmAttackPart;

    void Awake()
    {
        Debug.Log("The issue is the new Damage object assignments in the Awake not ever getting new guids");
        // Set up creature attack set
        CreatureAttack[] attackSet = new CreatureAttack[] {
            BipedalCreatureBaseAttackLibrary.LowPunch
                .SetAttackPart(ArmAttackPart)
                .SetDamage(new Damage(30, DamageType.RAW)),
            BipedalCreatureBaseAttackLibrary.DownwardSlam
                .SetAttackPart(ArmAttackPart)
                .SetDamage(new Damage(50, DamageType.RAW))
        };
        InitialSetUp(STATS, attackSet);
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
            aiStateMachine.ChangeState(new CreatureGroundPursueBehvior(this, Target, WALK_RANGE, WALK_RANGE * ATTACK_RANGE));
        }

        if (Target != null && distToTarget <= ATTACK_RANGE)
        {
            aiStateMachine.ChangeState(new CreatureAttackBehavior(this, Target));
        }
        UpdateBaseAnimationKeys();
    }

    void FixedUpdate()
    {
        aiStateMachine.Update();
    }
}
