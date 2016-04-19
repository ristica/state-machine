using System;

namespace StateMachine.Services.DeviceBase
{
    /// <summary>
    /// base class for all devices
    /// </summary>
    public abstract class Device
    {
        #region Fields

        Action<string, string, string> _devEvMethod;
        
        #endregion

        #region Properties

        public string DevName { get; private set; }

        #endregion 

        #region C-Tor

        protected Device(
            string deviceName, 
            Action<string, string, string> eventCallBack)
        {
            this.DevName        = deviceName;
            this._devEvMethod   = eventCallBack;
        }

        #endregion

        #region Device initialization method

        public abstract void OnInit();

        #endregion

        #region Event infrastructure

        public void RegisterEventCallback(Action<string, string, string> method)
        {
            this._devEvMethod = method;
        }

        public void DoNotificationCallBack(string name, string eventInfo, string source)
        {
            this._devEvMethod.Invoke(name, eventInfo, source);
        }

        #endregion
    }
}
