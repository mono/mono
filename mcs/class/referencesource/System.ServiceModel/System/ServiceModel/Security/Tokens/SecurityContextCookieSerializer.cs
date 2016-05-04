//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security.Tokens
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    struct SecurityContextCookieSerializer
    {
        const int SupportedPersistanceVersion = 1;

        SecurityStateEncoder securityStateEncoder;
        IList<Type> knownTypes;

        public SecurityContextCookieSerializer(SecurityStateEncoder securityStateEncoder, IList<Type> knownTypes)
        {
            if (securityStateEncoder == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityStateEncoder");
            }
            this.securityStateEncoder = securityStateEncoder;
            this.knownTypes = knownTypes ?? new List<Type>();
        }

        SecurityContextSecurityToken DeserializeContext(byte[] serializedContext, byte[] cookieBlob, string id, XmlDictionaryReaderQuotas quotas)
        {
            SctClaimDictionary dictionary = SctClaimDictionary.Instance;
            XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(serializedContext, 0, serializedContext.Length, dictionary, quotas, null, null);
            int cookieVersion = -1;
            UniqueId cookieContextId = null;
            DateTime effectiveTime = SecurityUtils.MinUtcDateTime;
            DateTime expiryTime = SecurityUtils.MaxUtcDateTime;
            byte[] key = null;
            string localId = null;
            UniqueId keyGeneration = null;
            DateTime keyEffectiveTime = SecurityUtils.MinUtcDateTime;
            DateTime keyExpirationTime = SecurityUtils.MaxUtcDateTime;
            List<ClaimSet> claimSets = null;
            IList<IIdentity> identities = null;
            bool isCookie = true;

            reader.ReadFullStartElement(dictionary.SecurityContextSecurityToken, dictionary.EmptyString);

            while (reader.IsStartElement())
            {
                if (reader.IsStartElement(dictionary.Version, dictionary.EmptyString))
                {
                    cookieVersion = reader.ReadElementContentAsInt();
                }
                else if (reader.IsStartElement(dictionary.ContextId, dictionary.EmptyString))
                {
                    cookieContextId = reader.ReadElementContentAsUniqueId();
                }
                else if (reader.IsStartElement(dictionary.Id, dictionary.EmptyString))
                {
                    localId = reader.ReadElementContentAsString();
                }
                else if (reader.IsStartElement(dictionary.EffectiveTime, dictionary.EmptyString))
                {
                    effectiveTime = new DateTime(XmlHelper.ReadElementContentAsInt64(reader), DateTimeKind.Utc);
                }
                else if (reader.IsStartElement(dictionary.ExpiryTime, dictionary.EmptyString))
                {
                    expiryTime = new DateTime(XmlHelper.ReadElementContentAsInt64(reader), DateTimeKind.Utc);
                }
                else if (reader.IsStartElement(dictionary.Key, dictionary.EmptyString))
                {
                    key = reader.ReadElementContentAsBase64();
                }
                else if (reader.IsStartElement(dictionary.KeyGeneration, dictionary.EmptyString))
                {
                    keyGeneration = reader.ReadElementContentAsUniqueId();
                }
                else if (reader.IsStartElement(dictionary.KeyEffectiveTime, dictionary.EmptyString))
                {
                    keyEffectiveTime = new DateTime(XmlHelper.ReadElementContentAsInt64(reader), DateTimeKind.Utc);
                }
                else if (reader.IsStartElement(dictionary.KeyExpiryTime, dictionary.EmptyString))
                {
                    keyExpirationTime = new DateTime(XmlHelper.ReadElementContentAsInt64(reader), DateTimeKind.Utc);
                }
                else if (reader.IsStartElement(dictionary.Identities, dictionary.EmptyString))
                {
                    identities = SctClaimSerializer.DeserializeIdentities(reader, dictionary, DataContractSerializerDefaults.CreateSerializer(typeof(IIdentity), this.knownTypes, int.MaxValue));
                }
                else if (reader.IsStartElement(dictionary.ClaimSets, dictionary.EmptyString))
                {
                    reader.ReadStartElement();

                    DataContractSerializer claimSetSerializer = DataContractSerializerDefaults.CreateSerializer(typeof(ClaimSet), this.knownTypes, int.MaxValue);
                    DataContractSerializer claimSerializer = DataContractSerializerDefaults.CreateSerializer(typeof(Claim), this.knownTypes, int.MaxValue);
                    claimSets = new List<ClaimSet>(1);
                    while (reader.IsStartElement())
                    {
                        claimSets.Add(SctClaimSerializer.DeserializeClaimSet(reader, dictionary, claimSetSerializer, claimSerializer));
                    }
                    
                    reader.ReadEndElement();
                }
                else if (reader.IsStartElement(dictionary.IsCookieMode, dictionary.EmptyString))
                {
                    isCookie = reader.ReadElementString() == "1" ? true : false;
                }
                else
                {
                    OnInvalidCookieFailure(SR.GetString(SR.SctCookieXmlParseError));
                }
            }
            reader.ReadEndElement();
            if (cookieVersion != SupportedPersistanceVersion)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SerializedTokenVersionUnsupported, cookieVersion)));
            }
            if (cookieContextId == null)
            {
                OnInvalidCookieFailure(SR.GetString(SR.SctCookieValueMissingOrIncorrect, "ContextId"));
            }
            if (key == null || key.Length == 0)
            {
                OnInvalidCookieFailure(SR.GetString(SR.SctCookieValueMissingOrIncorrect, "Key"));
            }
            if (localId != id)
            {
                OnInvalidCookieFailure(SR.GetString(SR.SctCookieValueMissingOrIncorrect, "Id"));
            }
            List<IAuthorizationPolicy> authorizationPolicies;
            if (claimSets != null)
            {
                authorizationPolicies = new List<IAuthorizationPolicy>(1);
                authorizationPolicies.Add(new SctUnconditionalPolicy(identities, claimSets, expiryTime));
            }
            else
            {
                authorizationPolicies = null;
            }
            return new SecurityContextSecurityToken(cookieContextId, localId, key, effectiveTime, expiryTime,
                authorizationPolicies != null ? authorizationPolicies.AsReadOnly() : null, isCookie, cookieBlob, keyGeneration, keyEffectiveTime, keyExpirationTime);
        }

        public byte[] CreateCookieFromSecurityContext(UniqueId contextId, string id, byte[] key, DateTime tokenEffectiveTime,
            DateTime tokenExpirationTime, UniqueId keyGeneration, DateTime keyEffectiveTime, DateTime keyExpirationTime,
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            if (contextId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextId");
            }

            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }

            MemoryStream stream = new MemoryStream();
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(stream, SctClaimDictionary.Instance, null);

            SctClaimDictionary dictionary = SctClaimDictionary.Instance;
            writer.WriteStartElement(dictionary.SecurityContextSecurityToken, dictionary.EmptyString);
            writer.WriteStartElement(dictionary.Version, dictionary.EmptyString);
            writer.WriteValue(SupportedPersistanceVersion);
            writer.WriteEndElement();
            if (id != null)
                writer.WriteElementString(dictionary.Id, dictionary.EmptyString, id);
            XmlHelper.WriteElementStringAsUniqueId(writer, dictionary.ContextId, dictionary.EmptyString, contextId);

            writer.WriteStartElement(dictionary.Key, dictionary.EmptyString);
            writer.WriteBase64(key, 0, key.Length);
            writer.WriteEndElement();

            if (keyGeneration != null)
            {
                XmlHelper.WriteElementStringAsUniqueId(writer, dictionary.KeyGeneration, dictionary.EmptyString, keyGeneration);
            }

            XmlHelper.WriteElementContentAsInt64(writer, dictionary.EffectiveTime, dictionary.EmptyString, tokenEffectiveTime.ToUniversalTime().Ticks);
            XmlHelper.WriteElementContentAsInt64(writer, dictionary.ExpiryTime, dictionary.EmptyString, tokenExpirationTime.ToUniversalTime().Ticks);
            XmlHelper.WriteElementContentAsInt64(writer, dictionary.KeyEffectiveTime, dictionary.EmptyString, keyEffectiveTime.ToUniversalTime().Ticks);
            XmlHelper.WriteElementContentAsInt64(writer, dictionary.KeyExpiryTime, dictionary.EmptyString, keyExpirationTime.ToUniversalTime().Ticks);

            AuthorizationContext authContext = null;
            if (authorizationPolicies != null)
                authContext = AuthorizationContext.CreateDefaultAuthorizationContext(authorizationPolicies);

            if (authContext != null && authContext.ClaimSets.Count != 0)
            {
                DataContractSerializer identitySerializer = DataContractSerializerDefaults.CreateSerializer(typeof(IIdentity), this.knownTypes, int.MaxValue);
                DataContractSerializer claimSetSerializer = DataContractSerializerDefaults.CreateSerializer(typeof(ClaimSet), this.knownTypes, int.MaxValue);
                DataContractSerializer claimSerializer = DataContractSerializerDefaults.CreateSerializer(typeof(Claim), this.knownTypes, int.MaxValue);
                SctClaimSerializer.SerializeIdentities(authContext, dictionary, writer, identitySerializer);

                writer.WriteStartElement(dictionary.ClaimSets, dictionary.EmptyString);
                for (int i = 0; i < authContext.ClaimSets.Count; i++)
                {
                    SctClaimSerializer.SerializeClaimSet(authContext.ClaimSets[i], dictionary, writer, claimSetSerializer, claimSerializer);
                }
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.Flush();

            byte[] serializedContext = stream.ToArray();
            return this.securityStateEncoder.EncodeSecurityState(serializedContext);
        }


        public SecurityContextSecurityToken CreateSecurityContextFromCookie(byte[] encodedCookie, UniqueId contextId, UniqueId generation, string id, XmlDictionaryReaderQuotas quotas)
        {
            byte[] cookie = null;

            try
            {
                cookie = this.securityStateEncoder.DecodeSecurityState(encodedCookie);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                OnInvalidCookieFailure(SR.GetString(SR.SctCookieBlobDecodeFailure), e);
            }
            SecurityContextSecurityToken sct = DeserializeContext(cookie, encodedCookie, id, quotas);
            if (sct.ContextId != contextId)
            {
                OnInvalidCookieFailure(SR.GetString(SR.SctCookieValueMissingOrIncorrect, "ContextId"));
            }
            if (sct.KeyGeneration != generation)
            {
                OnInvalidCookieFailure(SR.GetString(SR.SctCookieValueMissingOrIncorrect, "KeyGeneration"));
            }

            return sct;
        }
        
        internal static void OnInvalidCookieFailure(string reason)
        {
            OnInvalidCookieFailure(reason, null);
        }

        internal static void OnInvalidCookieFailure(string reason, Exception e)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.InvalidSecurityContextCookie, reason), e));
        }

        class SctUnconditionalPolicy : IAuthorizationPolicy
        {
            SecurityUniqueId id = SecurityUniqueId.Create();
            IList<IIdentity> identities; 
            IList<ClaimSet> claimSets;
            DateTime expirationTime;

            public SctUnconditionalPolicy(IList<IIdentity> identities, IList<ClaimSet> claimSets, DateTime expirationTime)
            {
                this.identities = identities;
                this.claimSets = claimSets;
                this.expirationTime = expirationTime;
            }

            public string Id
            {
                get { return this.id.Value; }
            }

            public ClaimSet Issuer 
            { 
                get { return ClaimSet.System; } 
            }

            public bool Evaluate(EvaluationContext evaluationContext, ref object state)
            {
                for (int i = 0; i < this.claimSets.Count; ++i)
                {
                    evaluationContext.AddClaimSet(this, this.claimSets[i]);
                }

                if (this.identities != null)
                {
                    object obj;
                    if (!evaluationContext.Properties.TryGetValue(SecurityUtils.Identities, out obj))
                    {
                        evaluationContext.Properties.Add(SecurityUtils.Identities, this.identities);
                    }
                    else
                    {
                        // null if other overrides the property with something else
                        List<IIdentity> dstIdentities = obj as List<IIdentity>;
                        if (dstIdentities != null)
                        {
                            dstIdentities.AddRange(this.identities);
                        }
                    }
                }
                evaluationContext.RecordExpirationTime(this.expirationTime);
                return true;
            }
        }
    }
}
