//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Xml.Serialization;
    using System.Xml;
    using System.Runtime.Serialization;
    using System.Globalization;
    using System.IdentityModel.Selectors;

    public class SamlConditions
    {
        readonly ImmutableCollection<SamlCondition> conditions = new ImmutableCollection<SamlCondition>();
        bool isReadOnly = false;

        // Calculate once
        DateTime notBefore = SecurityUtils.MinUtcDateTime;
        DateTime notOnOrAfter = SecurityUtils.MaxUtcDateTime;

        public SamlConditions()
        {
        }

        public SamlConditions(DateTime notBefore, DateTime notOnOrAfter)
            : this(notBefore, notOnOrAfter, null)
        {
        }

        public SamlConditions(DateTime notBefore, DateTime notOnOrAfter,
            IEnumerable<SamlCondition> conditions
            )
        {
            this.notBefore = notBefore.ToUniversalTime();
            this.notOnOrAfter = notOnOrAfter.ToUniversalTime();

            if (conditions != null)
            {
                foreach (SamlCondition condition in conditions)
                {
                    if (condition == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.SAMLEntityCannotBeNullOrEmpty, XD.SamlDictionary.Condition.Value));

                    this.conditions.Add(condition);
                }
            }
        }

        public IList<SamlCondition> Conditions
        {
            get { return this.conditions; }
        }

        public DateTime NotBefore
        {
            get { return this.notBefore; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                this.notBefore = value;
            }
        }

        public DateTime NotOnOrAfter
        {
            get { return this.notOnOrAfter; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                this.notOnOrAfter = value;
            }
        }

        public bool IsReadOnly
        {
            get { return this.isReadOnly; }
        }

        public void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                this.conditions.MakeReadOnly();

                foreach (SamlCondition condition in this.conditions)
                {
                    condition.MakeReadOnly();
                }

                this.isReadOnly = true;
            }
        }

        public virtual void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver)
        {
            if (reader == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            string time = reader.GetAttribute(dictionary.NotBefore, null);
            if (!string.IsNullOrEmpty(time))
                this.notBefore = DateTime.ParseExact(
                    time, SamlConstants.AcceptedDateTimeFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None).ToUniversalTime();

            time = reader.GetAttribute(dictionary.NotOnOrAfter, null);
            if (!string.IsNullOrEmpty(time))
                this.notOnOrAfter = DateTime.ParseExact(
                    time, SamlConstants.AcceptedDateTimeFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None).ToUniversalTime();

            // Saml Conditions element is an optional element and all its child element
            // are optional as well. So we can have a empty <saml:Conditions /> element
            // in a valid Saml token.
            if (reader.IsEmptyElement)
            {
                // Just issue a read to read the Empty element.
                reader.MoveToContent();
                reader.Read();
                return;
            }

            reader.MoveToContent();
            reader.Read();
            while (reader.IsStartElement())
            {
                SamlCondition condition = samlSerializer.LoadCondition(reader, keyInfoSerializer, outOfBandTokenResolver);
                if (condition == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLUnableToLoadCondtion)));
                this.conditions.Add(condition);
            }
            reader.MoveToContent();
            reader.ReadEndElement();
        }

        public virtual void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer)
        {
            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));

            if (samlSerializer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSerializer"));

#pragma warning suppress 56506 // samlSerializer.DictionaryManager is never null.
            SamlDictionary dictionary = samlSerializer.DictionaryManager.SamlDictionary;

            writer.WriteStartElement(dictionary.PreferredPrefix.Value, dictionary.Conditions, dictionary.Namespace);

            if (this.notBefore != SecurityUtils.MinUtcDateTime)
            {
                writer.WriteStartAttribute(dictionary.NotBefore, null);
                writer.WriteString(this.notBefore.ToString(SamlConstants.GeneratedDateTimeFormat, DateTimeFormatInfo.InvariantInfo));
                writer.WriteEndAttribute();
            }

            if (this.notOnOrAfter != SecurityUtils.MaxUtcDateTime)
            {
                writer.WriteStartAttribute(dictionary.NotOnOrAfter, null);
                writer.WriteString(this.notOnOrAfter.ToString(SamlConstants.GeneratedDateTimeFormat, DateTimeFormatInfo.InvariantInfo));
                writer.WriteEndAttribute();
            }

            for (int i = 0; i < this.conditions.Count; i++)
            {
                this.conditions[i].WriteXml(writer, samlSerializer, keyInfoSerializer);
            }

            writer.WriteEndElement();
        }
    }

}
