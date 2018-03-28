//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Globalization;

namespace System.ServiceModel.Description
{
    public sealed class PolicyVersion
    {
        string policyNamespace;

        static PolicyVersion policyVersion12;
        static PolicyVersion policyVersion15;

        static PolicyVersion()
        {
            policyVersion12 = new PolicyVersion(MetadataStrings.WSPolicy.NamespaceUri);
            policyVersion15 = new PolicyVersion(MetadataStrings.WSPolicy.NamespaceUri15);
        }

        PolicyVersion(string policyNamespace)
        {
            this.policyNamespace = policyNamespace;
        }

        public static PolicyVersion Policy12 { get { return policyVersion12; } }
        public static PolicyVersion Policy15 { get { return policyVersion15; } }
        public static PolicyVersion Default { get { return policyVersion12; } }
        public string Namespace { get { return policyNamespace; } }

        public override string ToString()
        {
            return policyNamespace;
        }
    }
}
