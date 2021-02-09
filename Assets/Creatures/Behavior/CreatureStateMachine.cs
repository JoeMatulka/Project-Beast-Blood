using CreatureSystems;

public class CreatureStateMachine
{
    private ICreatureState currentState;

    public void ChangeState(ICreatureState newState)
    {
        if (this.currentState != null && this.currentState.GetType().Equals(newState.GetType()))
        {
            // Return because we don't change the state if it is the same
            return;
        }
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

    public ICreatureState CurrentState
    {
        get { return currentState; }
    }
}
