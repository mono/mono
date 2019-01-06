
using System;

namespace System.Net.NetworkInformation
{
    /// Specifies the states of a Transmission Control Protocol (TCP) connection.
    public enum TcpState
    {
        Unknown,
        Established,
        SynSent,
        SynReceived,
        FinWait1,
        FinWait2,
        TimeWait,
        Closed,
        CloseWait,
        LastAck,
        Listen,
        Closing,
        DeleteTcb
    }
 }

