﻿using CreatuePartSystems;
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

    private const float ATTACK_RANGE = 2f;
    private const float WALK_RANGE = 3f;

    public CreaturePart ArmAttackPart;

    private const float SIGHT_RANGE = 15f;
    private LayerMask sightLayerMask;

    void Awake()
    {
        // Set up sight layer mask
        sightLayerMask = LayerMask.GetMask("Player", "Creature");
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

    private void Update()
    {
        aiStateMachine.ChangeState(DetermineBehavoir());
        UpdateBaseAnimationKeys();
    }

    private ICreatureState DetermineBehavoir()
    {
        if (Target != null)
        {
            float distToTarget = Vector2.Distance(Target.position, transform.position);
            if (distToTarget > ATTACK_RANGE)
            {
                return new CreatureGroundPursueBehvior(this, Target, WALK_RANGE, WALK_RANGE * ATTACK_RANGE);
            }
            else
            {
                return new CreatureAttackBehavior(this, Target);
            }
        }
        else
        {
            return new CreatureSearchForTargetBehavior(this, SIGHT_RANGE, sightLayerMask);
        }
    }

    void FixedUpdate()
    {
        aiStateMachine.Update();
    }
}
