//
// TODO: stub provided to compile, this code has not been open sourced.
//
using System.IO;
using System.Xml;
namespace System.ServiceModel.Diagnostics {

	internal class EncodingFallbackAwareXmlTextWriter : XmlTextWriter {
		public EncodingFallbackAwareXmlTextWriter(TextWriter writer) : base(writer)
		{
		}
	}
}
