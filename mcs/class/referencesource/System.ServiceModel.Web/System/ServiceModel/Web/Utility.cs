//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Web
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Mime;
    using System.Runtime;
    using System.Text;
    using System.Globalization;
    using System.ServiceModel.Channels;
    
    static class Utility
    {
        public const string applicationXml = "application/xml";
        public const string textXml = "text/xml";
        public const string applicationJson = "application/json";
        public const string textJson = "text/json";
        public const string GET = "GET";
        

        public static bool IsXmlContent(this string contentType)
        {
            if (contentType == null)
            {
                return true;
            }

            string contentTypeProcessed = contentType.Trim();

            return contentTypeProcessed.StartsWith(applicationXml, StringComparison.OrdinalIgnoreCase)
                || contentTypeProcessed.StartsWith(textXml, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsJsonContent(this string contentType)
        {
            if (contentType == null)
            {
                return true;
            }

            string contentTypeProcessed = contentType.Trim();

            return contentTypeProcessed.StartsWith(applicationJson, StringComparison.OrdinalIgnoreCase)
                || contentTypeProcessed.StartsWith(textJson, StringComparison.OrdinalIgnoreCase);
        }

        public static string CombineUri(string former, string latter)
        {
            // Appending the latter string to the form string,
            // while making sure there is a single slash char seperating the latter and the former.
            // This method behaves differently than new Uri(baseUri, relativeUri)
            // as CombineUri simply appends, whereas new Uri() actually replaces the last segment
            // of the its base path with the relative uri.

            StringBuilder builder = new StringBuilder();
            if (former.Length > 0 && latter.Length > 0)
            {
                if (former[former.Length - 1] == '/' && latter[0] == '/')
                {
                    builder.Append(former, 0, former.Length - 1);
                    builder.Append(latter);
                    return builder.ToString();
                }

                if (former[former.Length - 1] != '/' && latter[0] != '/')
                {
                    builder.Append(former);
                    builder.Append('/');
                    builder.Append(latter);
                    return builder.ToString();
                }
            }

            return former + latter;
        }
        public static List<string> QuoteAwareStringSplit(string str)
        {
            List<string> subStrings = new List<string>();
            int offset = 0;
            while (true)
            {
                string subString = QuoteAwareSubString(str, ref offset);
                if (subString == null)
                {
                    break;
                }
                subStrings.Add(subString);
            }

            return subStrings;
        }

        // This method extracts substrings from a string starting at the offset
        // and up until the next comma in the string.  The sub string extraction is 
        // quote aware such that commas inside quoted-strings are ignored.  On return, 
        // offset points to the next char beyond the comma of the substring returned 
        // and may point beyond the length of the header.
        public static string QuoteAwareSubString(string str, ref int offset)
        {
            // this method will filter out empty-string and white-space-only items in 
            // the header.  For example "x,,y" and "x, ,y" would result in just "x" and "y"
            // substrings being returned.

            if (string.IsNullOrEmpty(str) || offset >= str.Length)
            {
                return null;
            }

            int startIndex = (offset > 0) ? offset : 0;

            // trim whitespace and commas from the begining of the item
            while (char.IsWhiteSpace(str[startIndex]) || str[startIndex] == ',')
            {
                startIndex++;
                if (startIndex >= str.Length)
                {
                    return null;
                }
            }

            int endIndex = startIndex;
            bool insideQuotes = false;

            while (endIndex < str.Length)
            {
                if (str[endIndex] == '\"' &&
                   (!insideQuotes || endIndex == 0 || str[endIndex - 1] != '\\'))
                {
                    insideQuotes = !insideQuotes;
                }
                else if (str[endIndex] == ',' && !insideQuotes)
                {
                    break;
                }
                endIndex++;
            }
            offset = endIndex + 1;

            // trim whitespace from the end of the item; the substring is guaranteed to
            // have at least one non-whitespace character
            while (char.IsWhiteSpace(str[endIndex - 1]))
            {
                endIndex--;
            }

            return str.Substring(startIndex, endIndex - startIndex);
        }

        public static ContentType GetContentType(string contentType)
        {
            string contentTypeTrimmed = contentType.Trim();
            if (!string.IsNullOrEmpty(contentTypeTrimmed))
            {
                return GetContentTypeOrNull(contentTypeTrimmed);
            }
            return null;
        }

        public static ContentType GetContentTypeOrNull(string contentType)
        {
            try
            {
                Fx.Assert(contentType == contentType.Trim(), "The ContentType input argument should already be trimmed.");
                Fx.Assert(!string.IsNullOrEmpty(contentType), "The ContentType input argument should not be null or empty.");
                
                ContentType contentTypeToReturn = new ContentType(contentType);

                // Need to check for "*/<Something-other-than-*>" because the ContentType constructor doesn't catch this
                string[] typeAndSubType = contentTypeToReturn.MediaType.Split('/');
                Fx.Assert(typeAndSubType.Length == 2, "The creation of the ContentType would have failed if there wasn't a type and subtype.");
                if (typeAndSubType[0][0] == '*' && typeAndSubType[0].Length == 1 &&
                    !(typeAndSubType[1][0] == '*' && typeAndSubType[1].Length == 1))
                {
                    // 



                    // throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new FormatException(
                    // SR2.GetString(SR2.InvalidContentType, contentType)));
                    return null;
                }
                return contentTypeToReturn;
            }
            catch (FormatException e)
            {
                // Return null to indicate that the content type creation failed
                System.ServiceModel.DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
            }
            return null;
        }

        public static string IEnumerableToCommaSeparatedString(IEnumerable<string> items)
        {
            Fx.Assert(items != null, "The 'items' argument should never be null.");
            return string.Join(", ", items);
        }

        public static void AddRange<T>(ICollection<T> list, IEnumerable<T> itemsToAdd)
        {
            Fx.Assert(list != null, "The 'list' argument should never be null.");
            Fx.Assert(itemsToAdd != null, "The 'itemsToAdd' argument should never be null.");

            foreach (T item in itemsToAdd)
            {
                list.Add(item);
            }
        }
    }
}
