//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System.Runtime.Serialization;
////using System.ServiceModel.Channels;
using System.Globalization;
using System.Runtime.Serialization.Diagnostics.Application;

using SR_ = System.Runtime.Serialization.SR;

namespace System.Xml
{
    static class XmlExceptionHelper
    {
        static void ThrowXmlException(XmlDictionaryReader reader, string res)
        {
            ThrowXmlException(reader, res, null);
        }

        static void ThrowXmlException(XmlDictionaryReader reader, string res, string arg1)
        {
            ThrowXmlException(reader, res, arg1, null);
        }

        static void ThrowXmlException(XmlDictionaryReader reader, string res, string arg1, string arg2)
        {
            ThrowXmlException(reader, res, arg1, arg2, null);
        }

        static void ThrowXmlException(XmlDictionaryReader reader, string res, string arg1, string arg2, string arg3)
        {
            string s = SR_.GetString(res, arg1, arg2, arg3);
            IXmlLineInfo lineInfo = reader as IXmlLineInfo;
            if (lineInfo != null && lineInfo.HasLineInfo())
            {
                s += " " + SR_.GetString(SR_.XmlLineInfo, lineInfo.LineNumber, lineInfo.LinePosition);
            }

            if (TD.ReaderQuotaExceededIsEnabled())
            {
                TD.ReaderQuotaExceeded(s);
            }

            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(s));
        }

