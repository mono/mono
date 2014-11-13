//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.PeerResolvers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Threading;

    class PeerDefaultCustomResolverClient : PeerResolver
    {
        EndpointAddress address;
        Binding binding;
        TimeSpan defaultLifeTime;
        ClientCredentials credentials;
        Guid clientId;
        Guid registrationId;
        IOThreadTimer timer;
        bool opened = false;
        string meshId;
        PeerNodeAddress nodeAddress;
        ChannelFactory<IPeerResolverClient> channelFactory;
        PeerReferralPolicy referralPolicy;
        string bindingName, bindingConfigurationName;
        bool? shareReferrals;
        int updateSuccessful = 1;

        internal PeerDefaultCustomResolverClient()
        {
            this.address = null;
            this.binding = null;
            this.defaultLifeTime = TimeSpan.FromHours(1);
            clientId = Guid.NewGuid();
            timer = new IOThreadTimer(new Action<object>(RegistrationExpired), this, false);
        }

        public override bool CanShareReferrals
        {
            get
            {
                if (this.shareReferrals.HasValue)
                    return shareReferrals.Value;
                if (this.referralPolicy == PeerReferralPolicy.Service && opened)
                {
                    IPeerResolverClient proxy = GetProxy();
                    try
                    {
                        ServiceSettingsResponseInfo settings = proxy.GetServiceSettings();
                        shareReferrals = !settings.ControlMeshShape;
                        proxy.Close();
                    }
                    finally
                    {
                        proxy.Abort();
                    }
                }
                else
                {
                    shareReferrals = (PeerReferralPolicy.Share == this.referralPolicy);
                }

                return shareReferrals.Value;
            }
        }

        public override void Initialize(EndpointAddress address, Binding binding, ClientCredentials credentials, PeerReferralPolicy referralPolicy)
        {
            this.address = address;
            this.binding = binding;
            this.credentials = credentials;
            Validate();
            channelFactory = new ChannelFactory<IPeerResolverClient>(binding, address);
            channelFactory.Endpoint.Behaviors.Remove<ClientCredentials>();
            if (credentials != null)
                channelFactory.Endpoint.Behaviors.Add(credentials);
            channelFactory.Open();
            this.referralPolicy = referralPolicy;
            opened = true;
        }

        IPeerResolverClient GetProxy()
        {
            return (IPeerResolverClient)channelFactory.CreateChannel();
        }


        void Validate()
        {
            if (address == null || binding == null)
                PeerExceptionHelper.ThrowArgument_InsufficientResolverSettings();

        }

        // Register address for a node participating in a mesh identified by meshId with the resolver service
        public override object Register(string meshId, PeerNodeAddress nodeAddress, TimeSpan timeout)
        {
            if (opened)
            {

                long scopeId = -1;
                bool multipleScopes = false;

                if (nodeAddress.IPAddresses.Count == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MustRegisterMoreThanZeroAddresses)));
                }

                foreach (IPAddress address in nodeAddress.IPAddresses)
                {
                    if (address.IsIPv6LinkLocal)
                    {
                        if (scopeId == -1)
                        {
                            scopeId = address.ScopeId;
                        }
                        else if (scopeId != address.ScopeId)
                        {
                            multipleScopes = true;
                            break;
                        }
                    }
                }

                List<IPAddress> addresslist = new List<IPAddress>();
                foreach (IPAddress address in nodeAddress.IPAddresses)
                {
                    if (!multipleScopes || (!address.IsIPv6LinkLocal && !address.IsIPv6SiteLocal))
                        addresslist.Add(address);
                }

                if (addresslist.Count == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.AmbiguousConnectivitySpec)));
                }

                ReadOnlyCollection<IPAddress> addresses = new ReadOnlyCollection<IPAddress>(addresslist);
                this.meshId = meshId;
                this.nodeAddress = new PeerNodeAddress(nodeAddress.EndpointAddress, addresses);
                RegisterInfo info = new RegisterInfo(clientId, meshId, this.nodeAddress);
                IPeerResolverClient proxy = GetProxy();
                try
                {
                    proxy.OperationTimeout = timeout;
                    RegisterResponseInfo response = proxy.Register(info);
                    this.registrationId = response.RegistrationId;
                    timer.Set(response.RegistrationLifetime);
                    this.defaultLifeTime = response.RegistrationLifetime;
                    proxy.Close();
                }
                finally
                {
                    proxy.Abort();
                }
            }
            return registrationId;
        }

        void RegistrationExpired(object state)
        {
            if (!opened)
                return;

            try
            {
                IPeerResolverClient proxy = GetProxy();
                RefreshResponseInfo response;
                try
                {
                    int oldValue = Interlocked.Exchange(ref this.updateSuccessful, 1);
                    if (oldValue == 0)
                    {
                        SendUpdate(new UpdateInfo(this.registrationId, this.clientId, this.meshId, this.nodeAddress), ServiceDefaults.SendTimeout);
                        return;
                    }

                    RefreshInfo info = new RefreshInfo(this.meshId, this.registrationId);
                    response = proxy.Refresh(info);

                    if (response.Result == RefreshResult.RegistrationNotFound)
                    {
                        RegisterInfo registerInfo = new RegisterInfo(clientId, meshId, nodeAddress);
                        RegisterResponseInfo registerResponse = proxy.Register(registerInfo);
                        registrationId = registerResponse.RegistrationId;
                        this.defaultLifeTime = registerResponse.RegistrationLifetime;
                    }
                    else
                    {
                        Fx.Assert(response.Result == RefreshResult.Success, "Unrecognized value!!");
                    }
                    proxy.Close();
                }
                finally
                {
                    proxy.Abort();
                    timer.Set(this.defaultLifeTime);
                }
            }
            catch (CommunicationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
        }

        // Unregister address for a node from the resolver service
        public override void Unregister(object registrationId, TimeSpan timeout)
        {
            if (opened)
            {
                UnregisterInfo info = new UnregisterInfo(this.meshId, this.registrationId);
                try
                {
                    IPeerResolverClient proxy = GetProxy();
                    try
                    {
                        proxy.OperationTimeout = timeout;
                        proxy.Unregister(info);
                        proxy.Close();
                    }
                    finally
                    {
                        proxy.Abort();
                    }
                }
                catch (CommunicationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                finally
                {
                    opened = false;
                    timer.Cancel();
                }
            }
        }

        // Updates a node's registration with the resolver service.
        public override void Update(object registrationId, PeerNodeAddress updatedNodeAddress, TimeSpan timeout)
        {
            if (opened)
            {
                UpdateInfo info = new UpdateInfo(this.registrationId, clientId, meshId, updatedNodeAddress);
                this.nodeAddress = updatedNodeAddress;
                SendUpdate(info, timeout);
            }
        }

        void SendUpdate(UpdateInfo updateInfo, TimeSpan timeout)
        {
            try
            {
                RegisterResponseInfo response;
                IPeerResolverClient proxy = GetProxy();
                try
                {
                    proxy.OperationTimeout = timeout;
                    response = proxy.Update(updateInfo);
                    proxy.Close();
                    this.registrationId = response.RegistrationId;
                    this.defaultLifeTime = response.RegistrationLifetime;
                    Interlocked.Exchange(ref this.updateSuccessful, 1);
                    timer.Set(this.defaultLifeTime);
                }
                finally
                {
                    proxy.Abort();
                }
            }
            catch (CommunicationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                Interlocked.Exchange(ref this.updateSuccessful, 0);

            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                Interlocked.Exchange(ref this.updateSuccessful, 0);
                throw;
            }
        }

        // Query the resolver service for addresses associated with a mesh ID
        public override ReadOnlyCollection<PeerNodeAddress> Resolve(string meshId, int maxAddresses, TimeSpan timeout)
        {
            ResolveResponseInfo result = null;
            IList<PeerNodeAddress> addresses = null;
            List<PeerNodeAddress> output_addresses = new List<PeerNodeAddress>();

            if (opened)
            {
                ResolveInfo info = new ResolveInfo(clientId, meshId, maxAddresses);
                try
                {
                    IPeerResolverClient proxy = GetProxy();
                    try
                    {
                        proxy.OperationTimeout = timeout;
                        result = proxy.Resolve(info);
                        proxy.Close();
                    }
                    finally
                    {
                        proxy.Abort();
                    }

                    // If addresses couldn't be obtained, return empty collection
                    if (result != null && result.Addresses != null)
                        addresses = result.Addresses;
                }
                catch (CommunicationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;
                    opened = false;
                    throw;
                }
            }

            if (addresses != null)
            {
                foreach (PeerNodeAddress nodeaddr in addresses)
                {
                    bool valid = true;
                    long scopeId = -1;

                    if (nodeaddr == null) continue;

                    foreach (IPAddress addr in nodeaddr.IPAddresses)
                    {
                        if (addr.IsIPv6LinkLocal)
                        {
                            if (scopeId == -1)
                            {
                                scopeId = addr.ScopeId;
                            }
                            else if (scopeId != addr.ScopeId)
                            {
                                valid = false;
                                break;
                            }
                        }
                    }

                    if (valid)
                    {
                        output_addresses.Add(nodeaddr);
                    }
                }
            }

            return new ReadOnlyCollection<PeerNodeAddress>(output_addresses);
        }
        internal string BindingName
        {
            get { return bindingName; }
            set { this.bindingName = value; }
        }

        internal string BindingConfigurationName
        {
            get { return bindingName; }
            set { this.bindingConfigurationName = value; }
        }

        public override bool Equals(object other)
        {
            PeerDefaultCustomResolverClient that = other as PeerDefaultCustomResolverClient;
            if ((that == null) ||
                    (this.referralPolicy != that.referralPolicy) || !this.address.Equals(that.address))
                return false;
            if (this.BindingName != null || this.BindingConfigurationName != null)
                return ((this.BindingName == that.BindingName) && (this.BindingConfigurationName == that.BindingConfigurationName));
            else
                return this.binding.Equals(that.binding);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}

