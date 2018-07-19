//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;

    class MsmqHostedTransportManager : TransportManager
    {
        string[] hosts;
        List<MsmqBindingMonitor> bindingMonitors;
        HostedBindingFilter filter;
        MsmqUri.IAddressTranslator addressing;
        Action messageReceivedCallback;

        public MsmqHostedTransportManager(string[] hosts, MsmqUri.IAddressTranslator addressing)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

            this.hosts = hosts;
            this.bindingMonitors = new List<MsmqBindingMonitor>();
            this.addressing = addressing;
            this.filter = new HostedBindingFilter(HostingEnvironment.ApplicationVirtualPath, addressing);

            foreach (string host in this.hosts)
            {
                MsmqBindingMonitor monitor = new MsmqBindingMonitor(host, TimeSpan.FromMinutes(5), true);
                monitor.AddFilter(this.filter);
                monitor.Open();
                this.bindingMonitors.Add(monitor);
            }
        }

        public Uri[] GetBaseAddresses(string virtualPath)
        {
            // Make sure this is not called until initialization is done:
            foreach (MsmqBindingMonitor monitor in this.bindingMonitors)
            {
                monitor.WaitForFirstRoundComplete();
            }

            string absoluteVirtualPath = VirtualPathUtility.ToAbsolute(virtualPath, HostingEnvironment.ApplicationVirtualPath);

            List<Uri> baseAddresses = new List<Uri>(this.hosts.Length);
            string queueName = absoluteVirtualPath.Substring(1);

            foreach (string host in this.hosts)
            {
                bool isPrivate = this.filter.IsPrivateMatch(queueName);

                Uri uri = this.addressing.CreateUri(host, queueName, isPrivate);
                baseAddresses.Add(uri);
                MsmqDiagnostics.FoundBaseAddress(uri, absoluteVirtualPath);
            }
            return baseAddresses.ToArray();
        }

        internal override string Scheme
        {
            get { return this.addressing.Scheme; }
        }

        internal override void OnClose(TimeSpan timeout)
        {
            foreach (MsmqBindingMonitor monitor in this.bindingMonitors)
            {
                monitor.Close();
            }

            this.bindingMonitors.Clear();
        }

        internal override void OnOpen()
        {
            // Nothing to do - we only use the transport manager for WebHosted case.
        }

        internal override void Register(TransportChannelListener channelListener)
        {
            channelListener.SetMessageReceivedCallback(new Action(OnMessageReceived));
        }

        internal void Start(Action messageReceivedCallback)
        {
            this.messageReceivedCallback = messageReceivedCallback;
        }

        internal override void Unregister(TransportChannelListener channelListener)
        {
            // Nothing to do - we never use the transport manager during normal
            // operation.
        }

        void OnMessageReceived()
        {
            Action callback = this.messageReceivedCallback;
            if (callback != null)
            {
                callback();
            }
        }

        class HostedBindingFilter : MsmqBindingFilter
        {
            Dictionary<string, string> privateMatches = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            object thisLock = new object();

            public HostedBindingFilter(string path, MsmqUri.IAddressTranslator addressing)
                : base(path, addressing)
            {
                AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
            }

            public override object MatchFound(string host, string name, bool isPrivate)
            {
                string processedVirtualPath = CreateRelativeVirtualPath(host, name, isPrivate);
                string relativeServiceFile = ServiceHostingEnvironment.NormalizeVirtualPath(processedVirtualPath);

                // Compute the remainder path:
                if (isPrivate)
                {
                    lock (this.thisLock)
                    {
                        string baseQueue = CreateBaseQueue(relativeServiceFile);
                        this.privateMatches[baseQueue] = baseQueue;
                    }
                }

                // Start the service on a different thread so we can complete 
                // initialization
                if (CheckServiceExists(relativeServiceFile))
                {
                    MsmqDiagnostics.StartingService(host, name, isPrivate, processedVirtualPath);
                    ActionItem.Schedule(StartService, processedVirtualPath);
                }

                // no callback state here...
                return null;
            }

            public bool IsPrivateMatch(string processedVirtualPath)
            {
                lock (this.thisLock)
                {
                    return this.privateMatches.ContainsKey(processedVirtualPath);
                }
            }

            public override void MatchLost(string host, string name, bool isPrivate, object callbackState)
            {
                // We don't do anything here - the service will stay alive,
                // and if the queue ever comes back, then it will begin to
                // process again.
            }

            string CreateRelativeVirtualPath(string host, string name, bool isPrivate)
            {
                // the canonical prefix looks something like: "invoices/"
                // Because the queue name matched, it looks like "invoices/..."
                // remove the common piece, and prefix with the "~/" home specifier
                return "~/" + name.Substring(CanonicalPrefix.Length);
            }

            string CreateBaseQueue(string serviceFile)
            {
                // Clean up the service file...
                if (serviceFile.StartsWith("~", StringComparison.OrdinalIgnoreCase))
                    serviceFile = serviceFile.Substring(1);
                if (serviceFile.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    serviceFile = serviceFile.Substring(1);

                string virtualPath = HostingEnvironment.ApplicationVirtualPath;
                if (virtualPath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    virtualPath = virtualPath.Substring(0, virtualPath.Length - 1);
                if (virtualPath.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    virtualPath = virtualPath.Substring(1);

                return virtualPath + "/" + serviceFile;
            }

            bool CheckServiceExists(string serviceFile)
            {
                try
                {
                    return ((ServiceHostingEnvironment.IsConfigurationBasedService(serviceFile) 
                             || HostingEnvironmentWrapper.ServiceFileExists(serviceFile)) 
                             && AspNetEnvironment.Current.IsWithinApp(VirtualPathUtility.ToAbsolute(serviceFile)));
                }
                catch (ArgumentException ex)
                {
                    MsmqDiagnostics.ExpectedException(ex);
                    return false;
                }
            }

            void StartService(object state)
            {
                try
                {
                    string processedVirtualPath = (string)state;
                    ServiceHostingEnvironment.EnsureServiceAvailable(processedVirtualPath);
                }
                catch (ServiceActivationException e)
                {
                    // Non-fatal exceptions from the user code are wrapped in ServiceActivationException
                    // The best we can do is to trace them
                    MsmqDiagnostics.ExpectedException(e);
                }
                catch (EndpointNotFoundException e)
                {
                    // This means that the server disappeared between the time we
                    // saw the service, and the time we tried to start it.
                    // That's okay.
                    MsmqDiagnostics.ExpectedException(e);
                }
            }
        }
    }
}
