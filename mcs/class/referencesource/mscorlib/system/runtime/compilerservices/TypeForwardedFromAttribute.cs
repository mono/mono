// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false, AllowMultiple = false)]
    public sealed class TypeForwardedFromAttribute : Attribute
    {
        string assemblyFullName;

        private TypeForwardedFromAttribute()
        {
            // Disallow default constructor
        }


        public TypeForwardedFromAttribute(string assemblyFullName)
        {
            if (String.IsNullOrEmpty(assemblyFullName))
            {
                throw new ArgumentNullException("assemblyFullName");
            }
            this.assemblyFullName = assemblyFullName;    
        }

        public string AssemblyFullName
        {
            get { 
                return assemblyFullName; 
            }
        }
    }
}
