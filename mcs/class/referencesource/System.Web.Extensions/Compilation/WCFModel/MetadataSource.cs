#region Copyright (c) Microsoft Corporation
/// <copyright company='Microsoft Corporation'>
///    Copyright (c) Microsoft Corporation. All Rights Reserved.
///    Information Contained Herein is Proprietary and Confidential.
/// </copyright>
#endregion

using System;
using System.Diagnostics;
using XmlSerialization = System.Xml.Serialization;

#if WEB_EXTENSIONS_CODE
using System.Web.Resources;
#else
using Microsoft.VSDesigner.WCF.Resources;
#endif

#if WEB_EXTENSIONS_CODE
namespace System.Web.Compilation.WCFModel
#else
namespace Microsoft.VSDesigner.WCFModel
#endif
{
    /// <summary>
    /// This class implements a MetadataSource item in the svcmap file
    /// </summary>
    /// <remarks></remarks>
#if WEB_EXTENSIONS_CODE
    internal class MetadataSource
#else
    [CLSCompliant(true)]
    public class MetadataSource
#endif
    {
        // URL of metadata source
        private string m_Address;

        // protocol to download it
        private string m_Protocol;

        // ID of this source
        private int m_SourceId;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks> Must support a default construct for XmlSerializer</remarks>
        public MetadataSource()
        {
            m_Address = String.Empty;
            m_Protocol = String.Empty;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="address"></param>
        /// <param name="sourceId"></param>
        public MetadataSource(string protocol, string address, int sourceId)
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (protocol.Length == 0)
            {
                throw new ArgumentException(WCFModelStrings.ReferenceGroup_EmptyProtocol);
            }
            if (address == null)
            {
                throw new ArgumentException(WCFModelStrings.ReferenceGroup_EmptyAddress);
            }
            m_Protocol = protocol;
            m_Address = address;

            if (sourceId < 0)
            {
                Debug.Fail("Source ID shouldn't be a nagtive number");
                throw new ArgumentException(WCFModelStrings.ReferenceGroup_InvalidSourceId);
            }
            m_SourceId = sourceId;
        }

        /// <summary>
        /// URL address to download metadata
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlAttribute()]
        public string Address
        {
            get
            {
                return m_Address;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                m_Address = value;
            }
        }

        /// <summary>
        /// protocol used to download metadata
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlAttribute()]
        public string Protocol
        {
            get
            {
                return m_Protocol;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                m_Protocol = value;
            }
        }

        /// <summary>
        /// generated ID for this metadata source
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlAttribute()]
        public int SourceId
        {
            get
            {
                return m_SourceId;
            }
            set
            {
                if (value < 0)
                {
                    Debug.Fail("Source ID shouldn't be a nagtive number");
                    throw new ArgumentException(WCFModelStrings.ReferenceGroup_InvalidSourceId);
                }
                m_SourceId = value;
            }
        }

    }

}


