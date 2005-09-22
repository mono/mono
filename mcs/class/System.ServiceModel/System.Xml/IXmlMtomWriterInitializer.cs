using System;
using System.IO;
using System.Text;

namespace System.Xml
{
	public interface IXmlMtomWriterInitializer
	{
		void SetOutput (Stream stream, Encoding encoding,
			int maxSizeInBytes, string startInfo, string boundary,
			string startUri, bool writeMessageHeaders);
	}
}
