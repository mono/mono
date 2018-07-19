//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    public abstract class CorrelationDataDescription
    {
        public abstract bool IsOptional
        {
            get;
        }

        public abstract bool IsDefault
        {
            get;
        }

        public abstract bool KnownBeforeSend
        {
            get;
        }

        public abstract string Name
        {
            get;
        }

        public abstract bool ReceiveValue
        {
            get;
        }

        public abstract bool SendValue
        {
            get;
        }
    }
}
