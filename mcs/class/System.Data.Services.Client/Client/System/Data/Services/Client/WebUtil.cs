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


namespace System.Data.Services.Client
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
#if !ASTORIA_LIGHT    
    using System.Net;
#else
    using System.Data.Services.Http;
#endif

    internal static partial class WebUtil
    {
        private static bool? dataServiceCollectionAvailable = null;

        private static bool DataServiceCollectionAvailable
        {
            get
            {
                if (dataServiceCollectionAvailable == null)
                {
                    try
                    {
                        dataServiceCollectionAvailable = GetDataServiceCollectionOfTType() != null;
                    }
                    catch (FileNotFoundException)
                    {
                        dataServiceCollectionAvailable = false;
                    }
                }

                Debug.Assert(dataServiceCollectionAvailable != null, "observableCollectionOfTAvailable must not be null here.");

                return (bool)dataServiceCollectionAvailable;
            }
        }

        internal static long CopyStream(Stream input, Stream output, ref byte[] refBuffer)
        {
            Debug.Assert(null != input, "null input stream");
            Debug.Assert(null != output, "null output stream");

            long total = 0;
            byte[] buffer = refBuffer;
            if (null == buffer)
            {
                refBuffer = buffer = new byte[1000];
            }

            int count = 0;
            while (input.CanRead && (0 < (count = input.Read(buffer, 0, buffer.Length))))
            {
                output.Write(buffer, 0, count);
                total += count;
            }

            return total;
        }

        internal static void GetHttpWebResponse(InvalidOperationException exception, ref HttpWebResponse response)
        {
            if (null == response)
            {
                WebException webexception = (exception as WebException);
                if (null != webexception)
                {
                    response = (HttpWebResponse)webexception.Response;
                }
            }
        }

        internal static bool SuccessStatusCode(HttpStatusCode status)
        {
            return (200 <= (int)status && (int)status < 300);
        }

        internal static Dictionary<string, string> WrapResponseHeaders(HttpWebResponse response)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>(EqualityComparer<string>.Default);
            if (null != response)
            {
                foreach (string name in response.Headers.AllKeys)
                {
                    headers.Add(name, response.Headers[name]);
                }
            }

            return headers;
        }

        internal static void ApplyHeadersToRequest(Dictionary<string, string> headers, HttpWebRequest request, bool ignoreAcceptHeader)
        {
            foreach (KeyValuePair<string, string> header in headers)
            {
                if (string.Equals(header.Key, XmlConstants.HttpRequestAccept, StringComparison.Ordinal))
                {
                    if (!ignoreAcceptHeader)
                    {
                        request.Accept = header.Value;
                    }
                }
                else if (string.Equals(header.Key, XmlConstants.HttpContentType, StringComparison.Ordinal))
                {
                    request.ContentType = header.Value;
                }
                else
                {
                    request.Headers[header.Key] = header.Value;
                }
            }
        }

        internal static bool IsDataServiceCollectionType(Type t)
        {
            if (DataServiceCollectionAvailable)
            {
                return t == GetDataServiceCollectionOfTType();
            }

            return false;
        }

        internal static Type GetDataServiceCollectionOfT(params Type[] typeArguments)
        {
            if (DataServiceCollectionAvailable)
            {
                Debug.Assert(
                    GetDataServiceCollectionOfTType() != null, 
                    "DataServiceCollection is available so GetDataServiceCollectionOfTType() must not return null.");
                
                return GetDataServiceCollectionOfTType().MakeGenericType(typeArguments);
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Type GetDataServiceCollectionOfTType()
        {
            return typeof(DataServiceCollection<>);
        }
    }
}
