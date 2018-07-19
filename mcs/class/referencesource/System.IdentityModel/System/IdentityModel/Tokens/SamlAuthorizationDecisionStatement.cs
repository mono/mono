//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IdentityModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Selectors;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Serialization;

    public class SamlAuthorizationDecisionStatement : SamlSubjectStatement
    {

        SamlEvidence evidence;
        readonly ImmutableCollection<SamlAction> actions = new ImmutableCollection<SamlAction>();
        SamlAccessDecision accessDecision;
        string resource;
        bool isReadOnly = false;

        public SamlAuthorizationDecisionStatement()
        {
        }

        public SamlAuthorizationDecisionStatement(SamlSubject samlSubject, string resource, SamlAccessDecision accessDecision, IEnumerable<SamlAction> samlActions)
            : this(samlSubject, resource, accessDecision, samlActions, null)
        {
        }

        public SamlAuthorizationDecisionStatement(SamlSubject samlSubject, string resource, SamlAccessDecision accessDecision, IEnumerable<SamlAction> samlActions, SamlEvidence samlEvidence)
            : base(samlSubject)
        {
            if (samlActions == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlActions"));

            foreach (SamlAction action in samlActions)
            {
                if (action == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLEntityCannotBeNullOrEmpty, XD.SamlDictionary.Action.Value));

                this.actions.Add(action);
            }

            this.evidence = samlEvidence;
            this.accessDecision = accessDecision;
            this.resource = resource;

            CheckObjectValidity();
        }

        public static string ClaimType
        {
            get
            {
                return ClaimTypes.AuthorizationDecision;
            }
        }

        public IList<SamlAction> SamlActions
        {
            get { return this.actions; }
        }

        public SamlAccessDecision AccessDecision
        {
            get { return this.accessDecision; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                this.accessDecision = value;
            }
        }

        public SamlEvidence Evidence
        {
            get { return this.evidence; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                this.evidence = value;
            }
        }

        public string Resource
        {
            get { return this.resource; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                if (string.IsNullOrEmpty(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLAuthorizationDecisionResourceRequired));

                this.resource = value;
            }
        }

        public override bool IsReadOnly
        {
            get { return this.isReadOnly; }
        }

        public override void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                if (this.evidence != null)
                    this.evidence.MakeReadOnly();

                foreach (SamlAction action in this.actions)
                {
                    action.MakeReadOnly();
                }

                this.actions.MakeReadOnly();

                this.isReadOnly = true;
            }
        }

        protected override void AddClaimsToList(IList<Claim> claims)
        {
            if (claims == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("claims"));

            for (int i = 0; i < this.actions.Count; ++i)
            {
                claims.Add(new Claim(ClaimTypes.AuthorizationDecision, new SamlAuthorizationDecisionClaimResource(this.resource, this.accessDecision, this.actions[i].Namespace, this.actions[i].Action), Rights.PossessProperty));
            }
        }

        void CheckObjectValidity()
        {
            if (this.SamlSubject == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLSubjectStatementRequiresSubject)));

            if (string.IsNullOrEmpty(this.resource))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAuthorizationDecisionResourceRequired)));

            if (this.actions.Count == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAuthorizationDecisionShouldHaveOneAction)));
        }

        public override void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            this.resource = reader.GetAttribute(dictionary.Resource, null);
            if (string.IsNullOrEmpty(this.resource))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAuthorizationDecisionStatementMissingResourceAttributeOnRead)));

            string decisionString = reader.GetAttribute(dictionary.Decision, null);
            if (string.IsNullOrEmpty(decisionString))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAuthorizationDecisionStatementMissingDecisionAttributeOnRead)));

            if (decisionString.Equals(SamlAccessDecision.Deny.ToString(), StringComparison.OrdinalIgnoreCase))
                this.accessDecision = SamlAccessDecision.Deny;
            else if (decisionString.Equals(SamlAccessDecision.Permit.ToString(), StringComparison.OrdinalIgnoreCase))
                this.accessDecision = SamlAccessDecision.Permit;
            else
                accessDecision = SamlAccessDecision.Indeterminate;

            reader.MoveToContent();
            reader.Read();

            if (reader.IsStartElement(dictionary.Subject, dictionary.Namespace))
            {
                SamlSubject subject = new SamlSubject();
                subject.ReadXml(reader, samlSerializer, keyInfoSerializer, outOfBandTokenResolver);
                base.SamlSubject = subject;
            }
            else
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAuthorizationDecisionStatementMissingSubjectOnRead)));

            while (reader.IsStartElement())
            {
                if (reader.IsStartElement(dictionary.Action, dictionary.Namespace))
                {
                    SamlAction action = new SamlAction();
                    action.ReadXml(reader, samlSerializer, keyInfoSerializer, outOfBandTokenResolver);
                    this.actions.Add(action);
                }
                else if (reader.IsStartElement(dictionary.Evidence, dictionary.Namespace))
                {
                    if (this.evidence != null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAuthorizationDecisionHasMoreThanOneEvidence)));

                    this.evidence = new SamlEvidence();
                    this.evidence.ReadXml(reader, samlSerializer, keyInfoSerializer, outOfBandTokenResolver);
                }
                else
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLBadSchema, dictionary.AuthorizationDecisionStatement)));
            }

            if (this.actions.Count == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAuthorizationDecisionShouldHaveOneActionOnRead)));

            reader.MoveToContent();
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer)
        {
            CheckObjectValidity();

            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            writer.WriteStartElement(dictionary.PreferredPrefix.Value, dictionary.AuthorizationDecisionStatement, dictionary.Namespace);

            writer.WriteStartAttribute(dictionary.Decision, null);
            writer.WriteString(this.accessDecision.ToString());
            writer.WriteEndAttribute();

            writer.WriteStartAttribute(dictionary.Resource, null);
            writer.WriteString(this.resource);
            writer.WriteEndAttribute();

            this.SamlSubject.WriteXml(writer, samlSerializer, keyInfoSerializer);

            foreach (SamlAction action in this.actions)
                action.WriteXml(writer, samlSerializer, keyInfoSerializer);

            if (this.evidence != null)
                this.evidence.WriteXml(writer, samlSerializer, keyInfoSerializer);

            writer.WriteEndElement();
        }
    }
}

