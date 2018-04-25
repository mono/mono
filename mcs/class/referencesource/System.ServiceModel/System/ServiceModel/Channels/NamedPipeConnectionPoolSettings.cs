//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;

    public sealed class NamedPipeConnectionPoolSettings
    {
        string groupName;
        TimeSpan idleTimeout;
        int maxOutputConnectionsPerEndpoint;

        internal NamedPipeConnectionPoolSettings()
        {
            groupName = ConnectionOrientedTransportDefaults.ConnectionPoolGroupName;
            idleTimeout = ConnectionOrientedTransportDefaults.IdleTimeout;
            maxOutputConnectionsPerEndpoint = ConnectionOrientedTransportDefaults.MaxOutboundConnectionsPerEndpoint;
        }

        internal NamedPipeConnectionPoolSettings(NamedPipeConnectionPoolSettings namedPipe)
        {
            this.groupName = namedPipe.groupName;
            this.idleTimeout = namedPipe.idleTimeout;
            this.maxOutputConnectionsPerEndpoint = namedPipe.maxOutputConnectionsPerEndpoint;
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

        public int MaxOutboundConnectionsPerEndpoint
        {
            get { return this.maxOutputConnectionsPerEndpoint; }
            set
            {
                if (value < 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.ValueMustBeNonNegative)));

                this.maxOutputConnectionsPerEndpoint = value;
            }
        }

        internal NamedPipeConnectionPoolSettings Clone()
        {
            return new NamedPipeConnectionPoolSettings(this);
        }

        internal bool IsMatch(NamedPipeConnectionPoolSettings namedPipe)
        {
            if (this.groupName != namedPipe.groupName)
                return false;

            if (this.idleTimeout != namedPipe.idleTimeout)
                return false;

            if (this.maxOutputConnectionsPerEndpoint != namedPipe.maxOutputConnectionsPerEndpoint)
                return false;

            return true;
        }
    }
}
