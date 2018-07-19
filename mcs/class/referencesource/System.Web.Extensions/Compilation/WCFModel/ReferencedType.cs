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
    internal class ReferencedType
#else
    [CLSCompliant(true)]
    public class ReferencedType
#endif
    {
        private string m_TypeName;

        /// <summary>
        /// assembly qualified type name
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlAttribute()]
        public string TypeName
        {
            get
            {
                return m_TypeName;
            }
            set
            {
                m_TypeName = value;
            }
        }
    }
}

