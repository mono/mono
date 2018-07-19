//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;

    public sealed class TcpConnectionPoolSettings
    {
        string groupName;
        TimeSpan idleTimeout;
        TimeSpan leaseTimeout;
        int maxOutboundConnectionsPerEndpoint;

        internal TcpConnectionPoolSettings()
        {
            groupName = ConnectionOrientedTransportDefaults.ConnectionPoolGroupName;
            idleTimeout = ConnectionOrientedTransportDefaults.IdleTimeout;
            leaseTimeout = TcpTransportDefaults.ConnectionLeaseTimeout;
            maxOutboundConnectionsPerEndpoint = ConnectionOrientedTransportDefaults.MaxOutboundConnectionsPerEndpoint;
        }

        internal TcpConnectionPoolSettings(TcpConnectionPoolSettings tcp)
        {
            this.groupName = tcp.groupName;
            this.idleTimeout = tcp.idleTimeout;
            this.leaseTimeout = tcp.leaseTimeout;
            this.maxOutboundConnectionsPerEndpoint = tcp.maxOutboundConnectionsPerEndpoint;
        }

        public string GroupName
        {
            get { return this.groupName; }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");

                this.groupName = value;
            }
        }

        public TimeSpan IdleTimeout
        {
            get { return this.idleTimeout; }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.idleTimeout = value;
            }
        }

        public TimeSpan LeaseTimeout
        {
            get { return this.leaseTimeout; }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.leaseTimeout = value;
            }
        }

        public int MaxOutboundConnectionsPerEndpoint
        {
            get { return this.maxOutboundConnectionsPerEndpoint; }
            set
            {
                if (value < 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.ValueMustBeNonNegative)));

                this.maxOutboundConnectionsPerEndpoint = value;
            }
        }

        internal TcpConnectionPoolSettings Clone()
        {
            return new TcpConnectionPoolSettings(this);
        }

        internal bool IsMatch(TcpConnectionPoolSettings tcp)
        {
            if (this.groupName != tcp.groupName)
                return false;

            if (this.idleTimeout != tcp.idleTimeout)
                return false;

            if (this.leaseTimeout != tcp.leaseTimeout)
                return false;

            if (this.maxOutboundConnectionsPerEndpoint != tcp.maxOutboundConnectionsPerEndpoint)
                return false;

            return true;
        }
    }
}
