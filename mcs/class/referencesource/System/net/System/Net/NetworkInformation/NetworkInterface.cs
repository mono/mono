
using System;

namespace System.Net.NetworkInformation
{
    
    public abstract class NetworkInterface
    {
        /// Returns objects that describe the network interfaces on the local computer.
        public static NetworkInterface[] GetAllNetworkInterfaces(){
#if WASM
            throw new PlatformNotSupportedException ();
#else
#if MONO_FEATURE_CAS
            (new NetworkInformationPermission(NetworkInformationAccess.Read)).Demand();
#endif
            return SystemNetworkInterface.GetNetworkInterfaces();
#endif
        }

        public static bool GetIsNetworkAvailable(){
#if WASM
            throw new PlatformNotSupportedException ();
#else
            return SystemNetworkInterface.InternalGetIsNetworkAvailable();
#endif
        }

        public static int LoopbackInterfaceIndex{
            get{
#if WASM
                throw new PlatformNotSupportedException ();
#else
                return SystemNetworkInterface.InternalLoopbackInterfaceIndex;
#endif
            }
        }

        public static int IPv6LoopbackInterfaceIndex {
            get {
                return SystemNetworkInterface.InternalIPv6LoopbackInterfaceIndex;
            }
        }

        public virtual string Id { get { throw new NotImplementedException(); } }
        
        /// Gets the name of the network interface.
        public virtual string Name { get { throw new NotImplementedException(); } }

        /// Gets the description of the network interface
        public virtual string Description { get { throw new NotImplementedException(); } }

        /// Gets the IP properties for this network interface.
        public virtual IPInterfaceProperties GetIPProperties() {
            throw new NotImplementedException(); 
        }

        /// Provides Internet Protocol (IP) statistical data for thisnetwork interface.
        /// Despite the naming, the results are not IPv4 specific.
        /// Do not use this method, use GetIPStatistics instead.
        public virtual IPv4InterfaceStatistics GetIPv4Statistics() {
            throw new NotImplementedException();
        }

        /// Provides Internet Protocol (IP) statistical data for this network interface.
        public virtual IPInterfaceStatistics GetIPStatistics() {
            throw new NotImplementedException();
        }

        /// Gets the current operational state of the network connection.
        public virtual OperationalStatus OperationalStatus { get { throw new NotImplementedException(); } }
                           
        /// Gets the speed of the interface in bits per second as reported by the interface.
        public virtual long Speed { get { throw new NotImplementedException(); } }

        /// Gets a bool value that indicates whether the network interface is set to only receive data packets.
        public virtual bool IsReceiveOnly { get { throw new NotImplementedException(); } }

        /// Gets a bool value that indicates whether this network interface is enabled to receive multicast packets.
        public virtual bool SupportsMulticast { get { throw new NotImplementedException(); } }
        
        /// Gets the physical address of this network interface
        /// <b>deonb. This is okay if you don't support this in Whidbey. This actually belongs in the NetworkAdapter derived class</b>
        public virtual PhysicalAddress GetPhysicalAddress() {
            throw new NotImplementedException();
        }

        /// Gets the interface type.
        public virtual NetworkInterfaceType NetworkInterfaceType { get { throw new NotImplementedException(); } }

        public virtual bool Supports(NetworkInterfaceComponent networkInterfaceComponent) {
            throw new NotImplementedException();
        }
    }
}

