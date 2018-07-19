//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System.Collections.Generic;
    using System.IdentityModel.Tokens;
    using System.Xml;

    /// <summary>
    /// SecurityTokenSerializer is responsible for writing and reading SecurityKeyIdentifiers, SecurityKeyIdentifierClauses and SecurityTokens.
    /// In order to read SecurityTokens the SecurityTokenSerializer may need to resolve token references using the SecurityTokenResolvers that get passed in.
    /// The SecurityTokenSerializer is stateless
    /// Exceptions: XmlException, SecurityTokenException, NotSupportedException, InvalidOperationException, ArgumentException
    /// </summary>
    public abstract class SecurityTokenSerializer
    {
        // public methods
        public bool CanReadToken(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            return CanReadTokenCore(reader);
        }

        public bool CanWriteToken(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            return CanWriteTokenCore(token);
        }

        public bool CanReadKeyIdentifier(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            return CanReadKeyIdentifierCore(reader);
        }

        public bool CanWriteKeyIdentifier(SecurityKeyIdentifier keyIdentifier)
        {
            if (keyIdentifier == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifier");
            }
            return CanWriteKeyIdentifierCore(keyIdentifier);
        }

        public bool CanReadKeyIdentifierClause(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            return CanReadKeyIdentifierClauseCore(reader);
        }

        public bool CanWriteKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            if (keyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifierClause");
            }
            return CanWriteKeyIdentifierClauseCore(keyIdentifierClause);
        }


        public SecurityToken ReadToken(XmlReader reader, SecurityTokenResolver tokenResolver)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            return ReadTokenCore(reader, tokenResolver);
        }

        public void WriteToken(XmlWriter writer, SecurityToken token)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            WriteTokenCore(writer, token);
        }

        public SecurityKeyIdentifier ReadKeyIdentifier(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            return ReadKeyIdentifierCore(reader);
        }

        public void WriteKeyIdentifier(XmlWriter writer, SecurityKeyIdentifier keyIdentifier)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (keyIdentifier == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifier");
            }
            WriteKeyIdentifierCore(writer, keyIdentifier);
        }

        public SecurityKeyIdentifierClause ReadKeyIdentifierClause(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            return ReadKeyIdentifierClauseCore(reader);
        }

        public void WriteKeyIdentifierClause(XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            if (keyIdentifierClause == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifierClause");
            }
            WriteKeyIdentifierClauseCore(writer, keyIdentifierClause);
        }


        // protected abstract methods
        protected abstract bool CanReadTokenCore(XmlReader reader);
        protected abstract bool CanWriteTokenCore(SecurityToken token);
        protected abstract bool CanReadKeyIdentifierCore(XmlReader reader);
        protected abstract bool CanWriteKeyIdentifierCore(SecurityKeyIdentifier keyIdentifier);
        protected abstract bool CanReadKeyIdentifierClauseCore(XmlReader reader);
        protected abstract bool CanWriteKeyIdentifierClauseCore(SecurityKeyIdentifierClause keyIdentifierClause);

        protected abstract SecurityToken ReadTokenCore(XmlReader reader, SecurityTokenResolver tokenResolver);
        protected abstract void WriteTokenCore(XmlWriter writer, SecurityToken token);
        protected abstract SecurityKeyIdentifier ReadKeyIdentifierCore(XmlReader reader);
        protected abstract void WriteKeyIdentifierCore(XmlWriter writer, SecurityKeyIdentifier keyIdentifier);
        protected abstract SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlReader reader);
        protected abstract void WriteKeyIdentifierClauseCore(XmlWriter writer, SecurityKeyIdentifierClause keyIdentifierClause);

        internal abstract class KeyIdentifierClauseEntry
        {

            protected abstract XmlDictionaryString LocalName { get; }
            protected abstract XmlDictionaryString NamespaceUri { get; }

            public virtual bool CanReadKeyIdentifierClauseCore(XmlDictionaryReader reader)
            {
                return reader.IsStartElement(this.LocalName, this.NamespaceUri);
            }

            public abstract SecurityKeyIdentifierClause ReadKeyIdentifierClauseCore(XmlDictionaryReader reader);

            public abstract bool SupportsCore(SecurityKeyIdentifierClause keyIdentifierClause);

            public abstract void WriteKeyIdentifierClauseCore(XmlDictionaryWriter writer, SecurityKeyIdentifierClause keyIdentifierClause);
        }

        internal abstract class StrEntry
        {
            public abstract string GetTokenTypeUri();
            public abstract Type GetTokenType(SecurityKeyIdentifierClause clause);
            public abstract bool CanReadClause(XmlDictionaryReader reader, string tokenType);
            public abstract SecurityKeyIdentifierClause ReadClause(XmlDictionaryReader reader, byte[] derivationNonce, int derivationLength, string tokenType);
            public abstract bool SupportsCore(SecurityKeyIdentifierClause clause);
            public abstract void WriteContent(XmlDictionaryWriter writer, SecurityKeyIdentifierClause clause);
        }

        internal abstract class SerializerEntries
        {
            public virtual void PopulateTokenEntries(IList<TokenEntry> tokenEntries) { }
            public virtual void PopulateKeyIdentifierEntries(IList<KeyIdentifierEntry> keyIdentifierEntries) { }
            public virtual void PopulateKeyIdentifierClauseEntries(IList<KeyIdentifierClauseEntry> keyIdentifierClauseEntries) { }
            public virtual void PopulateStrEntries(IList<StrEntry> strEntries) { }
        }

        internal abstract class KeyIdentifierEntry
        {
            protected abstract XmlDictionaryString LocalName { get; }
            protected abstract XmlDictionaryString NamespaceUri { get; }

            public virtual bool CanReadKeyIdentifierCore(XmlDictionaryReader reader)
            {
                return reader.IsStartElement(this.LocalName, this.NamespaceUri);
            }

            public abstract SecurityKeyIdentifier ReadKeyIdentifierCore(XmlDictionaryReader reader);

            public abstract bool SupportsCore(SecurityKeyIdentifier keyIdentifier);

            public abstract void WriteKeyIdentifierCore(XmlDictionaryWriter writer, SecurityKeyIdentifier keyIdentifier);
        }

        internal abstract class TokenEntry
        {
            Type[] tokenTypes = null;

            protected abstract XmlDictionaryString LocalName { get; }
            protected abstract XmlDictionaryString NamespaceUri { get; }
            public Type TokenType { get { return GetTokenTypes()[0]; } }
            public abstract string TokenTypeUri { get; }
            protected abstract string ValueTypeUri { get; }

            public bool SupportsCore(Type tokenType)
            {
                Type[] tokenTypes = GetTokenTypes();
                for (int i = 0; i < tokenTypes.Length; ++i)
                {
                    if (tokenTypes[i].IsAssignableFrom(tokenType))
                        return true;
                }
                return false;
            }

            protected abstract Type[] GetTokenTypesCore();

            public Type[] GetTokenTypes()
            {
                if (this.tokenTypes == null)
                    this.tokenTypes = GetTokenTypesCore();
                return this.tokenTypes;
            }

            public virtual bool SupportsTokenTypeUri(string tokenTypeUri)
            {
                return (this.TokenTypeUri == tokenTypeUri);
            }

        }

    }

}
