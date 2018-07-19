//------------------------------------------------------------------------------
// <copyright file="RequestResponse.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols
{

    using System.IO;
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Threading;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Net;
    using System.Globalization;
    using System.Diagnostics;
    using System.Web.Services.Diagnostics;

    internal class RequestResponseUtils
    {
        private RequestResponseUtils() { }
        /*
        internal static string UTF8StreamToString(Stream stream) {
            long position = 0;
            if (stream.CanSeek)
                position = stream.Position;
            StreamReader reader = new StreamReader(stream, new System.Text.UTF8Encoding());
            string result = reader.ReadToEnd();
            if (stream.CanSeek)
                stream.Position = position;
            return result;
        }
        */

        // 

        internal static Encoding GetEncoding(string contentType)
        {
            string charset = ContentType.GetCharset(contentType);
            Encoding e = null;
            try
            {
                if (charset != null && charset.Length > 0)
                    e = Encoding.GetEncoding(charset);
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
                {
                    throw;
                }
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, typeof(RequestResponseUtils), "GetEncoding", ex);
            }
            // default to ASCII encoding per RFC 2376/3023
            return e == null ? new ASCIIEncoding() : e;
        }

        internal static Encoding GetEncoding2(string contentType)
        {
            // default to old text/* behavior for non-application base
            if (!ContentType.IsApplication(contentType))
                return GetEncoding(contentType);

            string charset = ContentType.GetCharset(contentType);
            Encoding e = null;
            try
            {
                if (charset != null && charset.Length > 0)
                    e = Encoding.GetEncoding(charset);
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
                {
                    throw;
                }
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, typeof(RequestResponseUtils), "GetEncoding2", ex);
            }
            // no default per application/* mime type
            return e;
        }

        internal static string ReadResponse(WebResponse response)
        {
            return ReadResponse(response, response.GetResponseStream());
        }

        internal static string ReadResponse(WebResponse response, Stream stream)
        {
            Encoding e = GetEncoding(response.ContentType);
            if (e == null) e = Encoding.Default;
            StreamReader reader = new StreamReader(stream, e, true);
            try
            {
                return reader.ReadToEnd();
            }
            finally
            {
                stream.Close();
            }
        }

        // used to copy an unbuffered stream to a buffered stream.
        internal static Stream StreamToMemoryStream(Stream stream)
        {
            MemoryStream memoryStream = new MemoryStream(1024);
            byte[] buffer = new byte[1024];
            int count;
            while ((count = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                memoryStream.Write(buffer, 0, count);
            }
            memoryStream.Position = 0;
            return memoryStream;
        }

        internal static string CreateResponseExceptionString(WebResponse response)
        {
            return CreateResponseExceptionString(response, response.GetResponseStream());
        }

        internal static string CreateResponseExceptionString(WebResponse response, Stream stream)
        {
            if (response is HttpWebResponse)
            {
                HttpWebResponse httpResponse = (HttpWebResponse)response;
                int statusCode = (int)httpResponse.StatusCode;
                if (statusCode >= 400 && statusCode != 500)
                    return Res.GetString(Res.WebResponseKnownError, statusCode, httpResponse.StatusDescription);
            }

            // 
            string content = (stream != null) ? ReadResponse(response, stream) : string.Empty;

            if (content.Length > 0)
            {
                content = HttpUtility.HtmlDecode(content);
                StringBuilder sb = new StringBuilder();
                sb.Append(Res.GetString(Res.WebResponseUnknownError));
                sb.Append(Environment.NewLine);
                sb.Append("--");
                sb.Append(Environment.NewLine);
                sb.Append(content);
                sb.Append(Environment.NewLine);
                sb.Append("--");
                sb.Append(".");
                return sb.ToString();
            }
            else
                return Res.GetString(Res.WebResponseUnknownErrorEmptyBody);
        }

        internal static int GetBufferSize(int contentLength)
        {
            int bufferSize;
            if (contentLength == -1)
                bufferSize = 8000;
            else if (contentLength <= 16000)
                bufferSize = contentLength;
            else
                bufferSize = 16000;

            return bufferSize;
        }

        static class HttpUtility
        {
            internal static string HtmlDecode(string s)
            {
                if (s == null)
                    return null;

                // See if this string needs to be decoded at all.  If no
                // ampersands are found, then no special HTML-encoded chars
                // are in the string.
                if (s.IndexOf('&') < 0)
                    return s;

                StringBuilder builder = new StringBuilder();
                StringWriter writer = new StringWriter(builder, CultureInfo.InvariantCulture);

                HtmlDecode(s, writer);

                return builder.ToString();
            }

            private static char[] s_entityEndingChars = new char[] { ';', '&' };
            public static void HtmlDecode(string s, TextWriter output)
            {
                if (s == null)
                    return;

                if (s.IndexOf('&') < 0)
                {
                    output.Write(s);        // good as is
                    return;
                }

                int l = s.Length;
                for (int i = 0; i < l; i++)
                {
                    char ch = s[i];

                    if (ch == '&')
                    {
                        // We found a '&'. Now look for the next ';' or '&'. The idea is that
                        // if we find another '&' before finding a ';', then this is not an entity,
                        // and the next '&' might start a real entity (VSWhidbey 275184)
                        int index = s.IndexOfAny(s_entityEndingChars, i + 1);
                        if (index > 0 && s[index] == ';')
                        {
                            string entity = s.Substring(i + 1, index - i - 1);

                            if (entity.Length > 1 && entity[0] == '#')
                            {
                                try
                                {
                                    // The # syntax can be in decimal or hex, e.g.
                                    //      &#229;  --> decimal
                                    //      &#xE5;  --> same char in hex
                                    // See http://www.w3.org/TR/REC-html40/charset.html#entities
                                    if (entity[1] == 'x' || entity[1] == 'X')
                                        ch = (char)Int32.Parse(entity.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                                    else
                                        ch = (char)Int32.Parse(entity.Substring(1), CultureInfo.InvariantCulture);
                                    i = index; // already looked at everything until semicolon
                                }
                                catch (System.FormatException e)
                                {
                                    i++;    //if the number isn't valid, ignore it
                                    if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, typeof(HttpUtility), "HtmlDecode", e);
                                }
                                catch (System.ArgumentException e)
                                {
                                    i++;    // if there is no number, ignore it.
                                    if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, typeof(HttpUtility), "HtmlDecode", e);
                                }
                            }
                            else
                            {
                                i = index; // already looked at everything until semicolon

                                char entityChar = HtmlEntities.Lookup(entity);
                                if (entityChar != (char)0)
                                {
                                    ch = entityChar;
                                }
                                else
                                {
                                    output.Write('&');
                                    output.Write(entity);
                                    output.Write(';');
                                    continue;
                                }
                            }

                        }
                    }

                    output.Write(ch);
                }

            }

            // helper class for lookup of HTML encoding entities
            static class HtmlEntities
            {
                private static object _lookupLockObject = new object();

                // The list is from http://www.w3.org/TR/REC-html40/sgml/entities.html
                private static String[] _entitiesList = new String[] {
            "\x0022-quot",
            "\x0026-amp",
            "\x003c-lt",
            "\x003e-gt",
            "\x00a0-nbsp",
            "\x00a1-iexcl",
            "\x00a2-cent",
            "\x00a3-pound",
            "\x00a4-curren",
            "\x00a5-yen",
            "\x00a6-brvbar",
            "\x00a7-sect",
            "\x00a8-uml",
            "\x00a9-copy",
            "\x00aa-ordf",
            "\x00ab-laquo",
            "\x00ac-not",
            "\x00ad-shy",
            "\x00ae-reg",
            "\x00af-macr",
            "\x00b0-deg",
            "\x00b1-plusmn",
            "\x00b2-sup2",
            "\x00b3-sup3",
            "\x00b4-acute",
            "\x00b5-micro",
            "\x00b6-para",
            "\x00b7-middot",
            "\x00b8-cedil",
            "\x00b9-sup1",
            "\x00ba-ordm",
            "\x00bb-raquo",
            "\x00bc-frac14",
            "\x00bd-frac12",
            "\x00be-frac34",
            "\x00bf-iquest",
            "\x00c0-Agrave",
            "\x00c1-Aacute",
            "\x00c2-Acirc",
            "\x00c3-Atilde",
            "\x00c4-Auml",
            "\x00c5-Aring",
            "\x00c6-AElig",
            "\x00c7-Ccedil",
            "\x00c8-Egrave",
            "\x00c9-Eacute",
            "\x00ca-Ecirc",
            "\x00cb-Euml",
            "\x00cc-Igrave",
            "\x00cd-Iacute",
            "\x00ce-Icirc",
            "\x00cf-Iuml",
            "\x00d0-ETH",
            "\x00d1-Ntilde",
            "\x00d2-Ograve",
            "\x00d3-Oacute",
            "\x00d4-Ocirc",
            "\x00d5-Otilde",
            "\x00d6-Ouml",
            "\x00d7-times",
            "\x00d8-Oslash",
            "\x00d9-Ugrave",
            "\x00da-Uacute",
            "\x00db-Ucirc",
            "\x00dc-Uuml",
            "\x00dd-Yacute",
            "\x00de-THORN",
            "\x00df-szlig",
            "\x00e0-agrave",
            "\x00e1-aacute",
            "\x00e2-acirc",
            "\x00e3-atilde",
            "\x00e4-auml",
            "\x00e5-aring",
            "\x00e6-aelig",
            "\x00e7-ccedil",
            "\x00e8-egrave",
            "\x00e9-eacute",
            "\x00ea-ecirc",
            "\x00eb-euml",
            "\x00ec-igrave",
            "\x00ed-iacute",
            "\x00ee-icirc",
            "\x00ef-iuml",
            "\x00f0-eth",
            "\x00f1-ntilde",
            "\x00f2-ograve",
            "\x00f3-oacute",
            "\x00f4-ocirc",
            "\x00f5-otilde",
            "\x00f6-ouml",
            "\x00f7-divide",
            "\x00f8-oslash",
            "\x00f9-ugrave",
            "\x00fa-uacute",
            "\x00fb-ucirc",
            "\x00fc-uuml",
            "\x00fd-yacute",
            "\x00fe-thorn",
            "\x00ff-yuml",
            "\x0152-OElig",
            "\x0153-oelig",
            "\x0160-Scaron",
            "\x0161-scaron",
            "\x0178-Yuml",
            "\x0192-fnof",
            "\x02c6-circ",
            "\x02dc-tilde",
            "\x0391-Alpha",
            "\x0392-Beta",
            "\x0393-Gamma",
            "\x0394-Delta",
            "\x0395-Epsilon",
            "\x0396-Zeta",
            "\x0397-Eta",
            "\x0398-Theta",
            "\x0399-Iota",
            "\x039a-Kappa",
            "\x039b-Lambda",
            "\x039c-Mu",
            "\x039d-Nu",
            "\x039e-Xi",
            "\x039f-Omicron",
            "\x03a0-Pi",
            "\x03a1-Rho",
            "\x03a3-Sigma",
            "\x03a4-Tau",
            "\x03a5-Upsilon",
            "\x03a6-Phi",
            "\x03a7-Chi",
            "\x03a8-Psi",
            "\x03a9-Omega",
            "\x03b1-alpha",
            "\x03b2-beta",
            "\x03b3-gamma",
            "\x03b4-delta",
            "\x03b5-epsilon",
            "\x03b6-zeta",
            "\x03b7-eta",
            "\x03b8-theta",
            "\x03b9-iota",
            "\x03ba-kappa",
            "\x03bb-lambda",
            "\x03bc-mu",
            "\x03bd-nu",
            "\x03be-xi",
            "\x03bf-omicron",
            "\x03c0-pi",
            "\x03c1-rho",
            "\x03c2-sigmaf",
            "\x03c3-sigma",
            "\x03c4-tau",
            "\x03c5-upsilon",
            "\x03c6-phi",
            "\x03c7-chi",
            "\x03c8-psi",
            "\x03c9-omega",
            "\x03d1-thetasym",
            "\x03d2-upsih",
            "\x03d6-piv",
            "\x2002-ensp",
            "\x2003-emsp",
            "\x2009-thinsp",
            "\x200c-zwnj",
            "\x200d-zwj",
            "\x200e-lrm",
            "\x200f-rlm",
            "\x2013-ndash",
            "\x2014-mdash",
            "\x2018-lsquo",
            "\x2019-rsquo",
            "\x201a-sbquo",
            "\x201c-ldquo",
            "\x201d-rdquo",
            "\x201e-bdquo",
            "\x2020-dagger",
            "\x2021-Dagger",
            "\x2022-bull",
            "\x2026-hellip",
            "\x2030-permil",
            "\x2032-prime",
            "\x2033-Prime",
            "\x2039-lsaquo",
            "\x203a-rsaquo",
            "\x203e-oline",
            "\x2044-frasl",
            "\x20ac-euro",
            "\x2111-image",
            "\x2118-weierp",
            "\x211c-real",
            "\x2122-trade",
            "\x2135-alefsym",
            "\x2190-larr",
            "\x2191-uarr",
            "\x2192-rarr",
            "\x2193-darr",
            "\x2194-harr",
            "\x21b5-crarr",
            "\x21d0-lArr",
            "\x21d1-uArr",
            "\x21d2-rArr",
            "\x21d3-dArr",
            "\x21d4-hArr",
            "\x2200-forall",
            "\x2202-part",
            "\x2203-exist",
            "\x2205-empty",
            "\x2207-nabla",
            "\x2208-isin",
            "\x2209-notin",
            "\x220b-ni",
            "\x220f-prod",
            "\x2211-sum",
            "\x2212-minus",
            "\x2217-lowast",
            "\x221a-radic",
            "\x221d-prop",
            "\x221e-infin",
            "\x2220-ang",
            "\x2227-and",
            "\x2228-or",
            "\x2229-cap",
            "\x222a-cup",
            "\x222b-int",
            "\x2234-there4",
            "\x223c-sim",
            "\x2245-cong",
            "\x2248-asymp",
            "\x2260-ne",
            "\x2261-equiv",
            "\x2264-le",
            "\x2265-ge",
            "\x2282-sub",
            "\x2283-sup",
            "\x2284-nsub",
            "\x2286-sube",
            "\x2287-supe",
            "\x2295-oplus",
            "\x2297-otimes",
            "\x22a5-perp",
            "\x22c5-sdot",
            "\x2308-lceil",
            "\x2309-rceil",
            "\x230a-lfloor",
            "\x230b-rfloor",
            "\x2329-lang",
            "\x232a-rang",
            "\x25ca-loz",
            "\x2660-spades",
            "\x2663-clubs",
            "\x2665-hearts",
            "\x2666-diams",
        };

                // Double-checked locking pattern requires volatile for read/write synchronization
                private static volatile Hashtable _entitiesLookupTable;

                internal /*public*/ static char Lookup(String entity)
                {
                    if (_entitiesLookupTable == null)
                    {
                        // populate hashtable on demand
                        lock (_lookupLockObject)
                        {
                            if (_entitiesLookupTable == null)
                            {
                                Hashtable t = new Hashtable();

                                foreach (String s in _entitiesList)
                                    t[s.Substring(2)] = s[0];  // 1st char is the code, 2nd '-'

                                _entitiesLookupTable = t;
                            }
                        }
                    }

                    Object obj = _entitiesLookupTable[entity];

                    if (obj != null)
                        return (char)obj;
                    else
                        return (char)0;
                }
            }
        }
    }
}


