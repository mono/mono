using System;
using System.IO;

namespace System.Xml
{
	public interface IXmlUTF8ReaderInitializer
	{
		void SetInput (byte [] buffer, int offset, int count,
			XmlDictionaryReaderQuotas quota,
			OnXmlDictionaryReaderClose onClose,
			XmlParserContext context);
	}
}
