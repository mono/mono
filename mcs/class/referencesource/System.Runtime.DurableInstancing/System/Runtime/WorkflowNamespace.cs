//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime
{
    using System.Xml.Linq;

    [Fx.Tag.XamlVisible(false)]
    static class WorkflowNamespace
    {
        const string baseNamespace = "urn:schemas-microsoft-com:System.Activities/4.0/properties";
        static readonly XNamespace workflowNamespace = XNamespace.Get(baseNamespace);
        static readonly XNamespace variablesNamespace = XNamespace.Get(baseNamespace + "/variables");
        static readonly XNamespace outputNamespace = XNamespace.Get(baseNamespace + "/output");

        static XName workflowHostType;
        static XName status;
        static XName bookmarks;
        static XName lastUpdate;
        static XName exception;
        static XName workflow;
        static XName keyProvider;

        public static XNamespace VariablesPath
        {
            get
            {
                return variablesNamespace;
            }
        }

        public static XNamespace OutputPath
        {
            get
            {
                return outputNamespace;
            }
        }

        public static XName WorkflowHostType
        {
            get
            {
                if (workflowHostType == null)
                {
                    workflowHostType = workflowNamespace.GetName("WorkflowHostType");
                }

                return workflowHostType;
            }
        }

        public static XName Status
        {
            get
            {
                if (status == null)
                {
                    status = workflowNamespace.GetName("Status");
                }
                return status;
            }
        }

        public static XName Bookmarks
        {
            get
            {
                if (bookmarks == null)
                {
                    bookmarks = workflowNamespace.GetName("Bookmarks");
                }
                return bookmarks;
            }
        }

        public static XName LastUpdate
        {
            get
            {
                if (lastUpdate == null)
                {
                    lastUpdate = workflowNamespace.GetName("LastUpdate");
                }
                return lastUpdate;
            }
        }

        public static XName Exception
        {
            get
            {
                if (exception == null)
                {
                    exception = workflowNamespace.GetName("Exception");
                }
                return exception;
            }
        }

        public static XName Workflow
        {
            get
            {
                if (workflow == null)
                {
                    workflow = workflowNamespace.GetName("Workflow");
                }
                return workflow;
            }
        }

        public static XName KeyProvider
        {
            get
            {
                if (keyProvider == null)
                {
                    keyProvider = workflowNamespace.GetName("KeyProvider");
                }
                return keyProvider;
            }
        }
    }
}
