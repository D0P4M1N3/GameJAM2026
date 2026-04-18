using System.Collections.Generic;
using UnityEngine;

public class B_STATEMACHINE : ScriptableObject
{
    private readonly List<B_STATE> states = new List<B_STATE>();
    private B_STATE currentState;
    private string currentState__str;

    public B_STATE CurrentState => currentState; 

    // NEW: Store current state name
    public string GetCurrentState()
    {
        return currentState__str;
    }

    public void AddStates(IEnumerable<B_STATE> newStates)
    {
        foreach (var state in newStates)
        {
            if (state == null) continue;
            state.Bind(this);
            states.Add(state);
        }
    }

    public void SetState<T>() where T : B_STATE
    {
        for (int i = 0; i < states.Count; i++)
        {
            if (states[i] is T targetState)
            {
                ChangeState(targetState);
                return;
            }
        }
    }

    public void ChangeState(B_STATE nextState)
    {
        //if (nextState == currentState)
        //    return;

        currentState?.OnExit();
        currentState = nextState;
        currentState?.OnEnter();

        //Update String
        if (currentState != null)
        {
            currentState__str = currentState.GetType().Name;
        }
        //currentState__str = currentState != null ? currentState.GetType().Name : "None";

    }

    public void Tick()
    {
        Tick_Override();
        currentState?.OnTick();
        //TickLate_Override();
    }

    public void TickLate()
    {
        TickLate_Override();
    }
    public virtual void Tick_Override()
    {
    }
    public virtual void TickLate_Override()
    {
    }


    public void Begin()
    {
        Begin_Override();
    }
    public virtual void Begin_Override()
    {
    }
    public void OnEnable()
    {
        OnEnable_Override();
    }
    public virtual void OnEnable_Override()
    {
    }
    public void OnDisable()
    {
        OnDisable_Override();
    }
    public virtual void OnDisable_Override()
    {
    }


}
