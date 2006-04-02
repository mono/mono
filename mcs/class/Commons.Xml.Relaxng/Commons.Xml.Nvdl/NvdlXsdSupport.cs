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
				if (reader.NodeType != XmlNodeType.Element) {
					reader.Read ();
					continue;
				}
				reader.MoveToContent ();
				XmlSchema xs = XmlSchema.Read (reader, null);
				xs.Compile (null, config.XmlResolverInternal);
				al.Add (xs);
				reader.Read ();
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
#if NET_2_0
			XmlReaderSettings s = new XmlReaderSettings ();
			s.ValidationType = ValidationType.Schema;
			// do not allow inline schema and schemaLocation.
			s.ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints;
			s.XmlResolver = resolver;
			foreach (XmlSchema schema in schemas)
				s.Schemas.Add (schema);
			return XmlReader.Create (reader, s);
#else
			XmlValidatingReader xvr =
				new XmlValidatingReader (reader);
			xvr.XmlResolver = resolver;
			foreach (XmlSchema schema in schemas)
				xvr.Schemas.Add (schema);
			return xvr;
#endif
		}

		public override bool AddOption (string name, string arg)
		{
			return false;
		}

		public override bool HandleError (Exception ex, XmlReader reader, string nvdlLocation)
		{
			if (ex is XmlSchemaException)
				throw new NvdlInstanceValidationException (String.Format ("XML schema validation error occured as a part of NVDL validation."), ex, this, nvdlLocation);
			return false;
		}
	}
}
