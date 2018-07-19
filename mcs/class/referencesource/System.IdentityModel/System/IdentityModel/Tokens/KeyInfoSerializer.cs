//------------------------------------------------------------------------------
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections.Generic;
    using System.IdentityModel;
    using System.IdentityModel.Security;
    using System.IdentityModel.Selectors;
    using System.Runtime;
    using System.Xml;
using System.Collections;

    /// <summary>
    /// Abstract class for SecurityKeyIdentifierClause Serializer.
    /// </summary>
    internal class KeyInfoSerializer : SecurityTokenSerializer
    {
        readonly List<SecurityTokenSerializer.KeyIdentifierEntry> keyIdentifierEntries;
        readonly List<SecurityTokenSerializer.KeyIdentifierClauseEntry> keyIdentifierClauseEntries;
        readonly List<SecurityTokenSerializer.SerializerEntries> serializerEntries;
        readonly List<TokenEntry> tokenEntries;


        DictionaryManager dictionaryManager;
        bool emitBspRequiredAttributes;
        SecurityTokenSerializer innerSecurityTokenSerializer;

        /// <summary>
        /// Creates an instance of <see cref="SecurityKeyIdentifierClauseSerializer"/>
        /// </summary>
        public KeyInfoSerializer(bool emitBspRequiredAttributes)
            : this(emitBspRequiredAttributes, new DictionaryManager(), XD.TrustDec2005Dictionary, null)
        {
        }

        public KeyInfoSerializer(
            bool emitBspRequiredAttributes,
            DictionaryManager dictionaryManager,
            TrustDictionary trustDictionary,
            SecurityTokenSerializer innerSecurityTokenSerializer ) :
            this( emitBspRequiredAttributes, dictionaryManager, trustDictionary, innerSecurityTokenSerializer, null )
        {
        }

        public KeyInfoSerializer(
            bool emitBspRequiredAttributes,
            DictionaryManager dictionaryManager,
            TrustDictionary trustDictionary,
            SecurityTokenSerializer innerSecurityTokenSerializer,
            Func<KeyInfoSerializer, IEnumerable<SerializerEntries>> additionalEntries)
        {
            this.dictionaryManager = dictionaryManager;
            this.emitBspRequiredAttributes = emitBspRequiredAttributes;
            this.innerSecurityTokenSerializer = innerSecurityTokenSerializer;

            this.serializerEntries = new List<SecurityTokenSerializer.SerializerEntries>();

            this.serializerEntries.Add(new XmlDsigSep2000(this));
            this.serializerEntries.Add(new XmlEncApr2001(this));
            this.serializerEntries.Add(new System.IdentityModel.Security.WSTrust(this, trustDictionary));
            if ( additionalEntries != null )
            {
                foreach ( SerializerEntries entries in additionalEntries( this ) )
                {
                    this.serializerEntries.Add(entries);
                }
            }

            bool wsSecuritySerializerFound = false;
            foreach ( SerializerEntries entry in this.serializerEntries )
            {
                if ( ( entry is WSSecurityXXX2005 ) || ( entry is WSSecurityJan2004 ) )
                {
                    wsSecuritySerializerFound = true;
                    break;
                }
            }

            if ( !wsSecuritySerializerFound )
            {
                this.serializerEntries.Add( new WSSecurityXXX2005( this ) );
            }

            this.tokenEntries = new List<TokenEntry>();
            this.keyIdentifierEntries = new List<SecurityTokenSerializer.KeyIdentifierEntry>();
            this.keyIdentifierClauseEntries = new List<SecurityTokenSerializer.KeyIdentifierClauseEntry>();

            for (int i = 0; i < this.serializerEntries.Count; ++i)
            {
                SecurityTokenSerializer.SerializerEntries serializerEntry = this.serializerEntries[i];
                serializerEntry.PopulateTokenEntries(this.tokenEntries);
                serializerEntry.PopulateKeyIdentifierEntries(this.keyIdentifierEntries);
                serializerEntry.PopulateKeyIdentifierClauseEntries(this.keyIdentifierClauseEntries);
            }
        }

        public DictionaryManager DictionaryManager
        {
            get { return this.dictionaryManager; }
        }

        /// <summary>
        /// Gets or sets a value indicating if BSP required attributes should be written out.
        /// </summary>
        public bool EmitBspRequiredAttributes
        {
            get
            {
                return this.emitBspRequiredAttributes;
            }
        }

        public SecurityTokenSerializer InnerSecurityTokenSerializer
        {
            get
            {
                return this.innerSecurityTokenSerializer == null ? this : this.innerSecurityTokenSerializer;
            }
           set
            {
                this.innerSecurityTokenSerializer = value;
            }
        }

        protected override bool CanReadTokenCore(XmlReader reader)
        {
            return false;
        }

        protected override SecurityToken ReadTokenCore(XmlReader reader, SecurityTokenResolver tokenResolver)
        {
            XmlDictionaryReader localReader = XmlDictionaryReader.CreateDictionaryReader( reader ); 
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError( new XmlException( SR.GetString( SR.CannotReadToken, reader.LocalName, reader.NamespaceURI, localReader.GetAttribute( XD.SecurityJan2004Dictionary.ValueType, null ) ) ) );
        }

        protected override bool CanWriteTokenCore(SecurityToken token)
        {
            return false;
        }

        protected override void WriteTokenCore(XmlWriter writer, SecurityToken token)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.StandardsManagerCannotWriteObject, token.GetType())));
        }

        protected override bool CanReadKeyIdentifierCore(XmlReader reader)
        {
            XmlDictionaryReader localReader = XmlDictionaryReader.CreateDictionaryReader(reader);
            for (int i = 0; i < this.keyIdentifierEntries.Count; i++)
            {
                KeyIdentifierEntry keyIdentifierEntry = this.keyIdentifierEntries[i];
                if (keyIdentifierEntry.CanReadKeyIdentifierCore(localReader))
                    return true;
            }
            return false;
        }

        protected override SecurityKeyIdentifier ReadKeyIdentifierCore(XmlReader reader)
        {
            XmlDictionaryReader localReader = XmlDictionaryReader.CreateDictionaryReader(reader);
            localReader.ReadStartElement(XD.XmlSignatureDictionary.KeyInfo, XD.XmlSignatureDictionary.Namespace);
            SecurityKeyIdentifier keyIdentifier = new SecurityKeyIdentifier();
            while (localReader.IsStartElement())
            {
                SecurityKeyIdentifierClause clause = this.InnerSecurityTokenSerializer.ReadKeyIdentifierClause(localReader);
                if (clause == null)
                {
                    localReader.Skip();
                }
                else
                {
                    keyIdentifier.Add(clause);
                }
            }
            if (keyIdentifier.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ErrorDeserializingKeyIdentifierClause)));
            }
            localReader.ReadEndElement();

            return keyIdentifier;
        }

        protected override bool CanWriteKeyIdentifierCore(SecurityKeyIdentifier keyIdentifier)
        {
            for (int i = 0; i < this.keyIdentifierEntries.Count; ++i)
            {
                KeyIdentifierEntry keyIdentifierEntry = this.keyIdentifierEntries[i];
                if (keyIdentifierEntry.SupportsCore(keyIdentifier))
                    return true;
            }
            return false;
        }

        protected override void WriteKeyIdentifierCore(XmlWriter writer, SecurityKeyIdentifier keyIdentifier)
        {
            bool wroteKeyIdentifier = false;
            XmlDictionaryWriter localWriter = XmlDictionaryWriter.CreateDictionaryWriter(writer);
            for (int i = 0; i < this.keyIdentifierEntries.Count; ++i)
            {
                KeyIdentifierEntry keyIdentifierEntry = this.keyIdentifierEntries[i];
                if (keyIdentifierEntry.SupportsCore(keyIdentifier))
                {
                    try
                    {
                        keyIdentifierEntry.WriteKeyIdentifierCore(localWriter, keyIdentifier);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;
                        
                        if (!ShouldWrapException(e))
                        {
                            throw;
                        }

                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ErrorSerializingKeyIdentifier), e));
                    }
                    wroteKeyIdentifier = true;
                    break;
                }
            }

            if (!wroteKeyIdentifier)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.StandardsManagerCannotWriteObject, keyIdentifier.GetType())));

            localWriter.Flush();
        }

        protected override bool CanReadKeyIdentifierClauseCore(XmlReader reader)
        {
            XmlDictionaryReader localReader = XmlDictionaryReader.CreateDictionaryReader(reader);
            for (int i = 0; i < this.keyIdentifierClauseEntries.Count; i++)
            {
                KeyIdentifierClauseEntry keyIdentifierClauseEntry = this.keyIdentifierClauseEntries[i];
                if (keyIdentifierClauseEntry.CanReadKeyIdentifierClauseCore(localReader))
                    return true;
            }
            return false;
        }

        protected override SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlReader reader)
        {
            XmlDictionaryReader localReader = XmlDictionaryReader.CreateDictionaryReader(reader);
            for (int i = 0; i < this.keyIdentifierClauseEntries.Count; i++)
            {
                KeyIdentifierClauseEntry keyIdentifierClauseEntry = this.keyIdentifierClauseEntries[i];
                if (keyIdentifierClauseEntry.CanReadKeyIdentifierClauseCore(localReader))
                {
                    try
                    {
                        return keyIdentifierClauseEntry.ReadKeyIdentifierClauseCore(localReader);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;

                        if (!ShouldWrapException(e))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ErrorDeserializingKeyIdentifierClause), e));
                    }
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.CannotReadKeyIdentifierClause, reader.LocalName, reader.NamespaceURI)));
        }

        protected override bool CanWriteKeyIdentifierClauseCore(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            for (int i = 0; i < this.keyIdentifierClauseEntries.Count; ++i)
            {
                KeyIdentifierClauseEntry keyIdentifierClauseEntry = this.keyIdentifierClauseEntries[i];
                if (keyIdentifierClauseEntry.SupportsCore(keyIdentifierClause))
                    return true;
            }
            return false;
        }

        protected override void WriteKeyIdentifierClauseCore(XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
        {
            bool wroteKeyIdentifierClause = false;
            XmlDictionaryWriter localWriter = XmlDictionaryWriter.CreateDictionaryWriter(writer);
            for (int i = 0; i < this.keyIdentifierClauseEntries.Count; ++i)
            {
                KeyIdentifierClauseEntry keyIdentifierClauseEntry = this.keyIdentifierClauseEntries[i];
                if (keyIdentifierClauseEntry.SupportsCore(keyIdentifierClause))
                {
                    try
                    {
                        keyIdentifierClauseEntry.WriteKeyIdentifierClauseCore(localWriter, keyIdentifierClause);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;

                        if (!ShouldWrapException(e))
                        {
                            throw;
                        }
                        
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.ErrorSerializingKeyIdentifierClause), e));
                    }
                    wroteKeyIdentifierClause = true;
                    break;
                }
            }

            if (!wroteKeyIdentifierClause)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.StandardsManagerCannotWriteObject, keyIdentifierClause.GetType())));

            localWriter.Flush();
        }

        internal void PopulateStrEntries(IList<StrEntry> strEntries)
        {
            foreach (SerializerEntries serializerEntry in serializerEntries)
            {
                serializerEntry.PopulateStrEntries(strEntries);
            }
        }

        bool ShouldWrapException(Exception e)
        {
            return ((e is ArgumentException) || (e is FormatException) || (e is InvalidOperationException));
        }

        internal Type[] GetTokenTypes(string tokenTypeUri)
        {
            if (tokenTypeUri != null)
            {
                for (int i = 0; i < this.tokenEntries.Count; i++)
                {
                    TokenEntry tokenEntry = this.tokenEntries[i];

                    if (tokenEntry.SupportsTokenTypeUri(tokenTypeUri))
                    {
                        return tokenEntry.GetTokenTypes();
                    }
                }
            }
            return null;
        }

        protected internal virtual string GetTokenTypeUri(Type tokenType)
        {
            if (tokenType != null)
            {
                for (int i = 0; i < this.tokenEntries.Count; i++)
                {
                    TokenEntry tokenEntry = this.tokenEntries[i];

                    if (tokenEntry.SupportsCore(tokenType))
                    {
                        return tokenEntry.TokenTypeUri;
                    }
                }
            }
            return null;
        }

    }

}
