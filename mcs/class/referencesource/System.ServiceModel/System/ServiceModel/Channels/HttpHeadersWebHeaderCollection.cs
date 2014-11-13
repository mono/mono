// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Runtime;
    using System.Text;

    /// <summary>
    /// The HttpHeadersWebHeaderCollection is an implementation of the <see cref="WebHeaderCollection"/> class
    /// that uses the HttpHeader collections on an <see cref="HttpRequestMessage"/> or an <see cref="HttpResponseMessage"/>
    /// instance to hold the header data instead of the traditional <see cref="NameValueCollection"/> of the
    /// <see cref="WebHeaderCollection"/>.  This is because the <see cref="HttpRequestMessage"/> or 
    /// <see cref="HttpResponseMessage"/> is the true data structure holding the HTTP information of the request/response
    /// being processed and we want to avoid copying header information between the <see cref="HttpRequestMessage"/> or 
    /// <see cref="HttpResponseMessage"/> and a <see cref="NameValueCollection"/>. 
    /// </summary>
    internal class HttpHeadersWebHeaderCollection : WebHeaderCollection
    {
        private const string HasKeysHeader = "hk";
        private static readonly string[] emptyStringArray = new string[] { string.Empty };
        private static readonly string[] stringSplitArray = new string[] { ", " };

        // Cloned from WebHeaderCollection
        private static readonly char[] HttpTrimCharacters = new char[] { (char)0x09, (char)0xA, (char)0xB, (char)0xC, (char)0xD, (char)0x20 };
        private static readonly char[] InvalidParamChars = new char[] { '(', ')', '<', '>', '@', ',', ';', ':', '\\', '"', '\'', '/', '[', ']', '?', '=', '{', '}', ' ', '\t', '\r', '\n' };
        
        private HttpRequestMessage httpRequestMessage;
        private HttpResponseMessage httpResponseMessage;
        private bool hasKeys;

        public HttpHeadersWebHeaderCollection(HttpRequestMessage httpRequestMessage)
        {
            Fx.Assert(httpRequestMessage != null, "The 'httpRequestMessage' parameter should never be null.");

            this.httpRequestMessage = httpRequestMessage;
            this.EnsureBaseHasKeysIsAccurate();
        }

        public HttpHeadersWebHeaderCollection(HttpResponseMessage httpResponseMessage)
        {
            Fx.Assert(httpResponseMessage != null, "The 'httpResponseMessage' parameter should never be null.");

            this.httpResponseMessage = httpResponseMessage;
            this.EnsureBaseHasKeysIsAccurate();
        }

        public override string[] AllKeys
        {
            get
            {
                return this.AllHeaders.Select(header => header.Key).ToArray();
            }
        }

        public override int Count
        {
            get
            {
                return this.AllHeaders.Count();
            }
        }

        public override KeysCollection Keys
        {
            get
            {
                // The perf here will be awful as we have to create a NameValueCollection and copy all the
                // headers over into it in order to get an instance of type KeysCollection; so framework
                // code should never use the Keys property.
                NameValueCollection collection = new NameValueCollection();
                foreach (KeyValuePair<string, IEnumerable<string>> header in this.AllHeaders)
                {
                    string[] values = header.Value.ToArray();
                    if (values.Length == 0)
                    {
                        collection.Add(header.Key, string.Empty);
                    }
                    else
                    {
                        foreach (string value in values)
                        {
                            collection.Add(header.Key, value);
                        }
                    }
                }

                return collection.Keys;
            }
        }

        private IEnumerable<KeyValuePair<string, IEnumerable<string>>> AllHeaders
        {
            get
            {
                HttpContent content = null;
                IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers;

                if (this.httpRequestMessage != null)
                {
                    headers = this.httpRequestMessage.Headers;
                    content = this.httpRequestMessage.Content;
                }
                else
                {
                    Fx.Assert(this.httpResponseMessage != null, "Either the 'httpRequestMessage' field or the 'httpResponseMessage' field should be non-null.");
                    headers = this.httpResponseMessage.Headers;
                    content = this.httpResponseMessage.Content;
                }

                if (content != null)
                {
                    headers = headers.Concat(content.Headers);
                }

                return headers;
            }
        }

        public override void Add(string name, string value)
        {
            name = CheckBadChars(name, false);
            value = CheckBadChars(value, true);

            if (this.httpRequestMessage != null)
            {
                this.httpRequestMessage.AddHeader(name, value);              
            }
            else
            {
                Fx.Assert(this.httpResponseMessage != null, "Either the 'httpRequestMessage' field or the 'httpResponseMessage' field should be non-null.");
                this.httpResponseMessage.AddHeader(name, value);
            }

            this.EnsureBaseHasKeysIsAccurate();
        }

        public override void Clear()
        {
            HttpContent content = null;

            if (this.httpRequestMessage != null)
            {
                this.httpRequestMessage.Headers.Clear();
                content = this.httpRequestMessage.Content;
            }
            else
            {
                Fx.Assert(this.httpResponseMessage != null, "Either the 'httpRequestMessage' field or the 'httpResponseMessage' field should be non-null.");
                this.httpResponseMessage.Headers.Clear();
                content = this.httpResponseMessage.Content;
            }

            if (content != null)
            {
                content.Headers.Clear();
            }

            this.EnsureBaseHasKeysIsAccurate();
        }

        public override void Remove(string name)
        {
            name = CheckBadChars(name, false);

            if (this.httpRequestMessage != null)
            {
                this.httpRequestMessage.RemoveHeader(name);
            }
            else
            {
                this.httpResponseMessage.RemoveHeader(name);
            }

            this.EnsureBaseHasKeysIsAccurate();
        }

        public override void Set(string name, string value)
        {
            name = CheckBadChars(name, false);
            value = CheckBadChars(value, true);

            if (this.httpRequestMessage != null)
            {
                this.httpRequestMessage.SetHeader(name, value);
            }
            else
            {
                Fx.Assert(this.httpResponseMessage != null, "Either the 'httpRequestMessage' field or the 'httpResponseMessage' field should be non-null.");
                this.httpResponseMessage.SetHeader(name, value);
            }

            this.EnsureBaseHasKeysIsAccurate();
        }

        public override IEnumerator GetEnumerator()
        {
            return new HttpHeadersEnumerator(this.AllKeys);
        }

        public override string Get(int index)
        {
            string[] values = this.GetValues(index);
            return GetSingleValue(values);
        }

        public override string GetKey(int index)
        {
            return this.GetHeaderAt(index).Key;
        }

        public override string[] GetValues(int index)
        {
            return this.GetHeaderAt(index).Value.ToArray();
        }

        public override string Get(string name)
        {
            string[] values = this.GetValues(name);
            return GetSingleValue(values);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var header in this.AllHeaders)
            {
                if (!string.IsNullOrEmpty(header.Key))
                {
                    builder.Append(header.Key);
                    builder.Append(": ");
                    builder.AppendLine(GetSingleValue(header.Value.ToArray()));
                }
            }

            return builder.ToString();
        }

        public override string[] GetValues(string header)
        {
            IEnumerable<string> values = null;

            if (this.httpRequestMessage != null)
            {
                values = this.httpRequestMessage.GetHeader(header);
            }
            else
            {
                Fx.Assert(this.httpResponseMessage != null, "Either the 'httpRequestMessage' field or the 'httpResponseMessage' field should be non-null.");
                values = this.httpResponseMessage.GetHeader(header);
            }

            if (values == null)
            {
                return emptyStringArray;
            }
            
            return values.SelectMany(str => str.Split(stringSplitArray, StringSplitOptions.None)).ToArray();
        }

        private static string GetSingleValue(string[] values)
        {
            if (values == null)
            {
                return null;
            }

            if (values.Length == 1)
            {
                return values[0];
            }

            // The current implemenation of the base WebHeaderCollection joins the string values
            // using a comma with no whitespace
            return string.Join(",", values);
        }

        // Cloned from WebHeaderCollection
        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, FxCop.Rule.WrapExceptionsRule,
            Justification = "This code is being used to reproduce behavior from the WebHeaderCollection, which does not trace exceptions via FxTrace.")]
        private static string CheckBadChars(string name, bool isHeaderValue)
        {
            if (name == null || name.Length == 0)
            {
                // emtpy name is invlaid
                if (!isHeaderValue)
                {
                    throw name == null ? 
                        new ArgumentNullException("name") :
                        new ArgumentException(SR.GetString(SR.WebHeaderEmptyStringCall, "name"), "name");
                }

                // empty value is OK
                return string.Empty;
            }

            if (isHeaderValue)
            {
                // VALUE check
                // Trim spaces from both ends
                name = name.Trim(HttpTrimCharacters);

                // First, check for correctly formed multi-line value
                // Second, check for absenece of CTL characters
                int crlf = 0;
                for (int i = 0; i < name.Length; ++i)
                {
                    char c = (char)(0x000000ff & (uint)name[i]);
                    switch (crlf)
                    {
                        case 0:
                            if (c == '\r')
                            {
                                crlf = 1;
                            }
                            else if (c == '\n')
                            {
                                // Technically this is bad HTTP.  But it would be a breaking change to throw here.
                                // Is there an exploit?
                                crlf = 2;
                            }
                            else if (c == 127 || (c < ' ' && c != '\t'))
                            {
                                throw new ArgumentException(SR.GetString(SR.WebHeaderInvalidControlChars), "value");
                            }

                            break;

                        case 1:
                            if (c == '\n')
                            {
                                crlf = 2;
                                break;
                            }

                            throw new ArgumentException(SR.GetString(SR.WebHeaderInvalidCRLFChars), "value");

                        case 2:
                            if (c == ' ' || c == '\t')
                            {
                                crlf = 0;
                                break;
                            }

                            throw new ArgumentException(SR.GetString(SR.WebHeaderInvalidCRLFChars), "value");
                    }
                }

                if (crlf != 0)
                {
                    throw new ArgumentException(SR.GetString(SR.WebHeaderInvalidCRLFChars), "value");
                }
            }
            else
            {
                // NAME check
                // First, check for absence of separators and spaces
                if (name.IndexOfAny(InvalidParamChars) != -1)
                {
                    throw new ArgumentException(SR.GetString(SR.WebHeaderInvalidHeaderChars), "name");
                }

                // Second, check for non CTL ASCII-7 characters (32-126)
                if (ContainsNonAsciiChars(name))
                {
                    throw new ArgumentException(SR.GetString(SR.WebHeaderInvalidNonAsciiChars), "name");
                }
            }

            return name;
        }

        // Cloned from WebHeaderCollection
        private static bool ContainsNonAsciiChars(string token)
        {
            for (int i = 0; i < token.Length; ++i)
            {
                if ((token[i] < 0x20) || (token[i] > 0x7e))
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureBaseHasKeysIsAccurate()
        {
            bool originalHasKeys = this.hasKeys;
            this.hasKeys = this.BackingHttpHeadersHasKeys();
            if (originalHasKeys && !this.hasKeys)
            {
                base.Remove(HasKeysHeader);
            }
            else if (!originalHasKeys && this.hasKeys)
            {
                this.AddWithoutValidate(HasKeysHeader, string.Empty);
            }
        }

        private bool BackingHttpHeadersHasKeys()
        {
            return this.httpRequestMessage != null ?
                this.httpRequestMessage.Headers.Any() || (this.httpRequestMessage.Content != null && this.httpRequestMessage.Content.Headers.Any()) :
                this.httpResponseMessage.Headers.Any() || (this.httpResponseMessage.Content != null && this.httpResponseMessage.Content.Headers.Any());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, FxCop.Rule.WrapExceptionsRule,
            Justification = "This code is being used to reproduce behavior from the WebHeaderCollection, which does not trace exceptions via FxTrace.")]
        private KeyValuePair<string, IEnumerable<string>> GetHeaderAt(int index)
        {
            if (index >= 0)
            {
                foreach (KeyValuePair<string, IEnumerable<string>> header in this.AllHeaders)
                {
                    if (index == 0)
                    {
                        return header;
                    }

                    index--;
                }
            }

            throw new ArgumentOutOfRangeException("index", SR.WebHeaderArgumentOutOfRange);
        }

        private class HttpHeadersEnumerator : IEnumerator
        {
            private string[] keys;
            private int position;

            public HttpHeadersEnumerator(string[] keys)
            {
                this.keys = keys;
                this.position = -1;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, FxCop.Rule.WrapExceptionsRule,
                Justification = "This code is being used to reproduce behavior from the WebHeaderCollection, which does not trace exceptions via FxTrace.")]
            public object Current
            {
                get
                {
                    if ((this.position < 0) || (this.position >= this.keys.Length))
                    {
                        throw new InvalidOperationException(SR.GetString(SR.WebHeaderEnumOperationCantHappen));
                    }

                    return this.keys[this.position];
                }
            }

            public bool MoveNext()
            {
                if (this.position < (this.keys.Length - 1))
                {
                    this.position++;
                    return true;
                }

                this.position = this.keys.Length;
                return false;
            }

            public void Reset()
            {
                this.position = -1;
            }            
        }
    }
}
