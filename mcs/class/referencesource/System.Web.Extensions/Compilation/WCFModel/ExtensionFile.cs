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
    /// This class presents a single file referenced by a svcmap file
    /// </summary>
    /// <remarks></remarks>
#if WEB_EXTENSIONS_CODE
    internal class ExtensionFile : ExternalFile
#else
    [CLSCompliant(true)]
    public class ExtensionFile : ExternalFile
#endif
    {
        // Extension Item Name
        private string m_Name;

        // content buffer
        private byte[] m_ContentBuffer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks> Must support a default construct for XmlSerializer</remarks>
        public ExtensionFile()
        {
            m_Name = string.Empty;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">name of extension item</param>
        /// <param name="fileName">Suggested File Name</param>
        public ExtensionFile(string name, string fileName, byte[] content)
            : base(fileName)
        {
            this.Name = name;
            m_ContentBuffer = content;

            IsExistingFile = false;
        }

        /// <summary>
        /// Content of the extension file
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlIgnore()]
        public byte[] ContentBuffer
        {
            get
            {
                return m_ContentBuffer;
            }
            set
            {
                m_ContentBuffer = value;
                ErrorInLoading = null;
            }
        }

        /// <summary>
        /// whether the content is buffered
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        internal bool IsBufferValid
        {
            get
            {
                return (m_ContentBuffer != null);
            }
        }

        /// <summary>
        /// Name in the storage
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlAttribute()]
        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                m_Name = value;
            }
        }

        /// <summary>
        ///  the function is called when the metadata is removed, and we need clean up the content
        /// </summary>
        /// <remarks></remarks>
        internal void CleanUpContent()
        {
            ErrorInLoading = null;
            m_ContentBuffer = null;
        }

    }
}

