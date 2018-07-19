#region Copyright (c) Microsoft Corporation
/// <copyright company='Microsoft Corporation'>
///    Copyright (c) Microsoft Corporation. All Rights Reserved.
///    Information Contained Herein is Proprietary and Confidential.
/// </copyright>
#endregion

using System;
using System.ServiceModel.Description;
using System.Xml;
using System.Xml.Schema;

#if WEB_EXTENSIONS_CODE
namespace System.Web.Compilation.WCFModel
#else
namespace Microsoft.VSDesigner.WCFModel
#endif
{
    /// <summary>
    /// This class represents an error message happens when we generate code
    /// </summary>
    /// <remarks></remarks>
#if WEB_EXTENSIONS_CODE
    internal class ProxyGenerationError
#else
    [CLSCompliant(true)]
    public class ProxyGenerationError
#endif
    {
        private bool m_IsWarning;
        private string m_Message;
        private string m_MetadataFile;
        private int m_LineNumber;
        private int m_LinePosition;
        private GeneratorState m_ErrorGeneratorState;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="errorMessage">MetadataConversionError</param>
        /// <remarks> </remarks>
        public ProxyGenerationError(MetadataConversionError errorMessage)
        {
            m_ErrorGeneratorState = GeneratorState.GenerateCode;
            m_IsWarning = errorMessage.IsWarning;
            m_Message = errorMessage.Message;
            m_MetadataFile = String.Empty;
            m_LineNumber = -1;
            m_LinePosition = -1;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="generatorState"></param>
        /// <param name="fileName"></param>
        /// <param name="errorException">An IOException</param>
        /// <remarks> </remarks>
        public ProxyGenerationError(GeneratorState generatorState, string fileName, Exception errorException)
        {
            m_ErrorGeneratorState = generatorState;
            m_IsWarning = false;
            m_Message = errorException.Message;
            m_MetadataFile = fileName;
            m_LineNumber = -1;
            m_LinePosition = -1;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="generatorState"></param>
        /// <param name="fileName"></param>
        /// <param name="errorException">An IOException</param>
        /// <param name="isWarning">An IOException</param>
        /// <remarks> </remarks>
        public ProxyGenerationError(GeneratorState generatorState, string fileName, Exception errorException, bool isWarning)
        {
            m_ErrorGeneratorState = generatorState;
            m_IsWarning = isWarning;
            m_Message = errorException.Message;
            m_MetadataFile = fileName;
            m_LineNumber = -1;
            m_LinePosition = -1;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="generatorState"></param>
        /// <param name="fileName"></param>
        /// <param name="errorException">An XmlException</param>
        /// <remarks> </remarks>
        public ProxyGenerationError(GeneratorState generatorState, string fileName, XmlException errorException)
        {
            m_ErrorGeneratorState = generatorState;
            m_IsWarning = false;
            m_Message = errorException.Message;
            m_MetadataFile = fileName;
            m_LineNumber = errorException.LineNumber;
            m_LinePosition = errorException.LinePosition;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="generatorState"></param>
        /// <param name="fileName"></param>
        /// <param name="errorException">An XmlException</param>
        /// <remarks> </remarks>
        public ProxyGenerationError(GeneratorState generatorState, string fileName, XmlSchemaException errorException)
        {
            m_ErrorGeneratorState = generatorState;
            m_IsWarning = false;
            m_Message = errorException.Message;
            m_MetadataFile = fileName;
            m_LineNumber = errorException.LineNumber;
            m_LinePosition = errorException.LinePosition;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="generatorState"></param>
        /// <param name="fileName"></param>
        /// <param name="errorException">An XmlException</param>
        /// <param name="isWarning">An XmlException</param>
        /// <remarks> </remarks>
        public ProxyGenerationError(GeneratorState generatorState, string fileName, XmlSchemaException errorException, bool isWarning)
        {
            m_ErrorGeneratorState = generatorState;
            m_IsWarning = isWarning;
            m_Message = errorException.Message;
            m_MetadataFile = fileName;
            m_LineNumber = errorException.LineNumber;
            m_LinePosition = errorException.LinePosition;
        }

        /// <summary>
        /// This property represents when an error message happens
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public GeneratorState ErrorGeneratorState
        {
            get
            {
                return m_ErrorGeneratorState;
            }
        }

        /// <summary>
        /// True: if it is a warning message, otherwise, an error
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public bool IsWarning
        {
            get
            {
                return m_IsWarning;
            }
        }

        /// <summary>
        /// Line Number when error happens
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public int LineNumber
        {
            get
            {
                return m_LineNumber;
            }
        }

        /// <summary>
        /// Column Number in the line when error happens
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public int LinePosition
        {
            get
            {
                return m_LinePosition;
            }
        }

        /// <summary>
        /// return the error message
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string Message
        {
            get
            {
                return m_Message;
            }
        }

        /// <summary>
        /// return the error message
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string MetadataFile
        {
            get
            {
                return m_MetadataFile;
            }
        }

        /// <summary>
        /// This enum represents when an error message happens
        /// </summary>
        /// <remarks></remarks>
        public enum GeneratorState
        {
            LoadMetadata = 0,
            MergeMetadata = 1,
            GenerateCode = 2,
        }

    }
}
