
using System;

namespace System.Net.NetworkInformation
{
    
    public abstract class NetworkInterface
    {
        internal const string EXCEPTION_MESSAGE = "System.Net.NetworkInformation.NetworkInterface is not supported on the current platform.";

        public static NetworkInterface [] GetAllNetworkInterfaces () {
            throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
        }

        public static bool GetIsNetworkAvailable () {
            throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
        }

        public static int LoopbackInterfaceIndex {
            get {
                throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
            }
        }

        public static int IPv6LoopbackInterfaceIndex {
            get {
                throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
            }
        }

        public virtual string Id { get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); } }
        public virtual string Name { get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); } }
        public virtual string Description { get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);} }
        public virtual IPInterfaceProperties GetIPProperties () {
            throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
        }

        public virtual IPv4InterfaceStatistics GetIPv4Statistics () {
            throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
        }

        public virtual IPInterfaceStatistics GetIPStatistics () {
            throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
        }

        public virtual OperationalStatus OperationalStatus { get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); } }
        public virtual long Speed { get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); } }
        public virtual bool IsReceiveOnly { get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); } }
        public virtual bool SupportsMulticast { get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); } }

        public virtual PhysicalAddress GetPhysicalAddress () {
            throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
        }

        public virtual NetworkInterfaceType NetworkInterfaceType { get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); } }

        public virtual bool Supports (NetworkInterfaceComponent networkInterfaceComponent) {
            throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
        }
    }
}