        static public void ThrowXmlException(XmlDictionaryReader reader, XmlException exception)
        {
            string s = exception.Message;
            IXmlLineInfo lineInfo = reader as IXmlLineInfo;
            if (lineInfo != null && lineInfo.HasLineInfo())
            {
                s += " " + SR_.GetString(SR_.XmlLineInfo, lineInfo.LineNumber, lineInfo.LinePosition);
            }
            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(s));
        }

        static string GetName(string prefix, string localName)
        {
            if (prefix.Length == 0)
                return localName;
            else
                return string.Concat(prefix, ":", localName);
        }

        static string GetWhatWasFound(XmlDictionaryReader reader)
        {
            if (reader.EOF)
                return SR_.GetString(SR_.XmlFoundEndOfFile);
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    return SR_.GetString(SR_.XmlFoundElement, GetName(reader.Prefix, reader.LocalName), reader.NamespaceURI);
                case XmlNodeType.EndElement:
                    return SR_.GetString(SR_.XmlFoundEndElement, GetName(reader.Prefix, reader.LocalName), reader.NamespaceURI);
                case XmlNodeType.Text:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    return SR_.GetString(SR_.XmlFoundText, reader.Value);
                case XmlNodeType.Comment:
                    return SR_.GetString(SR_.XmlFoundComment, reader.Value);
                case XmlNodeType.CDATA:
                    return SR_.GetString(SR_.XmlFoundCData, reader.Value);
            }
            return SR_.GetString(SR_.XmlFoundNodeType, reader.NodeType);
        }

        static public void ThrowStartElementExpected(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, SR_.XmlStartElementExpected, GetWhatWasFound(reader));
        }

        static public void ThrowStartElementExpected(XmlDictionaryReader reader, string name)
        {
            ThrowXmlException(reader, SR_.XmlStartElementNameExpected, name, GetWhatWasFound(reader));
        }

        static public void ThrowStartElementExpected(XmlDictionaryReader reader, string localName, string ns)
        {
            ThrowXmlException(reader, SR_.XmlStartElementLocalNameNsExpected, localName, ns, GetWhatWasFound(reader));
        }

        static public void ThrowStartElementExpected(XmlDictionaryReader reader, XmlDictionaryString localName, XmlDictionaryString ns)
        {
            ThrowStartElementExpected(reader, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(ns));
        }

        static public void ThrowFullStartElementExpected(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, SR_.XmlFullStartElementExpected, GetWhatWasFound(reader));
        }

        static public void ThrowFullStartElementExpected(XmlDictionaryReader reader, string name)
        {
            ThrowXmlException(reader, SR_.XmlFullStartElementNameExpected, name, GetWhatWasFound(reader));
        }

        static public void ThrowFullStartElementExpected(XmlDictionaryReader reader, string localName, string ns)
        {
            ThrowXmlException(reader, SR_.XmlFullStartElementLocalNameNsExpected, localName, ns, GetWhatWasFound(reader));
        }

        static public void ThrowFullStartElementExpected(XmlDictionaryReader reader, XmlDictionaryString localName, XmlDictionaryString ns)
        {
            ThrowFullStartElementExpected(reader, XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(ns));
        }

        static public void ThrowEndElementExpected(XmlDictionaryReader reader, string localName, string ns)
        {
            ThrowXmlException(reader, SR_.XmlEndElementExpected, localName, ns, GetWhatWasFound(reader));
        }

        static public void ThrowMaxStringContentLengthExceeded(XmlDictionaryReader reader, int maxStringContentLength)
        {
            ThrowXmlException(reader, SR_.XmlMaxStringContentLengthExceeded, maxStringContentLength.ToString(NumberFormatInfo.CurrentInfo));
        }

        static public void ThrowMaxArrayLengthExceeded(XmlDictionaryReader reader, int maxArrayLength)
        {
            ThrowXmlException(reader, SR_.XmlMaxArrayLengthExceeded, maxArrayLength.ToString(NumberFormatInfo.CurrentInfo));
        }

        static public void ThrowMaxArrayLengthOrMaxItemsQuotaExceeded(XmlDictionaryReader reader, int maxQuota)
        {
            ThrowXmlException(reader, SR_.XmlMaxArrayLengthOrMaxItemsQuotaExceeded, maxQuota.ToString(NumberFormatInfo.CurrentInfo));
        }

        static public void ThrowMaxDepthExceeded(XmlDictionaryReader reader, int maxDepth)
        {
            ThrowXmlException(reader, SR_.XmlMaxDepthExceeded, maxDepth.ToString(NumberFormatInfo.CurrentInfo));
        }

        static public void ThrowMaxBytesPerReadExceeded(XmlDictionaryReader reader, int maxBytesPerRead)
        {
            ThrowXmlException(reader, SR_.XmlMaxBytesPerReadExceeded, maxBytesPerRead.ToString(NumberFormatInfo.CurrentInfo));
        }

        static public void ThrowMaxNameTableCharCountExceeded(XmlDictionaryReader reader, int maxNameTableCharCount)
        {
            ThrowXmlException(reader, SR_.XmlMaxNameTableCharCountExceeded, maxNameTableCharCount.ToString(NumberFormatInfo.CurrentInfo));
        }

        static public void ThrowBase64DataExpected(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, SR_.XmlBase64DataExpected, GetWhatWasFound(reader));
        }

        static public void ThrowUndefinedPrefix(XmlDictionaryReader reader, string prefix)
        {
            ThrowXmlException(reader, SR_.XmlUndefinedPrefix, prefix);
        }

        static public void ThrowProcessingInstructionNotSupported(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, SR_.XmlProcessingInstructionNotSupported);
        }

        static public void ThrowInvalidXml(XmlDictionaryReader reader, byte b)
        {
            ThrowXmlException(reader, SR_.XmlInvalidXmlByte, b.ToString("X2", CultureInfo.InvariantCulture));
        }

        static public void ThrowUnexpectedEndOfFile(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, SR_.XmlUnexpectedEndOfFile, ((XmlBaseReader)reader).GetOpenElements());
        }

        static public void ThrowUnexpectedEndElement(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, SR_.XmlUnexpectedEndElement);
        }

        static public void ThrowTokenExpected(XmlDictionaryReader reader, string expected, char found)
        {
            ThrowXmlException(reader, SR_.XmlTokenExpected, expected, found.ToString());
        }

        static public void ThrowTokenExpected(XmlDictionaryReader reader, string expected, string found)
        {
            ThrowXmlException(reader, SR_.XmlTokenExpected, expected, found);
        }

        static public void ThrowInvalidCharRef(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, SR_.XmlInvalidCharRef);
        }

        static public void ThrowTagMismatch(XmlDictionaryReader reader, string expectedPrefix, string expectedLocalName, string foundPrefix, string foundLocalName)
        {
            ThrowXmlException(reader, SR_.XmlTagMismatch, GetName(expectedPrefix, expectedLocalName), GetName(foundPrefix, foundLocalName));
        }

        static public void ThrowDuplicateXmlnsAttribute(XmlDictionaryReader reader, string localName, string ns)
        {
            string name;
            if (localName.Length == 0)
                name = "xmlns";
            else
                name = "xmlns:" + localName;
            ThrowXmlException(reader, SR_.XmlDuplicateAttribute, name, name, ns);
        }

        static public void ThrowDuplicateAttribute(XmlDictionaryReader reader, string prefix1, string prefix2, string localName, string ns)
        {
            ThrowXmlException(reader, SR_.XmlDuplicateAttribute, GetName(prefix1, localName), GetName(prefix2, localName), ns);
        }

        static public void ThrowInvalidBinaryFormat(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, SR_.XmlInvalidFormat);
        }

        static public void ThrowInvalidRootData(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, SR_.XmlInvalidRootData);
        }

        static public void ThrowMultipleRootElements(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, SR_.XmlMultipleRootElements);
        }

        static public void ThrowDeclarationNotFirst(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, SR_.XmlDeclNotFirst);
        }

        static public void ThrowConversionOverflow(XmlDictionaryReader reader, string value, string type)
        {
            ThrowXmlException(reader, SR_.XmlConversionOverflow, value, type);
        }

        static public void ThrowXmlDictionaryStringIDOutOfRange(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, SR_.XmlDictionaryStringIDRange, XmlDictionaryString.MinKey.ToString(NumberFormatInfo.CurrentInfo), XmlDictionaryString.MaxKey.ToString(NumberFormatInfo.CurrentInfo));
        }

        static public void ThrowXmlDictionaryStringIDUndefinedStatic(XmlDictionaryReader reader, int key)
        {
            ThrowXmlException(reader, SR_.XmlDictionaryStringIDUndefinedStatic, key.ToString(NumberFormatInfo.CurrentInfo));
        }

        static public void ThrowXmlDictionaryStringIDUndefinedSession(XmlDictionaryReader reader, int key)
        {
            ThrowXmlException(reader, SR_.XmlDictionaryStringIDUndefinedSession, key.ToString(NumberFormatInfo.CurrentInfo));
        }

        static public void ThrowEmptyNamespace(XmlDictionaryReader reader)
        {
            ThrowXmlException(reader, SR_.XmlEmptyNamespaceRequiresNullPrefix);
        }

        static public XmlException CreateConversionException(string value, string type, Exception exception)
        {
            return new XmlException(SR_.GetString(SR_.XmlInvalidConversion, value, type), exception);
        }

        static public XmlException CreateEncodingException(byte[] buffer, int offset, int count, Exception exception)
        {
            return CreateEncodingException(new System.Text.UTF8Encoding(false, false).GetString(buffer, offset, count), exception);
        }

        static public XmlException CreateEncodingException(string value, Exception exception)
        {
            return new XmlException(SR_.GetString(SR_.XmlInvalidUTF8Bytes, value), exception);
        }
    }
}
