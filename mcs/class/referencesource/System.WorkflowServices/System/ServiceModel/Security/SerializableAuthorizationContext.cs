//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel.Policy;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.IO;
    using System.Xml;
    using System.ServiceModel.Dispatcher;
    using System.Security.Principal;
    using System.ServiceModel.Security.Tokens;

    [Serializable]
    class SerializableAuthorizationContext
    {
        static readonly IList<Type> redBitsKnownType = new List<Type>(
            new Type[]
            {
                typeof(DefaultClaimSet),
                typeof(WindowsClaimSet),
                typeof(X509CertificateClaimSet),
                typeof(Claim)
            });

        byte[] contextBlob;
        DateTime expirationTime;
        string id;
        IList<Type> knownTypes;

        SerializableAuthorizationContext(byte[] contextBlob, DateTime expirationTime, string id, IList<Type> knownTypes)
        {
            if (contextBlob == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextBlob");
            }

            this.expirationTime = expirationTime;
            this.id = id;
            this.contextBlob = contextBlob;
            this.knownTypes = knownTypes;
        }

        public static SerializableAuthorizationContext From(AuthorizationContext authorizationContext)
        {
            if (authorizationContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authorizationContext");
            }

            IList<Type> knownTypes = BuildKnownClaimTypes(authorizationContext);
            byte[] contextBlob = CreateSerializableBlob(authorizationContext, knownTypes);

            return new SerializableAuthorizationContext(contextBlob, authorizationContext.ExpirationTime, authorizationContext.Id, knownTypes);
        }

        public AuthorizationContext Retrieve()
        {
            List<IAuthorizationPolicy> authorizationPolicies = new List<IAuthorizationPolicy>(1);
            authorizationPolicies.Add(RetrievePolicyFromBlob(this.contextBlob, this.id, this.expirationTime, this.knownTypes));
            return AuthorizationContext.CreateDefaultAuthorizationContext(authorizationPolicies);
        }

        static IList<Type> BuildKnownClaimTypes(AuthorizationContext authorizationContext)
        {
            List<Type> knownTypes = new List<Type>();

            foreach (ClaimSet claimSet in authorizationContext.ClaimSets)
            {
                Type claimSetType = claimSet.GetType();

                if (!redBitsKnownType.Contains(claimSetType) && !knownTypes.Contains(claimSetType))
                {
                    knownTypes.Add(claimSetType);
                }

                foreach (Claim claim in claimSet)
                {
                    Type claimType = claim.GetType();

                    if (!redBitsKnownType.Contains(claimType) && !knownTypes.Contains(claimType))
                    {
                        knownTypes.Add(claimType);
                    }
                }
            }

            if (knownTypes.Count != 0)
            {
                return knownTypes;
            }

            return null;
        }

        static byte[] CreateSerializableBlob(AuthorizationContext authorizationContext, IList<Type> knownTypes)
        {
            if (authorizationContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authorizationContext");
            }

            MemoryStream stream = new MemoryStream();
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(stream, SctClaimDictionary.Instance, null);
            SctClaimDictionary claimDictionary = SctClaimDictionary.Instance;


            writer.WriteStartElement(claimDictionary.SecurityContextSecurityToken, claimDictionary.EmptyString);
            writer.WriteStartElement(claimDictionary.Version, claimDictionary.EmptyString);
            writer.WriteValue(1);
            writer.WriteEndElement();

            if ((authorizationContext != null) && (authorizationContext.ClaimSets.Count != 0))
            {
                DataContractSerializer identitySerializer = DataContractSerializerDefaults.CreateSerializer(typeof(IIdentity), knownTypes, 0x7fffffff);
                DataContractSerializer claimSetSerializer = DataContractSerializerDefaults.CreateSerializer(typeof(ClaimSet), knownTypes, 0x7fffffff);
                DataContractSerializer claimSerializer = DataContractSerializerDefaults.CreateSerializer(typeof(Claim), knownTypes, 0x7fffffff);
                SctClaimSerializer.SerializeIdentities(authorizationContext, claimDictionary, writer, identitySerializer);

                writer.WriteStartElement(claimDictionary.ClaimSets, claimDictionary.EmptyString);
                for (int i = 0; i < authorizationContext.ClaimSets.Count; i++)
                {
                    SctClaimSerializer.SerializeClaimSet(authorizationContext.ClaimSets[i], claimDictionary, writer, claimSetSerializer, claimSerializer);
                }
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.Flush();
            return stream.ToArray();
        }

        static IAuthorizationPolicy RetrievePolicyFromBlob(byte[] contextBlob, string id, DateTime expirationTime, IList<Type> knownTypes)
        {
            if (contextBlob == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contextBlob");
            }

            if (id == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("id");
            }

            SctClaimDictionary claimDictionary = SctClaimDictionary.Instance;
            XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(contextBlob, 0, contextBlob.Length, claimDictionary, XmlDictionaryReaderQuotas.Max, null, null);
            IList<IIdentity> identities = null;
            IList<ClaimSet> claimSets = null;
            int versionNumber = -1;

            reader.ReadFullStartElement(claimDictionary.SecurityContextSecurityToken, claimDictionary.EmptyString);

            while (reader.IsStartElement())
            {
                if (reader.IsStartElement(claimDictionary.Version, claimDictionary.EmptyString))
                {
                    versionNumber = reader.ReadElementContentAsInt();

                    if (versionNumber != 1)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR2.GetString(SR2.SerializedAuthorizationContextVersionUnsupported, versionNumber)));
                    }
                }
                else
                {
                    if (reader.IsStartElement(claimDictionary.Identities, claimDictionary.EmptyString))
                    {
                        identities = SctClaimSerializer.DeserializeIdentities(reader, claimDictionary, DataContractSerializerDefaults.CreateSerializer(typeof(IIdentity), knownTypes, 0x7fffffff));
                        continue;
                    }
                    if (reader.IsStartElement(claimDictionary.ClaimSets, claimDictionary.EmptyString))
                    {
                        reader.ReadStartElement();
                        DataContractSerializer claimSetSerializer = DataContractSerializerDefaults.CreateSerializer(typeof(ClaimSet), knownTypes, 0x7fffffff);
                        DataContractSerializer claimSerializer = DataContractSerializerDefaults.CreateSerializer(typeof(Claim), knownTypes, 0x7fffffff);
                        claimSets = new List<ClaimSet>(1);

                        while (reader.IsStartElement())
                        {
                            claimSets.Add(SctClaimSerializer.DeserializeClaimSet(reader, claimDictionary, claimSetSerializer, claimSerializer));
                        }

                        reader.ReadEndElement();
                        continue;
                    }
                }
            }
            reader.ReadEndElement();
            return new SctUnconditionalPolicy(identities, id, claimSets, expirationTime);
        }

        class SctUnconditionalPolicy : IAuthorizationPolicy, IAuthorizationComponent
        {

            IList<ClaimSet> claimSets;
            DateTime expirationTime;
            string id;
            IList<IIdentity> identities;

            public SctUnconditionalPolicy(IList<IIdentity> identities, string id, IList<ClaimSet> claimSets, DateTime expirationTime)
            {
                this.identities = identities;
                this.claimSets = claimSets;
                this.expirationTime = expirationTime;
                this.id = id;
            }

            public string Id
            {
                get
                {
                    return this.id;
                }
            }

            public ClaimSet Issuer
            {
                get
                {
                    return ClaimSet.System;
                }
            }

            public bool Evaluate(EvaluationContext evaluationContext, ref object state)
            {
                for (int num1 = 0; num1 < this.claimSets.Count; num1++)
                {
                    evaluationContext.AddClaimSet(this, this.claimSets[num1]);
                }
                if (this.identities != null)
                {
                    object obj;
                    if (!evaluationContext.Properties.TryGetValue("Identities", out obj))
                    {
                        evaluationContext.Properties.Add("Identities", (object)this.identities);
                    }
                    else
                    {
                        List<IIdentity> identities = obj as List<IIdentity>;
                        if (identities != null)
                        {
                            identities.AddRange(this.identities);
                        }
                    }
                }
                evaluationContext.RecordExpirationTime(this.expirationTime);
                return true;
            }
        }
    }
}
