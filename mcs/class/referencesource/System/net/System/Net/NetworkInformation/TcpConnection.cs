
using System;

namespace System.Net.NetworkInformation
{
    
    /// Provides information about the Transmission Control Protocol (TCP) connections on the local computer.
    public abstract class TcpConnectionInformation
    {

        /// Gets the local endpoint of a Transmission Control Protocol (TCP) connection.
        public abstract IPEndPoint LocalEndPoint { get;}

        /// Gets the remote endpoint of a Transmission Control Protocol (TCP) connection.
        public abstract IPEndPoint RemoteEndPoint { get;}

        /// Gets the state of this Transmission Control Protocol (TCP) connection.
        public abstract TcpState State { get;}
    }
}

