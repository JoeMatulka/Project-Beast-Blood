public class TestMonster : Creature
{
    private const float HEALTH = 1000;
    private const float SPEED = 10;
    private const float JUMP_FORCE = 25;
    private const float ATTACK_RANGE = 2;


    void Awake()
    {
        InitialSetUp(HEALTH, SPEED, JUMP_FORCE, ATTACK_RANGE);
    }

    void FixedUpdate()
    {
        Move(-WALK_INPUT, false);
    }

    private void Update()
    {
        UpdateBaseAnimationKeys();
    }
}
