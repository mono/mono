//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.CodeDom.Compiler;

    public class ImportOptions
    {
        bool generateSerializable;
        bool generateInternal;
        bool enableDataBinding;
        CodeDomProvider codeProvider;
        ICollection<Type> referencedTypes;
        ICollection<Type> referencedCollectionTypes;
        IDictionary<string, string> namespaces;
        bool importXmlType;
        IDataContractSurrogate dataContractSurrogate;

        public bool GenerateSerializable
        {
            get { return generateSerializable; }
            set { generateSerializable = value; }
        }

        public bool GenerateInternal
        {
            get { return generateInternal; }
            set { generateInternal = value; }
        }

        public bool EnableDataBinding
        {
            get { return enableDataBinding; }
            set { enableDataBinding = value; }
        }

        public CodeDomProvider CodeProvider
        {
            get { return codeProvider; }
            set { codeProvider = value; }
        }

        public ICollection<Type> ReferencedTypes
        {
            get
            {
                if (referencedTypes == null)
                {
                    referencedTypes = new List<Type>();
                }
                return referencedTypes;
            }
        }

        public ICollection<Type> ReferencedCollectionTypes
        {
            get
            {
                if (referencedCollectionTypes == null)
                {
                    referencedCollectionTypes = new List<Type>();
                }
                return referencedCollectionTypes;
            }
        }

        public IDictionary<String, String> Namespaces
        {
            get
            {
                if (namespaces == null)
                {
                    namespaces = new Dictionary<string, string>();
                }
                return namespaces;
            }
        }

        public bool ImportXmlType
        {
            get { return importXmlType; }
            set { importXmlType = value; }
        }

        public IDataContractSurrogate DataContractSurrogate
        {
            get { return dataContractSurrogate; }
            set { dataContractSurrogate = value; }
        }
    }
}

