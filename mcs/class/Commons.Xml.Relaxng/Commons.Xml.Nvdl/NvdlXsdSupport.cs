using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Schema;

namespace Commons.Xml.Nvdl
{
	public class NvdlXsdValidatorProvider : NvdlValidationProvider
	{
		public override NvdlValidatorGenerator CreateGenerator (
			XmlReader reader, NvdlConfig config)
		{
			if (reader.NamespaceURI != XmlSchema.Namespace)
				return null;
			ArrayList al = new ArrayList ();
			while (!reader.EOF) {
				XmlSchema xs = XmlSchema.Read (reader, null);
				xs.Compile (null, config.XmlResolverInternal);
				al.Add (xs);
			}
			return new NvdlXsdValidatorGenerator (al.ToArray (typeof (XmlSchema)) as XmlSchema []);
		}
	}

	internal class NvdlXsdValidatorGenerator : NvdlValidatorGenerator
	{
		XmlSchema [] schemas;

		public NvdlXsdValidatorGenerator (XmlSchema [] schemas)
		{
			this.schemas = schemas;
		}

		public override XmlReader CreateValidator (XmlReader reader,
			XmlResolver resolver)
		{
			XmlValidatingReader xvr =
				new XmlValidatingReader (reader);
			xvr.XmlResolver = resolver;
			foreach (XmlSchema schema in schemas)
				xvr.Schemas.Add (schema);

			return xvr;
		}

		public override bool AddOption (string name, string arg)
		{
			return false;
		}
	}
}
