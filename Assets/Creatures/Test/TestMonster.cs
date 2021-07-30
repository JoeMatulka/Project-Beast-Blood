using CreatuePartSystems;
using CreatureAttackLibrary;
using CreatureSystems;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TestMonster : Creature
{
    private readonly Creature.CreatureStats STATS = new Creature.CreatureStats
    {
        Name = "Test Monster",
        BaseHealth = 1000,
        TripThreshold = 200,
        KOThreshold = 400,
        BurnThreshold = 100,
        Speed = 5,
        BaseAggression = 2f,
        BaseSize = 10,
        SizeModifier = 1,
        CreatureType = CreatureType.Bipedal,
        ResistedElements = new Dictionary<DamageType, float> { { DamageType.RAW, 1.5f }, { DamageType.FIRE, -1.5f } },
    };

    private const float ATTACK_RANGE = 2.5f;
    private const float WALK_RANGE = 2.75f;

    public CreaturePart ArmAttackPart;

    private const float SIGHT_RANGE = 15f;
    private LayerMask sightLayerMask;

    private const float COLLISION_PATHING_RANGE = 2f;

    private const float FLEE_HEALTH_MOD = 5f;
    private const float FLEE_REFRESH_TIME = 360f;

    private const float ENRAGED_AGGRESSION = 10f;

    private readonly CreatureAttack roar = BipedalCreatureBaseAttackLibrary.Roar;

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

    private void FixedUpdate()
    {
        Tuple<ICreatureState, bool> state = DetermineBehavoir();
        aiStateMachine.ChangeState(state.Item1, state.Item2);
        aiStateMachine.Update();
    }

    private void Update()
    {
        UpdateBaseAnimationKeys();
    }

    // Returns a behavior and if the state should change
    protected override Tuple<ICreatureState, bool> DetermineBehavoir()
    {
        if (IsDead)
        {
            return new Tuple<ICreatureState, bool>(new CreatureDeadBehavior(this), false);
        }
        if (ShouldFlee())
        {
            // Should flee if health criteria is met and hasn't fled since flee timer refresh
            Vector2 fleeFrom = Target != null ? Target.position : transform.position;
            return new Tuple<ICreatureState, bool>(new CreatureGroundFleeBehavior(this, COLLISION_PATHING_RANGE, fleeFrom, roar, isPoisoned), isPoisoned);
        }
        if (Target != null && !IsFleeing)
        {
            float distToTarget = Vector2.Distance(Target.position, transform.position);
            if (distToTarget > ATTACK_RANGE)
            {
                return new Tuple<ICreatureState, bool>(new CreatureGroundPursueBehvior(this, Target, WALK_RANGE, WALK_RANGE * ATTACK_RANGE, isPoisoned), isPoisoned);
            }
            else
            {
                bool forceChange = false;
                if (CurrentHealth <= (Stats.BaseHealth * .5f) || isBurning)
                {
                    // If below half health, increase aggresssion
                    CurrentAgression = ENRAGED_AGGRESSION;
                    forceChange = true;
                }
                return new Tuple<ICreatureState, bool>(new CreatureAttackBehavior(this, Target, CurrentAgression), forceChange);
            }
        }
        else
        {
            return new Tuple<ICreatureState, bool>(new CreatureSearchForTargetBehavior(this, SIGHT_RANGE, COLLISION_PATHING_RANGE, sightLayerMask, isPoisoned), isPoisoned);
        }
    }

    private bool ShouldFlee()
    {
        return CurrentHealth <= (Stats.BaseHealth / FLEE_HEALTH_MOD) && (Time.time - TimeSinceLastFlee) >= FLEE_REFRESH_TIME;
    }
}
