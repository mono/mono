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
    using System.Reflection;

    internal class HeaderInfoTable
    {
        private static Dictionary<string, HeaderInfo> headerHashTable = CreateHeaderHashTable();

        private static HeaderInfo unknownHeaderInfo = new HeaderInfo(string.Empty, false);

        internal HeaderInfo this[string name]
        {
            get
            {
                Debug.Assert(name != null, "name != null");
                HeaderInfo tempHeaderInfo = null;
                headerHashTable.TryGetValue(name, out tempHeaderInfo);
                if (tempHeaderInfo == null)
                {
                    return unknownHeaderInfo;
                }

                return tempHeaderInfo;
            }
        }

        private static Dictionary<string, HeaderInfo> CreateHeaderHashTable()
        {
            HeaderInfo[] infoArray = new HeaderInfo[]
            { 
                new HeaderInfo("Allow", false),
                new HeaderInfo("Accept", false),
                new HeaderInfo("Authorization", false),
                new HeaderInfo("Accept-Charset", false), 
                new HeaderInfo("Accept-Encoding", false), 
                new HeaderInfo("Accept-Language", false), 
                new HeaderInfo("Cookie", false), 
                new HeaderInfo("Connection", true), 
                new HeaderInfo("Content-MD5", false), 
                new HeaderInfo("Content-Type", true), 
                new HeaderInfo("Cache-Control", false), 
                new HeaderInfo("Content-Range", false),
                new HeaderInfo("Content-Length", false),
                new HeaderInfo("Content-Encoding", false),
                new HeaderInfo("Content-Language", false),
                new HeaderInfo("Content-Location", false), 
                new HeaderInfo("Date", false),
                new HeaderInfo("Expect", false),
                new HeaderInfo("Expires", false),
                new HeaderInfo("From", false),
                new HeaderInfo("Host", false),
                new HeaderInfo("If-Match", false),
                new HeaderInfo("If-Range", false),
                new HeaderInfo("If-None-Match", false),
                new HeaderInfo("If-Modified-Since", false),
                new HeaderInfo("If-Unmodified-Since", false), 
                new HeaderInfo("Keep-Alive", false),
                new HeaderInfo("Last-Modified", false),
                new HeaderInfo("Max-Forwards", false),
                new HeaderInfo("Pragma", false), 
                new HeaderInfo("Proxy-Authorization", false), 
                new HeaderInfo("Range", false), 
                new HeaderInfo("Referer", false),
                new HeaderInfo("TE", false),
                new HeaderInfo("Trailer", false),
                new HeaderInfo("Transfer-Encoding", false), 
                new HeaderInfo("Upgrade", false), 
                new HeaderInfo("UserAgent", false), 
                new HeaderInfo("Via", false), 
                new HeaderInfo("Warning", false), 
                new HeaderInfo("Accept-Ranges", false), 
                new HeaderInfo("Age", false), 
                new HeaderInfo("ETag", false), 
                new HeaderInfo("Location", false),
                new HeaderInfo("Proxy-Authenticate", false),
                new HeaderInfo("Retry-After", false), 
                new HeaderInfo("Server", false), 
                new HeaderInfo("Set-Cookie", false), 
                new HeaderInfo("Vary", false),
                new HeaderInfo("WWW-Authenticate", false),
                new HeaderInfo("P3P", false), 
                new HeaderInfo("IM", false),
                new HeaderInfo("Slug", false),
                new HeaderInfo("Content-Disposition", false),
                new HeaderInfo("inline", false),
                new HeaderInfo("attachment", false), 
                new HeaderInfo("filename", false), 
                new HeaderInfo("Wlc-Safe-Agent", false),
                new HeaderInfo("with-tombstones", false),
                new HeaderInfo("feed", false),
                new HeaderInfo("feed-with-tombstones", false),
                new HeaderInfo("PagedRead", false), 
                new HeaderInfo("StatelessClient", false)
            };

            Dictionary<string, HeaderInfo> dict = new Dictionary<string, HeaderInfo>(infoArray.Length * 2, CaseInsensitiveAscii.StaticInstance);
            for (int i = 0; i < infoArray.Length; i++)
            {
                dict[infoArray[i].HeaderName] = infoArray[i];
            }

            return dict;
        }
    }
}

