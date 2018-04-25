#region Copyright (c) Microsoft Corporation
/// <copyright company='Microsoft Corporation'>
///    Copyright (c) Microsoft Corporation. All Rights Reserved.
///    Information Contained Herein is Proprietary and Confidential.
/// </copyright>
#endregion

using System;
using System.Globalization;
using System.IO;
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
    /// This class presents a single file referenced by a svcmap file
    /// </summary>
    /// <remarks></remarks>
#if WEB_EXTENSIONS_CODE
    internal class ExternalFile
#else
    [CLSCompliant(true)]
    public class ExternalFile
#endif
    {
        // File Name
        private string m_FileName;

        // Is the MeatadataFile loaded from the file? If it is false, we need create a new file when we save to the disket
        private bool m_IsExistingFile;

        // error happens when the file is loaded
        private Exception m_ErrorInLoading;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks> Must support a default construct for XmlSerializer</remarks>
        public ExternalFile()
        {
            m_FileName = String.Empty;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileName">File Name</param>
        public ExternalFile(string fileName)
        {
            this.FileName = fileName;
        }

        /// <summary>
        /// Error happens when the file is loaded
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlIgnore()]
        public Exception ErrorInLoading
        {
            get
            {
                return m_ErrorInLoading;
            }
            set
            {
                m_ErrorInLoading = value;
            }
        }

        /// <summary>
        /// FileName in the storage
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlAttribute()]
        public string FileName
        {
            get
            {
                return m_FileName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (!IsLocalFileName(value))
                {
                    throw new NotSupportedException(String.Format(CultureInfo.CurrentCulture, WCFModelStrings.ReferenceGroup_InvalidFileName, value));
                }

                m_FileName = value;
            }
        }

        /// <summary>
        /// Is the item loaded from the file? If it is false, we need create a new file when we save to the disket 
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlIgnore()]
        public bool IsExistingFile
        {
            get
            {
                return m_IsExistingFile;
            }
            set
            {
                m_IsExistingFile = value;
            }
        }

        /// <summary>
        /// Check the file name is a real file name but not a path
        /// </summary>
        /// <param name="fileName"></param>
        /// <remarks></remarks>
        public static bool IsLocalFileName(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || fileName.IndexOfAny(new Char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar }) >= 0)
            {
                return false;
            }

            return true;
        }

    }
}

