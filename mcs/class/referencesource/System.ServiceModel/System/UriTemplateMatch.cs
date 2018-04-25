//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System
{
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.Net;

    [TypeForwardedFrom("System.ServiceModel.Web, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class UriTemplateMatch
    {
        Uri baseUri;
        NameValueCollection boundVariables;
        object data;
        NameValueCollection queryParameters;
        Collection<string> relativePathSegments;
        Uri requestUri;
        UriTemplate template;
        Collection<string> wildcardPathSegments;
        int wildcardSegmentsStartOffset = -1;
        Uri originalBaseUri;
        HttpRequestMessageProperty requestProp;

        public UriTemplateMatch()
        {
        }

        public Uri BaseUri   // the base address, untouched
        {
            get
            {
                if (this.baseUri == null && this.originalBaseUri != null)
                {
                    this.baseUri = UriTemplate.RewriteUri(this.originalBaseUri, this.requestProp.Headers[HttpRequestHeader.Host]);
                }
                return this.baseUri;
            }
            set
            {
                this.baseUri = value;
                this.originalBaseUri = null;
                this.requestProp = null;
            }
        }
        public NameValueCollection BoundVariables // result of TryLookup, values are decoded
        {
            get
            {
                if (this.boundVariables == null)
                {
                    this.boundVariables = new NameValueCollection();
                }
                return this.boundVariables;
            }
        }
        public object Data
        {
            get
            {
                return this.data;
            }
            set
            {
                this.data = value;
            }
        }
        public NameValueCollection QueryParameters  // the result of UrlUtility.ParseQueryString (keys and values are decoded)
        {
            get
            {
                if (this.queryParameters == null)
                {
                    PopulateQueryParameters();
                }
                return this.queryParameters;
            }
        }
        public Collection<string> RelativePathSegments  // entire Path (after the base address), decoded
        {
            get
            {
                if (this.relativePathSegments == null)
                {
                    this.relativePathSegments = new Collection<string>();
                }
                return this.relativePathSegments;
            }
        }
        public Uri RequestUri  // uri on the wire, untouched
        {
            get
            {
                return this.requestUri;
            }
            set
            {
                this.requestUri = value;
            }
        }
        public UriTemplate Template // which one got matched
        {
            get
            {
                return this.template;
            }
            set
            {
                this.template = value;
            }
        }
        public Collection<string> WildcardPathSegments  // just the Path part matched by "*", decoded
        {
            get
            {
                if (this.wildcardPathSegments == null)
                {
                    PopulateWildcardSegments();
                }
                return this.wildcardPathSegments;
            }
        }

        internal void SetQueryParameters(NameValueCollection queryParameters)
        {
            this.queryParameters = new NameValueCollection(queryParameters);
        }
        internal void SetRelativePathSegments(Collection<string> segments)
        {
            Fx.Assert(segments != null, "segments != null");
            this.relativePathSegments = segments;
        }
        internal void SetWildcardPathSegmentsStart(int startOffset)
        {
            Fx.Assert(startOffset >= 0, "startOffset >= 0");
            this.wildcardSegmentsStartOffset = startOffset;
        }

        internal void SetBaseUri(Uri originalBaseUri, HttpRequestMessageProperty requestProp)
        {
            this.baseUri = null;
            this.originalBaseUri = originalBaseUri;
            this.requestProp = requestProp;
        }

        void PopulateQueryParameters()
        {
            if (this.requestUri != null)
            {
                this.queryParameters = UriTemplateHelpers.ParseQueryString(this.requestUri.Query);
            }
            else
            {
                this.queryParameters = new NameValueCollection();
            }
        }
        void PopulateWildcardSegments()
        {
            if (wildcardSegmentsStartOffset != -1)
            {
                this.wildcardPathSegments = new Collection<string>();
                for (int i = this.wildcardSegmentsStartOffset; i < this.RelativePathSegments.Count; ++i)
                {
                    this.wildcardPathSegments.Add(this.RelativePathSegments[i]);
                }
            }
            else
            {
                this.wildcardPathSegments = new Collection<string>();
            }
        }
    }
}
