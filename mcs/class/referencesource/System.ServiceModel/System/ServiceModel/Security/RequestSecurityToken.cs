//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel;
    using System.Security.Cryptography.Xml;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Selectors;
    using System.ServiceModel.Security.Tokens;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using System.Xml.Schema;
    using System.Xml;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.ServiceModel.Security;
    using System.Globalization;
    using System.ServiceModel.Dispatcher;
    using System.Security.Authentication.ExtendedProtection;


    class RequestSecurityToken : BodyWriter
    {
        string context;
        string tokenType;
        string requestType;
        SecurityToken entropyToken;
        BinaryNegotiation negotiationData;
        XmlElement rstXml;
        IList<XmlElement> requestProperties;
        byte[] cachedWriteBuffer;
        int cachedWriteBufferLength;
        int keySize;
        Message message;
        SecurityKeyIdentifierClause renewTarget;
        SecurityKeyIdentifierClause closeTarget;
        OnGetBinaryNegotiationCallback onGetBinaryNegotiation;
        SecurityStandardsManager standardsManager;
        bool isReceiver;
        bool isReadOnly;
        object appliesTo;
        DataContractSerializer appliesToSerializer;
        Type appliesToType;

        object thisLock = new Object();

        public RequestSecurityToken()
            : this(SecurityStandardsManager.DefaultInstance)
        {
        }

        public RequestSecurityToken(MessageSecurityVersion messageSecurityVersion, SecurityTokenSerializer securityTokenSerializer)
            : this(SecurityUtils.CreateSecurityStandardsManager(messageSecurityVersion, securityTokenSerializer))
        {
        }


        public RequestSecurityToken(MessageSecurityVersion messageSecurityVersion, 
                                    SecurityTokenSerializer securityTokenSerializer,
                                    XmlElement requestSecurityTokenXml,
                                    string context,
                                    string tokenType,
                                    string requestType,
                                    int keySize,
                                    SecurityKeyIdentifierClause renewTarget,
                                    SecurityKeyIdentifierClause closeTarget)
            : this(SecurityUtils.CreateSecurityStandardsManager(messageSecurityVersion, securityTokenSerializer),
                   requestSecurityTokenXml,
                   context,
                   tokenType,
                   requestType,
                   keySize,
                   renewTarget,
                   closeTarget)
        {
        }

        public RequestSecurityToken(XmlElement requestSecurityTokenXml,
                                    string context,
                                    string tokenType,
                                    string requestType,
                                    int keySize,
                                    SecurityKeyIdentifierClause renewTarget,
                                    SecurityKeyIdentifierClause closeTarget)
            : this(SecurityStandardsManager.DefaultInstance,
                   requestSecurityTokenXml,
                   context,
                   tokenType,
                   requestType,
                   keySize,
                   renewTarget,
                   closeTarget)
        {
        }

        internal RequestSecurityToken(SecurityStandardsManager standardsManager, 
                                      XmlElement rstXml,
                                      string context,
                                      string tokenType,
                                      string requestType,
                                      int keySize,
                                      SecurityKeyIdentifierClause renewTarget,
                                      SecurityKeyIdentifierClause closeTarget)
            : base(true)
        {
            if (standardsManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("standardsManager"));
            }
            this.standardsManager = standardsManager;
            if (rstXml == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rstXml");
            this.rstXml = rstXml;
            this.context = context;
            this.tokenType = tokenType;
            this.keySize = keySize;
            this.requestType = requestType;
            this.renewTarget = renewTarget;
            this.closeTarget = closeTarget;
            this.isReceiver = true;
            this.isReadOnly = true;
        }

        internal RequestSecurityToken(SecurityStandardsManager standardsManager) 
            : this(standardsManager, true)
        {
            // no op
        }

        internal RequestSecurityToken(SecurityStandardsManager standardsManager, bool isBuffered) 
            : base(isBuffered)
        {
            if (standardsManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("standardsManager"));
            }
            this.standardsManager = standardsManager;
            this.requestType = this.standardsManager.TrustDriver.RequestTypeIssue;
            this.requestProperties = null;
            this.isReceiver = false;
            this.isReadOnly = false;
        }

        public ChannelBinding GetChannelBinding()
        {
            if (this.message == null)
            {
                return null;
            }

            ChannelBindingMessageProperty channelBindingMessageProperty = null;
            ChannelBindingMessageProperty.TryGet( this.message, out channelBindingMessageProperty );
            ChannelBinding channelBinding = null;

            if ( channelBindingMessageProperty != null )
            {
                channelBinding = channelBindingMessageProperty.ChannelBinding;
            }

            return channelBinding;
        }

        /// <summary>
        /// Will hold a reference to the outbound message from which we will fish the ChannelBinding out of.
        /// </summary>
        public Message Message
        {
            get { return message; }
            set { message = value; }
        }

 
        public string Context
        {
            get
            {
                return this.context;
            }
            set 
            {
                if (this.IsReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                this.context = value;
            }
        }

        public string TokenType
        {
            get 
            {
                return this.tokenType;
            }
            set 
            {
                if (this.IsReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                this.tokenType = value;
            }   
        }

        public int KeySize
        {
            get
            {
                return this.keySize;
            }
            set
            {
                if (this.IsReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                if (value < 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", SR.GetString(SR.ValueMustBeNonNegative)));
                this.keySize = value;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        public delegate void OnGetBinaryNegotiationCallback( ChannelBinding channelBinding );
        public OnGetBinaryNegotiationCallback OnGetBinaryNegotiation
        {
            get
            {
                return this.onGetBinaryNegotiation;
            }
            set
            {
                if (this.IsReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                }
                this.onGetBinaryNegotiation = value;
            }
        }

        public IEnumerable<XmlElement> RequestProperties
        {
            get
            {
                if (this.isReceiver)
                {
                    // PreSharp 
                    #pragma warning suppress 56503
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ItemNotAvailableInDeserializedRST, "RequestProperties")));
                }
                return this.requestProperties;
            }
            set
            {
                if (this.IsReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                if (value != null)
                {
                    int index = 0;
                    Collection<XmlElement> coll = new Collection<XmlElement>();
                    foreach (XmlElement property in value)
                    {
                        if (property == null)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException(String.Format(CultureInfo.InvariantCulture, "value[{0}]", index)));
                        coll.Add(property);
                        ++index;
                    }
                    this.requestProperties = coll;
                }
                else
                {
                    this.requestProperties = null;
                }
            }
        }

        public string RequestType
        {
            get
            {
                return this.requestType;
            }
            set
            {
                if (this.IsReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                this.requestType = value;
            }
        }

        public SecurityKeyIdentifierClause RenewTarget
        {
            get
            {
                return this.renewTarget;
            }
            set
            {
                if (this.IsReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                this.renewTarget = value;
            }
        }

        public SecurityKeyIdentifierClause CloseTarget
        {
            get
            {
                return this.closeTarget;
            }
            set
            {
                if (this.IsReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                this.closeTarget = value;
            }
        }

        public XmlElement RequestSecurityTokenXml
        {
            get
            {
                if (!this.isReceiver)
                {
                    // PreSharp 
                    #pragma warning suppress 56503
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ItemAvailableInDeserializedRSTOnly, "RequestSecurityTokenXml")));
                }
                return this.rstXml;
            }
        }

        internal SecurityStandardsManager StandardsManager
        {
            get
            {
                return this.standardsManager;
            }
            set
            {
                if (this.IsReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.standardsManager = value;
            }
        }

        internal bool IsReceiver
        {
            get
            {
                return this.isReceiver;
            }
        }

        internal object AppliesTo
        {
            get
            {
                if (this.isReceiver)
                {
                    // PreSharp 
                    #pragma warning suppress 56503
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ItemNotAvailableInDeserializedRST, "AppliesTo")));
                }
                return this.appliesTo;
            }
        }

        internal DataContractSerializer AppliesToSerializer
        {
            get
            {
                if (this.isReceiver)
                {
                    // PreSharp 
                    #pragma warning suppress 56503
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ItemNotAvailableInDeserializedRST, "AppliesToSerializer")));
                }
                return this.appliesToSerializer;
            }
        }

        internal Type AppliesToType
        {
            get
            {
                if (this.isReceiver)
                {
                    // PreSharp 
                    #pragma warning suppress 56503
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ItemNotAvailableInDeserializedRST, "AppliesToType")));
                }
                return this.appliesToType;
            }
        }

        protected Object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        internal void SetBinaryNegotiation(BinaryNegotiation negotiation)
        {
            if (negotiation == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("negotiation");
            if (this.IsReadOnly)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            this.negotiationData = negotiation;
        }

        internal BinaryNegotiation GetBinaryNegotiation()
        {
            if (this.isReceiver)
            {
                return this.standardsManager.TrustDriver.GetBinaryNegotiation(this);
            }
            else if (this.negotiationData == null && this.onGetBinaryNegotiation != null)
            {
                this.onGetBinaryNegotiation(this.GetChannelBinding());
            }
            return this.negotiationData;
        }

        public SecurityToken GetRequestorEntropy()
        {
            return this.GetRequestorEntropy(null);
        }

        internal SecurityToken GetRequestorEntropy(SecurityTokenResolver resolver) 
        {
            if (this.isReceiver)
            {
                return this.standardsManager.TrustDriver.GetEntropy(this, resolver);
            }
            else
                return this.entropyToken;
        }

        public void SetRequestorEntropy(byte[] entropy)
        {
            if (this.IsReadOnly)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            this.entropyToken = (entropy != null) ? new NonceToken(entropy) : null;
        }

        internal void SetRequestorEntropy(WrappedKeySecurityToken entropyToken)
        {
            if (this.IsReadOnly)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            this.entropyToken = entropyToken;
        }

        public void SetAppliesTo<T>(T appliesTo, DataContractSerializer serializer)
        {
            if (this.IsReadOnly)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            if (appliesTo != null && serializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serializer");
            }
            this.appliesTo = appliesTo;
            this.appliesToSerializer = serializer;
            this.appliesToType = typeof(T);
        }

        public void GetAppliesToQName(out string localName, out string namespaceUri)
        {
            if (!this.isReceiver)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ItemAvailableInDeserializedRSTOnly, "MatchesAppliesTo")));
            this.standardsManager.TrustDriver.GetAppliesToQName(this, out localName, out namespaceUri);
        }

        public T GetAppliesTo<T>()
        {
            return this.GetAppliesTo<T>(DataContractSerializerDefaults.CreateSerializer(typeof(T), DataContractSerializerDefaults.MaxItemsInObjectGraph));
        }

        public T GetAppliesTo<T>(XmlObjectSerializer serializer)
        {
            if (this.isReceiver)
            {
                if (serializer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serializer");
                }
                return this.standardsManager.TrustDriver.GetAppliesTo<T>(this, serializer);
            }
            else
            {
                return (T)this.appliesTo;
            }
        }

        void OnWriteTo(XmlWriter writer)
        {
            if (this.isReceiver)
            {
                this.rstXml.WriteTo(writer);
            }
            else
            {
                this.standardsManager.TrustDriver.WriteRequestSecurityToken(this, writer);
            }
        }

        public void WriteTo(XmlWriter writer)
        {
            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            if (this.IsReadOnly)
            {
                // cache the serialized bytes to ensure repeatability
                if (this.cachedWriteBuffer == null)
                {
                    MemoryStream stream = new MemoryStream();
                    using (XmlDictionaryWriter binaryWriter = XmlDictionaryWriter.CreateBinaryWriter(stream, XD.Dictionary))
                    {
                        this.OnWriteTo(binaryWriter);
                        binaryWriter.Flush();
                        stream.Flush();
                        stream.Seek(0, SeekOrigin.Begin);
                        this.cachedWriteBuffer = stream.GetBuffer();
                        this.cachedWriteBufferLength = (int)stream.Length;
                    }
                }
                writer.WriteNode(XmlDictionaryReader.CreateBinaryReader(this.cachedWriteBuffer, 0, this.cachedWriteBufferLength, XD.Dictionary, XmlDictionaryReaderQuotas.Max), false);
            }
            else
                this.OnWriteTo(writer);
        }

        public static RequestSecurityToken CreateFrom(XmlReader reader) 
        {
            return CreateFrom(SecurityStandardsManager.DefaultInstance, reader);
        }

        public static RequestSecurityToken CreateFrom(XmlReader reader, MessageSecurityVersion messageSecurityVersion, SecurityTokenSerializer securityTokenSerializer)
        {
            return CreateFrom(SecurityUtils.CreateSecurityStandardsManager(messageSecurityVersion, securityTokenSerializer), reader);
        }

        internal static RequestSecurityToken CreateFrom(SecurityStandardsManager standardsManager,  XmlReader reader)
        {
            return standardsManager.TrustDriver.CreateRequestSecurityToken(reader);
        }

        public void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                this.isReadOnly = true;
                if (this.requestProperties != null)
                {
                    this.requestProperties = new ReadOnlyCollection<XmlElement>(this.requestProperties);
                }
                this.OnMakeReadOnly();
            }
        }

        internal protected virtual void OnWriteCustomAttributes(XmlWriter writer) { }

        internal protected virtual void OnWriteCustomElements(XmlWriter writer) { }

        internal protected virtual void OnMakeReadOnly() { }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            WriteTo(writer);
        }
    }
}
