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


namespace System.Data.Services.Client.Xml
{
    #region Namespaces.

    using System.Diagnostics;
    using System.Xml;

    #endregion Namespaces.

    [DebuggerDisplay("XmlAtomErrorReader {NodeType} {Name} {Value}")]
    internal class XmlAtomErrorReader : XmlWrappingReader
    {
        internal XmlAtomErrorReader(XmlReader baseReader) : base(baseReader)
        {
            Debug.Assert(baseReader != null, "baseReader != null");
            this.Reader = baseReader;
        }

        #region Methods.

        public override bool Read()
        {
            bool result = base.Read();

            if (this.NodeType == XmlNodeType.Element &&
                Util.AreSame(this.Reader, XmlConstants.XmlErrorElementName, XmlConstants.DataWebMetadataNamespace))
            {
                string message = ReadErrorMessage(this.Reader);

                throw new DataServiceClientException(Strings.Deserialize_ServerException(message));
            }

            return result;
        }

        internal static string ReadElementString(XmlReader reader, bool checkNullAttribute)
        {
            Debug.Assert(reader != null, "reader != null");
            Debug.Assert(XmlNodeType.Element == reader.NodeType, "not positioned on Element");

            string result = null;
            bool empty = checkNullAttribute && !Util.DoesNullAttributeSayTrue(reader);

            if (reader.IsEmptyElement)
            {
                return (empty ? String.Empty : null);
            }

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        return result ?? (empty ? String.Empty : null);
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Text:
                    case XmlNodeType.SignificantWhitespace:
                        if (null != result)
                        {
                            throw Error.InvalidOperation(Strings.Deserialize_MixedTextWithComment);
                        }

                        result = reader.Value;
                        break;
                    case XmlNodeType.Comment:
                    case XmlNodeType.Whitespace:
                        break;
                    case XmlNodeType.Element:
                    default:
                        throw Error.InvalidOperation(Strings.Deserialize_ExpectingSimpleValue);
                }
            }

            throw Error.InvalidOperation(Strings.Deserialize_ExpectingSimpleValue);
        }

        private static string ReadErrorMessage(XmlReader reader)
        {
            Debug.Assert(reader != null, "reader != null");
            Debug.Assert(reader.NodeType == XmlNodeType.Element, "reader.NodeType == XmlNodeType.Element");
            Debug.Assert(reader.LocalName == XmlConstants.XmlErrorElementName, "reader.LocalName == XmlConstants.XmlErrorElementName");

            int depth = 1;
            while (depth > 0 && reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (!reader.IsEmptyElement)
                    {
                        depth++;
                    }

                    if (depth == 2 &&
                        Util.AreSame(reader, XmlConstants.XmlErrorMessageElementName, XmlConstants.DataWebMetadataNamespace))
                    {
                        return ReadElementString(reader, false);
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    depth--;
                }
            }

            return String.Empty;
        }

        #endregion Methods.
    }
}
