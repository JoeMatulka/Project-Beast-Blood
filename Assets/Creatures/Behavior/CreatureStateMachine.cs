using CreatureSystems;

public class CreatureStateMachine
{
    private ICreatureState currentState;

    public void ChangeState(ICreatureState newState)
    {
        if (this.currentState != null)
        {
            this.currentState.Exit();
        }
        this.currentState = newState;
        this.currentState.Enter();
    }

    public void Update()
    {
        if (currentState != null) currentState.Execute();
    }
}
