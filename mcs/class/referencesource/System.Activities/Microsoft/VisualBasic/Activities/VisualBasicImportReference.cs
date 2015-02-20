//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace Microsoft.VisualBasic.Activities
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Activities.Expressions;
    using System.Xaml;
    using System.Xml.Linq;

    public class VisualBasicImportReference : IEquatable<VisualBasicImportReference>
    {
        static AssemblyNameEqualityComparer equalityComparer = new AssemblyNameEqualityComparer();
        AssemblyName assemblyName;
        string assemblyNameString;
        int hashCode;
        string import;

        public VisualBasicImportReference()
        {
        }

        public string Assembly
        {
            get { return this.assemblyNameString; }

            set
            {
                if (value == null)
                {
                    this.assemblyName = null;
                    this.assemblyNameString = null;
                }
                else
                {
                    // FileLoadException thrown from this ctor indicates invalid assembly name
                    this.assemblyName = new AssemblyName(value);
                    this.assemblyNameString = this.assemblyName.FullName;
                }
                this.EarlyBoundAssembly = null;
            }
        }

        public string Import
        {
            get
            {
                return this.import;
            }
            set
            {
                if (value != null)
                {
                    this.import = value.Trim();
                    this.hashCode = this.import.ToUpperInvariant().GetHashCode();
                }
                else
                {
                    this.import = null;
                    this.hashCode = 0;
                }
                this.EarlyBoundAssembly = null;
            }
        }

        internal AssemblyName AssemblyName
        {
            get { return this.assemblyName; }
        }

        internal XNamespace Xmlns
        {
            get;
            set;
        }
        
        // for the short-cut assembly resolution
        // from VBImportReference.AssemblyName ==> System.Reflection.Assembly
        // this is an internal state that implies the context in which a VB assembly resolution is progressing
        // once VB extracted this Assembly object to pass onto the compiler, 
        // it must explicitly set this property back to null.
        // Clone() will also explicitly set this property of the new to null to prevent users from inadvertently 
        // creating a copy of VBImportReference that might not resolve to the assembly of his or her intent.
        internal Assembly EarlyBoundAssembly
        {
            get;
            set;
        }

        internal VisualBasicImportReference Clone()
        {
            VisualBasicImportReference toReturn = (VisualBasicImportReference)this.MemberwiseClone();
            toReturn.EarlyBoundAssembly = null;
            // Also make a clone of the AssemblyName.
            toReturn.assemblyName = (AssemblyName) this.assemblyName.Clone();
            return toReturn;
        }

        
        public override int GetHashCode()
        {
            return this.hashCode;
        }

        public bool Equals(VisualBasicImportReference other)
        {
            if (other == null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            if (this.EarlyBoundAssembly != other.EarlyBoundAssembly)
            {
                return false;
            }

            // VB does case insensitive comparisons for imports
            if (string.Compare(this.Import, other.Import, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }

            // now compare the assemblies
            if (this.AssemblyName == null && other.AssemblyName == null)
            {
                return true;
            }
            else if (this.AssemblyName == null && other.AssemblyName != null)
            {
                return false;
            }
            else if (this.AssemblyName != null && other.AssemblyName == null)
            {
                return false;
            }

            return equalityComparer.Equals(this.AssemblyName, other.AssemblyName);            
        }

        internal void GenerateXamlNamespace(INamespacePrefixLookup namespaceLookup)
        {
            // promote reference to xmlns declaration
            string xamlNamespace = null;
            if (this.Xmlns != null && !string.IsNullOrEmpty(this.Xmlns.NamespaceName))
            {
                xamlNamespace = this.Xmlns.NamespaceName;
            }
            else
            {
                xamlNamespace = string.Format(CultureInfo.InvariantCulture, "clr-namespace:{0};assembly={1}", this.Import, this.Assembly);
            }
            // we don't need the return value since we just want to register the namespace/assembly pair
            namespaceLookup.LookupPrefix(xamlNamespace);
        }
    }
}
