

namespace System.Net.NetworkInformation
{
    public abstract class IPInterfaceStatistics
    {

        /// Gets the number of bytes received on the interface.
        public abstract long BytesReceived { get; }

        /// Gets the number of bytes sent on the interface.
        public abstract long BytesSent { get; }

        /// Gets the number of incoming packets discarded.
        public abstract long IncomingPacketsDiscarded { get; }

        /// Gets the number of incoming packets with errors.
        public abstract long IncomingPacketsWithErrors { get; }

        /// Gets the number of incoming packets with an unknown protocol.
        public abstract long IncomingUnknownProtocolPackets { get; }

        /// Gets the number of non-unicast packets received on the interface.
        public abstract long NonUnicastPacketsReceived { get; }

        /// Gets the number of non-unicast packets sent on the interface.
        public abstract long NonUnicastPacketsSent { get; }

        /// Gets the number of outgoing packets that were discarded.
        public abstract long OutgoingPacketsDiscarded { get; }

        /// Gets the number of outgoing packets with errors.
        public abstract long OutgoingPacketsWithErrors { get; }

        /// Gets the length of the output queue.
        public abstract long OutputQueueLength { get; }

        /// Gets the number of unicast packets received on the interface.
        public abstract long UnicastPacketsReceived { get; }

        /// Gets the number of unicast packets sent on the interface.
        public abstract long UnicastPacketsSent { get; }

    }
    
    // Despite the naming, the results are not IPv4 specific
    // Do not use this type.  Use IPInterfaceStatistics instead.
    public abstract class IPv4InterfaceStatistics
    {
    
        /// Gets the number of bytes received on the interface.
        public abstract long BytesReceived{get;}
    
        /// Gets the number of bytes sent on the interface.
        public abstract long BytesSent{get;}
    
        /// Gets the number of incoming packets discarded.
        public abstract long IncomingPacketsDiscarded{get;}
    
        /// Gets the number of incoming packets with errors.
        public abstract long IncomingPacketsWithErrors{get;}
    
        /// Gets the number of incoming packets with an unknown protocol.
        public abstract long IncomingUnknownProtocolPackets{get;}
    
        /// Gets the number of non-unicast packets received on the interface.
        public abstract long NonUnicastPacketsReceived{get;}
    
        /// Gets the number of non-unicast packets sent on the interface.
        public abstract long NonUnicastPacketsSent{get;}
    
        /// Gets the number of outgoing packets that were discarded.
        public abstract long OutgoingPacketsDiscarded{get;}
    
        /// Gets the number of outgoing packets with errors.
        public abstract long OutgoingPacketsWithErrors{get;}
    
        /// Gets the length of the output queue.
        public abstract long OutputQueueLength{get;}
    
        /// Gets the number of unicast packets received on the interface.
        public abstract long UnicastPacketsReceived{get;}
    
        /// Gets the number of unicast packets sent on the interface.
        public abstract long UnicastPacketsSent{get;}
    
    }
    
}

