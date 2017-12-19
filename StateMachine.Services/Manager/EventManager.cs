using System;
using System.Collections.Generic;
using System.Diagnostics;
using StateMachine.Common.Args;
using StateMachine.Common.Enums;

namespace StateMachine.Services.Manager
{
    public class EventManager
    {
        #region Fields

        // Collection of registered events
        private readonly Dictionary<string, object> _events;

        // Event manger event is used for logging
        public event EventHandler<StateMachineEventArgs> EventManagerEvent;

        #endregion

        #region Singleton implementation

        // Create a thread-safe singleton wit lazy initialization
        private static readonly Lazy<EventManager> _eventManager = 
            new Lazy<EventManager>(() => new EventManager());

        public static EventManager Instance { get { return _eventManager.Value; } }

        private EventManager()
        {
            _events = new Dictionary<string, object>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registration of an event used in the system
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="source"></param>
        public void RegisterEvent(string eventName, object source)
        {
            this._events.Add(eventName, source);
        }

        /// <summary>
        /// Subscription method maps handler method in a sink object to an event of the source object. 
        /// Of course, method signatures between delegate and handler need to match!
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handlerMethodName"></param>
        /// <param name="sink"></param>
        public bool SubscribeEvent(string eventName, string handlerMethodName, object sink)
        {
            try
            {
                // Get event from list
                var evt = _events[eventName];

                // Determine meta data from event and handler
                var eventInfo = evt.GetType().GetEvent(eventName);
                var methodInfo = sink.GetType().GetMethod(handlerMethodName);

                // Create new delegate mapping event to handler
                var handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, sink, methodInfo);
                eventInfo.AddEventHandler(evt, handler);
                return true;
            }
            catch (Exception ex)
            {
                // Log failure!
                var message = "Exception while subscribing to handler. Event:" + eventName + " - Handler: " + handlerMethodName + "- Exception: " + ex;
                Debug.Print(message);
                this.RaiseEventManagerEvent("EventManagerSystemEvent", message, StateMachineEventType.System);
                return false;
            }
        }

        private void RaiseEventManagerEvent(string eventName, string eventInfo, StateMachineEventType eventType)
        {
            var newArgs = new StateMachineEventArgs(eventName, eventInfo, "Event Manager", eventType);
            EventManagerEvent?.Invoke(this, newArgs);
        }

        #endregion
    }
}
