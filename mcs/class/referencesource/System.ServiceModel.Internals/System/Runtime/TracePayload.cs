//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime
{
    struct TracePayload
    {
        string serializedException;
        string eventSource;
        string appDomainFriendlyName;
        string extendedData;
        string hostReference;

        public TracePayload(string serializedException,
            string eventSource,
            string appDomainFriendlyName,
            string extendedData,
            string hostReference)
        {
            this.serializedException = serializedException;
            this.eventSource = eventSource;
            this.appDomainFriendlyName = appDomainFriendlyName;
            this.extendedData = extendedData;
            this.hostReference = hostReference;
        }

        public string SerializedException
        {
            get
            {
                return this.serializedException;
            }
        }

        public string EventSource
        {
            get
            {
                return this.eventSource;
            }
        }

        public string AppDomainFriendlyName
        {
            get
            {
                return this.appDomainFriendlyName;
            }
        }

        public string ExtendedData
        {
            get
            {
                return this.extendedData;
            }
        }

        public string HostReference
        {
            get
            {
                return this.hostReference;
            }
        }
    }
}
