using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StateMachine.ActiveStateMachine.Enums;
using StateMachine.ActiveStateMachine.Subjects;
using StateMachine.Common;
using StateMachine.Common.Args;
using StateMachine.Common.Enums;

namespace StateMachine.ActiveStateMachine
{
    public class BaseStateMachine
    {
        #region Fields

        private Task _queueWorkerTask;
        private readonly State _initialState;
        private ManualResetEvent _resumer;
        private CancellationTokenSource _tokenSource;

        #endregion

        #region Properties

        public Dictionary<string, State> States             { get; private set; }
        public BlockingCollection<string> TriggerQueue      { get; private set; }
        public State CurrentState                           { get; private set; }
        public State PreviousState                          { get; private set; }
        public EngineState StateMachineEngine               { get; private set; }

        #endregion

        #region Events

        public event EventHandler<StateMachineEventArgs> StateMachineEvent;

        #endregion

        #region C-Tor

        public BaseStateMachine(
            Dictionary<string, State> states,
            int queueCapacity)
        {
            this.States = states;
            this._initialState = new State("InitialState", null, null, null);
            this.TriggerQueue = new BlockingCollection<string>(queueCapacity);

            this.InitStateMachine();

            this.RaiseStateMachineSystemEvent("StateMachine: Initialized", "System ready to start");
            StateMachineEngine = EngineState.Initialized;
        }

        #endregion

        #region Engine

        public void Start()
        {
            // Create cancellation token for QueueWorker method
            this._tokenSource = new CancellationTokenSource();

            // Create a new worker thread, if it does not exist
            this._queueWorkerTask = Task.Factory.StartNew(QueueWorkerMethod, _tokenSource, TaskCreationOptions.LongRunning);

            // Set engine state
            this.StateMachineEngine = EngineState.Running;
            this.RaiseStateMachineSystemEvent("StateMachine: Started", "System running.");
        }

        public void Pause()
        {
            this._resumer.Reset();

            StateMachineEngine = EngineState.Paused;  
            this.RaiseStateMachineSystemEvent("StateMachine: Paused", "System  waiting.");

        }

        public void Resume()
        {
            // Worker thread exists, just resume where it was paused.
            this._resumer.Set();

            StateMachineEngine = EngineState.Running;
            this.RaiseStateMachineSystemEvent("StateMachine: Resumed", "System running.");
        }

        public void Stop()
        {
            // Cancel processing
            this._tokenSource.Cancel();

            // Wait for thread to return
            this._queueWorkerTask.Wait();

            // Free resources
            this._queueWorkerTask.Dispose();

            // Set engine state
            StateMachineEngine = EngineState.Stopped;
            this.RaiseStateMachineSystemEvent("StateMachine: Stopped", "System execution stopped.");
        }

        #endregion

        #region Methods

        public void InitStateMachine()
        {
            // Set previous state to an unspecific initial state. THe initial state never will be used during normal operation
            this.PreviousState = this._initialState;

            // Look for the default state, which is the state to begin with in StateList.
            foreach (var state in this.States)
            {
                if (!state.Value.IsDefaultState) continue;

                this.CurrentState = state.Value;
                this.RaiseStateMachineSystemCommand("OnInit", "StateMachineInitialized");
            }

            // This is the synchronization object for resuming - passing true means non-blocking (signaled), which is the normal operation mode.
            this._resumer = new ManualResetEvent(true);
        }

        private void EnterTrigger(string newTrigger)
        {
            try
            {
                // Put trigger in queue    
                this.TriggerQueue.Add(newTrigger);
            }
            catch (Exception e)
            {
                this.RaiseStateMachineSystemEvent("ActiveStateMachine - Error entering trigger", newTrigger + " - " + e);
            }

            // Raise an event
            this.RaiseStateMachineSystemEvent("ActiveStateMachine - Trigger entered", newTrigger);
        }

