//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime
{
    using System.Xml.Linq;

    [Fx.Tag.XamlVisible(false)]
    static class Workflow45Namespace
    {
        const string baseNamespace = "urn:schemas-microsoft-com:System.Activities/4.5/properties";
        static readonly XNamespace workflow45Namespace = XNamespace.Get(baseNamespace);

        static XName definitionIdentity;
        static XName definitionIdentities;
        static XName definitionIdentityFilter;
        static XName workflowApplication;
  

        public static XName DefinitionIdentity
        {
            get
            {
                if (definitionIdentity == null)
                {
                    definitionIdentity = workflow45Namespace.GetName("DefinitionIdentity");
                }

                return definitionIdentity;
            }
        }

        public static XName DefinitionIdentities
        {
            get
            {
                if (definitionIdentities == null)
                {
                    definitionIdentities = workflow45Namespace.GetName("DefinitionIdentities");
                }

                return definitionIdentities;
            }
        }

        public static XName DefinitionIdentityFilter
        {
            get
            {
                if (definitionIdentityFilter == null)
                {
                    definitionIdentityFilter = workflow45Namespace.GetName("DefinitionIdentityFilter");
                }

                return definitionIdentityFilter;
            }
        }

        public static XName WorkflowApplication
        {
            get
            {
                if (workflowApplication == null)
                {
                    workflowApplication = workflow45Namespace.GetName("WorkflowApplication");
                }

                return workflowApplication;
            }
        }
    }
}
