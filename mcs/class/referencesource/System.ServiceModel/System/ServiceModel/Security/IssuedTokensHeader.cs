//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.ServiceModel;
    using System.Xml;
    using System.IdentityModel.Selectors;
    using System.ServiceModel.Diagnostics;

    sealed class IssuedTokensHeader : MessageHeader
    {
        ReadOnlyCollection<RequestSecurityTokenResponse> tokenIssuances;
        SecurityStandardsManager standardsManager;
        string actor;
        bool mustUnderstand;
        bool relay;
        bool isRefParam;

        public IssuedTokensHeader(RequestSecurityTokenResponse tokenIssuance, MessageSecurityVersion version, SecurityTokenSerializer tokenSerializer)
            : this(tokenIssuance, new SecurityStandardsManager(version, tokenSerializer))
        {
        }


        public IssuedTokensHeader(RequestSecurityTokenResponse tokenIssuance, SecurityStandardsManager standardsManager)
            : base()
        {
            if (tokenIssuance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenIssuance");
            }
            Collection<RequestSecurityTokenResponse> coll = new Collection<RequestSecurityTokenResponse>();
            coll.Add(tokenIssuance);
            Initialize(coll, standardsManager);
        }

        public IssuedTokensHeader(IEnumerable<RequestSecurityTokenResponse> tokenIssuances, SecurityStandardsManager standardsManager)
            : base()
        {
            if (tokenIssuances == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenIssuances");
            }
            int index = 0;
            Collection<RequestSecurityTokenResponse> coll = new Collection<RequestSecurityTokenResponse>();
            foreach (RequestSecurityTokenResponse rstr in tokenIssuances)
            {
                if (rstr == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(String.Format(CultureInfo.InvariantCulture, "tokenIssuances[{0}]", index));
                }
                coll.Add(rstr);
                ++index;
            }
            Initialize(coll, standardsManager);
        }

        void Initialize(Collection<RequestSecurityTokenResponse> coll, SecurityStandardsManager standardsManager)
        {
            if (standardsManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("standardsManager"));
            }
            this.standardsManager = standardsManager;
            this.tokenIssuances = new ReadOnlyCollection<RequestSecurityTokenResponse>(coll);
            this.actor = base.Actor;
            this.mustUnderstand = base.MustUnderstand;
            this.relay = base.Relay;
        }


        public IssuedTokensHeader(XmlReader xmlReader, MessageVersion version, SecurityStandardsManager standardsManager)
            : base()
        {
            if (xmlReader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("xmlReader");
            }
            if (standardsManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("standardsManager"));
            }
            this.standardsManager = standardsManager;
            XmlDictionaryReader reader = XmlDictionaryReader.CreateDictionaryReader(xmlReader);
            MessageHeader.GetHeaderAttributes(reader, version, out this.actor, out this.mustUnderstand, out this.relay, out this.isRefParam);
            reader.ReadStartElement(this.Name, this.Namespace);
            Collection<RequestSecurityTokenResponse> coll = new Collection<RequestSecurityTokenResponse>();
            if (this.standardsManager.TrustDriver.IsAtRequestSecurityTokenResponseCollection(reader))
            {
                RequestSecurityTokenResponseCollection rstrColl = this.standardsManager.TrustDriver.CreateRequestSecurityTokenResponseCollection(reader);
                foreach (RequestSecurityTokenResponse rstr in rstrColl.RstrCollection)
                {
                    coll.Add(rstr);
                }
            }
            else
            {
                RequestSecurityTokenResponse rstr = this.standardsManager.TrustDriver.CreateRequestSecurityTokenResponse(reader);
                coll.Add(rstr);
            }
            this.tokenIssuances = new ReadOnlyCollection<RequestSecurityTokenResponse>(coll);
            reader.ReadEndElement();
        }


        public ReadOnlyCollection<RequestSecurityTokenResponse> TokenIssuances
        {
            get
            {
                return this.tokenIssuances;
            }
        }

        public override string Actor
        {
            get 
            {
                return this.actor;
            }
        }

        public override bool IsReferenceParameter
        {
            get
            {
                return this.isRefParam;
            }
        }

        public override bool MustUnderstand
        {
            get 
            { 
                return this.mustUnderstand; 
            }

        }

        public override bool Relay
        {
            get 
            { 
                return this.relay; 
            }
        }


        public override string Name
        {
            get
            {
                return this.standardsManager.TrustDriver.IssuedTokensHeaderName;
            }
        }

        public override string Namespace
        {
            get
            {
                return this.standardsManager.TrustDriver.IssuedTokensHeaderNamespace;
            }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            if (this.tokenIssuances.Count == 1)
            {
                this.standardsManager.TrustDriver.WriteRequestSecurityTokenResponse(this.tokenIssuances[0], writer);
            }
            else
            {
                RequestSecurityTokenResponseCollection rstrCollection = new RequestSecurityTokenResponseCollection(this.tokenIssuances, this.standardsManager);
                rstrCollection.WriteTo(writer);
            }
        }

        internal static Collection<RequestSecurityTokenResponse> ExtractIssuances(Message message, MessageSecurityVersion version, WSSecurityTokenSerializer tokenSerializer, string[] actors, XmlQualifiedName expectedAppliesToQName)
        {
            return ExtractIssuances(message, new SecurityStandardsManager(version, tokenSerializer), actors, expectedAppliesToQName);
        }

        // if expectedAppliesToQName is null all issuances matching the actors are returned.
        internal static Collection<RequestSecurityTokenResponse> ExtractIssuances(Message message, SecurityStandardsManager standardsManager, string[] actors, XmlQualifiedName expectedAppliesToQName)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (standardsManager == null)
            {
                standardsManager = SecurityStandardsManager.DefaultInstance;
            }
            if (actors == null)
            {
                throw TraceUtility.ThrowHelperArgumentNull("actors", message);
            }
            Collection<RequestSecurityTokenResponse> issuances = new Collection<RequestSecurityTokenResponse>();
            for (int i = 0; i < message.Headers.Count; ++i)
            {
                if (message.Headers[i].Name == standardsManager.TrustDriver.IssuedTokensHeaderName && message.Headers[i].Namespace == standardsManager.TrustDriver.IssuedTokensHeaderNamespace)
                {
                    bool isValidActor = false;
                    for (int j = 0; j < actors.Length; ++j)
                    {
                        if (actors[j] == message.Headers[i].Actor)
                        {
                            isValidActor = true;
                            break;
                        }
                    }
                    if (!isValidActor)
                    {
                        continue;
                    }
                    IssuedTokensHeader issuedTokensHeader = new IssuedTokensHeader(message.Headers.GetReaderAtHeader(i), message.Version, standardsManager);
                    for (int k = 0; k < issuedTokensHeader.TokenIssuances.Count; ++k)
                    {
                        bool isMatch;
                        if (expectedAppliesToQName != null)
                        {
                            string issuanceAppliesToName;
                            string issuanceAppliesToNs;
                            issuedTokensHeader.TokenIssuances[k].GetAppliesToQName(out issuanceAppliesToName, out issuanceAppliesToNs);
                            if (issuanceAppliesToName == expectedAppliesToQName.Name && issuanceAppliesToNs == expectedAppliesToQName.Namespace)
                            {
                                isMatch = true;
                            }
                            else
                            {
                                isMatch = false;
                            }
                        }
                        else
                        {
                            isMatch = true;
                        }
                        if (isMatch)
                        {
                            issuances.Add(issuedTokensHeader.TokenIssuances[k]);
                        }
                    }
                }
            }
            return issuances;
        }
    }
}
