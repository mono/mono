using System;
using System.IO;

namespace System.Xml
{
	public interface IXmlBinaryReaderInitializer
	{
		void SetInput (
			Stream stream,
			IXmlDictionary dictionary,
			XmlDictionaryReaderQuotas quota,
			XmlBinaryReaderSession session,
			OnXmlDictionaryReaderClose onClose,
			XmlParserContext context);

		void SetInput (byte [] buffer, int offset, int count,
			IXmlDictionary dictionary,
			XmlDictionaryReaderQuotas quota,
			XmlBinaryReaderSession session,
			OnXmlDictionaryReaderClose onClose,
			XmlParserContext context);
	}
}
