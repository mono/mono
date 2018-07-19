//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security.Tokens;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    
    using System.Xml;
    using System.Runtime.CompilerServices;

    class SecurityStandardsManager 
    {
        static SecurityStandardsManager instance;

        readonly SecureConversationDriver secureConversationDriver;
        readonly TrustDriver trustDriver;
        readonly SignatureTargetIdManager idManager;
        readonly MessageSecurityVersion messageSecurityVersion;
        readonly WSUtilitySpecificationVersion wsUtilitySpecificationVersion;
        readonly SecurityTokenSerializer tokenSerializer;
        WSSecurityTokenSerializer wsSecurityTokenSerializer;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public SecurityStandardsManager()
            : this(WSSecurityTokenSerializer.DefaultInstance)
        {
        }

        public SecurityStandardsManager(SecurityTokenSerializer tokenSerializer)
            : this(MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11, tokenSerializer)
        {
        }

        public SecurityStandardsManager(MessageSecurityVersion messageSecurityVersion, SecurityTokenSerializer tokenSerializer)
        {
            if (messageSecurityVersion == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("messageSecurityVersion"));
            if (tokenSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenSerializer");

            this.messageSecurityVersion = messageSecurityVersion;
            this.tokenSerializer = tokenSerializer;
            if (messageSecurityVersion.SecureConversationVersion == SecureConversationVersion.WSSecureConversation13)
                this.secureConversationDriver = new WSSecureConversationDec2005.DriverDec2005();
            else
                this.secureConversationDriver = new WSSecureConversationFeb2005.DriverFeb2005();

            if (this.SecurityVersion == SecurityVersion.WSSecurity10 || this.SecurityVersion == SecurityVersion.WSSecurity11)
            {
                this.idManager = WSSecurityJan2004.IdManager.Instance;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("messageSecurityVersion", SR.GetString(SR.MessageSecurityVersionOutOfRange)));
            }

            this.wsUtilitySpecificationVersion = WSUtilitySpecificationVersion.Default;
            if (messageSecurityVersion.MessageSecurityTokenVersion.TrustVersion == TrustVersion.WSTrust13)
                this.trustDriver = new WSTrustDec2005.DriverDec2005(this);
            else
                this.trustDriver = new WSTrustFeb2005.DriverFeb2005(this);
        }

        public static SecurityStandardsManager DefaultInstance
        {
            get
            {
                if (instance == null)
                    instance = new SecurityStandardsManager();
                return instance;
            }
        }

        public SecurityVersion SecurityVersion
        {
            get { return this.messageSecurityVersion == null ? null : this.messageSecurityVersion.SecurityVersion; }
        }

        public MessageSecurityVersion MessageSecurityVersion
        {
            get { return this.messageSecurityVersion; }
        }

        public TrustVersion TrustVersion
        {
            get { return this.messageSecurityVersion.TrustVersion; }
        }

        public SecureConversationVersion SecureConversationVersion
        {
            get { return this.messageSecurityVersion.SecureConversationVersion; }
        }

        internal SecurityTokenSerializer SecurityTokenSerializer
        {
            get { return this.tokenSerializer; }
        }

        internal WSUtilitySpecificationVersion WSUtilitySpecificationVersion
        {
            get { return this.wsUtilitySpecificationVersion; }
        }

        internal SignatureTargetIdManager IdManager
        {
            get { return this.idManager; }
        }

        internal SecureConversationDriver SecureConversationDriver
        {
            get { return this.secureConversationDriver; }
        }

        internal TrustDriver TrustDriver
        {
            get { return this.trustDriver; }
        }

        WSSecurityTokenSerializer WSSecurityTokenSerializer
        {
            get 
            {
                if (this.wsSecurityTokenSerializer == null)
                {
                    WSSecurityTokenSerializer wsSecurityTokenSerializer = this.tokenSerializer as WSSecurityTokenSerializer;
                    if (wsSecurityTokenSerializer == null)
                    {
                        wsSecurityTokenSerializer = new WSSecurityTokenSerializer(this.SecurityVersion);
                    }
                    this.wsSecurityTokenSerializer = wsSecurityTokenSerializer;
                }
                return this.wsSecurityTokenSerializer; 
            }
        }

        internal bool TryCreateKeyIdentifierClauseFromTokenXml(XmlElement element, SecurityTokenReferenceStyle tokenReferenceStyle, out SecurityKeyIdentifierClause securityKeyIdentifierClause)
        {
            return this.WSSecurityTokenSerializer.TryCreateKeyIdentifierClauseFromTokenXml(element, tokenReferenceStyle, out securityKeyIdentifierClause);
        }

        internal SecurityKeyIdentifierClause CreateKeyIdentifierClauseFromTokenXml(XmlElement element, SecurityTokenReferenceStyle tokenReferenceStyle)
        {
            return this.WSSecurityTokenSerializer.CreateKeyIdentifierClauseFromTokenXml(element, tokenReferenceStyle);
        }

        internal SendSecurityHeader CreateSendSecurityHeader(Message message,
            string actor, bool mustUnderstand, bool relay,
            SecurityAlgorithmSuite algorithmSuite, MessageDirection direction)
        {
            return this.SecurityVersion.CreateSendSecurityHeader(message, actor, mustUnderstand, relay, this, algorithmSuite, direction);
        }

        internal ReceiveSecurityHeader CreateReceiveSecurityHeader(Message message,
            string actor,
            SecurityAlgorithmSuite algorithmSuite, MessageDirection direction)
        {
            ReceiveSecurityHeader header = TryCreateReceiveSecurityHeader(message, actor, algorithmSuite, direction);
            if (header == null)
            {
                if (String.IsNullOrEmpty(actor))
                    throw System.ServiceModel.Diagnostics.TraceUtility.ThrowHelperError(new MessageSecurityException(
                        SR.GetString(SR.UnableToFindSecurityHeaderInMessageNoActor)), message);
                else
                    throw System.ServiceModel.Diagnostics.TraceUtility.ThrowHelperError(new MessageSecurityException(
                        SR.GetString(SR.UnableToFindSecurityHeaderInMessage, actor)), message);
            }
            return header;
        }

        internal ReceiveSecurityHeader TryCreateReceiveSecurityHeader(Message message,
            string actor,
            SecurityAlgorithmSuite algorithmSuite, MessageDirection direction)
        {
            return this.SecurityVersion.TryCreateReceiveSecurityHeader(message, actor, this, algorithmSuite, direction);
        }

        internal bool DoesMessageContainSecurityHeader(Message message)
        {
            return this.SecurityVersion.DoesMessageContainSecurityHeader(message);
        }

        internal bool TryGetSecurityContextIds(Message message, string[] actors, bool isStrictMode, ICollection<UniqueId> results)
        {
            if (results == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("results");
            }
            SecureConversationDriver driver = this.SecureConversationDriver;
            int securityHeaderIndex = this.SecurityVersion.FindIndexOfSecurityHeader(message, actors);
            if (securityHeaderIndex < 0)
            {
                return false;
            }
            bool addedContextIds = false;
            using (XmlDictionaryReader reader = message.Headers.GetReaderAtHeader(securityHeaderIndex))
            {
                if (!reader.IsStartElement())
                {
                    return false;
                }
                if (reader.IsEmptyElement)
                {
                    return false;
                }
                reader.ReadStartElement();
                while (reader.IsStartElement())
                {
                    if (driver.IsAtSecurityContextToken(reader))
                    {
                        results.Add(driver.GetSecurityContextTokenId(reader));
                        addedContextIds = true;
                        if (isStrictMode)
                        {
                            break;
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }
            return addedContextIds;
        }
    }
}


