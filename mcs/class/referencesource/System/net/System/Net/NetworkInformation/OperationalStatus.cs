
using System;

namespace System.Net.NetworkInformation
{
    public enum OperationalStatus{
        Up = 1,
        Down,
        Testing,
        Unknown,
        Dormant,
        NotPresent,
        LowerLayerDown
    }
}

