using System;
using System.Linq;
using StateMachine.Common.Args;
using StateMachine.Common.Contracts;
using StateMachine.Common.Enums;

namespace StateMachine.Services.Manager
{
    public class ViewManager
    {
        #region Fields

        // Private members
        private string[] _viewStates;
        private string _defaultViewState;

        // UI - make this a Dictionary<string,IUserInterfcae>, if you have to handle more than one
        private IUserInterface _ui;

        #endregion

        #region Events

        public event EventHandler<StateMachineEventArgs> ViewManagerEvent;

        #endregion

        #region Properties

        public string CurrentView                               { get; private set; }    
        public IViewStateConfiguration ViewStateConfiguration   { get; set; }

        #endregion

        #region Singleton implementation

        // Create a thread-safe singleton wit lazy initialization
        private static readonly Lazy<ViewManager> _viewManager = new Lazy<ViewManager>(() => new ViewManager());

        public static ViewManager Instance { get { return _viewManager.Value; } }

        private ViewManager()
        {
        }

        #endregion

        #region Methods

        // Load view state configuration
        public void LoadViewStateConfiguration(IViewStateConfiguration viewStateConfiguration, IUserInterface userInterface)
        {
            this.ViewStateConfiguration     = viewStateConfiguration;
            this._viewStates                = viewStateConfiguration.ViewStateList;
            this._ui                        = userInterface;
            this._defaultViewState          = viewStateConfiguration.DefaultViewState;
        }

        /// <summary>
        /// Method to raise a view manager event for logging, etc
        /// </summary>
        /// <param name="name"></param>
        /// <param name="info"></param>
        /// <param name="eventType"></param>
        public void RaiseViewManagerEvent(string name, string info, StateMachineEventType eventType = StateMachineEventType.System)
        {
            var newVMargs = new StateMachineEventArgs(name, "View manager event: " + info, "View Manager", eventType);
            this.ViewManagerEvent?.Invoke(this, newVMargs);
        }

        /// <summary>
        /// Sends a command to another service
        /// </summary>
        /// <param name="name"></param>
        /// <param name="command"></param>
        /// <param name="info"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public void RaiseUICommand(string command, string info, string source, string target)
        {
            var newUIargs = new StateMachineEventArgs(command, info, source, StateMachineEventType.Command, target);
            this.ViewManagerEvent?.Invoke(this, newUIargs);
        }

        #endregion

        #region Handler

        /// <summary>
        /// Handler method for state machine commands
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void ViewCommandHandler(object sender, StateMachineEventArgs args)
        {
            // This approach assumes that there is a dedicated view state for every state machine UI command.
            try
            {
                if (this._viewStates.Contains(args.EventName))
                {
                    // Convention: view command event names matches corresponding view state
                    this._ui.LoadViewState(args.EventName);
                    this.CurrentView = args.EventName;
                    this.RaiseViewManagerEvent("View Manager Command", "Successfully loaded view state: " + args.EventName);
                }
                else
                {
                    this.RaiseViewManagerEvent("View Manager Command", "View state not found!");
                }
            }
            catch (Exception ex)
            {
                this.RaiseViewManagerEvent("View Manager Command - Error", ex.ToString());
            }
        }

        /// <summary>
        ///  Handler method for special system events, e.g. initialization
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void SystemEventHandler(object sender, StateMachineEventArgs args)
        {
            // Initialize
            if (args.EventName == "OnInit")
            {
                this._ui.LoadViewState(this._defaultViewState);
                this.CurrentView = this._defaultViewState;
            }

            // Catastrophic Error handling
            if (args.EventName == "CompleteFailure")
            {
                _ui.LoadViewState("CompleteFailure");
            }
        }

        #endregion
    }
}
