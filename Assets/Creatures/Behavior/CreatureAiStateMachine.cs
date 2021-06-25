﻿using CreatureSystems;

public struct CreatureAiStateMachine
{
    private ICreatureState currentAiState;

    public void ChangeState(ICreatureState newState)
    {
        if (this.currentAiState != null && this.currentAiState.GetType().Equals(newState.GetType()))
        {
            // Return because we don't change the state if it is the same or if its's null
            return;
        }
        if (this.currentAiState != null && !this.currentAiState.GetType().Equals(newState.GetType()))
        {
            this.currentAiState.Exit();
        }
        this.currentAiState = newState;
        this.currentAiState.Enter();
    }

    public void Update()
    {
        if (currentAiState != null) currentAiState.Execute();
    }

    public ICreatureState CurrentAiState
    {
        get { return currentAiState; }
    }
}
