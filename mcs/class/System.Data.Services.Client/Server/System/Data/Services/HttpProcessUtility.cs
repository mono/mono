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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    internal static class HttpProcessUtility
    {
        internal static readonly UTF8Encoding EncodingUtf8NoPreamble = new UTF8Encoding(false, true);

        internal static Encoding FallbackEncoding
        {
            get
            {
                return EncodingUtf8NoPreamble;
            }
        }

        private static Encoding MissingEncoding
        {
            get
            {
#if ASTORIA_LIGHT                
                return Encoding.UTF8;
#else
                return Encoding.GetEncoding("ISO-8859-1", new EncoderExceptionFallback(), new DecoderExceptionFallback());
#endif
            }
        }


        internal static KeyValuePair<string, string>[] ReadContentType(string contentType, out string mime, out Encoding encoding)
        {
            if (String.IsNullOrEmpty(contentType))
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_ContentTypeMissing);
            }

            MediaType mediaType = ReadMediaType(contentType);
            mime = mediaType.MimeType;
            encoding = mediaType.SelectEncoding();
            return mediaType.Parameters;
        }

        internal static bool TryReadVersion(string text, out KeyValuePair<Version, string> result)
        {
            Debug.Assert(text != null, "text != null");

            int separator = text.IndexOf(';');
            string versionText, libraryName;
            if (separator >= 0)
            {
                versionText = text.Substring(0, separator);
                libraryName = text.Substring(separator + 1).Trim();
            }
            else
            {
                versionText = text;
                libraryName = null;
            }

            result = default(KeyValuePair<Version, string>);
            versionText = versionText.Trim();

            bool dotFound = false;
            for (int i = 0; i < versionText.Length; i++)
            {
                if (versionText[i] == '.')
                {
                    if (dotFound)
                    {
                        return false;
                    }

                    dotFound = true;
                }
                else if (versionText[i] < '0' || versionText[i] > '9')
                {
                    return false;
                }
            }

            try
            {
                result = new KeyValuePair<Version, string>(new Version(versionText), libraryName);
                return true;
            }
            catch (Exception e)
            {
                if (e is FormatException || e is OverflowException || e is ArgumentException)
                {
                    return false;
                }

                throw;
            }
        }

        private static Encoding EncodingFromName(string name)
        {
            if (name == null)
            {
                return MissingEncoding;
            }

            name = name.Trim();
            if (name.Length == 0)
            {
                return MissingEncoding;
            }
            else
            {
                try
                {
#if ASTORIA_LIGHT
                    return Encoding.UTF8;
#else
                    return Encoding.GetEncoding(name);
#endif
                }
                catch (ArgumentException)
                {
                    throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_EncodingNotSupported(name));
                }
            }
        }


        private static void ReadMediaTypeAndSubtype(string text, ref int textIndex, out string type, out string subType)
        {
            Debug.Assert(text != null, "text != null");
            int textStart = textIndex;
            if (ReadToken(text, ref textIndex))
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_MediaTypeUnspecified);
            }

            if (text[textIndex] != '/')
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_MediaTypeRequiresSlash);
            }

            type = text.Substring(textStart, textIndex - textStart);
            textIndex++;

            int subTypeStart = textIndex;
            ReadToken(text, ref textIndex);

            if (textIndex == subTypeStart)
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_MediaTypeRequiresSubType);
            }

            subType = text.Substring(subTypeStart, textIndex - subTypeStart);
        }

        private static MediaType ReadMediaType(string text)
        {
            Debug.Assert(text != null, "text != null");

            string type;
            string subType;
            int textIndex = 0;
            ReadMediaTypeAndSubtype(text, ref textIndex, out type, out subType);

            KeyValuePair<string, string>[] parameters = null;
            while (!SkipWhitespace(text, ref textIndex))
            {
                if (text[textIndex] != ';')
                {
                    throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_MediaTypeRequiresSemicolonBeforeParameter);
                }

                textIndex++;
                if (SkipWhitespace(text, ref textIndex))
                {
                    break;
                }

                ReadMediaTypeParameter(text, ref textIndex, ref parameters);
            }

            return new MediaType(type, subType, parameters);
        }

        private static bool ReadToken(string text, ref int textIndex)
        {
            while (textIndex < text.Length && IsHttpToken(text[textIndex]))
            {
                textIndex++;
            }

            return (textIndex == text.Length);
        }

        private static bool SkipWhitespace(string text, ref int textIndex)
        {
            Debug.Assert(text != null, "text != null");
            Debug.Assert(text.Length >= 0, "text >= 0");
            Debug.Assert(textIndex <= text.Length, "text <= text.Length");

            while (textIndex < text.Length && Char.IsWhiteSpace(text, textIndex))
            {
                textIndex++;
            }

            return (textIndex == text.Length);
        }

        private static void ReadMediaTypeParameter(string text, ref int textIndex, ref KeyValuePair<string, string>[] parameters)
        {
            int startIndex = textIndex;
            if (ReadToken(text, ref textIndex))
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_MediaTypeMissingValue);
            }

            string parameterName = text.Substring(startIndex, textIndex - startIndex);
            if (text[textIndex] != '=')
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_MediaTypeMissingValue);
            }

            textIndex++;

            string parameterValue = ReadQuotedParameterValue(parameterName, text, ref textIndex);

            if (parameters == null)
            {
                parameters = new KeyValuePair<string, string>[1];
            }
            else
            {
                KeyValuePair<string, string>[] grow = new KeyValuePair<string, string>[parameters.Length + 1];
                Array.Copy(parameters, grow, parameters.Length);
                parameters = grow;
            }

            parameters[parameters.Length - 1] = new KeyValuePair<string, string>(parameterName, parameterValue);
        }

        private static string ReadQuotedParameterValue(string parameterName, string headerText, ref int textIndex)
        {
            StringBuilder parameterValue = new StringBuilder();
            
            bool valueIsQuoted = false;
            if (textIndex < headerText.Length)
            {
                if (headerText[textIndex] == '\"')
                {
                    textIndex++;
                    valueIsQuoted = true;
                }
            }

            while (textIndex < headerText.Length)
            {
                char currentChar = headerText[textIndex];

                if (currentChar == '\\' || currentChar == '\"')
                {
                    if (!valueIsQuoted)
                    {
                        throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_EscapeCharWithoutQuotes(parameterName));
                    }

                    textIndex++;

                    if (currentChar == '\"')
                    {
                        valueIsQuoted = false;
                        break;
                    }

                    if (textIndex >= headerText.Length)
                    {
                        throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_EscapeCharAtEnd(parameterName));
                    }

                    currentChar = headerText[textIndex];        
                }
                else
                if (!IsHttpToken(currentChar))
                {
                    break;
                }

                parameterValue.Append(currentChar);
                textIndex++;
            }

            if (valueIsQuoted)
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_ClosingQuoteNotFound(parameterName));
            }

            return parameterValue.ToString();
        }


        private static bool IsHttpSeparator(char c)
        {
            return
                c == '(' || c == ')' || c == '<' || c == '>' || c == '@' ||
                c == ',' || c == ';' || c == ':' || c == '\\' || c == '"' ||
                c == '/' || c == '[' || c == ']' || c == '?' || c == '=' ||
                c == '{' || c == '}' || c == ' ' || c == '\x9';
        }

        private static bool IsHttpToken(char c)
        {
            return c < '\x7F' && c > '\x1F' && !IsHttpSeparator(c);
        }


        [DebuggerDisplay("MediaType [{type}/{subType}]")]
        private sealed class MediaType
        {
            private readonly KeyValuePair<string, string>[] parameters;

            private readonly string subType;

            private readonly string type;

            internal MediaType(string type, string subType, KeyValuePair<string, string>[] parameters)
            {
                Debug.Assert(type != null, "type != null");
                Debug.Assert(subType != null, "subType != null");

                this.type = type;
                this.subType = subType;
                this.parameters = parameters;
            }

            internal string MimeType
            {
                get { return this.type + "/" + this.subType; }
            }

            internal KeyValuePair<string, string>[] Parameters
            {
                get { return this.parameters; }
            }


            internal Encoding SelectEncoding()
            {
                if (this.parameters != null)
                {
                    foreach (KeyValuePair<string, string> parameter in this.parameters)
                    {
                        if (String.Equals(parameter.Key, XmlConstants.HttpCharsetParameter, StringComparison.OrdinalIgnoreCase))
                        {
                            string encodingName = parameter.Value.Trim();
                            if (encodingName.Length > 0)
                            {
                                return EncodingFromName(parameter.Value);
                            }
                        }
                    }
                }

                if (String.Equals(this.type, XmlConstants.MimeTextType, StringComparison.OrdinalIgnoreCase))
                {
                    if (String.Equals(this.subType, XmlConstants.MimeXmlSubType, StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                    else
                    {
                        return MissingEncoding;
                    }
                }
                else if (String.Equals(this.type, XmlConstants.MimeApplicationType, StringComparison.OrdinalIgnoreCase) &&
                    String.Equals(this.subType, XmlConstants.MimeJsonSubType, StringComparison.OrdinalIgnoreCase))
                {
                    return FallbackEncoding;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
