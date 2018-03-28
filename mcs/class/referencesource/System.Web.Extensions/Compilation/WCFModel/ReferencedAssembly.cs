#region Copyright (c) Microsoft Corporation
/// <copyright company='Microsoft Corporation'>
///    Copyright (c) Microsoft Corporation. All Rights Reserved.
///    Information Contained Herein is Proprietary and Confidential.
/// </copyright>
#endregion

using System;
using XmlSerialization = System.Xml.Serialization;

#if WEB_EXTENSIONS_CODE
namespace System.Web.Compilation.WCFModel
#else
namespace Microsoft.VSDesigner.WCFModel
#endif
{
#if WEB_EXTENSIONS_CODE
    internal class ReferencedAssembly
#else
    [CLSCompliant(true)]
    public class ReferencedAssembly
#endif
    {
        private string m_AssemblyName;

        /// <summary>
        /// Constructor
        /// </summary>
        public ReferencedAssembly()
        {
            m_AssemblyName = String.Empty;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ReferencedAssembly(string assemblyName)
        {
            if (assemblyName == null)
            {
                throw new ArgumentNullException("assemblyName");
            }

            m_AssemblyName = assemblyName;
        }

        /// <summary>
        /// assembly name
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlAttribute()]
        public string AssemblyName
        {
            get
            {
                return m_AssemblyName;
            }
            set
            {
                m_AssemblyName = value;
            }
        }
    }
}

