namespace StateMachine.Common.Contracts
{
    /// <summary>
    /// Interface defining view states
    /// </summary>
    public interface IViewState
    {
        // View state name
        string Name { get; }
        bool IsDefaultViewState { get;}
    }

}
