//------------------------------------------------------------------------------
// <copyright file="_HeaderInfoTable.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {

    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    
    internal class HeaderInfoTable {

        private static Hashtable HeaderHashTable;
        private static HeaderInfo UnknownHeaderInfo = new HeaderInfo(string.Empty, false, false, false, SingleParser);
        private static HeaderParser SingleParser = new HeaderParser(ParseSingleValue);
        private static HeaderParser MultiParser = new HeaderParser(ParseMultiValue);

        private static string[] ParseSingleValue(string value) {
            return new string[1]{value};
        }

        //
        // <



        private static string[] ParseMultiValue(string value) {
            StringCollection tempStringCollection = new StringCollection();

            bool inquote = false;
            int chIndex = 0;
            char[] vp = new char[value.Length];
            string singleValue;

            for (int i = 0; i < value.Length; i++) {
                if (value[i] == '\"') {
                    inquote = !inquote;
                }
                else if ((value[i] == ',') && !inquote) {
                    singleValue = new string(vp, 0, chIndex);
                    tempStringCollection.Add(singleValue.Trim());
                    chIndex = 0;
                    continue;
                }
                vp[chIndex++] = value[i];
            }

            //
            // Now add the last of the header values to the stringtable.
            //

            if (chIndex != 0) {
                singleValue = new string(vp, 0, chIndex);
                tempStringCollection.Add(singleValue.Trim());
            }

            string[] stringArray = new string[tempStringCollection.Count];
            tempStringCollection.CopyTo(stringArray, 0) ;
            return stringArray;
        }

        static HeaderInfoTable() {

            HeaderInfo[] InfoArray = new HeaderInfo[] {
                new HeaderInfo(HttpKnownHeaderNames.Age,                false,  false,  false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.Allow,              false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Accept,             true,   false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Authorization,      false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.AcceptRanges,       false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.AcceptCharset,      false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.AcceptEncoding,     false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.AcceptLanguage,     false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Cookie,             false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Connection,         true,   false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentMD5,         false,  false,  false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentType,        true,   false,  false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.CacheControl,       false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentRange,       false,  false,  false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentLength,      true,   true,   false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentEncoding,    false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentLanguage,    false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.ContentLocation,    false,  false,  false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.Date,               true,   false,  false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.ETag,               false,  false,  false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.Expect,             true,   false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Expires,            false,  false,  false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.From,               false,  false,  false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.Host,               true,   false,  false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.IfMatch,            false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.IfRange,            false,  false,  false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.IfNoneMatch,        false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.IfModifiedSince,    true,   false,  false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.IfUnmodifiedSince,  false,  false,  false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.KeepAlive,          false,  true,   false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.Location,           false,  false,  false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.LastModified,       false,  false,  false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.MaxForwards,        false,  false,  false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.Pragma,             false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.ProxyAuthenticate,  false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.ProxyAuthorization, false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.ProxyConnection,    true,   false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Range,              true,   false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Referer,            true,   false,  false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.RetryAfter,         false,  false,  false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.Server,             false,  false,  false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.SetCookie,          false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.SetCookie2,         false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.TE,                 false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Trailer,            false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.TransferEncoding,   true,   true,   true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Upgrade,            false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.UserAgent,          true,   false,  false,  SingleParser),
                new HeaderInfo(HttpKnownHeaderNames.Via,                false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Vary,               false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.Warning,            false,  false,  true,   MultiParser),
                new HeaderInfo(HttpKnownHeaderNames.WWWAuthenticate,    false,  true,   true,   SingleParser)
            };

            HeaderHashTable = new Hashtable(InfoArray.Length * 2, CaseInsensitiveAscii.StaticInstance);
            for (int i = 0; i < InfoArray.Length; i++) {
                HeaderHashTable[InfoArray[i].HeaderName] = InfoArray[i];
            }
        }

        internal HeaderInfo this[string name] {
            get {
                HeaderInfo tempHeaderInfo = (HeaderInfo)HeaderHashTable[name];
                if (tempHeaderInfo == null) {
                    return UnknownHeaderInfo;
                }
                return tempHeaderInfo;
            }
        }

    }

}
