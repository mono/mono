using System;
using System.IO;

namespace System.Xml
{
	public interface IXmlUTF8WriterInitializer
	{
		void SetOutput (Stream stream, bool ownsStream);
	}
}
