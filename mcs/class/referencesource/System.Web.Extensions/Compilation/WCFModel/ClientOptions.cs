#region Copyright (c) Microsoft Corporation
/// <copyright company='Microsoft Corporation'>
///    Copyright (c) Microsoft Corporation. All Rights Reserved.
///    Information Contained Herein is Proprietary and Confidential.
/// </copyright>
#endregion

using System;
using System.Collections.Generic;
using XmlSerialization = System.Xml.Serialization;

#if WEB_EXTENSIONS_CODE
namespace System.Web.Compilation.WCFModel
#else
namespace Microsoft.VSDesigner.WCFModel
#endif
{
#if WEB_EXTENSIONS_CODE
    internal class ClientOptions
#else
    [CLSCompliant(true)]
    public class ClientOptions
#endif
    {
        private bool m_GenerateAsynchronousMethods;
        private bool m_GenerateTaskBasedAsynchronousMethod;
        private bool m_GenerateTaskBasedAsynchronousMethodSpecified;
        private bool m_EnableDataBinding;
        private List<ReferencedType> m_ExcludedTypeList;
        private bool m_ImportXmlTypes;
        private bool m_GenerateInternalTypes;
        private bool m_GenerateMessageContracts;
        private List<NamespaceMapping> m_NamespaceMappingList;
        private List<ReferencedCollectionType> m_CollectionMappingList;
        private bool m_GenerateSerializableTypes;
        private ProxySerializerType m_Serializer;
        private bool m_ReferenceAllAssemblies;
        private List<ReferencedAssembly> m_ReferencedAssemblyList;
        private List<ReferencedType> m_ReferencedDataContractTypeList;
        private List<ContractMapping> m_ServiceContractMappingList;
        private bool m_UseSerializerForFaults;
        private bool m_UseSerializerForFaultsSpecified;
        private bool m_Wrapped;
        private bool m_WrappedSpecified;

        /// <summary>
        /// Control whether asynchronous proxy will be generated
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlElement()]
        public bool GenerateAsynchronousMethods
        {
            get
            {
                return m_GenerateAsynchronousMethods;
            }
            set
            {
                m_GenerateAsynchronousMethods = value;

                // GenerateAsynchronousMethods and GenerateTaskBasedAsynchronousMethod are mutually exclusive.
                if (GenerateAsynchronousMethods)
                {
                    GenerateTaskBasedAsynchronousMethod = false;
                }
            }
        }

        /// <summary>
        /// Control whether task-based async operations will be generated
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlElement()]
        public bool GenerateTaskBasedAsynchronousMethod
        {
            get
            {
                return m_GenerateTaskBasedAsynchronousMethod;
            }
            set
            {
                // In order to maximally keep compatible with Dev10 and previous VS, if GenerateTaskBasedAsynchronousMethod is false,
                // we will not persist it.
                m_GenerateTaskBasedAsynchronousMethod = value;
                m_GenerateTaskBasedAsynchronousMethodSpecified = value;

                // GenerateAsynchronousMethods and GenerateTaskBasedAsynchronousMethod are mutually exclusive.
                if (GenerateTaskBasedAsynchronousMethod)
                {
                    GenerateAsynchronousMethods = false;
                }
            }
        }

        /// <summary>
        /// Is GenerateTaskBasedAsynchronousMethod specified?
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlIgnore()]
        public bool GenerateTaskBasedAsynchronousMethodSpecified
        {
            get
            {
                return m_GenerateTaskBasedAsynchronousMethodSpecified;
            }
        }

        /// <summary>
        /// control whether to generate INotifyPropertyChanged interface on data contract types
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlElement()]
        public bool EnableDataBinding
        {
            get
            {
                return m_EnableDataBinding;
            }
            set
            {
                m_EnableDataBinding = value;
            }
        }

        /// <summary>
        /// contains a list of types which will be excluded when the design time tool matches types automatically
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlArray(ElementName = "ExcludedTypes")]
        [XmlSerialization.XmlArrayItem("ExcludedType", typeof(ReferencedType))]
        public List<ReferencedType> ExcludedTypeList
        {
            get
            {
                if (m_ExcludedTypeList == null)
                {
                    m_ExcludedTypeList = new List<ReferencedType>();
                }
                return m_ExcludedTypeList;
            }
        }

        /// <summary>
        /// control whether the data contract serializer should import non-DataContract types as IXmlSerializable types
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlElement()]
        public bool ImportXmlTypes
        {
            get
            {
                return m_ImportXmlTypes;
            }
            set
            {
                m_ImportXmlTypes = value;
            }
        }

        /// <summary>
        /// control whether to generate internal types
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlElement()]
        public bool GenerateInternalTypes
        {
            get
            {
                return m_GenerateInternalTypes;
            }
            set
            {
                m_GenerateInternalTypes = value;
            }
        }

        /// <summary>
        /// control whether to generate message contract types
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlElement()]
        public bool GenerateMessageContracts
        {
            get
            {
                return m_GenerateMessageContracts;
            }
            set
            {
                m_GenerateMessageContracts = value;
            }
        }

        /// <summary>
        /// namespace mapping between metadata namespace and clr namespace
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlArray(ElementName = "NamespaceMappings")]
        [XmlSerialization.XmlArrayItem("NamespaceMapping", typeof(NamespaceMapping))]
        public List<NamespaceMapping> NamespaceMappingList
        {
            get
            {
                if (m_NamespaceMappingList == null)
                {
                    m_NamespaceMappingList = new List<NamespaceMapping>();
                }
                return m_NamespaceMappingList;
            }
        }

        /// <summary>
        ///  known collection types which will be used by code generator
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlArray(ElementName = "CollectionMappings")]
        [XmlSerialization.XmlArrayItem("CollectionMapping", typeof(ReferencedCollectionType))]
        public List<ReferencedCollectionType> CollectionMappingList
        {
            get
            {
                if (m_CollectionMappingList == null)
                {
                    m_CollectionMappingList = new List<ReferencedCollectionType>();
                }
                return m_CollectionMappingList;
            }
        }

        /// <summary>
        /// whether class need be marked with Serializable attribute
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlElement()]
        public bool GenerateSerializableTypes
        {
            get
            {
                return m_GenerateSerializableTypes;
            }
            set
            {
                m_GenerateSerializableTypes = value;
            }
        }

        /// <summary>
        /// select serializer between DataContractSerializer or XmlSerializer
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlElement()]
        public ProxySerializerType Serializer
        {
            get
            {
                return m_Serializer;
            }
            set
            {
                m_Serializer = value;
            }
        }

        /// <summary>
        /// Control whether or not to UseSerializerForFaults.  The System.ServiceModel.FaultImportOptions 
        /// will set its UseMessageFormat Property using this value.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlElement()]
        public bool UseSerializerForFaults
        {
            get
            {
                if (m_UseSerializerForFaultsSpecified)
                {
                    return m_UseSerializerForFaults;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                m_UseSerializerForFaultsSpecified = true;
                m_UseSerializerForFaults = value;
            }
        }

        /// <summary>
        /// Is UseSerializerForFaults specified?
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlIgnore()]
        public bool UseSerializerForFaultsSpecified
        {
            get
            {
                return m_UseSerializerForFaultsSpecified;
            }
        }

        /// <summary>
        /// Control whether or not to WrappedOption.  The System.ServiceModel.Channels.WrappedOption
        /// will set its WrappedFlag Property using this value.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlElement()]
        public bool Wrapped
        {
            get
            {
                if (m_WrappedSpecified)
                {
                    return m_Wrapped;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                m_WrappedSpecified = true;
                m_Wrapped = value;
            }
        }

        /// <summary>
        /// Is WrappedOption specified?
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlIgnore()]
        public bool WrappedSpecified
        {
            get
            {
                return m_WrappedSpecified;
            }
        }

        /// <summary>
        /// Whether we will scan all dependent assemblies for type sharing
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlElement()]
        public bool ReferenceAllAssemblies
        {
            get
            {
                return m_ReferenceAllAssemblies;
            }
            set
            {
                m_ReferenceAllAssemblies = value;
            }
        }

        /// <summary>
        ///  controll DataContract type sharing
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlArray(ElementName = "ReferencedAssemblies")]
        [XmlSerialization.XmlArrayItem("ReferencedAssembly", typeof(ReferencedAssembly))]
        public List<ReferencedAssembly> ReferencedAssemblyList
        {
            get
            {
                if (m_ReferencedAssemblyList == null)
                {
                    m_ReferencedAssemblyList = new List<ReferencedAssembly>();
                }
                return m_ReferencedAssemblyList;
            }
        }

        /// <summary>
        ///  controll DataContract type sharing
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlArray(ElementName = "ReferencedDataContractTypes")]
        [XmlSerialization.XmlArrayItem("ReferencedDataContractType", typeof(ReferencedType))]
        public List<ReferencedType> ReferencedDataContractTypeList
        {
            get
            {
                if (m_ReferencedDataContractTypeList == null)
                {
                    m_ReferencedDataContractTypeList = new List<ReferencedType>();
                }
                return m_ReferencedDataContractTypeList;
            }
        }

        /// <summary>
        /// control service contract type sharing
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        [XmlSerialization.XmlArray(ElementName = "ServiceContractMappings")]
        [XmlSerialization.XmlArrayItem("ServiceContractMapping", typeof(ContractMapping))]
        public List<ContractMapping> ServiceContractMappingList
        {
            get
            {
                if (m_ServiceContractMappingList == null)
                {
                    m_ServiceContractMappingList = new List<ContractMapping>();
                }
                return m_ServiceContractMappingList;
            }
        }


        /// <summary>
        /// Serializer used in proxy generator
        /// </summary>
        /// <remarks></remarks>
        public enum ProxySerializerType
        {
            [XmlSerialization.XmlEnum(Name = "Auto")]
            Auto = 0,

            [XmlSerialization.XmlEnum(Name = "DataContractSerializer")]
            DataContractSerializer = 1,

            [XmlSerialization.XmlEnum(Name = "XmlSerializer")]
            XmlSerializer = 2,
        }
    }
}

