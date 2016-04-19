using System.Collections.Generic;

namespace StateMachine.Common.Contracts
{
    /// <summary>
    /// Interface for a viewstate configuration
    /// </summary>
    public interface IViewStateConfiguration
    {
        Dictionary<string, object> ViewStates { get; }
        string[] ViewStateList { get; }
        string DefaultViewState { get; }
    }
}