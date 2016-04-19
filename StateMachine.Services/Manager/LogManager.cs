using System;
using System.Diagnostics;
using StateMachine.Common.Args;
using StateMachine.Common.Enums;

namespace StateMachine.Services.Manager
{
    public class LogManager
    {
        #region Singleton implementation
        // Create a thread-safe singleton wit lazy initialization
        private static readonly Lazy<LogManager> _logger = 
            new Lazy<LogManager>(() => new LogManager());

        public static LogManager Instance { get { return _logger.Value; } }

        private LogManager()
        {
        }
        #endregion

        /// <summary>
        /// Log infos to debug window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void LogEventHandler(object sender, StateMachineEventArgs args)
        {
            // Log system events
            if (args.EventType != StateMachineEventType.Notification)
            {
                Debug.Print(args.TimeStamp + " SystemEvent: " + args.EventName + " - Info: " + args.EventInfo + " - StateMachineArgumentType: " + args.EventType + " - Source: " + args.Source + " - Target: " + args.Target);
            }
            // Log state machine notifications
            else
            {
                Debug.Print(args.TimeStamp + " Notification: " + args.EventName + " - Info: " + args.EventInfo + " - StateMachineArgumentType: " + args.EventType + " - Source: " + args.Source + " - Target: " + args.Target);
            }
        }
    }
}
