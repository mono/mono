
using System;

namespace System.Net.NetworkInformation
{
    /// Specifies the states of a Transmission Control Protocol (TCP) connection.
    public enum TcpState
    {
        Unknown,
        Closed,
        Listen,
        SynSent,
        SynReceived,
        Established,
        FinWait1,
        FinWait2,
        CloseWait,
        Closing,
        LastAck,
        TimeWait,
        DeleteTcb
    }
 }

