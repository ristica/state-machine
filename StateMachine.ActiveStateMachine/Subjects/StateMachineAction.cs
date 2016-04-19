using System;

namespace StateMachine.ActiveStateMachine.Subjects
{
    /// <summary>
    /// it describes any/every action in the state machine
    /// transitions, access actions, exit actions, guards
    /// </summary>
    public class StateMachineAction
    {
        #region Fields

        private readonly Action _action;

        #endregion

        #region Properties

        public string Name { get; private set; }

        #endregion

        #region C-Tor

        public StateMachineAction(string name, Action action)
        {
            this.Name       = name;
            this._action    = action;
        }

        #endregion

        #region Methods

        /// <summary>
        /// it will be called by the state machine
        /// on each action
        /// </summary>
        public void Execute()
        {
            this._action.Invoke();
        }

        #endregion
    }
}
