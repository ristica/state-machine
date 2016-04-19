namespace StateMachine.Common.Contracts
{
    /// <summary>
    /// Interface UIs need to implement to work with ViewManager
    /// </summary>
    public interface IUserInterface
    {
        void LoadViewState(string viewState);
    }
}