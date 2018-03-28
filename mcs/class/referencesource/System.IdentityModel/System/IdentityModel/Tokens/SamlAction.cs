//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Selectors;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Runtime.Serialization;

    public class SamlAction
    {
        string ns;
        string action;
        bool isReadOnly = false;

        public SamlAction(string action)
            : this(action, null)
        {
        }

        public SamlAction(string action, string ns)
        {
            if (string.IsNullOrEmpty(action))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("action", SR.GetString(SR.SAMLActionNameRequired));

            this.action = action;
            this.ns = ns;
        }

        public SamlAction()
        {
        }

        public string Action
        {
            get { return this.action; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                if (string.IsNullOrEmpty(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.SAMLActionNameRequired));

                this.action = value;
            }
        }

        public string Namespace
        {
            get { return this.ns; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                this.ns = value;
            }
        }

        public bool IsReadOnly
        {
            get { return this.isReadOnly; }
        }

        public void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        void CheckObjectValidity()
        {
            if (string.IsNullOrEmpty(action))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLActionNameRequired)));
        }

        public virtual void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            if (reader.IsStartElement(dictionary.Action, dictionary.Namespace))
            {
                // The Namespace attribute is optional.
                this.ns = reader.GetAttribute(dictionary.ActionNamespaceAttribute, null);

                reader.MoveToContent();
                this.action = reader.ReadString();
                if (string.IsNullOrEmpty(this.action))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLActionNameRequiredOnRead)));

                reader.MoveToContent();
                reader.ReadEndElement();
            }
        }

        public virtual void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer)
        {
            CheckObjectValidity();

            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            writer.WriteStartElement(dictionary.PreferredPrefix.Value, dictionary.Action, dictionary.Namespace);

            if (this.ns != null)
            {
                writer.WriteStartAttribute(dictionary.ActionNamespaceAttribute, null);
                writer.WriteString(this.ns);
                writer.WriteEndAttribute();
            }

            writer.WriteString(this.action);

            writer.WriteEndElement();
        }
    }
}
