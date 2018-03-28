//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.PeerResolvers;
    using System.ServiceProcess;
    using System.Text;
    using System.Threading;
    using Microsoft.Win32.SafeHandles;

    sealed class PnrpPeerResolver : PeerResolver
    {
        UnsafePnrpNativeMethods.PeerNameRegistrar registrar = new UnsafePnrpNativeMethods.PeerNameRegistrar();
        static bool isPnrpAvailable;
        static bool isPnrpInstalled;
        const UnsafePnrpNativeMethods.PnrpResolveCriteria resolutionScope = UnsafePnrpNativeMethods.PnrpResolveCriteria.NearestNonCurrentProcess;
        public const int PNRPINFO_HINT = 0x00000001;

        internal const int CommentLength = 80;
        internal const byte TcpTransport = 0x01;
        internal const byte PayloadVersion = 0x01;
        internal const char PathSeparator = '/';
        internal const int MinGuids = 1;
        internal const int MaxGuids = 2;
        internal const byte GuidEscape = 0xFF;
        internal const int MaxAddressEntries = 10;
        internal const int MaxAddressEntriesV1 = 4;
        internal const int MaxPathLength = 200; //this is known prefix+any guids
        static TimeSpan MaxTimeout = new TimeSpan(0, 10, 0); //PNRP validates the timeout to be no greater than 10 minutes
        static TimeSpan MaxResolveTimeout = new TimeSpan(0, 0, 45);
        internal const string GlobalCloudName = "Global_";
        static object SharedLock = new object();
        static Random randomGenerator = new Random();
        static TimeSpan TimeToWaitForStatus = TimeSpan.FromSeconds(15);
        PeerReferralPolicy referralPolicy = PeerReferralPolicy.Share;

        [Flags]
        internal enum PnrpResolveScope
        {
            None = 0,
            Global = 1,
            SiteLocal = 2,
            LinkLocal = 4,
            All = Global | SiteLocal | LinkLocal
        }

        static PnrpPeerResolver()
        {
            // determine if PNRP is installed
            isPnrpAvailable = false;
            using (UnsafePnrpNativeMethods.DiscoveryBase db = new UnsafePnrpNativeMethods.DiscoveryBase())
            {
                isPnrpInstalled = db.IsPnrpInstalled();
                isPnrpAvailable = db.IsPnrpAvailable(TimeToWaitForStatus);
            }
        }

        internal PnrpPeerResolver() : this(PeerReferralPolicy.Share) { }
        internal PnrpPeerResolver(PeerReferralPolicy referralPolicy)
        {
            this.referralPolicy = referralPolicy;
        }

        static Encoding PnrpEncoder
        {
            get
            {
                return System.Text.Encoding.UTF8;
            }
        }

        public static bool IsPnrpAvailable
        {
            get { return isPnrpAvailable; }
        }

        public static bool IsPnrpInstalled
        {
            get { return isPnrpInstalled; }
        }

        public static IPEndPoint GetHint()
        {
            byte[] bytes = new byte[16];
            lock (SharedLock)
            {
                randomGenerator.NextBytes(bytes);
            }
            return new IPEndPoint(new IPAddress(bytes), 0);
        }

        // Get the hint for the node in this process that handles this meshid
        // Get the nodeid for the current node - If the factory is using PrivatePeerNode then this can throw.
        // in which case use a hint of 0
        // The resolver prepends 0. to the meshid so we strip it off before locating the node.
        // false means that the search must include the current process.
        public static bool HasPeerNodeForMesh(string meshId)
        {
            PeerNodeImplementation node = null;
            return PeerNodeImplementation.TryGet(meshId, out node);
        }

        // PNRP doesn't support registering the same peername by the same identity in the same process.
        // Thus, we cannot test the PNRP resolver between two nodes in the same process without a little help.
        // By calling SetMeshExtensions, the resolver will register and resolver different ids, allowing two
        // nodes to work in the same process.
        string localExtension;
        string remoteExtension;
        internal void SetMeshExtensions(string local, string remote)
        {
            localExtension = local;
            remoteExtension = remote;
        }

        internal PnrpResolveScope EnumerateClouds(bool forResolve, Dictionary<uint, string> LinkCloudNames, Dictionary<uint, string> SiteCloudNames)
        {
            bool foundActive = false;
            PnrpResolveScope currentScope = PnrpResolveScope.None;
            LinkCloudNames.Clear();
            SiteCloudNames.Clear();
            UnsafePnrpNativeMethods.CloudInfo[] cloudInfos = UnsafePnrpNativeMethods.PeerCloudEnumerator.GetClouds();

            // If we are resolving we should first look for active clouds only
            // If we find some then we should return those to the caller
            // otherwise we should just load up with clouds
            if (forResolve)
            {
                foreach (UnsafePnrpNativeMethods.CloudInfo cloud in cloudInfos)
                {
                    if (cloud.State == UnsafePnrpNativeMethods.PnrpCloudState.Active)
                    {
                        if (cloud.Scope == UnsafePnrpNativeMethods.PnrpScope.Global)
                        {
                            currentScope |= PnrpResolveScope.Global;
                            foundActive = true;
                        }
                        else if (cloud.Scope == UnsafePnrpNativeMethods.PnrpScope.LinkLocal)
                        {
                            Fx.Assert(!String.IsNullOrEmpty(cloud.Name), "Unknown scope id in the IPAddress");
                            LinkCloudNames.Add(cloud.ScopeId, cloud.Name);
                            currentScope |= PnrpResolveScope.LinkLocal;
                            foundActive = true;
                        }
                        else if (cloud.Scope == UnsafePnrpNativeMethods.PnrpScope.SiteLocal)
                        {
                            Fx.Assert(!String.IsNullOrEmpty(cloud.Name), "Unknown scope id in the IPAddress");
                            SiteCloudNames.Add(cloud.ScopeId, cloud.Name);
                            currentScope |= PnrpResolveScope.SiteLocal;
                            foundActive = true;
                        }
                    }
                }
            }

            if (!foundActive)
            {
                foreach (UnsafePnrpNativeMethods.CloudInfo cloud in cloudInfos)
                {
                    if (!((cloud.State == UnsafePnrpNativeMethods.PnrpCloudState.Dead)
                        || (cloud.State == UnsafePnrpNativeMethods.PnrpCloudState.Disabled)
                        || (cloud.State == UnsafePnrpNativeMethods.PnrpCloudState.NoNet))
                       )
                    {
                        if (cloud.Scope == UnsafePnrpNativeMethods.PnrpScope.Global)
                        {
                            currentScope |= PnrpResolveScope.Global;
                            continue;
                        }
                        if (cloud.Scope == UnsafePnrpNativeMethods.PnrpScope.LinkLocal)
                        {
                            Fx.Assert(!String.IsNullOrEmpty(cloud.Name), "Unknown scope id in the IPAddress");
                            LinkCloudNames.Add(cloud.ScopeId, cloud.Name);
                            currentScope |= PnrpResolveScope.LinkLocal;
                        }
                        else if (cloud.Scope == UnsafePnrpNativeMethods.PnrpScope.SiteLocal)
                        {
                            Fx.Assert(!String.IsNullOrEmpty(cloud.Name), "Unknown scope id in the IPAddress");
                            SiteCloudNames.Add(cloud.ScopeId, cloud.Name);
                            currentScope |= PnrpResolveScope.SiteLocal;
                        }
                    }
                }
            }
            return currentScope;
        }

        class RegistrationHandle
        {
            public string PeerName;
            public List<string> Clouds;
            public RegistrationHandle(string peerName)
            {
                this.PeerName = peerName;
                Clouds = new List<string>();
            }
            public void AddCloud(string name)
            {
                this.Clouds.Add(name);
            }
        }

        public override bool CanShareReferrals
        {
            get
            {
                return referralPolicy != PeerReferralPolicy.DoNotShare;
            }
        }
        public override object Register(string meshId, PeerNodeAddress nodeAddress, TimeSpan timeout)
        {
            ThrowIfNoPnrp();

            PnrpRegistration globalEntry = null;
            PnrpRegistration[] linkEntries = null;
            PnrpRegistration[] siteEntries = null;

            RegistrationHandle regHandle = new RegistrationHandle(meshId);
            Dictionary<uint, string> SiteCloudNames = new Dictionary<uint, string>();
            Dictionary<uint, string> LinkCloudNames = new Dictionary<uint, string>();

            PnrpResolveScope availableScope = EnumerateClouds(false, LinkCloudNames, SiteCloudNames);

            if (availableScope == PnrpResolveScope.None)
            {
                //could not find any clouds.
                PeerExceptionHelper.ThrowInvalidOperation_PnrpNoClouds();
            }

            if (localExtension != null)
                meshId += localExtension;

            try
            {
                PeerNodeAddressToPnrpRegistrations(meshId, LinkCloudNames, SiteCloudNames, nodeAddress, out linkEntries, out siteEntries, out globalEntry);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.PeerPnrpIllegalUri), e));
            }
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            try
            {
                PnrpResolveScope currentScope = PnrpResolveScope.None;
                if (globalEntry != null)
                {
                    if (globalEntry.Addresses.Length > 0 && (availableScope & PnrpResolveScope.Global) != 0)
                    {
                        registrar.Register(globalEntry, timeoutHelper.RemainingTime());
                        regHandle.AddCloud(globalEntry.CloudName);
                        currentScope |= PnrpResolveScope.Global;
                    }
                }
                if (linkEntries.Length > 0)
                {
                    foreach (PnrpRegistration entry in linkEntries)
                    {
                        if (entry.Addresses.Length > 0)
                        {
                            registrar.Register(entry, timeoutHelper.RemainingTime());
                            regHandle.AddCloud(entry.CloudName);
                        }
                    }
                    currentScope |= PnrpResolveScope.LinkLocal;
                }
                if (siteEntries.Length > 0)
                {
                    foreach (PnrpRegistration entry in siteEntries)
                    {
                        if (entry.Addresses.Length > 0)
                        {
                            registrar.Register(entry, timeoutHelper.RemainingTime());
                            regHandle.AddCloud(entry.CloudName);
                        }
                    }
                    currentScope |= PnrpResolveScope.SiteLocal;
                }
                if (currentScope == PnrpResolveScope.None)
                {
                    // We have addresses but no cloud that corresponds to them
                    // so we should throw an exception
                    PeerExceptionHelper.ThrowInvalidOperation_PnrpAddressesUnsupported();
                }
            }
            catch (SocketException)
            {
                try
                {
                    Unregister(regHandle, timeoutHelper.RemainingTime());
                }
                catch (SocketException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                throw;
            }

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                PnrpRegisterTraceRecord record = new PnrpRegisterTraceRecord(meshId, globalEntry, siteEntries, linkEntries);
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PnrpRegisteredAddresses,
                    SR.GetString(SR.TraceCodePnrpRegisteredAddresses),
                    record, this, null);
            }

            return regHandle;
        }

        void ThrowIfNoPnrp()
        {
            if (!isPnrpAvailable)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.PeerPnrpNotAvailable)));
            }
        }

        public override void Unregister(object registrationId, TimeSpan timeout)
        {
            RegistrationHandle regHandle = registrationId as RegistrationHandle;
            if (regHandle == null || String.IsNullOrEmpty(regHandle.PeerName))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.PeerInvalidRegistrationId, regHandle), "registrationId"));
            string meshId = regHandle.PeerName;

            // prepend a 0. for unsecured peername
            string peerName = string.Format(CultureInfo.InvariantCulture, "0.{0}", meshId);
            registrar.Unregister(peerName, regHandle.Clouds, timeout);

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                PnrpPeerResolverTraceRecord record = new PnrpPeerResolverTraceRecord(meshId, new List<PeerNodeAddress>());
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PnrpUnregisteredAddresses,
                    SR.GetString(SR.TraceCodePnrpUnregisteredAddresses),
                    record, this, null);
            }
        }

        public override void Update(object registrationId, PeerNodeAddress updatedNodeAddress, TimeSpan timeout)
        {
            RegistrationHandle regHandle = registrationId as RegistrationHandle;
            if (regHandle == null || string.IsNullOrEmpty(regHandle.PeerName))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.PeerInvalidRegistrationId, regHandle), "registrationId"));

            string meshId = regHandle.PeerName;
            Register(meshId, updatedNodeAddress, timeout);
        }

        //return null in case of unrecognized format. consider adding logging.
        PeerNodeAddress PeerNodeAddressFromPnrpRegistration(PnrpRegistration input)
        {
            List<IPAddress> addresses = new List<IPAddress>();
            PeerNodeAddress result = null;
            Guid[] guids;
            StringBuilder pathBuilder = new StringBuilder(MaxPathLength);
            int version = 0;
            string protocolScheme;

            try
            {
                if (input == null || String.IsNullOrEmpty(input.Comment))
                    return null;
                Array.ForEach(input.Addresses, delegate(IPEndPoint obj) { addresses.Add(obj.Address); });
                if (addresses.Count != 0)
                {
                    UriBuilder uriBuilder = new UriBuilder();
                    uriBuilder.Port = input.Addresses[0].Port;
                    uriBuilder.Host = addresses[0].ToString();
                    pathBuilder.Append(PeerStrings.KnownServiceUriPrefix);
                    CharEncoder.Decode(input.Comment, out version, out protocolScheme, out guids);

                    if (
                        (version == PayloadVersion) &&
                        (guids != null) && (guids.Length <= MaxGuids) &&
                        (guids.Length >= MinGuids)
                    )
                    {
                        uriBuilder.Scheme = protocolScheme;
                        Array.ForEach(guids, delegate(Guid guid)
                                            {
                                                pathBuilder.Append(PathSeparator + String.Format(CultureInfo.InvariantCulture, "{0}", guid.ToString()));
                                            }
                        );
                        uriBuilder.Path = String.Format(CultureInfo.InvariantCulture, "{0}", pathBuilder.ToString());
                        result = new PeerNodeAddress(new EndpointAddress(uriBuilder.Uri), new ReadOnlyCollection<IPAddress>(addresses));
                    }
                }
            }
            catch (ArgumentException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (FormatException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (IndexOutOfRangeException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }

            return result;

        }

        void TrimToMaxAddresses(List<IPEndPoint> addressList)
        {
            if (addressList.Count > MaxAddressEntries)
            {
                addressList.RemoveRange(MaxAddressEntries, addressList.Count - MaxAddressEntries);
            }
        }

        void PeerNodeAddressToPnrpRegistrations(string meshName, Dictionary<uint, string> LinkCloudNames, Dictionary<uint, string> SiteCloudNames, PeerNodeAddress input, out PnrpRegistration[] linkRegs, out PnrpRegistration[] siteRegs, out PnrpRegistration global)
        {
            PnrpRegistration reg = new PnrpRegistration();

            Dictionary<uint, PnrpRegistration> resultsLink = new Dictionary<uint, PnrpRegistration>();
            Dictionary<uint, PnrpRegistration> resultsSite = new Dictionary<uint, PnrpRegistration>();
            PnrpRegistration entry = null;
            string scheme;
            Guid[] guids;
            ParseServiceUri(input.EndpointAddress.Uri, out scheme, out guids);
            int port = input.EndpointAddress.Uri.Port;
            if (port <= 0)
                port = TcpUri.DefaultPort;
            string peerName = string.Format(CultureInfo.InvariantCulture, "0.{0}", meshName);
            string comment = CharEncoder.Encode(PayloadVersion, scheme, guids);
            global = null;
            string cloudName = string.Empty;
            foreach (IPAddress address in input.IPAddresses)
            {
                if (address.AddressFamily == AddressFamily.InterNetworkV6
                    &&
                    ((address.IsIPv6LinkLocal) || (address.IsIPv6SiteLocal))
                )
                {
                    if (address.IsIPv6LinkLocal)
                    {
                        if (!resultsLink.TryGetValue((uint)address.ScopeId, out entry))
                        {
                            if (!LinkCloudNames.TryGetValue((uint)address.ScopeId, out cloudName))
                            {
                                continue;
                            }
                            entry = PnrpRegistration.Create(peerName, comment, cloudName);
                            resultsLink.Add((uint)address.ScopeId, entry);
                        }
                    }
                    else
                    {
                        if (!resultsSite.TryGetValue((uint)address.ScopeId, out entry))
                        {
                            if (!SiteCloudNames.TryGetValue((uint)address.ScopeId, out cloudName))
                            {
                                continue;
                            }
                            entry = PnrpRegistration.Create(peerName, comment, cloudName);
                            resultsSite.Add((uint)address.ScopeId, entry);
                        }
                    }
                    entry.addressList.Add(new IPEndPoint(address, port));
                }
                else
                {
                    if (global == null)
                    {
                        global = PnrpRegistration.Create(peerName, comment, GlobalCloudName);
                    }
                    global.addressList.Add(new IPEndPoint(address, port));
                }
            }
            if (global != null)
            {
                if (global.addressList != null)
                {
                    TrimToMaxAddresses(global.addressList);
                    global.Addresses = global.addressList.ToArray();
                }
                else
                    global.Addresses = new IPEndPoint[0];
            }

            if (resultsLink.Count != 0)
            {
                foreach (PnrpRegistration tempLink in resultsLink.Values)
                {
                    if (tempLink.addressList != null)
                    {
                        TrimToMaxAddresses(tempLink.addressList);
                        tempLink.Addresses = tempLink.addressList.ToArray();
                    }
                    else
                    {
                        tempLink.Addresses = new IPEndPoint[0];
                    }
                }
                linkRegs = new PnrpRegistration[resultsLink.Count];
                resultsLink.Values.CopyTo(linkRegs, 0);
            }
            else
                linkRegs = new PnrpRegistration[0];
            if (resultsSite.Count != 0)
            {
                foreach (PnrpRegistration tempSite in resultsSite.Values)
                {
                    if (tempSite.addressList != null)
                    {
                        TrimToMaxAddresses(tempSite.addressList);
                        tempSite.Addresses = tempSite.addressList.ToArray();
                    }
                    else
                    {
                        tempSite.Addresses = new IPEndPoint[0];
                    }
                }
                siteRegs = new PnrpRegistration[resultsSite.Count];
                resultsSite.Values.CopyTo(siteRegs, 0);
            }
            else
                siteRegs = new PnrpRegistration[0];
        }

        static int ProtocolFromName(string name)
        {
            if (name == Uri.UriSchemeNetTcp)
            {
                return TcpTransport;
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("name", SR.GetString(SR.PeerPnrpIllegalUri));
        }

        static string NameFromProtocol(byte number)
        {
            switch (number)
            {
                case TcpTransport:
                    return Uri.UriSchemeNetTcp;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.PeerPnrpIllegalUri)));
            }
        }

        void ParseServiceUri(Uri uri, out string scheme, out Guid[] result)
        {
            if (uri != null)
            {
                if ((ProtocolFromName(uri.Scheme) != 0) && !String.IsNullOrEmpty(uri.AbsolutePath))
                {
                    scheme = uri.Scheme;
                    string[] parts = uri.AbsolutePath.Trim(new char[] { ' ', PathSeparator }).Split(PathSeparator);
                    if ((0 == String.Compare(parts[0], PeerStrings.KnownServiceUriPrefix, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (parts.Length >= MinGuids && parts.Length <= MaxGuids + 1)
                        {
                            result = new Guid[parts.Length - 1];
                            try
                            {
                                for (int i = 1; i < parts.Length; i++)
                                    result[i - 1] = Fx.CreateGuid(parts[i]);
                                return;
                            }
                            catch (FormatException e)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.PeerPnrpIllegalUri), e));
                            }
                        }
                    }
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.PeerPnrpIllegalUri)));
        }

        void MergeResults(Dictionary<string, PnrpRegistration> results, List<PnrpRegistration> regs)
        {
            PnrpRegistration entry = null;
            foreach (PnrpRegistration reg in regs)
            {
                if (!results.TryGetValue(reg.Comment, out entry))
                {
                    entry = reg;
                    results.Add(reg.Comment, reg);
                    entry.addressList = new List<IPEndPoint>();
                }
                entry.addressList.AddRange(reg.Addresses);
                reg.Addresses = null;
            }
        }

        void MergeResults(List<PeerNodeAddress> nodeAddressList, List<PnrpRegistration> globalRegistrations, List<PnrpRegistration> linkRegistrations, List<PnrpRegistration> siteRegistrations)
        {
            Dictionary<string, PnrpRegistration> results = new Dictionary<string, PnrpRegistration>();
            MergeResults(results, globalRegistrations);
            MergeResults(results, siteRegistrations);
            MergeResults(results, linkRegistrations);
            PeerNodeAddress result;
            foreach (PnrpRegistration reg in results.Values)
            {
                reg.Addresses = reg.addressList.ToArray();
                result = PeerNodeAddressFromPnrpRegistration(reg);
                if (result != null)
                    nodeAddressList.Add(result);
            }
        }

        public override ReadOnlyCollection<PeerNodeAddress> Resolve(string meshId, int maxAddresses, TimeSpan timeout)
        {
            ThrowIfNoPnrp();
            UnsafePnrpNativeMethods.PeerNameResolver resolver;
            List<UnsafePnrpNativeMethods.PeerNameResolver> resolvers = new List<UnsafePnrpNativeMethods.PeerNameResolver>();
            List<PnrpRegistration> globalRegistrations = new List<PnrpRegistration>();
            List<PnrpRegistration> linkRegistrations = new List<PnrpRegistration>();
            List<PnrpRegistration> siteRegistrations = new List<PnrpRegistration>();
            List<WaitHandle> handles = new List<WaitHandle>();
            Dictionary<uint, string> SiteCloudNames = new Dictionary<uint, string>();
            Dictionary<uint, string> LinkCloudNames = new Dictionary<uint, string>();
            UnsafePnrpNativeMethods.PnrpResolveCriteria targetScope = resolutionScope;
            TimeoutHelper timeoutHelper = new TimeoutHelper(TimeSpan.Compare(timeout, MaxResolveTimeout) <= 0 ? timeout : MaxResolveTimeout);

            if (!HasPeerNodeForMesh(meshId))
                targetScope = UnsafePnrpNativeMethods.PnrpResolveCriteria.Any;
            PnrpResolveScope currentScope = EnumerateClouds(true, LinkCloudNames, SiteCloudNames);

            if (remoteExtension != null)
                meshId += remoteExtension;

            // prepend a 0. for unsecured peername
            string peerName = string.Format(CultureInfo.InvariantCulture, "0.{0}", meshId);
            if ((currentScope & PnrpResolveScope.Global) != 0)
            {
                resolver = new UnsafePnrpNativeMethods.PeerNameResolver(
                                    peerName, maxAddresses, targetScope, 0, GlobalCloudName, timeoutHelper.RemainingTime(), globalRegistrations);
                handles.Add(resolver.AsyncWaitHandle);
                resolvers.Add(resolver);
            }

            if ((currentScope & PnrpResolveScope.LinkLocal) != 0)
            {
                foreach (KeyValuePair<uint, string> linkEntry in LinkCloudNames)
                {
                    resolver = new UnsafePnrpNativeMethods.PeerNameResolver(
                                    peerName, maxAddresses, targetScope, linkEntry.Key, linkEntry.Value, timeoutHelper.RemainingTime(), linkRegistrations);
                    handles.Add(resolver.AsyncWaitHandle);
                    resolvers.Add(resolver);
                }
            }

            if ((currentScope & PnrpResolveScope.SiteLocal) != 0)
            {
                foreach (KeyValuePair<uint, string> siteEntry in SiteCloudNames)
                {
                    resolver = new UnsafePnrpNativeMethods.PeerNameResolver(
                                    peerName, maxAddresses, targetScope, siteEntry.Key, siteEntry.Value, timeoutHelper.RemainingTime(), siteRegistrations);
                    handles.Add(resolver.AsyncWaitHandle);
                    resolvers.Add(resolver);
                }
            }
            if (handles.Count == 0)
            {
                //could not find any clouds.
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    Exception exception = new InvalidOperationException(SR.GetString(SR.PnrpNoClouds));
                    PnrpResolveExceptionTraceRecord record = new PnrpResolveExceptionTraceRecord(meshId, string.Empty, exception);
                    TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.PnrpResolvedAddresses,
                        SR.GetString(SR.TraceCodePnrpResolvedAddresses),
                        record, this, null);
                }
                return new ReadOnlyCollection<PeerNodeAddress>(new List<PeerNodeAddress>());
            }

            Exception lastException = null;
            foreach (UnsafePnrpNativeMethods.PeerNameResolver handle in resolvers)
            {
                try
                {
                    handle.End();
                }
                catch (SocketException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    lastException = e;
                }
            }

            List<PeerNodeAddress> nodeAddressList = new List<PeerNodeAddress>();
            MergeResults(nodeAddressList, globalRegistrations, linkRegistrations, siteRegistrations);
            if ((lastException != null) && (nodeAddressList.Count == 0))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(lastException);
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                PnrpPeerResolverTraceRecord record = new PnrpPeerResolverTraceRecord(meshId, nodeAddressList);
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PnrpResolvedAddresses,
                    SR.GetString(SR.TraceCodePnrpResolvedAddresses),
                    record, this, null);
            }
            return new ReadOnlyCollection<PeerNodeAddress>(nodeAddressList);
        }


        // contains the friendly PNRP information
        internal class PnrpRegistration
        {
            public string PeerName;
            public string CloudName;
            public string Comment;
            public IPEndPoint[] Addresses;
            public List<IPEndPoint> addressList;

            internal static PnrpRegistration Create(string peerName, string comment, string cloudName)
            {
                PnrpRegistration reg = new PnrpRegistration();
                reg.Comment = comment;
                reg.CloudName = cloudName;
                reg.PeerName = peerName;
                reg.addressList = new List<IPEndPoint>();
                return reg;
            }
        }

        internal class CharEncoder
        {
            static void CheckAtLimit(int current)
            {
                if (current + 1 >= CommentLength)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.PeerPnrpIllegalUri)));
            }
            static void EncodeByte(byte b, ref int offset, byte[] bytes)
            {
                if (b == 0 || b == GuidEscape)
                {
                    CheckAtLimit(offset);
                    bytes[offset++] = GuidEscape;
                }
                CheckAtLimit(offset);
                bytes[offset++] = b;
            }

            static internal string Encode(int version, string protocolName, Guid[] guids)
            {
                byte[] bytes = new byte[CommentLength];
                int i = 0;
                int protocol = ProtocolFromName(protocolName);
                EncodeByte(Convert.ToByte(version), ref i, bytes);
                EncodeByte(Convert.ToByte(protocol), ref i, bytes);
                EncodeByte(Convert.ToByte(guids.Length), ref i, bytes);
                foreach (Guid guid in guids)
                {
                    foreach (byte b in guid.ToByteArray())
                    {
                        EncodeByte(Convert.ToByte(b), ref i, bytes);
                    }
                }
                if (i % 2 != 0 && i < bytes.Length)
                    bytes[i] = GuidEscape;
                // Now we have a collection of bytes lets turn it into a string
                int length = i;
                int clength = (length / 2) + (length % 2);         // Pack 2 bytes per char
                char[] chars = new char[clength];
                i = 0;
                for (int j = 0; j < clength; j++)
                {
                    chars[j] = Convert.ToChar(bytes[i++] * 0x100 + bytes[i++]);
                }
                return new string(chars);
            }

            static byte GetByte(int offset, char[] chars)
            {
                int p = offset / 2;
                int lo = offset % 2;
                return Convert.ToByte(lo == 1 ? chars[p] & GuidEscape : chars[p] / 0x100);
            }

            static byte DecodeByte(ref int offset, char[] chars)
            {
                byte b = GetByte(offset++, chars);
                if (b == 0xff)
                {
                    b = GetByte(offset++, chars);
                }
                return b;
            }

            static internal void Decode(string buffer, out int version, out string protocolName, out Guid[] guids)
            {
                char[] chars = buffer.ToCharArray();
                byte protocol;
                int i = 0;

                version = DecodeByte(ref i, chars);
                protocol = DecodeByte(ref i, chars);
                protocolName = NameFromProtocol(protocol);
                int length = DecodeByte(ref i, chars);
                guids = new Guid[length];

                for (int g = 0; g < length; g++)
                {
                    byte[] bytes = new byte[16];
                    for (int j = 0; j < 16; j++)
                    {
                        bytes[j] = DecodeByte(ref i, chars);
                    }
                    guids[g] = new Guid(bytes);
                }
            }
        }

        internal enum PnrpErrorCodes
        {
            WSA_PNRP_ERROR_BASE = 11500,
            WSA_PNRP_CLOUD_NOT_FOUND = 11501,
            WSA_PNRP_CLOUD_DISABLED = 11502,
            //these error codes are not relevant for now
            //            WSA_PNRP_INVALID_IDENTITY = 11503,
            //            WSA_PNRP_TOO_MUCH_LOAD = 11504,
            WSA_PNRP_CLOUD_IS_RESOLVE_ONLY = 11505,
            //            WSA_PNRP_CLIENT_INVALID_COMPARTMENT_ID = 11506,
            WSA_PNRP_FW_PORT_BLOCKED = 11507,
            WSA_PNRP_DUPLICATE_PEER_NAME = 11508,
        }

        internal class PnrpException : SocketException
        {
            string message;

            internal PnrpException(int errorCode, string cloud)
                : base(errorCode)
            {
                LoadMessage(errorCode, cloud);
            }

            public override string Message
            {
                get
                {
                    if (!String.IsNullOrEmpty(message))
                        return message;
                    else
                        return base.Message;
                }
            }

            void LoadMessage(int errorCode, string cloud)
            {
                string formatString;
                switch ((PnrpErrorCodes)errorCode)
                {
                    case PnrpErrorCodes.WSA_PNRP_CLOUD_DISABLED:
                        formatString = SR.PnrpCloudDisabled;
                        break;
                    case PnrpErrorCodes.WSA_PNRP_CLOUD_NOT_FOUND:
                        formatString = SR.PnrpCloudNotFound;
                        break;
                    case PnrpErrorCodes.WSA_PNRP_CLOUD_IS_RESOLVE_ONLY:
                        formatString = SR.PnrpCloudResolveOnly;
                        break;
                    case PnrpErrorCodes.WSA_PNRP_FW_PORT_BLOCKED:
                        formatString = SR.PnrpPortBlocked;
                        break;
                    case PnrpErrorCodes.WSA_PNRP_DUPLICATE_PEER_NAME:
                        formatString = SR.PnrpDuplicatePeerName;
                        break;
                    default:
                        formatString = null;
                        break;
                }
                if (formatString != null)
                    message = SR.GetString(formatString, cloud);
            }
        }

        internal static class UnsafePnrpNativeMethods
        {
            // WSA import functions
            [DllImport("ws2_32.dll", CharSet = CharSet.Unicode)]
            [ResourceExposure(ResourceScope.None)]
            static extern int WSASetService(CriticalAllocHandle querySet, WsaSetServiceOp essOperation, int dwControlFlags);

            [DllImport("ws2_32.dll", CharSet = CharSet.Unicode)]
            [ResourceExposure(ResourceScope.None)]
            static extern int WSALookupServiceNext(CriticalLookupHandle hLookup,
                WsaNspControlFlags dwControlFlags, ref int lpdwBufferLength, IntPtr Results);
            [DllImport("ws2_32.dll", CharSet = CharSet.Unicode), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [ResourceExposure(ResourceScope.None)]
            static extern int WSALookupServiceEnd(IntPtr hLookup);
            [DllImport("ws2_32.dll", CharSet = CharSet.Unicode)]
            [ResourceExposure(ResourceScope.None)]
            static extern int WSALookupServiceBegin(CriticalAllocHandle query, WsaNspControlFlags dwControlFlags, out CriticalLookupHandle hLookup);
            [DllImport("ws2_32.dll", CharSet = CharSet.Ansi)]
            [ResourceExposure(ResourceScope.None)]
            static extern int WSAStartup(Int16 wVersionRequested, ref WsaData lpWSAData);
            [DllImport("ws2_32.dll", CharSet = CharSet.Ansi)]
            [ResourceExposure(ResourceScope.None)]
            static extern int WSACleanup();
            [DllImport("ws2_32.dll", CharSet = CharSet.Ansi)]
            [ResourceExposure(ResourceScope.None)]
            static extern int WSAGetLastError();
            [DllImport("ws2_32.dll", CharSet = CharSet.Unicode)]
            [ResourceExposure(ResourceScope.None)]
            static extern int WSAEnumNameSpaceProviders(ref int lpdwBufferLength, IntPtr lpnspBuffer);

            // PNRP namespace identifiers
            static Guid SvcIdCloud = new Guid(0xc2239ce6, 0x00c0, 0x4fbf, 0xba, 0xd6, 0x18, 0x13, 0x93, 0x85, 0xa4, 0x9a);
            static Guid SvcIdNameV1 = new Guid(0xc2239ce5, 0x00c0, 0x4fbf, 0xba, 0xd6, 0x18, 0x13, 0x93, 0x85, 0xa4, 0x9a);
            static Guid SvcIdName = new Guid(0xc2239ce7, 0x00c0, 0x4fbf, 0xba, 0xd6, 0x18, 0x13, 0x93, 0x85, 0xa4, 0x9a);
            static Guid NsProviderName = new Guid(0x03fe89cd, 0x766d, 0x4976, 0xb9, 0xc1, 0xbb, 0x9b, 0xc4, 0x2c, 0x7b, 0x4d);
            static Guid NsProviderCloud = new Guid(0x03fe89ce, 0x766d, 0x4976, 0xb9, 0xc1, 0xbb, 0x9b, 0xc4, 0x2c, 0x7b, 0x4d);

            const int MaxAddresses = 10;
            const int MaxAddressesV1 = 4;
            const Int16 RequiredWinsockVersion = 0x0202;

            // specifies the namespace used used by a specified WSAQUERYSET
            [Serializable]
            internal enum NspNamespaces
            {
                Cloud = 39,
                Name = 38,
            }

            [Serializable]
            [Flags]
            internal enum PnrpCloudFlags
            {
                None = 0x0000,
                LocalName = 0x0001, //  Name not valid on other computers
            }

            [Serializable]
            internal enum PnrpCloudState
            {
                Virtual = 0,        //  Not initialized
                Synchronizing = 1,  //  The cache is initializing
                Active = 2,         //  Cloud is active
                Dead = 3,            //  Initialized but lost network
                Disabled = 4,       //disabled in the registry
                NoNet = 5,          //active but lost network
                Alone = 6,
            }

            [Serializable]
            internal enum PnrpExtendedPayloadType
            {
                None = 0,
                Binary,
                String
            }

            // internal because it is exposed by PeerNameResolver
            [Serializable]
            internal enum PnrpResolveCriteria
            {
                Default = 0,                    // Default = PNRP_RESOLVE_CRITERIA_NON_CURRENT_PROCESS_PEER_NAME
                Remote = 1,                     // match first 128 bits (remote node)
                NearestRemote = 2,              // match first 128 bits, and close to top 64 bits
                // of the second 128 bits (remote node)
                NonCurrentProcess = 3,          //  match first 128 bits (not in the current process) 
                NearestNonCurrentProcess = 4,   // match first 128 bits, and close to top 64 bits
                // of the second 128 bits (not in the current process)   
                Any = 5,                        // match first 128 bits (any node)
                Nearest = 6                     // match first 128 bits, and close to top 64 bits
                // of the second 128 bits (any node)   
            }

            [Serializable]
            internal enum PnrpRegisteredIdState
            {
                Ok = 1,     //  Id is active in cloud
                Problem = 2 //  Id is no longer registered in cloud
            }

            internal enum PnrpScope
            {
                Any = 0,
                Global = 1,
                SiteLocal = 2,
                LinkLocal = 3,
            }

            // primary use in this code is to specify what information should be returned by WSALookupServiceNext
            [Flags]
            internal enum WsaNspControlFlags
            {
                Deep = 0x0001,
                Containers = 0x0002,
                NoContainers = 0x0004,
                Nearest = 0x0008,
                ReturnName = 0x0010,
                ReturnType = 0x0020,
                ReturnVersion = 0x0040,
                ReturnComment = 0x0080,
                ReturnAddr = 0x0100,
                ReturnBlob = 0x0200,
                ReturnAliases = 0x0400,
                ReturnQueryString = 0x0800,
                ReturnAll = 0x0FF0,
                ResService = 0x8000,
                FlushCache = 0x1000,
                FlushPrevious = 0x2000,
            }

            internal enum WsaError
            {
                WSAEINVAL = 10022,
                WSAEFAULT = 10014,
                WSAENOMORE = 10102,
                WSA_E_NO_MORE = 10110,
                WSANO_DATA = 11004
            }

            // specifies the operation of WSASetService
            internal enum WsaSetServiceOp
            {
                Register = 0,
                Deregister,
                Delete
            }

            internal struct BlobSafe
            {
                public int cbSize;
                public CriticalAllocHandle pBlobData;
            }

            internal struct BlobNative
            {
                public int cbSize;
                public IntPtr pBlobData;
            }


            // PnrpResolver does not currently support any cloud except Global. If this needs to be changed, we will
            // need to be able to enumerate clouds.

            // managed equivalent of both PNRPCLOUDINFO and PNRP_CLOUD_ID
            // internal because it is exposed by PeerCloudEnumerator
            internal class CloudInfo
            {
                public string Name;
                public PnrpScope Scope;
                public uint ScopeId;
                public PnrpCloudState State;
                public PnrpCloudFlags Flags;
            }

            [Serializable]
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            internal struct CsAddrInfo
            {
                public IPEndPoint LocalAddr;
                public IPEndPoint RemoteAddr;
                public int iSocketType;
                public int iProtocol;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            internal class CsAddrInfoSafe : IDisposable
            {
                public SOCKET_ADDRESS_SAFE LocalAddr;
                public SOCKET_ADDRESS_SAFE RemoteAddr;
                public int iSocketType;
                public int iProtocol;
                bool disposed;

                public static CsAddrInfoSafe[] FromAddresses(CsAddrInfo[] addresses)
                {
                    CsAddrInfoSafe addr;
                    CsAddrInfoSafe[] result = null;
                    if (addresses == null || addresses.Length == 0)
                        return null;

                    result = new CsAddrInfoSafe[addresses.Length];
                    int i = 0;
                    foreach (CsAddrInfo info in addresses)
                    {
                        addr = new CsAddrInfoSafe();
                        addr.LocalAddr = SOCKET_ADDRESS_SAFE.SocketAddressFromIPEndPoint(info.LocalAddr);
                        addr.RemoteAddr = SOCKET_ADDRESS_SAFE.SocketAddressFromIPEndPoint(info.RemoteAddr);
                        addr.iProtocol = info.iProtocol;
                        addr.iSocketType = info.iSocketType;
                        result[i++] = addr;
                    }
                    return result;
                }
                public static void StructureToPtr(CsAddrInfoSafe input, IntPtr target)
                {
                    CsAddrInfoNative native;
                    native.iProtocol = input.iProtocol;
                    native.iSocketType = input.iSocketType;
                    native.LocalAddr.iSockaddrLength = input.LocalAddr.iSockaddrLength;
                    native.LocalAddr.lpSockAddr = input.LocalAddr.lpSockAddr;
                    native.RemoteAddr.iSockaddrLength = input.RemoteAddr.iSockaddrLength;
                    native.RemoteAddr.lpSockAddr = input.RemoteAddr.lpSockAddr;

                    Marshal.StructureToPtr(native, target, false);
                }
                ~CsAddrInfoSafe()
                {
                    Dispose(false);
                }
                public virtual void Dispose()
                {
                    Dispose(true);
                    GC.SuppressFinalize(this);
                }
                void Dispose(bool disposing)
                {
                    if (disposed)
                    {
                        if (disposing)
                        {
                            LocalAddr.Dispose();
                            RemoteAddr.Dispose();
                        }
                    }
                    disposed = true;
                }

            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            internal struct CsAddrInfoNative
            {
                public SOCKET_ADDRESS_NATIVE LocalAddr;
                public SOCKET_ADDRESS_NATIVE RemoteAddr;
                public int iSocketType;
                public int iProtocol;
            }

            [Serializable]
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            internal struct PnrpCloudId
            {
                public int AddressFamily; // should be AF_INET6
                public PnrpScope Scope;     // Global, site, or link
                public uint ScopeId;       // specifies interface

            }

            internal struct PnrpCloudInfo
            {
                public int dwSize;            // size of this struct
                public PnrpCloudId Cloud;       // network cloud information
                public PnrpCloudState dwCloudState; // state of cloud
                public PnrpCloudFlags Flags;
            }

            //native equivalent for easy marshalling. 
            //should be exactly like PnrpInfo except CriticalHandles
            internal struct PnrpInfoNative
            {
                public int dwSize;            // size of this struct
                public string lpwszIdentity;  // identity name string
                public int nMaxResolve;       // number of desired resolutions
                public int dwTimeout;         // time in seconds to wait for responses
                public int dwLifetime;        // time in seconds for validity
                public PnrpResolveCriteria enResolveCriteria; // criteria for resolve matches
                public int dwFlags;           // set of flags
                public SOCKET_ADDRESS_NATIVE saHint; // IPv6 addr use for location
                public PnrpRegisteredIdState enNameState; // state of registered name
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct PnrpInfo
            {
                public int dwSize;            // size of this struct
                public string lpwszIdentity;  // identity name string
                public int nMaxResolve;       // number of desired resolutions
                public int dwTimeout;         // time in seconds to wait for responses
                public int dwLifetime;        // time in seconds for validity
                public PnrpResolveCriteria enResolveCriteria; // criteria for resolve matches
                public int dwFlags;           // set of flags
                public SOCKET_ADDRESS_SAFE saHint; // IPv6 addr use for location
                public PnrpRegisteredIdState enNameState; // state of registered name
                public static void ToPnrpInfoNative(PnrpInfo source, ref PnrpInfoNative target)
                {
                    target.dwSize = source.dwSize;
                    target.lpwszIdentity = source.lpwszIdentity;
                    target.nMaxResolve = source.nMaxResolve;
                    target.dwTimeout = source.dwTimeout;
                    target.dwLifetime = source.dwLifetime;
                    target.enResolveCriteria = source.enResolveCriteria;
                    target.dwFlags = source.dwFlags;
                    if (source.saHint != null)
                    {
                        target.saHint.lpSockAddr = source.saHint.lpSockAddr;
                        target.saHint.iSockaddrLength = source.saHint.iSockaddrLength;
                    }
                    else
                    {
                        target.saHint.lpSockAddr = IntPtr.Zero;
                        target.saHint.iSockaddrLength = 0;
                    }
                    target.enNameState = source.enNameState;
                }
            }

            [Serializable]
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            internal struct sockaddr_in
            {
                public short sin_family;
                public ushort sin_port;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public byte[] sin_addr;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
                public byte[] sin_zero;
            }

            [Serializable]
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            internal struct sockaddr_in6
            {
                public short sin6_family;
                public ushort sin6_port;
                public uint sin6_flowinfo;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
                public byte[] sin6_addr;
                public uint sin6_scope_id;
            }

            internal class SOCKET_ADDRESS_SAFE : IDisposable
            {
                public CriticalAllocHandle lpSockAddr;
                public int iSockaddrLength;
                bool disposed;
                public static SOCKET_ADDRESS_SAFE SocketAddressFromIPEndPoint(IPEndPoint endpoint)
                {
                    SOCKET_ADDRESS_SAFE socketAddress = new SOCKET_ADDRESS_SAFE();
                    if (endpoint == null)
                        return socketAddress;

                    if (endpoint.AddressFamily == AddressFamily.InterNetwork)
                    {
                        socketAddress.iSockaddrLength = Marshal.SizeOf(typeof(sockaddr_in));
                        socketAddress.lpSockAddr = CriticalAllocHandle.FromSize(socketAddress.iSockaddrLength);
                        sockaddr_in sa = new sockaddr_in();
                        sa.sin_family = (short)AddressFamily.InterNetwork;
                        sa.sin_port = (ushort)endpoint.Port;
                        sa.sin_addr = endpoint.Address.GetAddressBytes();
                        Marshal.StructureToPtr(sa, (IntPtr)socketAddress.lpSockAddr, false);
                    }
                    else if (endpoint.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        socketAddress.iSockaddrLength = Marshal.SizeOf(typeof(sockaddr_in6));
                        socketAddress.lpSockAddr = CriticalAllocHandle.FromSize(socketAddress.iSockaddrLength);
                        sockaddr_in6 sa = new sockaddr_in6();
                        sa.sin6_family = (short)AddressFamily.InterNetworkV6;
                        sa.sin6_port = (ushort)endpoint.Port;
                        sa.sin6_addr = endpoint.Address.GetAddressBytes();
                        sa.sin6_scope_id = (uint)endpoint.Address.ScopeId;
                        Marshal.StructureToPtr(sa, (IntPtr)socketAddress.lpSockAddr, false);
                    }
                    return socketAddress;
                }

                ~SOCKET_ADDRESS_SAFE()
                {
                    Dispose(false);
                }

                public virtual void Dispose()
                {
                    Dispose(true);
                    GC.SuppressFinalize(this);
                }

                void Dispose(bool disposing)
                {
                    if (!disposed)
                    {
                        if (disposing)
                            lpSockAddr.Dispose();
                    }
                    disposed = true;
                }

            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            internal struct SOCKET_ADDRESS_NATIVE
            {
                public IntPtr lpSockAddr;
                public int iSockaddrLength;
            }

            [Serializable]
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            internal struct WsaData
            {
                public Int16 wVersion;
                public Int16 wHighVersion;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
                public string szDescription;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
                public string szSystemStatus;

                public Int16 iMaxSockets;
                public Int16 iMaxUdpDg;
                public IntPtr lpVendorInfo;
            }

            [Serializable]
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct WsaNamespaceInfo
            {
                public Guid NSProviderId;
                public int dwNameSpace;
                public int fActive;
                public int dwVersion;
                // don't bother marshalling this as a string since we don't need to look at it
                public IntPtr lpszIdentifier;
            }

            // managed equivalent of WSAQUERYSET
            internal class WsaQuerySet
            {
                public string ServiceInstanceName;
                public Guid ServiceClassId;
                public string Comment;
                public NspNamespaces NameSpace;
                public Guid NSProviderId;
                public string Context;
                public CsAddrInfo[] CsAddrInfos;
                public object Blob;
                static public WsaQuerySetSafe ToWsaQuerySetSafe(WsaQuerySet input)
                {

                    WsaQuerySetSafe result = new WsaQuerySetSafe();
                    if (input == null)
                        return result;

                    result.dwSize = Marshal.SizeOf(typeof(WsaQuerySetNative));
                    result.lpszServiceInstanceName = CriticalAllocHandleString.FromString(input.ServiceInstanceName);
                    result.lpServiceClassId = CriticalAllocHandleGuid.FromGuid(input.ServiceClassId);
                    result.lpszComment = CriticalAllocHandleString.FromString(input.Comment);
                    result.dwNameSpace = input.NameSpace;
                    result.lpNSProviderId = CriticalAllocHandleGuid.FromGuid(input.NSProviderId);
                    result.lpszContext = CriticalAllocHandleString.FromString(input.Context);
                    result.dwNumberOfProtocols = 0;
                    result.lpafpProtocols = IntPtr.Zero; // not used
                    result.lpszQueryString = IntPtr.Zero;

                    if (input.CsAddrInfos != null)
                    {
                        result.dwNumberOfCsAddrs = input.CsAddrInfos.Length;
                        result.addressList = CsAddrInfoSafe.FromAddresses(input.CsAddrInfos);
                    }
                    result.dwOutputFlags = 0;
                    result.lpBlob = CriticalAllocHandlePnrpBlob.FromPnrpBlob(input.Blob);

                    return result;
                }
            }

            internal class CriticalAllocHandlePnrpBlob : CriticalAllocHandle
            {
                public static CriticalAllocHandle FromPnrpBlob(object input)
                {
                    BlobSafe blob = new BlobSafe();
                    if (input != null)
                    {
                        if (input.GetType() == typeof(PnrpInfo))
                        {
                            int blobSize = Marshal.SizeOf(typeof(PnrpInfoNative));
                            blob.pBlobData = CriticalAllocHandle.FromSize(blobSize + Marshal.SizeOf(typeof(BlobNative)));

                            //write the BlobSafe fields first,
                            BlobNative nativeBlob;
                            nativeBlob.cbSize = blobSize;
                            nativeBlob.pBlobData = (IntPtr)(((IntPtr)blob.pBlobData).ToInt64() + Marshal.SizeOf(typeof(BlobNative)));
                            Marshal.StructureToPtr(nativeBlob, (IntPtr)blob.pBlobData, false);
                            PnrpInfo pnrpInfo = (PnrpInfo)input;
                            pnrpInfo.dwSize = blobSize;
                            PnrpInfoNative nativeInfo = new PnrpInfoNative();
                            PnrpInfo.ToPnrpInfoNative(pnrpInfo, ref nativeInfo);
                            Marshal.StructureToPtr(nativeInfo, (IntPtr)nativeBlob.pBlobData, false);
                            blob.cbSize = blobSize;
                        }
                        else if (input.GetType() == typeof(PnrpCloudInfo))
                        {
                            int blobSize = Marshal.SizeOf(input.GetType());
                            blob.pBlobData = CriticalAllocHandle.FromSize(blobSize + Marshal.SizeOf(typeof(BlobNative)));

                            //write the BlobSafe fields first,
                            BlobNative nativeBlob;
                            nativeBlob.cbSize = blobSize;
                            nativeBlob.pBlobData = (IntPtr)(((IntPtr)blob.pBlobData).ToInt64() + Marshal.SizeOf(typeof(BlobNative)));
                            Marshal.StructureToPtr(nativeBlob, (IntPtr)blob.pBlobData, false);
                            PnrpCloudInfo cloudInfo = (PnrpCloudInfo)input;
                            cloudInfo.dwSize = Marshal.SizeOf(typeof(PnrpCloudInfo));
                            Marshal.StructureToPtr(cloudInfo, (IntPtr)nativeBlob.pBlobData, false);
                            blob.cbSize = blobSize;
                        }
                        else
                        {
                            throw Fx.AssertAndThrow("Unknown payload type!");
                        }
                    }
                    return blob.pBlobData;

                }
            }

            internal class CriticalAllocHandleString : CriticalAllocHandle
            {
                public static CriticalAllocHandle FromString(string input)
                {
                    CriticalAllocHandleString result = new CriticalAllocHandleString();
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try { }
                    finally
                    {
                        result.SetHandle(Marshal.StringToHGlobalUni(input));
                    }
                    return result;
                }
            }

            internal class CriticalAllocHandleWsaQuerySetSafe : CriticalAllocHandle
            {
                static int CalculateSize(WsaQuerySetSafe safeQuerySet)
                {
                    int structSize = Marshal.SizeOf(typeof(WsaQuerySetNative));
                    if (safeQuerySet.addressList != null)
                        structSize += safeQuerySet.addressList.Length * Marshal.SizeOf(typeof(CsAddrInfoNative));
                    return structSize;
                }

                public static CriticalAllocHandle FromWsaQuerySetSafe(WsaQuerySetSafe safeQuerySet)
                {
                    CriticalAllocHandle result = CriticalAllocHandle.FromSize(CalculateSize(safeQuerySet));
                    WsaQuerySetSafe.StructureToPtr(safeQuerySet, (IntPtr)result);
                    return result;
                }
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal class WsaQuerySetSafe : IDisposable
            {
                public int dwSize;
                public CriticalAllocHandle lpszServiceInstanceName;
                public CriticalAllocHandle lpServiceClassId;
                public IntPtr lpVersion; // not used
                public CriticalAllocHandle lpszComment;
                public NspNamespaces dwNameSpace;
                public CriticalAllocHandle lpNSProviderId;
                public CriticalAllocHandle lpszContext;
                public int dwNumberOfProtocols; // 0
                public IntPtr lpafpProtocols; // not used
                public IntPtr lpszQueryString; // not used
                public int dwNumberOfCsAddrs;
                public CsAddrInfoSafe[] addressList;
                public int dwOutputFlags; // 0
                public CriticalAllocHandle lpBlob;
                bool disposed;

                ~WsaQuerySetSafe()
                {
                    Dispose(false);
                }

                public virtual void Dispose()
                {
                    Dispose(true);
                    GC.SuppressFinalize(this);
                }

                void Dispose(bool disposing)
                {
                    if (!disposed)
                    {
                        if (disposing)
                        {
                            if (lpszServiceInstanceName != null)
                                lpszServiceInstanceName.Dispose();
                            if (lpServiceClassId != null)
                                lpServiceClassId.Dispose();
                            if (lpszComment != null)
                                lpszComment.Dispose();
                            if (lpNSProviderId != null)
                                lpNSProviderId.Dispose();
                            if (lpBlob != null)
                                lpBlob.Dispose();
                            if (addressList != null)
                            {
                                foreach (CsAddrInfoSafe addr in addressList)
                                {
                                    addr.Dispose();
                                }
                            }
                        }
                    }
                    disposed = true;
                }

                static public void StructureToPtr(WsaQuerySetSafe input, IntPtr target)
                {
                    WsaQuerySetNative native = new WsaQuerySetNative();
                    native.dwSize = input.dwSize;
                    native.lpszServiceInstanceName = input.lpszServiceInstanceName;
                    native.lpServiceClassId = input.lpServiceClassId;
                    native.lpVersion = IntPtr.Zero; // not used
                    native.lpszComment = input.lpszComment;
                    native.dwNameSpace = input.dwNameSpace;
                    native.lpNSProviderId = input.lpNSProviderId;
                    native.lpszContext = input.lpszContext;
                    native.dwNumberOfProtocols = 0; // 0
                    native.lpafpProtocols = IntPtr.Zero; // not used
                    native.lpszQueryString = IntPtr.Zero; // not used
                    native.dwNumberOfCsAddrs = input.dwNumberOfCsAddrs;
                    native.dwOutputFlags = 0; // 0
                    native.lpBlob = input.lpBlob;

                    Int64 sockAddressStart = target.ToInt64() + Marshal.SizeOf(typeof(WsaQuerySetNative));
                    native.lpcsaBuffer = (IntPtr)sockAddressStart;

                    Marshal.StructureToPtr(native, target, false);
                    MarshalSafeAddressesToNative(input, (IntPtr)sockAddressStart);

                }

                public static void MarshalSafeAddressesToNative(WsaQuerySetSafe safeQuery, IntPtr target)
                {
                    // marshal the addresses
                    if (safeQuery.addressList != null && safeQuery.addressList.Length > 0)
                    {
                        int sizeOfCsAddrInfo = Marshal.SizeOf(typeof(CsAddrInfoNative));
                        Int64 start = target.ToInt64();
                        Fx.Assert(start % IntPtr.Size == 0, "Invalid alignment!!");
                        foreach (CsAddrInfoSafe safeAddress in safeQuery.addressList)
                        {
                            CsAddrInfoSafe.StructureToPtr(safeAddress, (IntPtr)start);
                            start += sizeOfCsAddrInfo;
                        }
                    }
                }

            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct WsaQuerySetNative
            {
                public int dwSize;
                public IntPtr lpszServiceInstanceName;
                public IntPtr lpServiceClassId;
                public IntPtr lpVersion; // not used
                public IntPtr lpszComment;
                public NspNamespaces dwNameSpace;
                public IntPtr lpNSProviderId;
                public IntPtr lpszContext;
                public int dwNumberOfProtocols; // 0
                public IntPtr lpafpProtocols; // not used
                public IntPtr lpszQueryString; // not used
                public int dwNumberOfCsAddrs;
                public IntPtr lpcsaBuffer;
                public int dwOutputFlags; // 0
                public IntPtr lpBlob;
            }

            internal class CriticalLookupHandle : CriticalHandleZeroOrMinusOneIsInvalid
            {
                protected override bool ReleaseHandle()
                {
                    return WSALookupServiceEnd(handle) == 0;
                }
            }

            // base class for ref-counting WSA uses and calling WSAStartup/WSAShutdown
            internal class DiscoveryBase : MarshalByRefObject, IDisposable
            {
                static int refCount = 0;
                static object refCountLock = new object();
                bool disposed;

                public DiscoveryBase()
                {
                    lock (refCountLock)
                    {
                        if (refCount == 0)
                        {
                            WsaData WinsockVersion = new WsaData();
                            int ret = WSAStartup(UnsafePnrpNativeMethods.RequiredWinsockVersion, ref WinsockVersion);
                            if (ret != 0)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SocketException(ret));
                            }
                        }
                        refCount++;
                    }
                }

                public void Dispose()
                {
                    Dispose(true);
                    GC.SuppressFinalize(this);
                }

                public void Dispose(bool disposing)
                {
                    if (!disposed)
                    {
                        lock (refCountLock)
                        {
                            refCount--;
                            if (refCount == 0)
                            {
                                WSACleanup();
                            }
                        }
                    }
                    disposed = true;
                }

                ~DiscoveryBase()
                {
                    this.Dispose(false);
                }

                [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "ServiceController has demands for ServiceControllerPermission.")]
                public bool IsPnrpServiceRunning(TimeSpan waitForService)
                {
                    TimeoutHelper timeoutHelper = new TimeoutHelper(waitForService);
                    try
                    {
                        using (ServiceController sc = new ServiceController("pnrpsvc"))
                        {
                            try
                            {
                                if (sc.Status == ServiceControllerStatus.StopPending)
                                {
                                    sc.WaitForStatus(ServiceControllerStatus.Stopped, timeoutHelper.RemainingTime());
                                }
                                if (sc.Status == ServiceControllerStatus.Stopped)
                                {
                                    sc.Start();
                                }
                                sc.WaitForStatus(ServiceControllerStatus.Running, timeoutHelper.RemainingTime());
                            }
                            catch (Exception e)
                            {
                                if (Fx.IsFatal(e)) throw;
                                if (e is InvalidOperationException || e is TimeoutException)
                                    return false;
                                else
                                    throw;
                            }
                            return (sc.Status == ServiceControllerStatus.Running);
                        }
                    }
                    catch (InvalidOperationException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        Fx.Assert("IsPnrpServiceRunning should be called after IsPnrpInstalled");
                        return false;
                    }
                }

                public bool IsPnrpAvailable(TimeSpan waitForService)
                {
                    if (!IsPnrpInstalled())
                        return false;

                    //make sure that the service is running
                    if (!IsPnrpServiceRunning(waitForService))
                        return false;
                    // If PNRP is installed, ensure that it supports extended payload by attempting to register with
                    // an invalid query set. If extended payload is not available, "WSASERVICE_NOT_FOUND" is returned.
                    // Otherwise, "WSAEINVAL" is returned.

                    //UPDATE: we will work with PNRP 1.0 if it is available.
                    // a separate implementation will work with payload support when available.
                    WsaQuerySet querySet = new WsaQuerySet();
                    querySet.NSProviderId = NsProviderName;
                    querySet.ServiceClassId = SvcIdNameV1;
                    int res = InvokeService(querySet, WsaSetServiceOp.Register, 0);

                    //on xp 64bit, WSANO_DATA is returned
                    if (res == (int)WsaError.WSAEINVAL || res == (int)WsaError.WSANO_DATA)
                        return true;

                    // if the call didn't fail or returned any other error, PNRP clearly isn't working properly
                    return false;

                }

                // determine if any version of PNRP is installed and available
                public bool IsPnrpInstalled()
                {
                    int size = 0;
                    int nProviders;
                    CriticalAllocHandle dataPtr = null;
                    // retrieve the list of installed namespace providers
                    // implemented in a loop in case the size changes between the first and second calls
                    while (true)
                    {
                        nProviders = WSAEnumNameSpaceProviders(ref size, (IntPtr)dataPtr);
                        if (nProviders != -1) // success
                            break;

                        int error = WSAGetLastError();
                        if (error != (int)WsaError.WSAEFAULT) // buffer length to small
                            return false; // any other error effectively means that PNRP isn't usable

                        dataPtr = CriticalAllocHandle.FromSize(size);
                    }

                    // loop through the providers
                    for (int i = 0; i < nProviders; i++)
                    {
                        IntPtr nsInfoPtr = (IntPtr)(((IntPtr)dataPtr).ToInt64() + i *
                            Marshal.SizeOf(typeof(WsaNamespaceInfo)));
                        WsaNamespaceInfo nsInfo = (WsaNamespaceInfo)Marshal.PtrToStructure(nsInfoPtr,
                            typeof(WsaNamespaceInfo));

                        // if this is the PNRP name namespace provider and it is active, it is installed
                        if (nsInfo.NSProviderId == NsProviderName && nsInfo.fActive != 0)
                            return true;
                    }

                    // no PNRP name namespace provider found
                    return false;
                }

                int InvokeService(WsaQuerySet registerQuery, WsaSetServiceOp op, int flags)
                {
                    WsaQuerySetSafe native = WsaQuerySet.ToWsaQuerySetSafe(registerQuery);
                    int error = 0;
                    using (native)
                    {
                        CriticalAllocHandle handle = CriticalAllocHandleWsaQuerySetSafe.FromWsaQuerySetSafe(native);
                        using (handle)
                        {
                            int retval = WSASetService(handle, op, flags);
                            if (retval != 0)
                            {
                                error = WSAGetLastError();
                            }
                        }
                    }
                    return error;
                }

            }

            public class PeerCloudEnumerator : DiscoveryBase
            {
                static public CloudInfo[] GetClouds()
                {
                    int retval = 0;
                    ArrayList clouds = new ArrayList();
                    WsaQuerySet querySet = new WsaQuerySet();
                    CriticalLookupHandle hLookup;

                    PnrpCloudInfo cloudInfo = new PnrpCloudInfo();
                    cloudInfo.dwSize = Marshal.SizeOf(typeof(PnrpCloudInfo));
                    cloudInfo.Cloud.Scope = PnrpScope.Any;
                    cloudInfo.dwCloudState = (PnrpCloudState)0;
                    cloudInfo.Flags = PnrpCloudFlags.None;
                    querySet.NameSpace = NspNamespaces.Cloud;
                    querySet.NSProviderId = NsProviderCloud;
                    querySet.ServiceClassId = SvcIdCloud;
                    querySet.Blob = cloudInfo;

                    WsaQuerySetSafe native = WsaQuerySet.ToWsaQuerySetSafe(querySet);
                    using (native)
                    {
                        CriticalAllocHandle handle = CriticalAllocHandleWsaQuerySetSafe.FromWsaQuerySetSafe(native);
                        retval = WSALookupServiceBegin(handle, WsaNspControlFlags.ReturnAll, out hLookup);
                    }
                    if (retval != 0)
                    {
                        // unable to start the enumeration
                        SocketException exception = new SocketException(WSAGetLastError());
                        Utility.CloseInvalidOutCriticalHandle(hLookup);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                    }

                    // start with a sensible default size
                    int size = Marshal.SizeOf(typeof(WsaQuerySetSafe)) + 200;
                    //wrap in CriticalAllocHandle when PAYLOAD is enabled
                    CriticalAllocHandle nativeQuerySetPtr = CriticalAllocHandle.FromSize(size);
                    using (hLookup)
                    {
                        while (true)
                        {
                            retval = WSALookupServiceNext(hLookup, 0, ref size, (IntPtr)nativeQuerySetPtr);
                            if (retval != 0)
                            {
                                int error = WSAGetLastError();
                                if (error == (int)WsaError.WSAENOMORE || error == (int)WsaError.WSA_E_NO_MORE)
                                {
                                    // no more
                                    break;
                                }
                                if (error == (int)WsaError.WSAEFAULT)
                                {
                                    // buffer too small, allocate a bigger one of the specified size
                                    if (nativeQuerySetPtr != null)
                                    {
                                        nativeQuerySetPtr.Dispose();
                                        nativeQuerySetPtr = null;
                                    }
                                    //wrap in CriticalAllocHandle when PAYLOAD is enabled
                                    nativeQuerySetPtr = CriticalAllocHandle.FromSize(size);
                                    continue;
                                }

                                // unexpected error
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SocketException(error));
                            }
                            else
                            {
                                if (nativeQuerySetPtr != IntPtr.Zero)
                                {
                                    // marshal the results into something usable
                                    WsaQuerySet resultQuerySet = PeerNameResolver.MarshalWsaQuerySetNativeToWsaQuerySet(nativeQuerySetPtr, 0);
                                    // extract out the friendly cloud attributes
                                    CloudInfo resultCloudInfo = new CloudInfo();
                                    PnrpCloudInfo prnpCloudInfo = (PnrpCloudInfo)resultQuerySet.Blob;
                                    resultCloudInfo.Name = resultQuerySet.ServiceInstanceName;
                                    resultCloudInfo.Scope = prnpCloudInfo.Cloud.Scope;
                                    resultCloudInfo.ScopeId = prnpCloudInfo.Cloud.ScopeId;
                                    resultCloudInfo.State = prnpCloudInfo.dwCloudState;
                                    resultCloudInfo.Flags = prnpCloudInfo.Flags;

                                    // add it to the list to return later
                                    clouds.Add(resultCloudInfo);
                                }
                            }
                        }
                    }

                    // package up the results into a nice array
                    return (CloudInfo[])clouds.ToArray(typeof(CloudInfo));
                }

            }

            internal class PeerNameRegistrar : DiscoveryBase
            {
                const int RegistrationLifetime = 60 * 60; // 1 hour

                public PeerNameRegistrar()
                    : base()
                {
                }

                public void Register(PnrpRegistration registration, TimeSpan timeout)
                {
                    // fill in the PnrpInfo blob using the defaults
                    PnrpInfo pnrpInfo = new PnrpInfo();
                    pnrpInfo.dwLifetime = RegistrationLifetime;
                    pnrpInfo.lpwszIdentity = null;
                    pnrpInfo.dwSize = Marshal.SizeOf(pnrpInfo);
                    pnrpInfo.dwFlags = PNRPINFO_HINT;
                    IPEndPoint hint = PnrpPeerResolver.GetHint();
                    pnrpInfo.saHint = SOCKET_ADDRESS_SAFE.SocketAddressFromIPEndPoint(hint);

                    // fill in the query set
                    WsaQuerySet registerQuery = new WsaQuerySet();
                    registerQuery.NameSpace = NspNamespaces.Name;
                    registerQuery.NSProviderId = NsProviderName;
                    registerQuery.ServiceClassId = SvcIdNameV1;
                    registerQuery.ServiceInstanceName = registration.PeerName;
                    registerQuery.Comment = registration.Comment;
                    registerQuery.Context = registration.CloudName;

                    // copy over the addresses
                    if (registration.Addresses != null)
                    {
                        Fx.Assert(registration.Addresses.Length <= 4, "Pnrp supports only 4 addresses");
                        registerQuery.CsAddrInfos = new CsAddrInfo[registration.Addresses.Length];
                        for (int i = 0; i < registration.Addresses.Length; i++)
                        {
                            // the only interesting part of the CsAddrInfo is the LocalAddress
                            registerQuery.CsAddrInfos[i].LocalAddr = registration.Addresses[i];
                            registerQuery.CsAddrInfos[i].iProtocol = (int)ProtocolType.Tcp;
                            registerQuery.CsAddrInfos[i].iSocketType = (int)SocketType.Stream;
                        }
                    }

                    // copy the blob
                    registerQuery.Blob = pnrpInfo;
                    RegisterService(registerQuery);
                }

                public void Unregister(string peerName, List<string> clouds, TimeSpan timeout)
                {
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    foreach (string cloud in clouds)
                    {
                        try
                        {
                            Unregister(peerName, cloud, timeoutHelper.RemainingTime());
                        }
                        catch (SocketException e)
                        {
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        }
                    }
                }

                public void Unregister(string peerName, string cloudName, TimeSpan timeout)
                {
                    // fill in the PnrpInfo with defaults
                    PnrpInfo identityInfo = new PnrpInfo();
                    identityInfo.lpwszIdentity = null;
                    identityInfo.dwSize = Marshal.SizeOf(typeof(PnrpInfo));

                    // fill in the query set
                    WsaQuerySet registerQuery = new WsaQuerySet();
                    registerQuery.NameSpace = NspNamespaces.Name;
                    registerQuery.NSProviderId = NsProviderName;
                    registerQuery.ServiceClassId = SvcIdNameV1;
                    registerQuery.ServiceInstanceName = peerName;
                    registerQuery.Context = cloudName;
                    registerQuery.Blob = identityInfo;

                    DeleteService(registerQuery);
                }

                void RegisterService(WsaQuerySet registerQuery)
                {
                    try
                    {
                        InvokeService(registerQuery, WsaSetServiceOp.Register, 0);
                    }
                    catch (PnrpException)
                    {
                        if (PnrpPeerResolver.MaxAddressEntriesV1 < registerQuery.CsAddrInfos.Length)
                        {
                            List<CsAddrInfo> infos = new List<CsAddrInfo>(registerQuery.CsAddrInfos);
                            infos.RemoveRange(PnrpPeerResolver.MaxAddressEntriesV1, registerQuery.CsAddrInfos.Length - PnrpPeerResolver.MaxAddressEntriesV1);
                            registerQuery.CsAddrInfos = infos.ToArray();
                            InvokeService(registerQuery, WsaSetServiceOp.Register, 0);
                        }
                        else
                            throw;
                    }
                }

                void DeleteService(WsaQuerySet registerQuery)
                {
                    InvokeService(registerQuery, WsaSetServiceOp.Delete, 0);
                }

                static void InvokeService(WsaQuerySet registerQuery, WsaSetServiceOp op, int flags)
                {
                    WsaQuerySetSafe native = WsaQuerySet.ToWsaQuerySetSafe(registerQuery);
                    using (native)
                    {
                        CriticalAllocHandle handle = CriticalAllocHandleWsaQuerySetSafe.FromWsaQuerySetSafe(native);
                        int retval = WSASetService(handle, op, flags);
                        if (retval != 0)
                        {
                            int error = WSAGetLastError();
                            PeerExceptionHelper.ThrowPnrpError(error, registerQuery.Context);
                        }
                    }
                }
            }

            internal class PeerNameResolver : AsyncResult
            {
                WsaQuerySet resolveQuery;
                List<PnrpRegistration> results;
                uint scopeId;
                Exception lastException;
                TimeoutHelper timeoutHelper;

                public PeerNameResolver(string peerName, int numberOfResultsRequested,
                    PnrpResolveCriteria resolveCriteria, TimeSpan timeout, List<PnrpRegistration> results)
                    : this(peerName, numberOfResultsRequested, resolveCriteria, 0, GlobalCloudName, timeout, results)
                {
                }

                public PeerNameResolver(string peerName, int numberOfResultsRequested,
                    PnrpResolveCriteria resolveCriteria, uint scopeId, string cloudName, TimeSpan timeout, List<PnrpRegistration> results)
                    : base(null, null)
                {
                    // pnrp has a hard-coded limit on the timeout value that can be passed to it
                    // maximum value is 10 minutes
                    if (timeout > MaxTimeout)
                    {
                        timeout = MaxTimeout;
                    }
                    timeoutHelper = new TimeoutHelper(timeout);
                    PnrpInfo resolveQueryInfo = new PnrpInfo();
                    resolveQueryInfo.dwSize = Marshal.SizeOf(typeof(PnrpInfo));
                    resolveQueryInfo.nMaxResolve = numberOfResultsRequested;
                    resolveQueryInfo.dwTimeout = (int)timeout.TotalSeconds;
                    resolveQueryInfo.dwLifetime = 0;
                    resolveQueryInfo.enNameState = 0;
                    resolveQueryInfo.lpwszIdentity = null;
                    resolveQueryInfo.dwFlags = PNRPINFO_HINT;
                    IPEndPoint hint = PnrpPeerResolver.GetHint();
                    resolveQueryInfo.enResolveCriteria = resolveCriteria;
                    resolveQueryInfo.saHint = SOCKET_ADDRESS_SAFE.SocketAddressFromIPEndPoint(hint);
                    resolveQuery = new WsaQuerySet();
                    resolveQuery.ServiceInstanceName = peerName;
                    resolveQuery.ServiceClassId = SvcIdNameV1;
                    resolveQuery.NameSpace = NspNamespaces.Name;
                    resolveQuery.NSProviderId = NsProviderName;
                    resolveQuery.Context = cloudName;
                    resolveQuery.Blob = resolveQueryInfo;
                    this.results = results;
                    this.scopeId = scopeId;
                    ActionItem.Schedule(new Action<object>(SyncEnumeration), null);
                }

                public void End()
                {
                    AsyncResult.End<PeerNameResolver>(this);
                }

                public void SyncEnumeration(object state)
                {
                    int retval = 0;
                    CriticalLookupHandle hLookup;
                    WsaQuerySetSafe native = WsaQuerySet.ToWsaQuerySetSafe(resolveQuery);
                    using (native)
                    {
                        CriticalAllocHandle handle = CriticalAllocHandleWsaQuerySetSafe.FromWsaQuerySetSafe(native);
                        retval = WSALookupServiceBegin(handle, WsaNspControlFlags.ReturnAll, out hLookup);
                    }
                    if (retval != 0)
                    {
                        lastException = new PnrpException(WSAGetLastError(), resolveQuery.Context);
                        Utility.CloseInvalidOutCriticalHandle(hLookup);
                        Complete(false, lastException);
                        return;
                    }
                    WsaQuerySet querySet = new WsaQuerySet();

                    // start with a sensible default size
                    int size = Marshal.SizeOf(typeof(WsaQuerySetSafe)) + 400;
                    CriticalAllocHandle nativeQuerySetPtr = CriticalAllocHandle.FromSize(size);
                    try
                    {
                        using (hLookup)
                        {
                            while (true)
                            {
                                if (timeoutHelper.RemainingTime() == TimeSpan.Zero)
                                {
                                    break;
                                }
                                retval = WSALookupServiceNext(hLookup, 0, ref size, (IntPtr)nativeQuerySetPtr);
                                if (retval != 0)
                                {
                                    int error = WSAGetLastError();
                                    if (error == (int)WsaError.WSAENOMORE || error == (int)WsaError.WSA_E_NO_MORE)
                                    {
                                        // no more
                                        break;
                                    }

                                    if (error == (int)WsaError.WSAEFAULT)
                                    {
                                        nativeQuerySetPtr = CriticalAllocHandle.FromSize(size);
                                        continue;
                                    }

                                    // unexpected error
                                    PeerExceptionHelper.ThrowPnrpError(error, querySet.Context);
                                }
                                else
                                {
                                    if (nativeQuerySetPtr != IntPtr.Zero)
                                    {
                                        // marshal the results into something useful
                                        querySet = MarshalWsaQuerySetNativeToWsaQuerySet(nativeQuerySetPtr, scopeId);

                                        // allocate the friendly PnrpRegistration and fill it in
                                        PnrpRegistration pnrpRegistration = new PnrpRegistration();
                                        pnrpRegistration.CloudName = querySet.Context;
                                        pnrpRegistration.Comment = querySet.Comment;
                                        pnrpRegistration.PeerName = querySet.ServiceInstanceName;
                                        pnrpRegistration.Addresses = new IPEndPoint[querySet.CsAddrInfos.Length];
                                        for (int i = 0; i < querySet.CsAddrInfos.Length; i++)
                                            pnrpRegistration.Addresses[i] = querySet.CsAddrInfos[i].LocalAddr;

                                        // add it to the list to return later.
                                        // all cloud enumeratos in the same scope will reference the same list and hence the lock.
                                        lock (results)
                                        {
                                            results.Add(pnrpRegistration);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            PnrpResolveExceptionTraceRecord record = new PnrpResolveExceptionTraceRecord(resolveQuery.ServiceInstanceName, resolveQuery.Context, e);
                            if (DiagnosticUtility.ShouldTraceError)
                            {
                                TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.PnrpResolveException,
                                    SR.GetString(SR.TraceCodePnrpResolveException), record, this, null);
                            }
                        }
                        lastException = e;
                    }
                    finally
                    {
                        Complete(false, lastException);
                    }
                }

                static internal WsaQuerySet MarshalWsaQuerySetNativeToWsaQuerySet(IntPtr pNativeData)
                {
                    return MarshalWsaQuerySetNativeToWsaQuerySet(pNativeData, 0);
                }

                static internal WsaQuerySet MarshalWsaQuerySetNativeToWsaQuerySet(IntPtr pNativeData, uint scopeId)
                {
                    if (pNativeData == IntPtr.Zero)
                        return null;

                    WsaQuerySet querySet = new WsaQuerySet();
                    // build a native structure from the raw memory
                    WsaQuerySetNative nativeQuerySet;
                    nativeQuerySet = (WsaQuerySetNative)Marshal.PtrToStructure(pNativeData,
                        typeof(WsaQuerySetNative));
                    CsAddrInfoNative nativeCsAddrInfo;
                    int sizeOfCsAddrInfo = Marshal.SizeOf(typeof(CsAddrInfoNative));

                    // copy over the simple fields
                    querySet.Context = Marshal.PtrToStringUni(nativeQuerySet.lpszContext);
                    querySet.NameSpace = nativeQuerySet.dwNameSpace;
                    querySet.ServiceInstanceName = Marshal.PtrToStringUni(nativeQuerySet.lpszServiceInstanceName);
                    querySet.Comment = Marshal.PtrToStringUni(nativeQuerySet.lpszComment);

                    // copy the addresses
                    querySet.CsAddrInfos = new CsAddrInfo[nativeQuerySet.dwNumberOfCsAddrs];
                    for (int i = 0; i < nativeQuerySet.dwNumberOfCsAddrs; i++)
                    {
                        IntPtr addressPtr = (IntPtr)(nativeQuerySet.lpcsaBuffer.ToInt64() + (i * sizeOfCsAddrInfo));
                        nativeCsAddrInfo = (CsAddrInfoNative)Marshal.PtrToStructure(addressPtr,
                            typeof(CsAddrInfoNative));
                        querySet.CsAddrInfos[i].iProtocol = nativeCsAddrInfo.iProtocol;
                        querySet.CsAddrInfos[i].iSocketType = nativeCsAddrInfo.iSocketType;
                        querySet.CsAddrInfos[i].LocalAddr = IPEndPointFromSocketAddress(nativeCsAddrInfo.LocalAddr, scopeId);
                        querySet.CsAddrInfos[i].RemoteAddr = IPEndPointFromSocketAddress(nativeCsAddrInfo.RemoteAddr, scopeId);
                    }

                    // copy the GUIDs
                    if (nativeQuerySet.lpNSProviderId != IntPtr.Zero)
                        querySet.NSProviderId = (Guid)Marshal.PtrToStructure(nativeQuerySet.lpNSProviderId,
                            typeof(Guid));

                    if (nativeQuerySet.lpServiceClassId != IntPtr.Zero)
                        querySet.ServiceClassId = (Guid)Marshal.PtrToStructure(nativeQuerySet.lpServiceClassId,
                            typeof(Guid));

                    // marshal the BLOB according to namespace
                    if (querySet.NameSpace == NspNamespaces.Cloud)
                    {
                        if (nativeQuerySet.lpBlob != IntPtr.Zero)
                        {
                            // give it a default value
                            querySet.Blob = new PnrpCloudInfo();
                            // marshal the blob in order to get the pointer
                            BlobNative blob = (BlobNative)Marshal.PtrToStructure(nativeQuerySet.lpBlob, typeof(BlobNative));
                            // marshal the actual PnrpCloudInfo
                            if (blob.pBlobData != IntPtr.Zero)
                                querySet.Blob = (PnrpCloudInfo)Marshal.PtrToStructure(blob.pBlobData,
                                    typeof(PnrpCloudInfo));
                        }
                    }

                    else if (querySet.NameSpace == NspNamespaces.Name)
                    {
                        if (nativeQuerySet.lpBlob != IntPtr.Zero)
                        {
                            // give it a default value
                            querySet.Blob = new PnrpInfo();
                            // marshal the blob in order to get the pointer
                            BlobSafe blob = (BlobSafe)Marshal.PtrToStructure(nativeQuerySet.lpBlob, typeof(BlobSafe));
                            // marshal the actual PnrpInfo
                            if (blob.pBlobData != IntPtr.Zero)
                            {
                                PnrpInfo pnrpInfo = (PnrpInfo)Marshal.PtrToStructure(blob.pBlobData,
                                    typeof(PnrpInfo));
                                querySet.Blob = pnrpInfo;
                            }
                        }
                    }

                    return querySet;
                }

                static IPEndPoint IPEndPointFromSocketAddress(SOCKET_ADDRESS_NATIVE socketAddress, uint scopeId)
                {
                    IPEndPoint endPoint = null;
                    if (socketAddress.lpSockAddr != IntPtr.Zero)
                    {
                        AddressFamily addressFamily = (AddressFamily)Marshal.ReadInt16(socketAddress.lpSockAddr);
                        if (addressFamily == AddressFamily.InterNetwork)
                        {
                            // if the sockaddr length is not the sizeof(sockaddr_in), the data is invalid so
                            // return an null endpoint
                            if (socketAddress.iSockaddrLength == Marshal.SizeOf(typeof(sockaddr_in)))
                            {
                                sockaddr_in sa = (sockaddr_in)Marshal.PtrToStructure(socketAddress.lpSockAddr,
                                    typeof(sockaddr_in));
                                endPoint = new IPEndPoint(new IPAddress(sa.sin_addr), sa.sin_port);
                            }
                        }
                        else if (addressFamily == AddressFamily.InterNetworkV6)
                        {
                            // if the sockaddr length is not the sizeof(sockaddr_in6), the data is invalid so
                            // return an null endpoint
                            if (socketAddress.iSockaddrLength == Marshal.SizeOf(typeof(sockaddr_in6)))
                            {
                                sockaddr_in6 sa = (sockaddr_in6)Marshal.PtrToStructure(socketAddress.lpSockAddr,
                                    typeof(sockaddr_in6));
                                if (scopeId != 0 && sa.sin6_scope_id != 0)
                                    scopeId = sa.sin6_scope_id;
                                endPoint = new IPEndPoint(new IPAddress(sa.sin6_addr, scopeId), sa.sin6_port);
                            }
                        }
                        // else this is an unknown address family, so return null
                    }

                    return endPoint;
                }

            }
        }
        public override bool Equals(object other)
        {
            return ((other as PnrpPeerResolver) != null);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
