//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime
{
    using System.Xml.Linq;

    [Fx.Tag.XamlVisible(false)]
    static class WorkflowServiceNamespace
    {
        const string baseNamespace = "urn:schemas-microsoft-com:System.ServiceModel.Activities/4.0/properties";
        static readonly XNamespace workflowServiceNamespace = XNamespace.Get(baseNamespace);
        static readonly XNamespace endpointsNamespace = XNamespace.Get(baseNamespace + "/endpoints");

        static XName controlEndpoint;
        static XName suspendException;
        static XName suspendReason;
        static XName siteName;
        static XName relativeApplicationPath;
        static XName relativeServicePath;
        static XName creationContext;
        static XName service;
        static XName requestReplyCorrelation;
        static XName messageVersionForReplies;

        public static XNamespace EndpointsPath
        {
            get
            {
                return endpointsNamespace;
            }
        }

        public static XName ControlEndpoint
        {
            get
            {
                if (controlEndpoint == null)
                {
                    controlEndpoint = workflowServiceNamespace.GetName("ControlEndpoint");
                }
                return controlEndpoint;
            }
        }

        public static XName MessageVersionForReplies
        {
            get
            {
                if (messageVersionForReplies == null)
                {
                    messageVersionForReplies = workflowServiceNamespace.GetName("MessageVersionForReplies");
                }
                return messageVersionForReplies;
            }
        }

        public static XName RequestReplyCorrelation
        {
            get
            {
                if (requestReplyCorrelation == null)
                {
                    requestReplyCorrelation = workflowServiceNamespace.GetName("RequestReplyCorrelation");
                }
                return requestReplyCorrelation;
            }
        }

        public static XName SuspendReason
        {
            get
            {
                if (suspendReason == null)
                {
                    suspendReason = workflowServiceNamespace.GetName("SuspendReason");
                }
                return suspendReason;
            }
        }

        public static XName SiteName
        {
            get
            {
                if (siteName == null)
                {
                    siteName = workflowServiceNamespace.GetName("SiteName");
                }
                return siteName;
            }
        }

        public static XName SuspendException
        {
            get
            {
                if (suspendException == null)
                {
                    suspendException = workflowServiceNamespace.GetName("SuspendException");
                }

                return suspendException;
            }
        }

        public static XName RelativeApplicationPath
        {
            get
            {
                if (relativeApplicationPath == null)
                {
                    relativeApplicationPath = workflowServiceNamespace.GetName("RelativeApplicationPath");
                }
                return relativeApplicationPath;
            }
        }

        public static XName RelativeServicePath
        {
            get
            {
                if (relativeServicePath == null)
                {
                    relativeServicePath = workflowServiceNamespace.GetName("RelativeServicePath");
                }
                return relativeServicePath;
            }
        }

        public static XName CreationContext
        {
            get
            {
                if (creationContext == null)
                {
                    creationContext = workflowServiceNamespace.GetName("CreationContext");
                }
                return creationContext;
            }
        }

        public static XName Service
        {
            get
            {
                if (service == null)
                {
                    service = workflowServiceNamespace.GetName("Service");
                }
                return service;
            }
        }
    }
}
