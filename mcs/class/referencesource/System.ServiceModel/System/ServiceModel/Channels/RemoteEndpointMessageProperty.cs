//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Net;

    public sealed class RemoteEndpointMessageProperty
    {
        string address;
        int port;
        IPEndPoint remoteEndPoint;
        IRemoteEndpointProvider remoteEndpointProvider;
        InitializationState state;
        object thisLock = new object();

        public RemoteEndpointMessageProperty(string address, int port)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("port",
                    SR.GetString(SR.ValueMustBeInRange, IPEndPoint.MinPort, IPEndPoint.MaxPort));
            }

            this.port = port;
            this.address = address;
            this.state = InitializationState.All;
        }

        internal RemoteEndpointMessageProperty(IRemoteEndpointProvider remoteEndpointProvider)
        {
            this.remoteEndpointProvider = remoteEndpointProvider;
        }

        internal RemoteEndpointMessageProperty(IPEndPoint remoteEndPoint)
        {
            this.remoteEndPoint = remoteEndPoint;
        }

        public static string Name
        {
            get { return "System.ServiceModel.Channels.RemoteEndpointMessageProperty"; }
        }

        public string Address
        {
            get
            {
                if ((this.state & InitializationState.Address) != InitializationState.Address)
                {
                    lock (ThisLock)
                    {
                        if ((this.state & InitializationState.Address) != InitializationState.Address)
                        {
                            Initialize(false);
                        }
                    }
                }
                return this.address;
            }
        }

        public int Port
        {
            get
            {
                if ((this.state & InitializationState.Port) != InitializationState.Port)
                {
                    lock (ThisLock)
                    {
                        if ((this.state & InitializationState.Port) != InitializationState.Port)
                        {
                            Initialize(true);
                        }
                    }
                }
                return this.port;
            }
        }

        object ThisLock
        {
            get { return thisLock; }
        }

        void Initialize(bool getHostedPort)
        {
            if (remoteEndPoint != null)
            {
                this.address = remoteEndPoint.Address.ToString();
                this.port = remoteEndPoint.Port;
                this.state = InitializationState.All;
                this.remoteEndPoint = null;
            }
            else
            {
                if ((this.state & InitializationState.Address) != InitializationState.Address)
                {
                    this.address = remoteEndpointProvider.GetAddress();
                    this.state |= InitializationState.Address;
                }

                if (getHostedPort)
                {
                    this.port = remoteEndpointProvider.GetPort();
                    this.state |= InitializationState.Port;
                    this.remoteEndpointProvider = null;
                }
            }
        }

        internal interface IRemoteEndpointProvider
        {
            string GetAddress();
            int GetPort();
        }

        [Flags]
        enum InitializationState
        {
            None = 0,
            Address = 1,
            Port = 2,
            All = 3
        }
    }
}
