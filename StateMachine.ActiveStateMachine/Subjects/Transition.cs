using System.Collections.Generic;

namespace StateMachine.ActiveStateMachine.Subjects
{
    public class Transition
    {
        #region Properties

        public string Name                                  { get; private set; }
        public string SourceState                           { get; private set; }
        public string TargetState                           { get; private set; }
        public List<StateMachineAction> Guards              { get; private set; }
        public List<StateMachineAction> Transitions         { get; private set; }
        public string Trigger                               { get; private set; }

        #endregion

        #region C-Tor

        public Transition(
            string name,
            string sourceState,
            string targetState,
            List<StateMachineAction> guards,
            List<StateMachineAction> transitions,
            string trigger)
        {
            this.Name           = name;
            this.SourceState    = sourceState;
            this.TargetState    = targetState;
            this.Guards         = guards;
            this.Transitions    = transitions;
            this.Trigger        = trigger;
        }

        #endregion
    }
}
