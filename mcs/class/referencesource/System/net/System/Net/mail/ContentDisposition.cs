//-----------------------------------------------------------------------------
// <copyright file="ContentDispositionField.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------

namespace System.Net.Mime
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Text;
    using System.Globalization;
    using System.Net.Mail;
    using System.Collections.Generic;

    public class ContentDisposition
    {
        string dispositionType;
        TrackingValidationObjectDictionary parameters;
        bool isChanged;
        bool isPersisted;
        string disposition;
        const string creationDate = "creation-date";
        const string readDate = "read-date";
        const string modificationDate = "modification-date";
        const string size = "size";
        const string fileName = "filename";

        private static readonly TrackingValidationObjectDictionary.ValidateAndParseValue dateParser =
                new TrackingValidationObjectDictionary.ValidateAndParseValue
                ((object value) =>
                {
                    // this will throw a FormatException if the value supplied is not a valid SmtpDateTime
                    SmtpDateTime date = new SmtpDateTime(value.ToString());
                    return date;
                });

        private static readonly TrackingValidationObjectDictionary.ValidateAndParseValue longParser =
                new TrackingValidationObjectDictionary.ValidateAndParseValue
                ((object value) =>
                {
                    long longValue;
                    if (!long.TryParse(value.ToString(),
                        NumberStyles.None, CultureInfo.InvariantCulture, out longValue))
                    {
                        throw new FormatException(SR.GetString(SR.ContentDispositionInvalid));
                    }
                    return longValue;
                });

        private static readonly IDictionary<string, TrackingValidationObjectDictionary.ValidateAndParseValue>
            validators;

        static ContentDisposition()
        {
            validators = new Dictionary<string, TrackingValidationObjectDictionary.ValidateAndParseValue>();

            validators.Add(creationDate, dateParser);
            validators.Add(modificationDate, dateParser);
            validators.Add(readDate, dateParser);
            validators.Add(size, longParser);
        }

        public ContentDisposition()
        {
            isChanged = true;
            dispositionType = "attachment";
            disposition = dispositionType;
            // no need to parse disposition since there's nothing to parse
        }

        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="fieldValue">Unparsed header value.</param>
        public ContentDisposition(string disposition)
        {
            if (disposition == null)
                throw new ArgumentNullException("disposition");
            isChanged = true;
            this.disposition = disposition;
            ParseValue();
        }

        internal DateTime GetDateParameter(string parameterName)
        {
            SmtpDateTime dateValue =
                ((TrackingValidationObjectDictionary)Parameters).InternalGet(parameterName) as SmtpDateTime;
            if (dateValue == null)
            {
                return DateTime.MinValue;
            }
            return dateValue.Date;
        }

        /// <summary>
        /// Gets the disposition type of the content.
        /// </summary>
        public string DispositionType
        {
            get
            {
                return dispositionType;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (value == string.Empty)
                {
                    throw new ArgumentException(SR.GetString(SR.net_emptystringset), "value");
                }

                isChanged = true;
                dispositionType = value;
            }
        }

        public StringDictionary Parameters
        {
            get
            {
                if (parameters == null)
                {
                    parameters = new TrackingValidationObjectDictionary(validators);
                }

                return parameters;
            }
        }

        /// <summary>
        /// Gets the value of the Filename parameter.
        /// </summary>
        public string FileName
        {
            get
            {
                return Parameters[fileName];
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    Parameters.Remove(fileName);
                }
                else
                {
                    Parameters[fileName] = value;
                }
            }
        }

        /// <summary>
        /// Gets the value of the Creation-Date parameter.
        /// </summary>
        public DateTime CreationDate
        {
            get
            {
                return GetDateParameter(creationDate);
            }
            set
            {
                SmtpDateTime date = new SmtpDateTime(value);
                ((TrackingValidationObjectDictionary)Parameters).InternalSet(creationDate, date);
            }
        }

        /// <summary>
        /// Gets the value of the Modification-Date parameter.
        /// </summary>
        public DateTime ModificationDate
        {
            get
            {
                return GetDateParameter(modificationDate);
            }
            set
            {
                SmtpDateTime date = new SmtpDateTime(value);
                ((TrackingValidationObjectDictionary)Parameters).InternalSet(modificationDate, date);
            }
        }

        public bool Inline
        {
            get
            {
                return (dispositionType == DispositionTypeNames.Inline);
            }
            set
            {
                isChanged = true;
                if (value)
                {
                    dispositionType = DispositionTypeNames.Inline;
                }
                else
                {
                    dispositionType = DispositionTypeNames.Attachment;
                }
            }
        }

        /// <summary>
        /// Gets the value of the Read-Date parameter.
        /// </summary>
        public DateTime ReadDate
        {
            get
            {
                return GetDateParameter(readDate);
            }
            set
            {
                SmtpDateTime date = new SmtpDateTime(value);
                ((TrackingValidationObjectDictionary)Parameters).InternalSet(readDate, date);
            }
        }

        /// <summary>
        /// Gets the value of the Size parameter (-1 if unspecified).
        /// </summary>
        public long Size
        {
            get
            {
                object sizeValue = ((TrackingValidationObjectDictionary)Parameters).InternalGet(size);
                if (sizeValue == null)
                    return -1;
                else
                    return (long)sizeValue;
            }
            set
            {
                ((TrackingValidationObjectDictionary)Parameters).InternalSet(size, value);
            }
        }

        internal void Set(string contentDisposition, HeaderCollection headers)
        {
            // we don't set ischanged because persistence was already handled
            // via the headers.
            disposition = contentDisposition;
            ParseValue();
            headers.InternalSet(MailHeaderInfo.GetString(MailHeaderID.ContentDisposition), ToString());
            isPersisted = true;
        }

        internal void PersistIfNeeded(HeaderCollection headers, bool forcePersist)
        {
            if (IsChanged || !isPersisted || forcePersist)
            {
                headers.InternalSet(MailHeaderInfo.GetString(MailHeaderID.ContentDisposition), ToString());
                isPersisted = true;
            }
        }

        internal bool IsChanged
        {
            get
            {
                return (isChanged || parameters != null && parameters.IsChanged);
            }
        }

        public override string ToString()
        {
            if (disposition == null || isChanged || parameters != null && parameters.IsChanged)
            {
                disposition = Encode(false); // Legacy wire-safe format
                isChanged = false;
                parameters.IsChanged = false;
                isPersisted = false;
            }
            return disposition;
        }

        internal string Encode(bool allowUnicode)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(dispositionType); // Must not have unicode, already validated
            // Validate and encode unicode where required
            foreach (string key in Parameters.Keys)
            {
                builder.Append("; ");
                EncodeToBuffer(key, builder, allowUnicode);
                builder.Append('=');
                EncodeToBuffer(parameters[key], builder, allowUnicode);
            }
            return builder.ToString();
        }

        private static void EncodeToBuffer(string value, StringBuilder builder, bool allowUnicode)
        {
            Encoding encoding = MimeBasePart.DecodeEncoding(value);
            if (encoding != null) // Manually encoded elsewhere, pass through
            {
                builder.Append("\"" + value + "\"");
            }
            else if ((allowUnicode && !MailBnfHelper.HasCROrLF(value)) // Unicode without CL or LF's
                || MimeBasePart.IsAscii(value, false)) // Ascii
            {
                MailBnfHelper.GetTokenOrQuotedString(value, builder, allowUnicode);
            }
            else
            {
                // MIME Encoding required
                encoding = Encoding.GetEncoding(MimeBasePart.defaultCharSet);
                builder.Append("\"" + MimeBasePart.EncodeHeaderValue(value, encoding,
                    MimeBasePart.ShouldUseBase64Encoding(encoding)) + "\"");
            }
        }

        public override bool Equals(object rparam)
        {
            if (rparam == null)
            {
                return false;
            }
            return (String.Compare(ToString(), rparam.ToString(), StringComparison.OrdinalIgnoreCase) == 0);
        }

        public override int GetHashCode()
        {
            return ToString().ToLowerInvariant().GetHashCode();
        }

        void ParseValue()
        {
            int offset = 0;
            try
            {
                // the disposition MUST be the first parameter in the string
                dispositionType = MailBnfHelper.ReadToken(disposition, ref offset, null);

                // disposition MUST not be empty
                if (String.IsNullOrEmpty(dispositionType))
                {
                    throw new FormatException(SR.GetString(SR.MailHeaderFieldMalformedHeader));
                }

                // now we know that there are parameters so we must initialize or clear
                // and parse
                if (parameters == null)
                {
                    parameters = new TrackingValidationObjectDictionary(validators);
                }
                else
                {
                    parameters.Clear();
                }

                while (MailBnfHelper.SkipCFWS(disposition, ref offset))
                {
                    // ensure that the separator charactor is present
                    if (disposition[offset++] != ';')
                        throw new FormatException(SR.GetString(SR.MailHeaderFieldInvalidCharacter, disposition[offset-1]));

                    // skip whitespace and see if there's anything left to parse or if we're done
                    if (!MailBnfHelper.SkipCFWS(disposition, ref offset))
                        break;

                    string paramAttribute = MailBnfHelper.ReadParameterAttribute(disposition, ref offset, null);
                    string paramValue;

                    // verify the next character after the parameter is correct
                    if (disposition[offset++] != '=')
                    {
                        throw new FormatException(SR.GetString(SR.MailHeaderFieldMalformedHeader));
                    }

                    if (!MailBnfHelper.SkipCFWS(disposition, ref offset))
                    {
                        // parameter was at end of string and has no value
                        // this is not valid
                        throw new FormatException(SR.GetString(SR.ContentDispositionInvalid));
                    }

                    if (disposition[offset] == '"')
                    {
                        paramValue = MailBnfHelper.ReadQuotedString(disposition, ref offset, null);
                    }
                    else
                    {
                        paramValue = MailBnfHelper.ReadToken(disposition, ref offset, null);
                    }

                    // paramValue could potentially still be empty if it was a valid quoted string that
                    // contained no inner value.  this is invalid
                    if (String.IsNullOrEmpty(paramAttribute) || string.IsNullOrEmpty(paramValue))
                    {
                        throw new FormatException(SR.GetString(SR.ContentDispositionInvalid));
                    }

                    // if validation is needed, the parameters dictionary will have a validator registered  
                    // for the parameter that is being set so no additional formatting checks are needed here
                    Parameters.Add(paramAttribute, paramValue);
                }
            }
            catch (FormatException exception)
            {
                // it's possible that something in MailBNFHelper could throw so ensure that we catch it and wrap it
                // so that the exception has the correct text
                throw new FormatException(SR.GetString(SR.ContentDispositionInvalid), exception);
            }
            parameters.IsChanged = false;
        }
    }
}
