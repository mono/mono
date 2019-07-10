
using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Security.Permissions;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace System.Net.NetworkInformation
{

    public abstract class IPGlobalProperties
    {
#if UNITY
        // defaults to false, but may be replaced by intrinsic in runtime
        static bool PlatformNeedsLibCWorkaround { get; }
#endif

        public static IPGlobalProperties GetIPGlobalProperties()
        {
#if MONODROID
            return new AndroidIPGlobalProperties ();
#elif MONOTOUCH || XAMMAC
            return new UnixIPGlobalProperties ();
#elif MONO
            switch (Environment.OSVersion.Platform) {
            case PlatformID.Unix:
#if UNITY
                if (PlatformNeedsLibCWorkaround)
                    return new UnixNoLibCIPGlobalProperties();
#endif
                MibIPGlobalProperties impl = null;
                if (Directory.Exists (MibIPGlobalProperties.ProcDir)) {
                    impl = new MibIPGlobalProperties (MibIPGlobalProperties.ProcDir);
                    if (File.Exists (impl.StatisticsFile))
                        return impl;
                }
                if (Directory.Exists (MibIPGlobalProperties.CompatProcDir)) {
                    impl = new MibIPGlobalProperties (MibIPGlobalProperties.CompatProcDir);
                    if (File.Exists (impl.StatisticsFile))
                        return impl;
                }
                return new UnixIPGlobalProperties ();
            default:
#if WIN_PLATFORM
                return new Win32IPGlobalProperties ();
#else
                return new UnixIPGlobalProperties ();
#endif
            }
#else          
            (new NetworkInformationPermission(NetworkInformationAccess.Read)).Demand();
            return new SystemIPGlobalProperties();
#endif
        }

        internal static IPGlobalProperties InternalGetIPGlobalProperties()
        {
#if MONO
            return GetIPGlobalProperties();
#else
            return new SystemIPGlobalProperties();
#endif
        }

        /// Gets the Active Udp Listeners on this machine
        public abstract IPEndPoint[] GetActiveUdpListeners();
                
        /// Gets the Active Tcp Listeners on this machine
        public abstract IPEndPoint[] GetActiveTcpListeners ();

        /// Gets the Active Udp Listeners on this machine
        public abstract TcpConnectionInformation[] GetActiveTcpConnections();

        /// Gets the Dynamic Host Configuration Protocol (DHCP) scope name.
        public abstract string DhcpScopeName {get;}

        /// Gets the domain in which the local computer is registered.
        
        public abstract string DomainName {get;}

        /// Gets the host name for the local computer.
        
        public abstract string HostName {get;}

        /// Gets a bool value that specifies whether the local computer is acting as a Windows Internet Name Service (WINS) proxy.
        public abstract bool IsWinsProxy {get;}

        /// Gets the Network Basic Input/Output System (NetBIOS) node type of the local computer.
        public abstract NetBiosNodeType NodeType{get;}
        

        public abstract TcpStatistics GetTcpIPv4Statistics();

        public abstract TcpStatistics GetTcpIPv6Statistics();

        /// Provides Internet Control Message Protocol (ICMP) version 4 statistical data for the local computer.
        /// Provides User Datagram Protocol (UDP) statistical data for the local computer.
        
        public abstract UdpStatistics GetUdpIPv4Statistics();
        public abstract UdpStatistics GetUdpIPv6Statistics();
        
        /// Provides Internet Control Message Protocol (ICMP) version 4 statistical data for the local computer.
        
        public abstract IcmpV4Statistics GetIcmpV4Statistics();

        /// Provides Internet Control Message Protocol (ICMP) version 6 statistical data for the local computer.
        
        public abstract IcmpV6Statistics GetIcmpV6Statistics();    

        /// Provides Internet Protocol (IP) statistical data for the local computer.
        public abstract IPGlobalStatistics GetIPv4GlobalStatistics();    
        public abstract IPGlobalStatistics GetIPv6GlobalStatistics();

        /// Returns a list of all unicast IP addresses after ensuring they are all stable
        public virtual UnicastIPAddressInformationCollection GetUnicastAddresses()
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        public virtual IAsyncResult BeginGetUnicastAddresses(AsyncCallback callback, object state)
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        public virtual UnicastIPAddressInformationCollection EndGetUnicastAddresses(IAsyncResult asyncResult)
        {
            throw ExceptionHelper.MethodNotImplementedException;
        }

        //************* Task-based async public methods *************************
        [HostProtection(ExternalThreading = true)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Unicast")]
        public virtual Task<UnicastIPAddressInformationCollection> GetUnicastAddressesAsync()
        {
            return Task<UnicastIPAddressInformationCollection>.Factory.FromAsync(BeginGetUnicastAddresses, EndGetUnicastAddresses, null);
        }

    }    
}

