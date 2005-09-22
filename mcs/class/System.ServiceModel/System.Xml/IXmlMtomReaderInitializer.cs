using System;
using System.IO;
using System.Text;

namespace System.Xml
{
	public interface IXmlMtomReaderInitializer
	{
		void SetInput (Stream stream, Encoding [] encodings,
			string contentType, OnXmlDictionaryReaderClose onClose,
			XmlParserContext context);

		void SetInput (byte [] buffer, int offset, int count, Encoding [] encodings,
			string contentType, OnXmlDictionaryReaderClose onClose,
			XmlParserContext context);
	}
}
