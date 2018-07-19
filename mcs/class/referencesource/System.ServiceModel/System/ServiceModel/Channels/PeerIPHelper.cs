//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;


    // IP address helper class for multi-homing support
    class PeerIPHelper
    {
        public event EventHandler AddressChanged;

        bool isOpen;
        readonly IPAddress listenAddress;       // To listen on a single IP address.
        IPAddress[] localAddresses;
        AddressChangeHelper addressChangeHelper;
        Socket ipv6Socket;
        object thisLock;

        const uint Six2FourPrefix = 0x220;
        const uint TeredoPrefix = 0x00000120;
        const uint IsatapIdentifier = 0xfe5e0000;

        enum AddressType
        {
            Unknown,
            Teredo,
            Isatap,
            Six2Four
        }

        public PeerIPHelper()
        {
            Initialize();
        }

        public PeerIPHelper(IPAddress listenAddress)
        {
            if (!(listenAddress != null))
            {
                throw Fx.AssertAndThrow("listenAddress expected to be non-null");
            }
            this.listenAddress = listenAddress;
            Initialize();
        }

        void Initialize()
        {
            this.localAddresses = new IPAddress[0];
            this.thisLock = new object();
        }

        // NOTE: This is just for suites -- to skip timeout usage
        internal int AddressChangeWaitTimeout
        {
            set { this.addressChangeHelper.Timeout = value; }
        }

        object ThisLock
        {
            get { return this.thisLock; }
        }

        // Compares if the specified collection matches our local cache. Return true on mismatch.
        public bool AddressesChanged(ReadOnlyCollection<IPAddress> addresses)
        {
            bool changed = false;
            lock (ThisLock)
            {
                if (addresses.Count != this.localAddresses.Length)
                {
                    changed = true;
                }
                else
                {
                    // If every specified addresses exist in the cache, addresses haven't changed
                    foreach (IPAddress address in this.localAddresses)
                    {
                        if (!addresses.Contains(address))
                        {
                            changed = true;
                            break;
                        }
                    }
                }
            }

            return changed;
        }

        // Since scope ID of IPAddress is mutable, you want to be able to clone an IP address
        public static IPAddress CloneAddress(IPAddress source, bool maskScopeId)
        {
            IPAddress clone = null;
            if (maskScopeId || V4Address(source))
                clone = new IPAddress(source.GetAddressBytes());
            else
                clone = new IPAddress(source.GetAddressBytes(), source.ScopeId);
            return clone;
        }

        // Since scope ID of IPAddress is mutable, you want to be able to clone IP addresses in an array or collection
        static ReadOnlyCollection<IPAddress> CloneAddresses(IPAddress[] sourceArray)
        {
            IPAddress[] cloneArray = new IPAddress[sourceArray.Length];
            for (int i = 0; i < sourceArray.Length; i++)
            {
                cloneArray[i] = CloneAddress(sourceArray[i], false);
            }
            return new ReadOnlyCollection<IPAddress>(cloneArray);
        }

        public static ReadOnlyCollection<IPAddress> CloneAddresses(ReadOnlyCollection<IPAddress> sourceCollection, bool maskScopeId)
        {
            IPAddress[] cloneArray = new IPAddress[sourceCollection.Count];
            for (int i = 0; i < sourceCollection.Count; i++)
            {
                cloneArray[i] = CloneAddress(sourceCollection[i], maskScopeId);
            }
            return new ReadOnlyCollection<IPAddress>(cloneArray);
        }

        // When listening on a specific IP address, creates an array containing just that address
        static IPAddress[] CreateAddressArray(IPAddress address)
        {
            IPAddress[] addressArray = new IPAddress[1];
            addressArray[0] = CloneAddress(address, false);
            return addressArray;
        }

        public void Close()
        {
            if (this.isOpen)
            {
                lock (ThisLock)
                {
                    if (this.isOpen)
                    {
                        this.addressChangeHelper.Unregister();
                        if (this.ipv6Socket != null)
                        {
                            this.ipv6Socket.Close();
                        }
                        this.isOpen = false;
                        this.addressChangeHelper = null;
                    }
                }
            }
        }

        // Retrieve the IP addresses configured on the machine. 
        IPAddress[] GetAddresses()
        {
            List<IPAddress> addresses = new List<IPAddress>();
            List<IPAddress> temporaryAddresses = new List<IPAddress>();

            if (this.listenAddress != null)     // single local address scenario?
            {
                // Check if the specified address is configured
                if (ValidAddress(this.listenAddress))
                {
                    return (CreateAddressArray(this.listenAddress));
                }
            }

            // Walk the interfaces
            NetworkInterface[] networkIfs = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface networkIf in networkIfs)
            {
                if (!ValidInterface(networkIf))
                {
                    continue;
                }

                // Process each unicast address for the interface. Pick at most one IPv4 and one IPv6 address.
                // Add remaining eligible addresses on the list to surplus list. We can add these back to the list
                // being returned, if there is room.
                IPInterfaceProperties properties = networkIf.GetIPProperties();
                if (properties != null)
                {
                    foreach (UnicastIPAddressInformation unicastAddress in properties.UnicastAddresses)
                    {
                        if (NonTransientAddress(unicastAddress))
                        {
                            if (unicastAddress.SuffixOrigin == SuffixOrigin.Random)
                                temporaryAddresses.Add(unicastAddress.Address);
                            else
                                addresses.Add(unicastAddress.Address);
                        }

                    }

                }

            }
            if (addresses.Count > 0)
                return ReorderAddresses(addresses);
            else
                return temporaryAddresses.ToArray();
        }


        internal static IPAddress[] ReorderAddresses(IEnumerable<IPAddress> sourceAddresses)
        {
            List<IPAddress> result = new List<IPAddress>();
            List<IPAddress> notAdded = new List<IPAddress>();

            AddressType addressType = AddressType.Unknown;
            IPAddress v4Address = null, v6Address = null, isatapAddress = null, teredoAddress = null, six2FourAddress = null;

            foreach (IPAddress address in sourceAddresses)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (v4Address != null)
                        notAdded.Add(address);
                    else
                    {
                        v4Address = address;
                    }
                    continue;
                }
                if (address.AddressFamily != AddressFamily.InterNetworkV6)
                {
                    notAdded.Add(address);
                    continue;
                }
                if (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal)
                {
                    notAdded.Add(address);
                    continue;
                }
                addressType = GetAddressType(address);
                switch (addressType)
                {
                    case AddressType.Teredo:
                        {
                            if (teredoAddress == null)
                            {
                                teredoAddress = address;
                            }
                            else
                            {
                                notAdded.Add(address);
                            }
                            continue;
                        }
                    case AddressType.Six2Four:
                        {
                            if (six2FourAddress == null)
                            {
                                six2FourAddress = address;
                            }
                            else
                            {
                                notAdded.Add(address);
                            }
                            continue;
                        }
                    case AddressType.Isatap:
                        {
                            if (isatapAddress == null)
                            {
                                isatapAddress = address;
                            }
                            else
                            {
                                notAdded.Add(address);
                            }
                            continue;
                        }
                    default:
                        {
                            if (v6Address != null)
                                notAdded.Add(address);
                            else
                            {
                                v6Address = address;
                            }
                            continue;
                        }
                }
            }
            if (six2FourAddress != null)
                result.Add(six2FourAddress);
            if (teredoAddress != null)
                result.Add(teredoAddress);
            if (isatapAddress != null)
                result.Add(isatapAddress);
            if (v6Address != null)
                result.Add(v6Address);
            if (v4Address != null)
                result.Add(v4Address);

            result.AddRange(notAdded);

            return result.ToArray();

        }

        static AddressType GetAddressType(IPAddress address)
        {
            AddressType result = AddressType.Unknown;

            byte[] bytes = address.GetAddressBytes();
            if (BitConverter.ToUInt16(bytes, 0) == Six2FourPrefix)
                result = AddressType.Six2Four;
            else if (BitConverter.ToUInt32(bytes, 0) == TeredoPrefix)
                result = AddressType.Teredo;
            else if (BitConverter.ToUInt32(bytes, 8) == IsatapIdentifier)
                result = AddressType.Isatap;

            return result;
        }


        // Given an EPR, replaces its URI with the specified IP address
        public static EndpointAddress GetIPEndpointAddress(EndpointAddress epr, IPAddress address)
        {
            EndpointAddressBuilder eprBuilder = new EndpointAddressBuilder(epr);
            eprBuilder.Uri = GetIPUri(epr.Uri, address);
            return eprBuilder.ToEndpointAddress();
        }

        // Given a hostName based URI, replaces hostName with the IP address
        public static Uri GetIPUri(Uri uri, IPAddress ipAddress)
        {
            UriBuilder uriBuilder = new UriBuilder(uri);
            if (V6Address(ipAddress) && (ipAddress.IsIPv6LinkLocal || ipAddress.IsIPv6SiteLocal))
            {
                // We make a copy of the IP address because scopeID will not be part of ToString() if set after IP address was created            
                uriBuilder.Host = new IPAddress(ipAddress.GetAddressBytes(), ipAddress.ScopeId).ToString();
            }
            else
            {
                uriBuilder.Host = ipAddress.ToString();
            }
            return uriBuilder.Uri;
        }

        // Retrieve the currently configured addresses (cached)
        public ReadOnlyCollection<IPAddress> GetLocalAddresses()
        {
            lock (ThisLock)
            {
                // Return a clone of the address cache
                return CloneAddresses(this.localAddresses);
            }
        }

        // An address is valid if it is a non-transient addresss (if global, must be DNS-eligible as well)
        static bool NonTransientAddress(UnicastIPAddressInformation address)
        {
            return (!address.IsTransient);
        }



        public static bool V4Address(IPAddress address)
        {
            return address.AddressFamily == AddressFamily.InterNetwork;
        }

        public static bool V6Address(IPAddress address)
        {
            return address.AddressFamily == AddressFamily.InterNetworkV6;
        }

        // Returns true if the specified address is configured on the machine
        public static bool ValidAddress(IPAddress address)
        {
            // Walk the interfaces
            NetworkInterface[] networkIfs = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface networkIf in networkIfs)
            {
                if (ValidInterface(networkIf))
                {
                    IPInterfaceProperties properties = networkIf.GetIPProperties();
                    if (properties != null)
                    {
                        foreach (UnicastIPAddressInformation unicastAddress in properties.UnicastAddresses)
                        {
                            if (address.Equals(unicastAddress.Address))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        static bool ValidInterface(NetworkInterface networkIf)
        {
            return (networkIf.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    networkIf.OperationalStatus == OperationalStatus.Up);
        }

        // Process the address change notification from the system and check if the addresses
        // have really changed. If so, update the local cache and notify interested parties.
        void OnAddressChanged()
        {
            bool changed = false;

            IPAddress[] newAddresses = GetAddresses();
            lock (ThisLock)
            {
                if (AddressesChanged(Array.AsReadOnly<IPAddress>(newAddresses)))
                {
                    this.localAddresses = newAddresses;
                    changed = true;
                }
            }

            if (changed)
            {
                EventHandler handler = AddressChanged;
                if (handler != null && this.isOpen)
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }

        public void Open()
        {
            lock (ThisLock)
            {
                Fx.Assert(!this.isOpen, "Helper not expected to be open");

                // Register for addr changed event and retrieve addresses from the system
                this.addressChangeHelper = new AddressChangeHelper(OnAddressChanged);
                this.localAddresses = GetAddresses();
                if (Socket.OSSupportsIPv6)
                {
                    this.ipv6Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.IP);
                }
                this.isOpen = true;
            }
        }

        // Sorts the collection of addresses using sort ioctl
        public ReadOnlyCollection<IPAddress> SortAddresses(ReadOnlyCollection<IPAddress> addresses)
        {
            ReadOnlyCollection<IPAddress> sortedAddresses = SocketAddressList.SortAddresses(this.ipv6Socket, listenAddress, addresses);

            // If listening on specific address, copy the scope ID that we're listing on for the
            // link and site local addresses in the sorted list (so that the connect will work)
            if (this.listenAddress != null)
            {
                if (this.listenAddress.IsIPv6LinkLocal)
                {
                    foreach (IPAddress address in sortedAddresses)
                    {
                        if (address.IsIPv6LinkLocal)
                        {
                            address.ScopeId = this.listenAddress.ScopeId;
                        }
                    }
                }
                else if (this.listenAddress.IsIPv6SiteLocal)
                {
                    foreach (IPAddress address in sortedAddresses)
                    {
                        if (address.IsIPv6SiteLocal)
                        {
                            address.ScopeId = this.listenAddress.ScopeId;
                        }
                    }
                }
            }
            return sortedAddresses;
        }

        //
        // Helper class to handle system address change events. Because multiple events can be fired as a result of
        // a single significant change (such as interface being enabled/disabled), we try to handle the event just 
        // once using the below mechanism:
        // Start a timer that goes off after Timeout seconds. If get another address change event from the system 
        // within this timespan, reset the timer to go off after another Timeout secs. When the timer finally fires,
        // the registered handlers are notified. This should minimize (but not completely eliminate -- think wireless
        // DHCP interface being enabled, for instance -- this could take longer) reacting multiple times for
        // a single change.
        //
        class AddressChangeHelper
        {
            public delegate void AddedChangedCallback();

            // To avoid processing multiple addr change events within this time span (5 seconds)
            public int Timeout = 5000;
            IOThreadTimer timer;
            AddedChangedCallback addressChanged;

            public AddressChangeHelper(AddedChangedCallback addressChanged)
            {
                Fx.Assert(addressChanged != null, "addressChanged expected to be non-null");
                this.addressChanged = addressChanged;
                this.timer = new IOThreadTimer(new Action<object>(FireAddressChange), null, true);
                NetworkChange.NetworkAddressChanged += OnAddressChange;
            }

            public void Unregister()
            {
                NetworkChange.NetworkAddressChanged -= OnAddressChange;
            }

            void OnAddressChange(object sender, EventArgs args)
            {
                this.timer.Set(Timeout);
            }

            // Now fire address change event to the interested parties
            void FireAddressChange(object asyncState)
            {
                this.timer.Cancel();
                this.addressChanged();
            }
        }
    }
}
