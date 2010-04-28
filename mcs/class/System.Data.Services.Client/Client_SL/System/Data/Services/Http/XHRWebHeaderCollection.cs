//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Http
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    internal sealed class XHRWebHeaderCollection : WebHeaderCollection
    {
        private const int ApproxHighAvgNumHeaders = 16;

        private static readonly System.Data.Services.Http.HeaderInfoTable headerTable = new System.Data.Services.Http.HeaderInfoTable();

        private NameValueFromDictionary innerCollection;

        private System.Data.Services.Http.WebHeaderCollectionType collectionType;

        public XHRWebHeaderCollection() : this(System.Data.Services.Http.WebHeaderCollectionType.Unknown)
        {
        }

        internal XHRWebHeaderCollection(System.Data.Services.Http.WebHeaderCollectionType type)
        {
            this.collectionType = type;
        }

        #region Properties.
        public override int Count
        {
            get
            {
                return this.InnerCollection.Count;
            }
        }

        public override ICollection<string> AllKeys
        {
            get
            {
                return this.InnerCollection.Keys;
            }
        }

        private bool AllowHttpRequestHeader
        {
            get
            {
                if (this.collectionType == System.Data.Services.Http.WebHeaderCollectionType.Unknown)
                {
                    this.collectionType = System.Data.Services.Http.WebHeaderCollectionType.WebRequest;
                }

                return
                    ((this.collectionType == System.Data.Services.Http.WebHeaderCollectionType.WebRequest) ||
                     (this.collectionType == System.Data.Services.Http.WebHeaderCollectionType.HttpWebRequest));
            }
        }

        private NameValueFromDictionary InnerCollection
        {
            get
            {
                if (this.innerCollection == null)
                {
                    this.innerCollection = new NameValueFromDictionary(ApproxHighAvgNumHeaders, System.Data.Services.Http.CaseInsensitiveAscii.StaticInstance);
                }

                return this.innerCollection;
            }
        }

        public override string this[string name]
        {
            get
            {
                return this.InnerCollection.Get(name);
            }

            set
            {
                name = System.Data.Services.Http.ValidationHelper.CheckBadChars(name, false);
                value = System.Data.Services.Http.ValidationHelper.CheckBadChars(value, true);
                this.ThrowOnRestrictedHeader(name);
                this.InnerCollection.Set(name, value);
            }
        }

        public override string this[System.Data.Services.Http.HttpRequestHeader header]
        {
            get
            {
                if (!this.AllowHttpRequestHeader)
                {
                    throw new InvalidOperationException(
                        System.Data.Services.Client.Strings.HttpWeb_Internal("WebHeaderCollection.this[HttpRequestHeader].get"));
                }

                return this[HttpHeaderToName.RequestHeaderNames[header]];
            }

            set
            {
                if (!this.AllowHttpRequestHeader)
                {
                    throw new InvalidOperationException(
                        System.Data.Services.Client.Strings.HttpWeb_Internal("WebHeaderCollection.this[HttpRequestHeader].set"));
                }

                this[HttpHeaderToName.RequestHeaderNames[header]] = value;
            }
        }
        #endregion Properties.

        internal void Add(string name, string value)
        {
            Debug.Assert(name != null, "name != null");
            Debug.Assert(value != null, "value != null");
            this.InnerCollection.Add(name, value);
        }

        internal void SetSpecialHeader(string headerName, string value)
        {
            Debug.Assert(headerName != null, "headerName != null");
            value = System.Data.Services.Http.ValidationHelper.CheckBadChars(value, true);
            this.InnerCollection.Remove(headerName);
            if (value.Length != 0)
            {
                this.InnerCollection.Add(headerName, value);
            }
        }

        internal DataParseStatus ParseHeaders(
            byte[] byteBuffer,
            int size,
            ref int unparsed,
            ref int totalResponseHeadersLength,
            int maximumResponseHeadersLength,
            ref WebParseError parseError)
        {

            if (byteBuffer.Length < size)
            {
                return DataParseStatus.NeedMoreData;
            }

            char ch;
            int headerNameStartOffset = -1;
            int headerNameEndOffset = -1;
            int headerValueStartOffset = -1;
            int headerValueEndOffset = -1;
            int numberOfLf = -1;
            int index = unparsed;
            bool spaceAfterLf;
            string headerMultiLineValue;
            string headerName;
            string headerValue;

            int localTotalResponseHeadersLength = totalResponseHeadersLength;

            WebParseErrorCode parseErrorCode = WebParseErrorCode.Generic;
            DataParseStatus parseStatus = DataParseStatus.Invalid;


            for (;;)
            {
                headerName = string.Empty;
                headerValue = string.Empty;
                spaceAfterLf = false;
                headerMultiLineValue = null;

                if (this.Count == 0)
                {
                    while (index < size)
                    {
                        ch = (char)byteBuffer[index];
                        if (ch == ' ' || ch == '\t')
                        {
                            ++index;
                            if (maximumResponseHeadersLength >= 0 && ++localTotalResponseHeadersLength >= maximumResponseHeadersLength)
                            {
                                parseStatus = DataParseStatus.DataTooBig;
                                goto quit;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (index == size)
                    {
                        parseStatus = DataParseStatus.NeedMoreData;
                        goto quit;
                    }
                }

                headerNameStartOffset = index;

                while (index < size)
                {
                    ch = (char)byteBuffer[index];
                    if (ch != ':' && ch != '\n')
                    {
                        if (ch > ' ')
                        {
                            headerNameEndOffset = index;
                        }

                        ++index;
                        if (maximumResponseHeadersLength >= 0 && ++localTotalResponseHeadersLength >= maximumResponseHeadersLength)
                        {
                            parseStatus = DataParseStatus.DataTooBig;
                            goto quit;
                        }
                    }
                    else
                    {
                        if (ch == ':')
                        {
                            ++index;
                            if (maximumResponseHeadersLength >= 0 && ++localTotalResponseHeadersLength >= maximumResponseHeadersLength)
                            {
                                parseStatus = DataParseStatus.DataTooBig;
                                goto quit;
                            }
                        }
                        
                        break;
                    }
                }
                
                if (index == size)
                {
                    parseStatus = DataParseStatus.NeedMoreData;
                    goto quit;
                }

            startOfValue:
                numberOfLf = (this.Count == 0 && headerNameEndOffset < 0) ? 1 : 0;
                while (index < size && numberOfLf < 2)
                {
                    ch = (char)byteBuffer[index];
                    if (ch <= ' ')
                    {
                        if (ch == '\n')
                        {
                            numberOfLf++;
                            
                            if (numberOfLf == 1)
                            {
                                if (index + 1 == size)
                                {
                                    parseStatus = DataParseStatus.NeedMoreData;
                                    goto quit;
                                }

                                spaceAfterLf = (char)byteBuffer[index + 1] == ' ' || (char)byteBuffer[index + 1] == '\t';
                            }
                        }

                        ++index;
                        if (maximumResponseHeadersLength >= 0 && ++localTotalResponseHeadersLength >= maximumResponseHeadersLength)
                        {
                            parseStatus = DataParseStatus.DataTooBig;
                            goto quit;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (numberOfLf == 2 || (numberOfLf == 1 && !spaceAfterLf))
                {
                    goto addHeader;
                }
                
                if (index == size)
                {
                    parseStatus = DataParseStatus.NeedMoreData;
                    goto quit;
                }

                headerValueStartOffset = index;

                while (index < size)
                {
                    ch = (char)byteBuffer[index];
                    if (ch != '\n')
                    {
                        if (ch > ' ')
                        {
                            headerValueEndOffset = index;
                        }

                        ++index;
                        if (maximumResponseHeadersLength >= 0 && ++localTotalResponseHeadersLength >= maximumResponseHeadersLength)
                        {
                            parseStatus = DataParseStatus.DataTooBig;
                            goto quit;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (index == size)
                {
                    parseStatus = DataParseStatus.NeedMoreData;
                    goto quit;
                }

                numberOfLf = 0;
                while (index < size && numberOfLf < 2)
                {
                    ch = (char)byteBuffer[index];
                    if (ch == '\r' || ch == '\n')
                    {
                        if (ch == '\n')
                        {
                            numberOfLf++;
                        }
                        
                        ++index;
                        if (maximumResponseHeadersLength >= 0 && ++localTotalResponseHeadersLength >= maximumResponseHeadersLength)
                        {
                            parseStatus = DataParseStatus.DataTooBig;
                            goto quit;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                
                if (index == size && numberOfLf < 2)
                {
                    parseStatus = DataParseStatus.NeedMoreData;
                    goto quit;
                }

            addHeader:
                if (headerValueStartOffset >= 0 && headerValueStartOffset > headerNameEndOffset && headerValueEndOffset >= headerValueStartOffset)
                {
                    headerValue = System.Text.Encoding.UTF8.GetString(byteBuffer, headerValueStartOffset, headerValueEndOffset - headerValueStartOffset + 1);
                }

                headerMultiLineValue = (headerMultiLineValue == null ? headerValue : headerMultiLineValue + " " + headerValue);

                if (index < size && numberOfLf == 1)
                {
                    ch = (char)byteBuffer[index];
                    if (ch == ' ' || ch == '\t')
                    {
                        ++index;
                        if (maximumResponseHeadersLength >= 0 && ++localTotalResponseHeadersLength >= maximumResponseHeadersLength)
                        {
                            parseStatus = DataParseStatus.DataTooBig;
                            goto quit;
                        }

                        goto startOfValue;
                    }
                }

                if (headerNameStartOffset >= 0 && headerNameEndOffset >= headerNameStartOffset)
                {
                    headerName = System.Text.Encoding.UTF8.GetString(byteBuffer, headerNameStartOffset, headerNameEndOffset - headerNameStartOffset + 1);
                }

                if (headerName.Length > 0)
                {
                    this.Add(headerName, headerMultiLineValue);
                }

                totalResponseHeadersLength = localTotalResponseHeadersLength;
                unparsed = index;

                if (numberOfLf == 2)
                {
                    parseStatus = DataParseStatus.Done;
                    goto quit;
                }
            }

            quit:
            if (parseStatus == DataParseStatus.Invalid)
            {
                parseError.Section = WebParseErrorSection.ResponseHeader;
                parseError.Code = parseErrorCode;
            }

            return parseStatus;
        }
        
        private void ThrowOnRestrictedHeader(string headerName)
        {
            if ((this.collectionType == System.Data.Services.Http.WebHeaderCollectionType.HttpWebRequest) && headerTable[headerName].IsRequestRestricted)
            {
                throw new InvalidOperationException(
                    System.Data.Services.Client.Strings.HttpWeb_Internal("WebHeaderCollection.ThrowOnRestrictedHeader"));
            }
        }        
    }
}
