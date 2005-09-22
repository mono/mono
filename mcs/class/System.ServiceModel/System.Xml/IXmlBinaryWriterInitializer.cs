using System;
using System.IO;

namespace System.Xml
{
	public interface IXmlBinaryWriterInitializer
	{
		void SetOutput (Stream stream,
			IXmlDictionary dictionary,
			XmlBinaryWriterSession session,
			bool ownsStream);
	}
}
