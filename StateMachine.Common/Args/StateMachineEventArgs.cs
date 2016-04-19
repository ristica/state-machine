using System;
using StateMachine.Common.Enums;

namespace StateMachine.Common.Args
{
    public class StateMachineEventArgs
    {
        #region Properties

        public string EventName                     { get; set; }
        public string EventInfo                     { get; set; }
        public DateTime TimeStamp                   { get; set; }
        public string Source                        { get; set; }
        public string Target                        { get; set; }
        public StateMachineEventType EventType      { get; set; }

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
