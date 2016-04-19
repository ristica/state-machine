using System.Collections.Generic;

namespace StateMachine.Common.Contracts
{
    public interface IDeviceConfiguration
    {
        Dictionary<string, object> Devices { get; set; }
    }
}