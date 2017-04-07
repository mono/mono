// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>Microsoft</OWNER>
// 

using System;
using System.Reflection;


namespace System.Reflection
{
    [AttributeUsage (AttributeTargets.Assembly, AllowMultiple=false, Inherited=false)]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class ObfuscateAssemblyAttribute : Attribute
    {
        private bool m_assemblyIsPrivate;
        private bool m_strip = true;

        public ObfuscateAssemblyAttribute(bool assemblyIsPrivate)
        {
            m_assemblyIsPrivate = assemblyIsPrivate;
        }

        public bool AssemblyIsPrivate
        {
            get
            {
                return m_assemblyIsPrivate;
            }
        }

        public bool StripAfterObfuscation
        {
            get
            {
                return m_strip;
            }
            set
            {
                m_strip = value;
            }
        }
    }
}

