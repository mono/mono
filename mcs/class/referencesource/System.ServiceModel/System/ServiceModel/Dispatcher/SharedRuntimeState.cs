//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;

    class SharedRuntimeState
    {
        bool isImmutable;
        bool enableFaults = true;
        bool isOnServer;
        bool manualAddressing;
        bool validateMustUnderstand = true;

        internal SharedRuntimeState(bool isOnServer)
        {
            this.isOnServer = isOnServer;
        }

        internal bool EnableFaults
        {
            get { return this.enableFaults; }
            set { this.enableFaults = value; }
        }

        internal bool IsOnServer
        {
            get { return this.isOnServer; }
        }

        internal bool ManualAddressing
        {
            get { return this.manualAddressing; }
            set { this.manualAddressing = value; }
        }

        internal bool ValidateMustUnderstand
        {
            get { return this.validateMustUnderstand; }
            set { this.validateMustUnderstand = value; }
        }

        internal void LockDownProperties()
        {
            this.isImmutable = true;
        }

        internal void ThrowIfImmutable()
        {
            if (this.isImmutable)
            {
                if (this.IsOnServer)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxImmutableServiceHostBehavior0)));
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxImmutableChannelFactoryBehavior0)));
                }
            }
        }
    }
}
