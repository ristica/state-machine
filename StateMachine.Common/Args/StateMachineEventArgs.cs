using System;
using StateMachine.Common.Enums;

namespace StateMachine.Common.Args
{
    public class StateMachineEventArgs
    {
        #region Properties

        public string EventName                     { get; private set; }
        public string EventInfo                     { get; private set; }
        public DateTime TimeStamp                   { get; private set; }
        public string Source                        { get; private set; }
        public string Target                        { get; private set; }
        public StateMachineEventType EventType      { get; private set; }

        #endregion

        #region C-Tor

        public StateMachineEventArgs(
            string eventName, 
            string eventInfo,  
            string source,
            StateMachineEventType eventType, 
            string target = "All")
        {
            EventName       = eventName;
            EventInfo       = eventInfo;
            EventType       = eventType;
            Source          = source;
            Target          = target;
            TimeStamp       = DateTime.Now;
        }

        #endregion
    }
}
