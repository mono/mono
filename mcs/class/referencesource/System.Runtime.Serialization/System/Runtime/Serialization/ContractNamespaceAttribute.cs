//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module, Inherited = false, AllowMultiple = true)]
    public sealed class ContractNamespaceAttribute : Attribute
    {
        string clrNamespace;
        string contractNamespace;

        public ContractNamespaceAttribute(string contractNamespace)
        {
            this.contractNamespace = contractNamespace;
        }

        public string ClrNamespace
        {
            get { return clrNamespace; }
            set { clrNamespace = value; }
        }

        public string ContractNamespace
        {
            get { return contractNamespace; }
        }
    }
}

