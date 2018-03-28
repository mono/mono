//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
#pragma warning disable 1634, 1691

namespace System.ServiceModel.Web
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Net;
    using System.Net.Mime;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    public class IncomingWebRequestContext
    {
        static readonly string HttpGetMethod = "GET";
        static readonly string HttpHeadMethod = "HEAD";
        static readonly string HttpPutMethod = "PUT";
        static readonly string HttpPostMethod = "POST";
        static readonly string HttpDeleteMethod = "DELETE";

        Collection<ContentType> cachedAcceptHeaderElements;
        string acceptHeaderWhenHeaderElementsCached;
        internal const string UriTemplateMatchResultsPropertyName = "UriTemplateMatchResults";
        OperationContext operationContext;

        internal IncomingWebRequestContext(OperationContext operationContext)
        {
            Fx.Assert(operationContext != null, "operationContext is null");
            this.operationContext = operationContext;
        }

        public string Accept
        { 
            get { return EnsureMessageProperty().Headers[HttpRequestHeader.Accept]; } 
        }

        public long ContentLength
        { 
            get { return long.Parse(this.EnsureMessageProperty().Headers[HttpRequestHeader.ContentLength], CultureInfo.InvariantCulture); } 
        }

        public string ContentType
        { 
            get { return this.EnsureMessageProperty().Headers[HttpRequestHeader.ContentType]; } 
        }

        public IEnumerable<string> IfMatch
        {
            get 
            { 
                string ifMatchHeader = MessageProperty.Headers[HttpRequestHeader.IfMatch];
                return (string.IsNullOrEmpty(ifMatchHeader)) ? null : Utility.QuoteAwareStringSplit(ifMatchHeader);
            }
        }

        public IEnumerable<string> IfNoneMatch
        {
            get
            {
                string ifNoneMatchHeader = MessageProperty.Headers[HttpRequestHeader.IfNoneMatch];
                return (string.IsNullOrEmpty(ifNoneMatchHeader)) ? null : Utility.QuoteAwareStringSplit(ifNoneMatchHeader);
            }
        }

        public DateTime? IfModifiedSince
        {
            get 
            { 
                string dateTime = this.MessageProperty.Headers[HttpRequestHeader.IfModifiedSince];
                if (!string.IsNullOrEmpty(dateTime))
                {
                    DateTime parsedDateTime;
                    if (HttpDateParse.ParseHttpDate(dateTime, out parsedDateTime))
                    {
                        return parsedDateTime;
                    }
                }
                return null;
            }
        }

        public DateTime? IfUnmodifiedSince
        {
            get 
            { 
                string dateTime = this.MessageProperty.Headers[HttpRequestHeader.IfUnmodifiedSince];
                if (!string.IsNullOrEmpty(dateTime))
                {
                    DateTime parsedDateTime;
                    if (HttpDateParse.ParseHttpDate(dateTime, out parsedDateTime))
                    {
                        return parsedDateTime;
                    }
                }
                return null;
            }
        }

        public WebHeaderCollection Headers
        { 
            get { return this.EnsureMessageProperty().Headers; } 
        }

        public string Method
        { 
            get { return this.EnsureMessageProperty().Method; } 
        }

        public UriTemplateMatch UriTemplateMatch
        {
            get
            {
                if (this.operationContext.IncomingMessageProperties.ContainsKey(UriTemplateMatchResultsPropertyName))
                {
                    return this.operationContext.IncomingMessageProperties[UriTemplateMatchResultsPropertyName] as UriTemplateMatch;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                this.operationContext.IncomingMessageProperties[UriTemplateMatchResultsPropertyName] = value;
            }
        }

        public string UserAgent
        { 
            get { return this.EnsureMessageProperty().Headers[HttpRequestHeader.UserAgent]; } 
        }

        HttpRequestMessageProperty MessageProperty
        {
            get
            {
                if (operationContext.IncomingMessageProperties == null)
                {
                    return null;
                }
                if (!operationContext.IncomingMessageProperties.ContainsKey(HttpRequestMessageProperty.Name))
                {
                    return null;
                }
                return operationContext.IncomingMessageProperties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
            }
        }

        public void CheckConditionalRetrieve(string entityTag)
        {
            string validEtag = OutgoingWebResponseContext.GenerateValidEtagFromString(entityTag);
            CheckConditionalRetrieveWithValidatedEtag(validEtag);
        }

        public void CheckConditionalRetrieve(int entityTag)
        {
            string validEtag = OutgoingWebResponseContext.GenerateValidEtag(entityTag);
            CheckConditionalRetrieveWithValidatedEtag(validEtag);
        }

        public void CheckConditionalRetrieve(long entityTag)
        {
            string validEtag = OutgoingWebResponseContext.GenerateValidEtag(entityTag);
            CheckConditionalRetrieveWithValidatedEtag(validEtag);
        }

        public void CheckConditionalRetrieve(Guid entityTag)
        {
            string validEtag = OutgoingWebResponseContext.GenerateValidEtag(entityTag);
            CheckConditionalRetrieveWithValidatedEtag(validEtag);
        }

        public void CheckConditionalRetrieve(DateTime lastModified)
        {
            if (!string.Equals(this.Method, IncomingWebRequestContext.HttpGetMethod, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(this.Method, IncomingWebRequestContext.HttpHeadMethod, StringComparison.OrdinalIgnoreCase))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR2.GetString(SR2.ConditionalRetrieveGetAndHeadOnly, this.Method)));
            }

            DateTime? ifModifiedSince = this.IfModifiedSince;
            if (ifModifiedSince.HasValue)
            {
                long ticksDifference = lastModified.ToUniversalTime().Ticks - ifModifiedSince.Value.ToUniversalTime().Ticks;
                if (ticksDifference < TimeSpan.TicksPerSecond)
                {
                    WebOperationContext.Current.OutgoingResponse.LastModified = lastModified;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WebFaultException(HttpStatusCode.NotModified));
                }
            }            
        }

        public void CheckConditionalUpdate(string entityTag)
        {
            string validEtag = OutgoingWebResponseContext.GenerateValidEtagFromString(entityTag);
            CheckConditionalUpdateWithValidatedEtag(validEtag);
        }

        public void CheckConditionalUpdate(int entityTag)
        {
            string validEtag = OutgoingWebResponseContext.GenerateValidEtag(entityTag);
            CheckConditionalUpdateWithValidatedEtag(validEtag);
        }

        public void CheckConditionalUpdate(long entityTag)
        {
            string validEtag = OutgoingWebResponseContext.GenerateValidEtag(entityTag);
            CheckConditionalUpdateWithValidatedEtag(validEtag);
        }

        public void CheckConditionalUpdate(Guid entityTag)
        {
            string validEtag = OutgoingWebResponseContext.GenerateValidEtag(entityTag);
            CheckConditionalUpdateWithValidatedEtag(validEtag);
        }

        public Collection<ContentType> GetAcceptHeaderElements()
        {
            string acceptHeader = this.Accept;
            if (cachedAcceptHeaderElements == null ||
                (!string.Equals(acceptHeaderWhenHeaderElementsCached, acceptHeader, StringComparison.OrdinalIgnoreCase)))
            {
                if (string.IsNullOrEmpty(acceptHeader))
                {
                    cachedAcceptHeaderElements = new Collection<ContentType>();
                    acceptHeaderWhenHeaderElementsCached = acceptHeader;
                }
                else
                {
                    List<ContentType> contentTypeList = new List<ContentType>();
                    int offset = 0;
                    while (true)
                    {
                        string nextItem = Utility.QuoteAwareSubString(acceptHeader, ref offset);
                        if (nextItem == null)
                        {
                            break;
                        }

                        ContentType contentType = Utility.GetContentTypeOrNull(nextItem);
                        if (contentType != null)
                        {
                            contentTypeList.Add(contentType);
                        }
                    }
                    
                    contentTypeList.Sort(new AcceptHeaderElementComparer());
                    cachedAcceptHeaderElements = new Collection<ContentType>(contentTypeList);
                    acceptHeaderWhenHeaderElementsCached = acceptHeader;
                }
            }
            return cachedAcceptHeaderElements;
        }

        HttpRequestMessageProperty EnsureMessageProperty()
        {
            if (this.MessageProperty == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR2.GetString(SR2.HttpContextNoIncomingMessageProperty, typeof(HttpRequestMessageProperty).Name)));
            }
            return this.MessageProperty;
        }


        void CheckConditionalRetrieveWithValidatedEtag(string entityTag)
        {
            if (!string.Equals(this.Method, IncomingWebRequestContext.HttpGetMethod, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(this.Method, IncomingWebRequestContext.HttpHeadMethod, StringComparison.OrdinalIgnoreCase))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR2.GetString(SR2.ConditionalRetrieveGetAndHeadOnly, this.Method)));
            }

            if (!string.IsNullOrEmpty(entityTag))
            {               
                string entityTagHeader = this.Headers[HttpRequestHeader.IfNoneMatch];
                if (!string.IsNullOrEmpty(entityTagHeader))
                {
                    if (IsWildCardCharacter(entityTagHeader) ||
                        DoesHeaderContainEtag(entityTagHeader, entityTag))
                    {
                        // set response entityTag directly because it has already been validated
                        WebOperationContext.Current.OutgoingResponse.ETag = entityTag;
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WebFaultException(HttpStatusCode.NotModified));
                    }
                }               
            }
        }

        void CheckConditionalUpdateWithValidatedEtag(string entityTag)
        {
            bool isPutMethod = string.Equals(this.Method, IncomingWebRequestContext.HttpPutMethod, StringComparison.OrdinalIgnoreCase);
            if (!isPutMethod &&
                !string.Equals(this.Method, IncomingWebRequestContext.HttpPostMethod, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(this.Method, IncomingWebRequestContext.HttpDeleteMethod, StringComparison.OrdinalIgnoreCase))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR2.GetString(SR2.ConditionalUpdatePutPostAndDeleteOnly, this.Method)));
            }

            string headerOfInterest;

            // if the current entityTag is null then the resource doesn't currently exist and the
            //   a PUT request should only succeed if If-None-Match equals '*'.  
            if (isPutMethod && string.IsNullOrEmpty(entityTag))
            {
                headerOfInterest = this.Headers[HttpRequestHeader.IfNoneMatch];
                if (string.IsNullOrEmpty(headerOfInterest) ||
                    !IsWildCardCharacter(headerOfInterest))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WebFaultException(HttpStatusCode.PreconditionFailed));
                }
            }
            else
            {
                // all remaining cases are with an If-Match header
                headerOfInterest = this.Headers[HttpRequestHeader.IfMatch];
                if (string.IsNullOrEmpty(headerOfInterest) ||
                    (!IsWildCardCharacter(headerOfInterest) &&
                    !DoesHeaderContainEtag(headerOfInterest, entityTag)))
                {
                    // set response entityTag directly because it has already been validated
                    WebOperationContext.Current.OutgoingResponse.ETag = entityTag;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WebFaultException(HttpStatusCode.PreconditionFailed));
                }
            }
            
        }

        static bool DoesHeaderContainEtag(string header, string entityTag)
        {
            int offset = 0;
            while (true)
            {
                string nextEntityTag = Utility.QuoteAwareSubString(header, ref offset);
                if (nextEntityTag == null)
                {
                    break;
                }
                if (string.Equals(nextEntityTag, entityTag, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsWildCardCharacter(string header)
        {
            return (header.Trim() == "*");
        }
    }

    class AcceptHeaderElementComparer : IComparer<ContentType>
    {
        static NumberStyles numberStyles = NumberStyles.AllowDecimalPoint;

        public int Compare(ContentType x, ContentType y)
        {
            string[] xTypeSubType = x.MediaType.Split('/');
            string[] yTypeSubType = y.MediaType.Split('/');

            Fx.Assert(xTypeSubType.Length == 2, "The creation of the ContentType would have failed if there wasn't a type and subtype.");
            Fx.Assert(yTypeSubType.Length == 2, "The creation of the ContentType would have failed if there wasn't a type and subtype.");

            if (string.Equals(xTypeSubType[0], yTypeSubType[0], StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(xTypeSubType[1], yTypeSubType[1], StringComparison.OrdinalIgnoreCase))
                {
                    // need to check the number of parameters to determine which is more specific
                    bool xHasParam = HasParameters(x);
                    bool yHasParam = HasParameters(y);
                    if (xHasParam && !yHasParam)
                    {
                        return 1;
                    }
                    else if (!xHasParam && yHasParam)
                    {
                        return -1;
                    }
                }
                else
                {
                    if (xTypeSubType[1][0] == '*' && xTypeSubType[1].Length == 1)
                    {
                        return 1;
                    }
                    if (yTypeSubType[1][0] == '*' && yTypeSubType[1].Length == 1)
                    {
                        return -1;
                    }
                }
            }
            else if (xTypeSubType[0][0] == '*' && xTypeSubType[0].Length == 1)
            {
                return 1;
            }
            else if (yTypeSubType[0][0] == '*' && yTypeSubType[0].Length == 1)
            {
                return -1;
            }

            decimal qualityDifference = GetQualityFactor(x) - GetQualityFactor(y);
            if (qualityDifference < 0)
            {
                return 1;
            }
            else if (qualityDifference > 0)
            {
                return -1;
            }
            return 0;
        }

        decimal GetQualityFactor(ContentType contentType)
        {
            decimal result;
            foreach (string key in contentType.Parameters.Keys)
            {
                if (string.Equals("q", key, StringComparison.OrdinalIgnoreCase))
                {
                    if (decimal.TryParse(contentType.Parameters[key], numberStyles, CultureInfo.InvariantCulture, out result) &&
                        (result <= (decimal)1.0))
                    {
                        return result;
                    }
                }
            }

            return (decimal)1.0;
        }

        bool HasParameters(ContentType contentType)
        {
            int number = 0;
            foreach (string param in contentType.Parameters.Keys)
            {
                if (!string.Equals("q", param, StringComparison.OrdinalIgnoreCase))
                {
                    number++;
                }
            }

            return (number > 0);
        }
    }
}