        private void QueueWorkerMethod(object dummy)
        {
            // Blocks execution until it is reset. 
            // Used to pause the state machine.
            this._resumer.WaitOne();

            // Block the queue and loop through all triggers available. Blocking queue guarantees FIFO and the GetConsumingEnumerable method
            // automatically removes triggers from queue!
            try
            {
                foreach (var trigger in this.TriggerQueue.GetConsumingEnumerable())
                {
                    if (this._tokenSource.IsCancellationRequested)
                    {
                        this.RaiseStateMachineSystemEvent("State machine: QueueWorker", "Processing canceled!");
                        return;
                    }

                    // Compare trigger
                    var t = trigger;
                    foreach (
                        var transition in
                            CurrentState.StateTansitions.Where(
                                transition => t == transition.Value.Trigger))
                    {
                        this.ExecuteTransition(transition.Value);
                    }

                    // Do not place any code here, because it will not be executed!
                    // The foreach loop keeps spinning on the queue until thread is canceled.   
                }
            }
            catch (Exception ex)
            {
                this.RaiseStateMachineSystemEvent("State machine: QueueWorker", "Processing canceled! Exception: " + ex);

                // Create a new queue worker task. THe previous one is completing right now.
                this.Start();
            }
        }

        protected virtual void ExecuteTransition(Transition transition)
        {
            // Default checking, if this is a valid transaction.
            if (CurrentState.StateName != transition.SourceState)
            {
                var message =
                    string.Format("Transition has wrong source state {0}, when system is in {1}",
                        transition.SourceState, 
                        this.CurrentState.StateName);

                this.RaiseStateMachineSystemEvent("State machine: Default guard execute transition.", message);
                return;
            }
            if (!States.ContainsKey(transition.TargetState))
            {
                var message = string.Format("Transition has wrong target state {0}, when system is in {1}. State not in global state list",
                                    transition.SourceState, 
                                    CurrentState.StateName);

                this.RaiseStateMachineSystemEvent("State machine: Default guard execute transition.", message);
                return;
            }

            // Run all exit actions of the old state
            this.CurrentState.ExitActions.ForEach(a => a.Execute());

            // Run all guards of the transition
            transition.Guards.ForEach(g => g.Execute());

            var info = transition.Guards.Count + " guard actions executed!";
            this.RaiseStateMachineSystemEvent("State machine: ExecuteTransition", info);

            // Run all actions of the transition
            transition.Transitions.ForEach(t => t.Execute());

            this.ChangeState(transition);
        }

        #endregion

        #region Helpers

        private void ChangeState(Transition transition)
        {
            var info = transition.Transitions.Count + " transition actions executed!";
            this.RaiseStateMachineSystemEvent("State machine: Begin state change!", info);


            // First resolve the target state with the help of its name
            var targetState = GetStateFromStateList(transition.TargetState);

            // Transition successful - Change state
            this.PreviousState = this.CurrentState;
            this.CurrentState = targetState;

            // Run all entry actions of new state
            foreach (var entryAction in this.CurrentState.AccessActions)
            {
                entryAction.Execute();
            }

            this.RaiseStateMachineSystemEvent("State machine: State change completed successfully!", "Previous state: "
                + PreviousState.StateName + " - New state = " + CurrentState.StateName);
        }

        private State GetStateFromStateList(string targetStateName)
        {
            return this.States[targetStateName];
        }

        #endregion

        #region Infrastructure

        /// <summary>
        /// Helper method to raise state machine system events
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventInfo"></param>
        private void RaiseStateMachineSystemEvent(string eventName, string eventInfo)
        {
            if (this.StateMachineEvent == null)
                return;

            this.StateMachineEvent.Invoke(
                this, 
                new StateMachineEventArgs(eventName, eventInfo, "State machine", StateMachineEventType.System));
        }

        /// <summary>
        /// Raises an event of type command
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventInfo"></param>
        private void RaiseStateMachineSystemCommand(string eventName, string eventInfo)
        {
            if (this.StateMachineEvent == null)
                return;

            StateMachineEvent.Invoke(
                this, 
                new StateMachineEventArgs(eventName, eventInfo, "State machine", StateMachineEventType.Command));
        }

        /// <summary>
        /// Event Handler for internal events triggering the state machine
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="intArgs"></param>
        public void InternalNotificationHandler(object sender, StateMachineEventArgs intArgs)
        {
            EnterTrigger(intArgs.EventName);
        }

        #endregion
    }
}
