//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
#pragma warning disable 1634, 1691

namespace System.ServiceModel.Web
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Text;
    using System.Collections.Generic;

    public class OutgoingWebResponseContext
    {
        internal static readonly string WebResponseFormatPropertyName = "WebResponseFormatProperty";
        internal static readonly string AutomatedFormatSelectionContentTypePropertyName = "AutomatedFormatSelectionContentTypePropertyName";

        Encoding bindingWriteEncoding = null;

        OperationContext operationContext;
        internal OutgoingWebResponseContext(OperationContext operationContext)
        {
            Fx.Assert(operationContext != null, "operationContext is null");
            this.operationContext = operationContext;
        }

        public long ContentLength
        {
            get { return long.Parse(this.MessageProperty.Headers[HttpResponseHeader.ContentLength], CultureInfo.InvariantCulture); }
            set { this.MessageProperty.Headers[HttpResponseHeader.ContentLength] = value.ToString(CultureInfo.InvariantCulture); }
        }

        public string ContentType
        {
            get { return this.MessageProperty.Headers[HttpResponseHeader.ContentType]; }
            set { this.MessageProperty.Headers[HttpResponseHeader.ContentType] = value; }
        }

        public string ETag
        {
            get { return this.MessageProperty.Headers[HttpResponseHeader.ETag]; }
            set { this.MessageProperty.Headers[HttpResponseHeader.ETag] = value; }
        }

        public WebHeaderCollection Headers
        {
            get { return this.MessageProperty.Headers; }
        }

        public DateTime LastModified
        {
            get
            {
                string dateTime = this.MessageProperty.Headers[HttpRequestHeader.LastModified];
                if (!string.IsNullOrEmpty(dateTime))
                {
                    DateTime parsedDateTime;
                    if (DateTime.TryParse(dateTime, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
                    {
                        return parsedDateTime;
                    }
                }
                return DateTime.MinValue;
            }
            set
            {
                this.MessageProperty.Headers[HttpResponseHeader.LastModified] =
                    (value.Kind == DateTimeKind.Utc ?
                    value.ToString("R", CultureInfo.InvariantCulture) :
                    value.ToUniversalTime().ToString("R", CultureInfo.InvariantCulture));
            }
        }

        public string Location
        {
            get { return this.MessageProperty.Headers[HttpResponseHeader.Location]; }
            set { this.MessageProperty.Headers[HttpResponseHeader.Location] = value; }
        }

        public HttpStatusCode StatusCode
        {
            get { return this.MessageProperty.StatusCode; }
            set { this.MessageProperty.StatusCode = value; }
        }

        public string StatusDescription
        {
            get { return this.MessageProperty.StatusDescription; }
            set { this.MessageProperty.StatusDescription = value; }
        }

        public bool SuppressEntityBody
        {
            get { return this.MessageProperty.SuppressEntityBody; }
            set { this.MessageProperty.SuppressEntityBody = value; }
        }

        public WebMessageFormat? Format
        {
            get
            {
                if (!operationContext.OutgoingMessageProperties.ContainsKey(WebResponseFormatPropertyName))
                {
                    return null;
                }
                return operationContext.OutgoingMessageProperties[WebResponseFormatPropertyName] as WebMessageFormat?;
            }
            set
            {
                if (value.HasValue)
                {
                    if (!WebMessageFormatHelper.IsDefined(value.Value))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                    }
                    else
                    {
                        operationContext.OutgoingMessageProperties[WebResponseFormatPropertyName] = value.Value;
                    }
                }
                else
                {
                    operationContext.OutgoingMessageProperties[WebResponseFormatPropertyName] = null;
                }
                this.AutomatedFormatSelectionContentType = null;
            }
        }

        // This is an internal property because we need to carry the content-type that was selected by the FormatSelectingMessageInspector
        // forward so that the formatter has access to it. However, we dond't want to use the ContentType property on this, because then
        // developers would have to clear the ContentType property manually when overriding the format set by the 
        // FormatSelectingMessageInspector
        internal string AutomatedFormatSelectionContentType
        {
            get
            {
                if (!operationContext.OutgoingMessageProperties.ContainsKey(AutomatedFormatSelectionContentTypePropertyName))
                {
                    return null;
                }
                return operationContext.OutgoingMessageProperties[AutomatedFormatSelectionContentTypePropertyName] as string;
            }
            set
            {
                operationContext.OutgoingMessageProperties[AutomatedFormatSelectionContentTypePropertyName] = value;
            }
        }

        public Encoding BindingWriteEncoding
        {
            get
            {
                if (this.bindingWriteEncoding == null)
                {
                    string endpointId = this.operationContext.EndpointDispatcher.Id;
                    Fx.Assert(endpointId != null, "There should always be an valid EndpointDispatcher.Id");
                    foreach (ServiceEndpoint endpoint in this.operationContext.Host.Description.Endpoints)
                    {
                        if (endpoint.Id == endpointId)
                        {
                            WebMessageEncodingBindingElement encodingElement = endpoint.Binding.CreateBindingElements().Find<WebMessageEncodingBindingElement>() as WebMessageEncodingBindingElement;
                            if (encodingElement != null)
                            {
                                this.bindingWriteEncoding = encodingElement.WriteEncoding;
                            }
                        }
                    }
                }
                return this.bindingWriteEncoding;
            }
        }

        internal HttpResponseMessageProperty MessageProperty
        {
            get
            {
                if (!operationContext.OutgoingMessageProperties.ContainsKey(HttpResponseMessageProperty.Name))
                {
                    operationContext.OutgoingMessageProperties.Add(HttpResponseMessageProperty.Name, new HttpResponseMessageProperty());
                }
                return operationContext.OutgoingMessageProperties[HttpResponseMessageProperty.Name] as HttpResponseMessageProperty;
            }
        }

        public void SetETag(string entityTag)
        {
            this.ETag = GenerateValidEtagFromString(entityTag);
        }

        public void SetETag(int entityTag)
        {
            this.ETag = GenerateValidEtag(entityTag);
        }

        public void SetETag(long entityTag)
        {
            this.ETag = GenerateValidEtag(entityTag);
        }

        public void SetETag(Guid entityTag)
        {
            this.ETag = GenerateValidEtag(entityTag);
        }

        public void SetStatusAsCreated(Uri locationUri)
        {
            if (locationUri == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("locationUri");
            }
            this.StatusCode = HttpStatusCode.Created;
            this.Location = locationUri.ToString();
        }

        public void SetStatusAsNotFound()
        {
            this.StatusCode = HttpStatusCode.NotFound;
        }

        public void SetStatusAsNotFound(string description)
        {
            this.StatusCode = HttpStatusCode.NotFound;
            this.StatusDescription = description;
        }

        internal static string GenerateValidEtagFromString(string entityTag)
        {
            // This method will generate a valid entityTag from a string by doing the following:
            //   1) Adding surrounding double quotes if the string doesn't already start and end with them
            //   2) Escaping any internal double quotes that aren't already escaped (preceded with a backslash)
            //   3) If a string starts with a double quote but doesn't end with one, or vice-versa, then the 
            //      double quote is considered internal and is escaped.

            if (string.IsNullOrEmpty(entityTag))
            {
                return null;
            }

            if (entityTag.StartsWith("W/\"", StringComparison.OrdinalIgnoreCase) &&
                entityTag.EndsWith("\"", StringComparison.OrdinalIgnoreCase))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR2.GetString(SR2.WeakEntityTagsNotSupported, entityTag)));
            }

            List<int> escapeCharacterInsertIndices = null;
            int lastEtagIndex = entityTag.Length - 1;
            bool startsWithQuote = entityTag[0] == '\"';
            bool endsWithQuote = entityTag[lastEtagIndex] == '\"';

            // special case where the entityTag is a single character, a double quote, '"'
            if (lastEtagIndex == 0 && startsWithQuote)
            {
                endsWithQuote = false;
            }

            bool needsSurroundingQuotes = !startsWithQuote || !endsWithQuote;

            if (startsWithQuote && !endsWithQuote)
            {
                if (escapeCharacterInsertIndices == null)
                {
                    escapeCharacterInsertIndices = new List<int>();
                }
                escapeCharacterInsertIndices.Add(0);
            }

            for (int x = 1; x < lastEtagIndex; x++)
            {
                if (entityTag[x] == '\"' && entityTag[x - 1] != '\\')
                {
                    if (escapeCharacterInsertIndices == null)
                    {
                        escapeCharacterInsertIndices = new List<int>();
                    }
                    escapeCharacterInsertIndices.Add(x + escapeCharacterInsertIndices.Count);
                }
            }

            // Possible that the ending internal quote is already escaped so must check the character before it
            if (!startsWithQuote && endsWithQuote && entityTag[lastEtagIndex - 1] != '\\')
            {
                if (escapeCharacterInsertIndices == null)
                {
                    escapeCharacterInsertIndices = new List<int>();
                }
                escapeCharacterInsertIndices.Add(lastEtagIndex + escapeCharacterInsertIndices.Count);
            }

            if (needsSurroundingQuotes || escapeCharacterInsertIndices != null)
            {
                int escapeCharacterInsertIndicesCount = (escapeCharacterInsertIndices == null) ? 0 : escapeCharacterInsertIndices.Count;
                StringBuilder editedEtag = new StringBuilder(entityTag, entityTag.Length + escapeCharacterInsertIndicesCount + 2);
                for (int x = 0; x < escapeCharacterInsertIndicesCount; x++)
                {
                    editedEtag.Insert(escapeCharacterInsertIndices[x], '\\');
                }
                if (needsSurroundingQuotes)
                {
                    editedEtag.Insert(entityTag.Length + escapeCharacterInsertIndicesCount, '\"');
                    editedEtag.Insert(0, '\"');
                }
                entityTag = editedEtag.ToString();
            }

            return entityTag;
        }

        internal static string GenerateValidEtag(object entityTag)
        {
            return string.Format(CultureInfo.InvariantCulture, "\"{0}\"", entityTag.ToString());
        }
    }
}

