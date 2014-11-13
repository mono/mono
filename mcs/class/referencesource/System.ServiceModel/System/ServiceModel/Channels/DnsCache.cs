//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Runtime;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.Text;

    static class DnsCache
    {
        const int mruWatermark = 64;
        static MruCache<string, DnsCacheEntry> resolveCache = new MruCache<string, DnsCacheEntry>(mruWatermark);
        static readonly TimeSpan cacheTimeout = TimeSpan.FromSeconds(2);

        // Double-checked locking pattern requires volatile for read/write synchronization
        static volatile string machineName;

        static object ThisLock
        {
            get
            {
                return resolveCache;
            }
        }

        public static string MachineName
        {
            get
            {
                if (machineName == null)
                {
                    lock (ThisLock)
                    {
                        if (machineName == null)
                        {
                            try
                            {
                                machineName = Dns.GetHostEntry(String.Empty).HostName;
                            }
                            catch (SocketException exception)
                            {
                                DiagnosticUtility.TraceHandledException(exception,
                                        TraceEventType.Information);                                
                                // we fall back to the NetBios machine if Dns fails
                                machineName = UnsafeNativeMethods.GetComputerName(ComputerNameFormat.PhysicalNetBIOS);
                            }
                        }
                    }
                }

                return machineName;
            }
        }

        public static IPHostEntry Resolve(Uri uri)
        {
            string hostName = uri.DnsSafeHost;
            IPHostEntry hostEntry = null;
            DateTime now = DateTime.UtcNow;

            lock (ThisLock)
            {
                DnsCacheEntry cacheEntry;
                if (resolveCache.TryGetValue(hostName, out cacheEntry))
                {
                    if (now.Subtract(cacheEntry.TimeStamp) > cacheTimeout)
                    {
                        resolveCache.Remove(hostName);
                    }
                    else
                    {
                        if (cacheEntry.HostEntry == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new EndpointNotFoundException(SR.GetString(SR.DnsResolveFailed, hostName)));
                        }
                        hostEntry = cacheEntry.HostEntry;
                    }
                }
            }

            if (hostEntry == null)
            {
                SocketException dnsException = null;
                try
                {
                    hostEntry = Dns.GetHostEntry(hostName);
                }
                catch (SocketException e)
                {
                    dnsException = e;
                }

                lock (ThisLock)
                {
                    // MruCache doesn't have a this[] operator, so we first remove (just in case it exists already)
                    resolveCache.Remove(hostName);
                    resolveCache.Add(hostName, new DnsCacheEntry(hostEntry, now));
                }

                if (dnsException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new EndpointNotFoundException(SR.GetString(SR.DnsResolveFailed, hostName), dnsException));
                }
            }

            return hostEntry;
        }

        class DnsCacheEntry
        {
            IPHostEntry hostEntry;
            DateTime timeStamp;

            public DnsCacheEntry(IPHostEntry hostEntry, DateTime timeStamp)
            {
                this.hostEntry = hostEntry;
                this.timeStamp = timeStamp;
            }

            public IPHostEntry HostEntry
            {
                get
                {
                    return hostEntry;
                }
            }

            public DateTime TimeStamp
            {
                get
                {
                    return timeStamp;
                }
            }
        }
    }
}
