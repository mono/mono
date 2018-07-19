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
    /// Map between (targetNamespace, PortTypeName) --> CLR TypeName
    /// </summary>
    /// <remarks></remarks>
#if WEB_EXTENSIONS_CODE
    internal class GeneratedContractType
#else
    [CLSCompliant(true)]
    public class GeneratedContractType
#endif
    {

        private string m_TargetNamespace;
        private string m_Name;
        private string m_ContractType;
        private string m_ConfigurationName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>required by xml serializer</remarks> 
        public GeneratedContractType()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="targetNamespace"></param>
        /// <param name="portName"></param>
        /// <param name="contractType"></param>
        /// <param name="configurationName"></param>
        /// <remarks></remarks> 
        public GeneratedContractType(string targetNamespace, string portName, string contractType, string configurationName)
        {
            m_TargetNamespace = targetNamespace;
            m_Name = portName;
            m_ContractType = contractType;
            m_ConfigurationName = configurationName;
        }

        /// <summary>
        /// The TargetNamespace of this contract type in the WSDL file
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
        /// The portTypeName of this contract type in the WSDL file
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
                m_Name = value;
            }
        }

        /// <summary>
        /// The generated CLR type name of this contract type
        /// </summary>
        /// <value></value>
        /// <remarks></remarks> 
        [XmlSerialization.XmlAttribute()]
        public string ContractType
        {
            get
            {
                return m_ContractType;
            }
            set
            {
                m_ContractType = value;
            }
        }

        /// <summary>
        /// The name of this contract in the config file
        /// </summary>
        /// <value></value>
        /// <remarks></remarks> 
        [XmlSerialization.XmlAttribute()]
        public string ConfigurationName
        {
            get
            {
                return m_ConfigurationName;
            }
            set
            {
                m_ConfigurationName = value;
            }
        }

    }

}

