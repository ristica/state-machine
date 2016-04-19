using System.Collections.Generic;

namespace StateMachine.ActiveStateMachine.Subjects
{
    public class State
    {
        #region Properties

        public string StateName                                     { get; private set; }
        public Dictionary<string, Transition> StateTansitions       { get; private set; }
        public List<StateMachineAction> AccessActions               { get; private set; }
        public List<StateMachineAction> ExitActions                 { get; private set; }
        public bool IsDefaultState                                  { get; private set; }

        #endregion

        #region C-Tor

        public State(
            string stateName,
            Dictionary<string, Transition> stateTransitions,
            List<StateMachineAction> accessActions,
            List<StateMachineAction> exitActions,
            bool isDefaultState = false)
        {
            this.StateName          = stateName;
            this.StateTansitions    = stateTransitions;
            this.AccessActions      = accessActions;
            this.ExitActions        = exitActions;
            this.IsDefaultState     = isDefaultState;
        }

        #endregion
    }
}
