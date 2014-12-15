//-----------------------------------------------------------------------------
// <copyright file="ContentTypeField.cs" company="Microsoft">
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

    /// <summary>
    /// Typed Content-Type header
    ///
    /// We parse the type during construction and set.
    /// null and string.empty will throw for construction,set and mediatype/subtype
    /// constructors set isPersisted to false.  isPersisted needs to be tracked seperately
    /// than isChanged because isChanged only determines if the cached value should be used.
    /// isPersisted tracks if the object has been persisted. However, obviously if isChanged is true
    /// the object isn't  persisted.
    /// If any subcomponents are changed, isChanged is set to true and isPersisted is false
    /// ToString caches the value until a isChanged is true, then it recomputes the full value.
    /// </summary>



    public class ContentType
    {
        string mediaType;
        string subType;
        bool isChanged;
        string type;
        bool isPersisted;
        TrackingStringDictionary parameters;

        /// <summary>
        /// Default content type - can be used if the Content-Type header
        /// is not defined in the message headers.
        /// </summary>
        internal readonly static string Default = "application/octet-stream";

        public ContentType() : this(Default)
        {
        }

        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="fieldValue">Unparsed value of the Content-Type header.</param>
        public ContentType(string contentType)
        {
            if (contentType == null) {
                throw new ArgumentNullException("contentType");
            }
            if (contentType == String.Empty) {
                throw new ArgumentException(SR.GetString(SR.net_emptystringcall,"contentType"), "contentType");
            }                
            isChanged = true;
            type = contentType;
            ParseValue();
        }

        public string Boundary
        {
            get
            {
                return Parameters["boundary"];
            }
            set
            {
                if (value == null || value == string.Empty) {
                    Parameters.Remove("boundary");
                }
                else{
                    Parameters["boundary"] = value;
                }
            }
        }

        public string CharSet
        {
            get
            {
                return Parameters["charset"];
            }
            set
            {
                if (value == null || value == string.Empty) {
                    Parameters.Remove("charset");
                }
                else{
                    Parameters["charset"] = value;
                }
            }
        }

        /// <summary>
        /// Gets the media type.
        /// </summary>
        public string MediaType
        {
            get
            {
                return mediaType + "/" + subType;
            }
            set
            {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                
                if (value == string.Empty) {
                    throw new ArgumentException(SR.GetString(SR.net_emptystringset), "value");
                }

                int offset = 0;
                mediaType = MailBnfHelper.ReadToken(value, ref offset, null);
                if (mediaType.Length == 0 || offset >= value.Length || value[offset++] != '/')
                    throw new FormatException(SR.GetString(SR.MediaTypeInvalid));

                subType = MailBnfHelper.ReadToken(value, ref offset, null);
                if(subType.Length == 0 || offset < value.Length){
                    throw new FormatException(SR.GetString(SR.MediaTypeInvalid));
                }

                isChanged = true;
                isPersisted = false;
            }
        }


        public string Name {
            get {
                string value = Parameters["name"];
                Encoding nameEncoding = MimeBasePart.DecodeEncoding(value);
                if(nameEncoding != null)
                    value = MimeBasePart.DecodeHeaderValue(value);
                return value;
            }
            set {
                if (value == null || value == string.Empty) {
                    Parameters.Remove("name");
                }
                else{
                    Parameters["name"] = value;
                }
            }
        }
        

        public StringDictionary Parameters
        {
            get
            {
                if (parameters == null)
                {
                    if (type == null) {
                        parameters = new TrackingStringDictionary();
                    }
                }
                return parameters;
            }
        }


        
        internal void Set(string contentType, HeaderCollection headers) {
            type = contentType;
            ParseValue();
            headers.InternalSet(MailHeaderInfo.GetString(MailHeaderID.ContentType), ToString());
            isPersisted = true;
        }
        
        
        internal void PersistIfNeeded(HeaderCollection headers, bool forcePersist) {
            if (IsChanged || !isPersisted || forcePersist) {
                headers.InternalSet(MailHeaderInfo.GetString(MailHeaderID.ContentType), ToString());
                isPersisted = true;
            }
        }

        internal bool IsChanged {
            get {
                return (isChanged || parameters != null && parameters.IsChanged);
            }
        }

        public override string ToString() {
            if (type == null || IsChanged)
            {
                type = Encode(false); // Legacy wire-safe format
                isChanged = false;
                parameters.IsChanged = false;
                isPersisted = false;
            }
            return type;
        }

        internal string Encode(bool allowUnicode)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(mediaType); // Must not have unicode, already validated
            builder.Append('/');
            builder.Append(subType);  // Must not have unicode, already validated
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
                encoding =Encoding.GetEncoding(MimeBasePart.defaultCharSet);
                builder.Append("\"" + MimeBasePart.EncodeHeaderValue(value, encoding,
                    MimeBasePart.ShouldUseBase64Encoding(encoding)) + "\"");
            }
        }

        public override bool Equals(object rparam) {
            if (rparam == null) {
                return false;
            }
            
            return (String.Compare(ToString(), rparam.ToString(), StringComparison.OrdinalIgnoreCase ) == 0);
        }

        public override int GetHashCode(){
            return ToString().ToLowerInvariant().GetHashCode();
        }

        // Helper methods.

        void ParseValue()
        {
            int offset = 0;
            Exception exception = null;
            parameters = new TrackingStringDictionary();
            
            try{
                mediaType = MailBnfHelper.ReadToken(type, ref offset, null);
                if (mediaType == null || mediaType.Length == 0 ||  offset >= type.Length || type[offset++] != '/'){
                    exception = new FormatException(SR.GetString(SR.ContentTypeInvalid));
                }

                if (exception == null) {
                    subType = MailBnfHelper.ReadToken(type, ref offset, null);
                    if (subType == null || subType.Length == 0){
                        exception = new FormatException(SR.GetString(SR.ContentTypeInvalid));
                    }
                }
                
                if (exception == null) {
                    while (MailBnfHelper.SkipCFWS(type, ref offset))
                    {
                        if (type[offset++] != ';'){
                            exception = new FormatException(SR.GetString(SR.ContentTypeInvalid));
                            break;
                        }
        
                        if (!MailBnfHelper.SkipCFWS(type, ref offset))
                            break;
        
                        string paramAttribute = MailBnfHelper.ReadParameterAttribute(type, ref offset, null);
                        
                        if(paramAttribute == null || paramAttribute.Length == 0){
                            exception = new FormatException(SR.GetString(SR.ContentTypeInvalid));
                            break;
                        }
                        
                        string paramValue;
                        if ( offset >= type.Length || type[offset++] != '='){
                            exception = new FormatException(SR.GetString(SR.ContentTypeInvalid));
                            break;
                        }
                        
                        if (!MailBnfHelper.SkipCFWS(type, ref offset)){
                            exception = new FormatException(SR.GetString(SR.ContentTypeInvalid));
                            break;
                        }
        
                        if (type[offset] == '"')
                            paramValue = MailBnfHelper.ReadQuotedString(type, ref offset, null);
                        else
                            paramValue = MailBnfHelper.ReadToken(type, ref offset, null);
                        
                        if(paramValue == null){
                            exception = new FormatException(SR.GetString(SR.ContentTypeInvalid));
                            break;
                        }
        
                        parameters.Add(paramAttribute, paramValue);
                    }
                }
                parameters.IsChanged = false;
            }
            catch(FormatException){
                throw new FormatException(SR.GetString(SR.ContentTypeInvalid));
            }

            if(exception != null){
                throw new FormatException(SR.GetString(SR.ContentTypeInvalid));
            }
        }
    }
}
