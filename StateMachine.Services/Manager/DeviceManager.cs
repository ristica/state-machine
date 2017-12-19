using System;
using System.Collections.Generic;
using System.Linq;
using StateMachine.Common.Args;
using StateMachine.Common.Contracts;
using StateMachine.Common.Enums;

namespace StateMachine.Services.Manager
{
    public class DeviceManager
    {
        #region Singleton implementation

        // Create a thread-safe singleton with lazy initialization
        private static readonly Lazy<DeviceManager> _deviceManager = 
            new Lazy<DeviceManager>(() => new DeviceManager());

        public static DeviceManager Instance { get { return _deviceManager.Value; } }

        private DeviceManager()
        {
            Devices = new Dictionary<string, object>();
        }

        #endregion

        #region Properties

        public Dictionary<string, object> Devices { get; set; }

        #endregion

        #region Events

        // Device manager event is used for logging
        public event EventHandler<StateMachineEventArgs> DeviceManagerEvent;

        public event EventHandler<StateMachineEventArgs> DeviceManagerNotification;

        #endregion

        #region Methods

        public void AddDevice(string name, object device)
        {
            this.Devices.Add(name, device);
            this.RaiseDeviceManagerEvent("Added device", name);
        }

        public void RemoveDevice(string name)
        {
            this.Devices.Remove(name);
            this.RaiseDeviceManagerEvent("Removed device", name);
        }

        public void LoadDeviceConfiguration(IDeviceConfiguration devManConfiguration)
        {
            this.Devices = devManConfiguration.Devices;
        }

        #endregion

        #region Handlers

        /// <summary>
        /// Handler method for state machine commands
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void DeviceCommandHandler(object sender, StateMachineEventArgs args)
        {
            // Listen to command events only
            if (args.EventType != StateMachineEventType.Command) return;

            // Get device and execute command action method
            try
            {
                if (!Devices.Keys.Contains(args.Target)) return;
                // Convention device commands and method names must mach!
                var device = Devices[args.Target];
                var deviceMethod = device.GetType().GetMethod(args.EventName);
                deviceMethod.Invoke(device, new object[] { });
                this.RaiseDeviceManagerEvent("DeviceCommand", "Successful device command: " + args.Target + " - " + args.EventName);
            }
            catch (Exception ex)
            {
                this.RaiseDeviceManagerEvent("DeviceCommand - Error", ex.ToString());
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
            if (args.EventName == "OnInit" && args.EventType == StateMachineEventType.Command)
            {
                foreach (var dev in this.Devices)
                {
                    try
                    {
                        var initMethod = dev.Value.GetType().GetMethod("OnInit");
                        initMethod.Invoke(dev.Value, new object[] { });
                        this.RaiseDeviceManagerEvent("DeviceCommand - Initialization device", dev.Key);
                    }
                    catch (Exception ex)
                    {
                        this.RaiseDeviceManagerEvent("DeviceCommand - Initialization error device" + dev.Key, ex.ToString());
                    }
                }
            }

            // Notification handling
            // because we use UI to trigger transitions devices would trigger normally themselves.
            // Nevertheless, this is common, if SW user interfaces control devices
            // View and device managers communicate on system event bus and use notifications to trigger state machine as needed!
            if (args.EventType != StateMachineEventType.Command) return;

            // Check for right condition 
            if (args.EventName == "OnInit") return;
            if (!this.Devices.ContainsKey(args.Target)) return;

            // Dispatch command to device
            this.DeviceCommandHandler(this, args);
        }

        /// <summary>
        /// Method to raise a device manager event for logging, etc.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="info"></param>
        private void RaiseDeviceManagerEvent(string name, string info)
        {
            var newDmArgs = new StateMachineEventArgs(name, "Device manager event: " + info, "Device Manager", StateMachineEventType.System);
            this.DeviceManagerEvent?.Invoke(this, newDmArgs);
        }

        /// <summary>
        /// Sends a command from device manager to state machine
        /// </summary>
        /// <param name="command"></param>
        /// <param name="info"></param>
        /// <param name="source"></param>
        public void RaiseDeviceManagerNotification(string command, string info, string source)
        {
            var newDmArgs = new StateMachineEventArgs(command, info, source, StateMachineEventType.Notification, "State Machine");
            this.DeviceManagerNotification?.Invoke(this, newDmArgs);
        }

        #endregion
    }
}
