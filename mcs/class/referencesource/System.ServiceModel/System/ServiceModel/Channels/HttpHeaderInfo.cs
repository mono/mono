// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Runtime;
    using System.Text;

    internal class HttpHeaderInfo
    {
        private static readonly HttpHeaderInfo[] knownContentHeaders = new HttpHeaderInfo[]
            {
                new HttpHeaderInfo("Allow") { IsContentHeader = true },
                new HttpHeaderInfo("Content-Encoding") { IsContentHeader = true },
                new HttpHeaderInfo("Content-Language") { IsContentHeader = true },
                new HttpHeaderInfo("Content-Length") { IsContentHeader = true },
                new HttpHeaderInfo("Content-Location") { IsContentHeader = true },
                new HttpHeaderInfo("Content-MD5") { IsContentHeader = true },
                new HttpHeaderInfo("Content-Range") { IsContentHeader = true },
                new HttpHeaderInfo("Content-Type") { IsContentHeader = true },
                new HttpHeaderInfo("Expires") { IsContentHeader = true },
                new HttpHeaderInfo("Last-Modified") { IsContentHeader = true },
                new HttpHeaderInfo("Content-Disposition") { IsContentHeader = true }       
            };

        private static readonly Type httpRequestHeaderType = typeof(HttpRequestHeader);
        private static readonly Type httpResponseHeaderType = typeof(HttpResponseHeader);

        private static ConcurrentDictionary<string, HttpHeaderInfo> knownHeadersInfos;

        private bool isUnknownHeader;

        static HttpHeaderInfo()
        {
            // Create the known headers list with the content headers 
            knownHeadersInfos = new ConcurrentDictionary<string, HttpHeaderInfo>(
                knownContentHeaders.ToDictionary(headerInfo => headerInfo.Name), 
                StringComparer.OrdinalIgnoreCase);

            // Add the request and response headers to the known headers list
            AddKnownHeaders(Enum.GetNames(httpRequestHeaderType).Select(enumString => GetHeaderString(enumString)), true);
            AddKnownHeaders(Enum.GetNames(httpResponseHeaderType).Select(enumString => GetHeaderString(enumString)), false);
        }

        private HttpHeaderInfo(string name, bool isUnknownHeader = false)
        {
            Fx.Assert(!string.IsNullOrWhiteSpace(name), "The 'name' parameter should not be null or whitespace.");
            this.Name = name;

            this.isUnknownHeader = isUnknownHeader;
            if (this.isUnknownHeader)
            {
                this.IsRequestHeader = true;
                this.IsResponseHeader = true;
                this.IsContentHeader = true;
            }
        }

        public string Name { get; private set; }

        public bool IsRequestHeader { get; private set; }

        public bool IsResponseHeader { get; private set; }

        public bool IsContentHeader { get; private set; }

        public static HttpHeaderInfo Create(string headerName)
        {
            Fx.Assert(!string.IsNullOrWhiteSpace(headerName), "The 'headerName' should not be null or whitespace.");

            HttpHeaderInfo headerInfo;
            if (!knownHeadersInfos.TryGetValue(headerName, out headerInfo))
            {
                headerInfo = new HttpHeaderInfo(headerName, true);           
            }

            return headerInfo;
        }

        public bool TryAddHeader(HttpHeaders headers, string value)
        {
            Fx.Assert(headers != null, "The 'headers' parameter should never be null.");
            if (!headers.TryAddWithoutValidation(this.Name, value))
            {
                this.UpdateHeaderInfo(headers);
                return false;
            }

            return true;
        }

        public bool TryRemoveHeader(HttpHeaders headers)
        {
            Fx.Assert(headers != null, "The 'headers' parameter should never be null.");
            
            try
            {
                headers.Remove(this.Name);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                FxTrace.Exception.TraceHandledException(ex, TraceEventType.Information);
                this.UpdateHeaderInfo(headers);
            }

            return false;
        }

        public IEnumerable<string> TryGetHeader(HttpHeaders headers)
        {
            Fx.Assert(headers != null, "The 'headers' parameter should never be null.");
            
            IEnumerable<string> values = null;

            if (!headers.TryGetValues(this.Name, out values))
            {
                values = null;
                this.UpdateHeaderInfo(headers);
            }

            return values;
        }

        private static void AddKnownHeaders(IEnumerable<string> headers, bool asRequestHeader)
        {
            foreach (string header in headers)
            {
                HttpHeaderInfo headerInfo = null;
                if (knownHeadersInfos.TryGetValue(header, out headerInfo))
                {
                    if (headerInfo.IsContentHeader)
                    {
                        // this header is actually a content header so continue
                        continue;
                    }
                }

                if (headerInfo == null)
                {
                    headerInfo = new HttpHeaderInfo(header);
                    knownHeadersInfos.TryAdd(headerInfo.Name, headerInfo);
                }

                if (asRequestHeader)
                {
                    headerInfo.IsRequestHeader = true;
                }
                else
                {
                    headerInfo.IsResponseHeader = true;
                }
            }
        }

        private static string GetHeaderString(string headerEnumString)
        {
            // Have to special case 'ETag' as it should not be hypenated
            if (string.Equals(headerEnumString, HttpResponseHeader.ETag.ToString(), StringComparison.Ordinal))
            {
                return headerEnumString;
            }

            StringBuilder asStringBuilder = new StringBuilder(headerEnumString);

            // Note that we are not considering the first and last characters of the string
            // as headers never start or end with '-'
            for (int i = asStringBuilder.Length - 2; i > 0; i--)
            {
                if (char.IsUpper(asStringBuilder[i]) && char.IsLower(asStringBuilder[i + 1]))
                {
                    asStringBuilder.Insert(i, '-');
                }
            }

            return asStringBuilder.ToString();
        }

        private void UpdateHeaderInfo(HttpHeaders headers)
        {
            Fx.Assert(headers != null, "The 'headers' parameter should never be null.");

            if (headers is HttpContentHeaders)
            {
                this.IsContentHeader = false;
            }
            else if (headers is HttpRequestHeaders)
            {
                this.IsRequestHeader = false;
            }
            else if (headers is HttpResponseHeaders)
            {
                this.IsResponseHeader = false;
            }

            if (this.isUnknownHeader)
            {
                this.isUnknownHeader = !knownHeadersInfos.TryAdd(this.Name, this);
            }
        }
    }
}
