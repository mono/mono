using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using Commons.Xml;

namespace Commons.Xml.Nvdl
{
	public class NvdlBuiltInValidationProvider : NvdlValidationProvider
	{
		public NvdlBuiltInValidationProvider ()
		{
		}

		public override NvdlValidatorGenerator CreateGenerator (
			XmlReader reader, NvdlConfig config)
		{
			reader.MoveToContent ();
			if (reader.NodeType != XmlNodeType.Element ||
				reader.NamespaceURI != Nvdl.BuiltInValidationNamespace)
				return null;
			return new NvdlBuiltInValidatorGenerator (reader.LocalName == "allow");
		}
	}

	internal class NvdlBuiltInValidatorGenerator : NvdlValidatorGenerator
	{
		bool allow;

		public NvdlBuiltInValidatorGenerator (bool allow)
		{
			this.allow = allow;
		}

		public override XmlReader CreateValidator (XmlReader reader, XmlResolver resolver)
		{
			return new NvdlBuiltInValidationReader (reader, allow);
		}

		public override bool AddOption (string name, string arg)
		{
			return false;
		}
	}

	internal class NvdlBuiltInValidationReader : XmlDefaultReader
	{
		bool allow;

		public NvdlBuiltInValidationReader (XmlReader reader, bool allow)
			: base (reader)
		{
			this.allow = allow;
		}

		public override bool Read ()
		{
			if (!Reader.Read ())
				return false;
			if (!allow)
				throw new NvdlValidationException (String.Format ("The NVDL script does not allow an element whose namespace is '{0}'", Reader.NamespaceURI), Reader as IXmlLineInfo);
			return true;
		}
	}
}
