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
    /// <summary>
    /// mapping between metadata namespace and CLR namespace
    /// </summary>
    /// <remarks></remarks>
#if WEB_EXTENSIONS_CODE
    internal class NamespaceMapping
#else
    [CLSCompliant(true)]
    public class NamespaceMapping
#endif
    {
        private string m_TargetNamespace;
        private string m_ClrNamespace;

        /// <summary>
        /// Target Namespace
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlAttribute()]
        public string TargetNamespace
        {
            get
            {
                return m_TargetNamespace;
            }
            set
            {
                m_TargetNamespace = value;
            }
        }

        /// <summary>
        /// Clr Namespace
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlAttribute()]
        public string ClrNamespace
        {
            get
            {
                return m_ClrNamespace;
            }
            set
            {
                m_ClrNamespace = value;
            }
        }

    }
}

