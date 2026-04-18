

public abstract class B_STATE
{
    protected B_STATEMACHINE stateMachine;

    internal void Bind(B_STATEMACHINE machine)
    {
        stateMachine = machine;
    }

    public virtual void OnEnter() { }
    public virtual void OnTick() { }
    public virtual void OnExit() { }
}
