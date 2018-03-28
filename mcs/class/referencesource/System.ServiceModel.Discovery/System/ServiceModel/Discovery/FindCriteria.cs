//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Description;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using SR2 = System.ServiceModel.Discovery.SR;

    [Fx.Tag.XamlVisible(false)]
    public class FindCriteria
    {
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly Uri ScopeMatchByExact = new Uri(ProtocolStrings.VersionInternal.ScopeMatchByExact);

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly Uri ScopeMatchByLdap = new Uri(ProtocolStrings.VersionInternal.ScopeMatchByLdap);

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly Uri ScopeMatchByPrefix = new Uri(ProtocolStrings.VersionInternal.ScopeMatchByPrefix);

        [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldBeSpelledCorrectly)]
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly Uri ScopeMatchByUuid = new Uri(ProtocolStrings.VersionInternal.ScopeMatchByUuid);

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly Uri ScopeMatchByNone = new Uri(ProtocolStrings.VersionInternal.ScopeMatchByNone);

        ContractTypeNameCollection contractTypeNames;
        NonNullItemCollection<XElement> extensions;
        Uri scopeMatchBy;
        ScopeCollection scopes;
        int maxResults;
        TimeSpan duration;

        public FindCriteria()
        {
            Initialize(null, DiscoveryDefaults.ScopeMatchBy);
        }

        public FindCriteria(Type contractType)
        {
            if (contractType == null)
            {
                throw FxTrace.Exception.ArgumentNull("contractType");
            }

            ContractTypeNameCollection contractTypeNamesArg = new ContractTypeNameCollection();
            contractTypeNamesArg.Add(GetContractTypeName(contractType));

            Initialize(contractTypeNamesArg, DiscoveryDefaults.ScopeMatchBy);
        }

        public Collection<XmlQualifiedName> ContractTypeNames
        {
            get
            {
                if (this.contractTypeNames == null)
                {
                    this.contractTypeNames = new ContractTypeNameCollection();
                }

                return this.contractTypeNames;
            }
        }

        public Collection<XElement> Extensions
        {
            get
            {
                if (this.extensions == null)
                {
                    this.extensions = new NonNullItemCollection<XElement>();
                }

                return this.extensions;
            }
        }

        public Uri ScopeMatchBy
        {
            get
            {
                return this.scopeMatchBy;
            }

            set
            {
                if (value == null)
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }

                this.scopeMatchBy = value;
            }
        }

        public Collection<Uri> Scopes
        {
            get
            {
                if (this.scopes == null)
                {
                    this.scopes = new ScopeCollection();
                }

                return this.scopes;
            }
        }

        public int MaxResults
        {
            get
            {
                return this.maxResults;
            }
            set
            {
                if (value <= 0)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("value", value, SR2.DiscoveryFindMaxResultsLessThanZero);
                }
                this.maxResults = value;
            }
        }

        public TimeSpan Duration
        {
            get
            {
                return this.duration;
            }
            set
            {
                if (value.CompareTo(TimeSpan.Zero) <= 0)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("duration", value, SR2.DiscoveryFindDurationLessThanZero);
                }
                this.duration = value;
            }
        }

        internal Collection<Uri> InternalScopes
        {
            get
            {
                return this.scopes;
            }
        }

        public static FindCriteria CreateMetadataExchangeEndpointCriteria()
        {
            FindCriteria criteria = new FindCriteria();
            criteria.ContractTypeNames.Add(EndpointDiscoveryMetadata.MetadataContractName);

            return criteria;
        }

        public static FindCriteria CreateMetadataExchangeEndpointCriteria(Type contractType)
        {
            FindCriteria criteria = CreateMetadataExchangeEndpointCriteria();
            criteria.Scopes.Add(GetContractTypeNameScope(GetContractTypeName(contractType)));

            return criteria;
        }

        public static FindCriteria CreateMetadataExchangeEndpointCriteria(IEnumerable<XmlQualifiedName> contractTypeNames)
        {
            if (contractTypeNames == null)
            {
                throw FxTrace.Exception.ArgumentNull("contractTypeNames");
            }

            FindCriteria criteria = CreateMetadataExchangeEndpointCriteria();
            foreach (XmlQualifiedName item in contractTypeNames)
            {
                if (item == null)
                {
                    throw FxTrace.Exception.ArgumentNull("item");
                }

                criteria.Scopes.Add(GetContractTypeNameScope(item));
            }

            return criteria;
        }

        public bool IsMatch(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            if (endpointDiscoveryMetadata == null)
            {
                throw FxTrace.Exception.ArgumentNull("endpointDiscoveryMetadata");
            }

            return IsMatch(endpointDiscoveryMetadata, ScopeCompiler.CompileMatchCriteria(this.scopes, this.scopeMatchBy));
        }

        internal bool IsMatch(EndpointDiscoveryMetadata endpointDiscoveryMetadata, CompiledScopeCriteria[] compiledScopeMatchCriterias)
        {
            return (MatchTypes(endpointDiscoveryMetadata, this.contractTypeNames) &&
                MatchScopes(endpointDiscoveryMetadata, compiledScopeMatchCriterias, this.scopeMatchBy));
        }

        static bool MatchTypes(EndpointDiscoveryMetadata endpointDiscoveryMetadata, Collection<XmlQualifiedName> contractTypeNames)
        {
            if ((contractTypeNames == null) || (contractTypeNames.Count == 0))
            {
                return true;
            }

            if ((endpointDiscoveryMetadata.InternalContractTypeNames == null) ||
                (endpointDiscoveryMetadata.InternalContractTypeNames.Count == 0))
            {
                return false;
            }

            foreach (XmlQualifiedName contractTypeName in contractTypeNames)
            {
                if (!endpointDiscoveryMetadata.InternalContractTypeNames.Contains(contractTypeName))
                {
                    return false;
                }
            }

            return true;
        }

        static bool MatchScopes(EndpointDiscoveryMetadata endpointDiscoveryMetadata, CompiledScopeCriteria[] compiledScopeMatchCriterias, Uri scopeMatchBy)
        {
            if (compiledScopeMatchCriterias == null)
            {
                if (scopeMatchBy != FindCriteria.ScopeMatchByNone)
                {
                    return true;
                }
                else
                {
                    // the criteria matches any service with no scopes defined
                    return endpointDiscoveryMetadata.Scopes.Count == 0;
                }
            }

            if (scopeMatchBy == FindCriteria.ScopeMatchByNone)
            {
                // if scopeMatchBy is None, the Probe shouldn't have any Scopes defined
                return false;
            }

            string[] compiledScopes;
            if (endpointDiscoveryMetadata.IsOpen)
            {
                compiledScopes = endpointDiscoveryMetadata.CompiledScopes;
            }
            else
            {
                compiledScopes = ScopeCompiler.Compile(endpointDiscoveryMetadata.Scopes);
            }

            if (compiledScopes == null)
            {
                // non-zero scopes in the criteria, but zero scopes in the metadata
                return false;
            }

            for (int i = 0; i < compiledScopeMatchCriterias.Length; i++)
            {
                if (!ScopeCompiler.IsMatch(compiledScopeMatchCriterias[i], compiledScopes))
                {
                    return false;
                }
            }

            return true;
        }

        [Fx.Tag.Throws(typeof(XmlException), "throws on incorrect xml data")]
        internal void ReadFrom(DiscoveryVersion discoveryVersion, XmlReader reader)
        {            
            if (discoveryVersion == null)
            {
                throw FxTrace.Exception.ArgumentNull("discoveryVersion");
            }
            if (reader == null)
            {
                throw FxTrace.Exception.ArgumentNull("reader");
            }

            this.contractTypeNames = null;
            this.scopes = null;
            this.scopeMatchBy = DiscoveryDefaults.ScopeMatchBy;
            this.extensions = null;
            this.duration = TimeSpan.MaxValue;
            this.maxResults = int.MaxValue;

            reader.MoveToContent();
            if (reader.IsEmptyElement)
            {
                reader.Read();
                return;
            }

            int startDepth = reader.Depth;
            reader.ReadStartElement();

            if (reader.IsStartElement(ProtocolStrings.SchemaNames.TypesElement, discoveryVersion.Namespace))
            {
                this.contractTypeNames = new ContractTypeNameCollection();
                SerializationUtility.ReadContractTypeNames(this.contractTypeNames, reader);
            }

            if (reader.IsStartElement(ProtocolStrings.SchemaNames.ScopesElement, discoveryVersion.Namespace))
            {
                this.scopes = new ScopeCollection();
                Uri scopeMatchBy = SerializationUtility.ReadScopes(this.scopes, reader);
                if (scopeMatchBy != null)
                {
                    this.scopeMatchBy = discoveryVersion.Implementation.ToVersionIndependentScopeMatchBy(scopeMatchBy);
                }
            }

            while (true)
            {
                reader.MoveToContent();

                if ((reader.NodeType == XmlNodeType.EndElement) && (reader.Depth == startDepth))
                {
                    break;
                }
                else if (reader.IsStartElement(ProtocolStrings.SchemaNames.MaxResultsElement, ProtocolStrings.VersionInternal.Namespace))
                {
                    this.maxResults = SerializationUtility.ReadMaxResults(reader);
                }
                else if (reader.IsStartElement(ProtocolStrings.SchemaNames.DurationElement, ProtocolStrings.VersionInternal.Namespace))
                {
                    this.duration = SerializationUtility.ReadDuration(reader);
                }
                else if (reader.IsStartElement())
                {
                    XElement xElement = XElement.ReadFrom(reader) as XElement;
                    Extensions.Add(xElement);
                }
                else
                {
                    reader.Read();
                }
            }

            reader.ReadEndElement();         
        }

        internal void WriteTo(DiscoveryVersion discoveryVersion, XmlWriter writer)
        {
            if (discoveryVersion == null)
            {
                throw FxTrace.Exception.ArgumentNull("discoveryVersion");
            }
            if (writer == null)
            {
                throw FxTrace.Exception.ArgumentNull("writer");
            }

            SerializationUtility.WriteContractTypeNames(discoveryVersion, this.contractTypeNames, writer);                   

            SerializationUtility.WriteScopes(discoveryVersion, this.scopes, this.scopeMatchBy, writer);

            if (this.maxResults != int.MaxValue)
            {
                writer.WriteElementString(
                    ProtocolStrings.SchemaNames.MaxResultsElement, 
                    ProtocolStrings.VersionInternal.Namespace, 
                    this.maxResults.ToString(CultureInfo.InvariantCulture));
            }
            if (this.duration != TimeSpan.MaxValue)
            {
                writer.WriteElementString(
                    ProtocolStrings.SchemaNames.DurationElement, 
                    ProtocolStrings.VersionInternal.Namespace, 
                    XmlConvert.ToString(this.Duration));
            }

            if (this.extensions != null)
            {
                foreach (XElement xElement in Extensions)
                {
                    xElement.WriteTo(writer);
                }
            }
        }

        internal static XmlQualifiedName GetContractTypeName(Type contractType)
        {
            if (contractType == null)
            {
                throw FxTrace.Exception.ArgumentNull("contractType");
            }

            ContractDescription contract = ContractDescription.GetContract(contractType);
            return new XmlQualifiedName(contract.Name, contract.Namespace);
        }

        internal static Uri GetContractTypeNameScope(XmlQualifiedName contractTypeName)
        {
            Fx.Assert(contractTypeName != null, "The contractTypeName must be non null.");

            return new Uri(string.Format(CultureInfo.InvariantCulture, "urn:{0}", contractTypeName.ToString()));
        }

        internal FindCriteria Clone()
        {
            FindCriteria findCriteriaClone = new FindCriteria();

            foreach (Uri scope in this.Scopes)
            {
                findCriteriaClone.Scopes.Add(scope);
            }

            foreach (XmlQualifiedName contractTypeName in this.ContractTypeNames)
            {
                findCriteriaClone.ContractTypeNames.Add(new XmlQualifiedName(contractTypeName.Name, contractTypeName.Namespace));
            }

            foreach (XElement extension in this.Extensions)
            {
                findCriteriaClone.Extensions.Add(new XElement(extension));
            }

            findCriteriaClone.ScopeMatchBy = this.ScopeMatchBy;
            findCriteriaClone.Duration = this.Duration;
            findCriteriaClone.MaxResults = this.MaxResults;

            return findCriteriaClone;
        }

        void Initialize(ContractTypeNameCollection contractTypeNames, Uri scopeMatchBy)
        {
            this.contractTypeNames = contractTypeNames;
            this.scopeMatchBy = scopeMatchBy;
            this.maxResults = int.MaxValue;
            this.duration = DiscoveryDefaults.DiscoveryOperationDuration;
        }        
    }
}
