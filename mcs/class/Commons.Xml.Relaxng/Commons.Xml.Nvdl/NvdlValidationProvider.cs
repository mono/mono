using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Xml;

namespace Commons.Xml.Nvdl
{
	public class NvdlValidationProvider
	{
		NvdlValidate validate;
		string schema_type;
		NvdlConfig config;

		public virtual NvdlValidatorGenerator CreateGenerator (NvdlValidate validate, string schemaType, NvdlConfig config)
		{
			this.validate = validate;
			this.schema_type = schemaType;
			this.config = config;

			XmlReader schema = null;
			// FIXME: we need a bit more strict check.
			if (schemaType.Length < 5 ||
				!schemaType.EndsWith ("xml") ||
				Char.IsLetter (schemaType, schemaType.Length - 4))
				return null;

			string schemaUri = validate.SchemaUri;
			XmlElement schemaBody = validate.SchemaBody;

			if (schemaUri != null) {
				if (schemaBody != null)
					throw new NvdlCompileException ("Both 'schema' attribute and 'schema' element are specified in a 'validate' element.", validate);
				schema = GetSchemaXmlStream (schemaUri, config, validate);
			}
			else if (validate.SchemaBody != null) {
				XmlReader r = new XmlNodeReader (schemaBody);
				r.MoveToContent ();
				r.Read (); // Skip "schema" element
				r.MoveToContent ();
				if (r.NodeType == XmlNodeType.Element)
					schema = r;
				else
					schema = GetSchemaXmlStream (r.ReadString (), config, validate);
			}

			if (schema == null)
				return null;

			return CreateGenerator (schema, config);
		}

		public NvdlValidate ValidateAction {
			get { return validate; }
		}

		public NvdlConfig Config {
			get { return config; }
		}

		public string SchemaType {
			get { return schema_type; }
		}

		public virtual NvdlValidatorGenerator CreateGenerator (XmlReader schema, NvdlConfig config)
		{
			return null;
		}

		public string GetSchemaUri (NvdlValidate validate)
		{
			if (validate.SchemaUri != null)
				return validate.SchemaUri;
			if (validate.SchemaBody == null)
				return null;
			for (XmlNode n = validate.SchemaBody.FirstChild; n != null; n = n.NextSibling)
				if (n.NodeType == XmlNodeType.Element)
					return null; // not a URI
			return validate.SchemaBody.InnerText;
		}

		private static XmlReader GetSchemaXmlStream (string schemaUri, NvdlConfig config, NvdlValidate validate)
		{
			XmlResolver r = config.XmlResolverInternal;
			if (r == null)
				return null;
			Uri baseUri = r.ResolveUri (null, validate.SourceUri);
			Uri uri = r.ResolveUri (baseUri, validate.SchemaUri);
			Stream stream = (Stream) r.GetEntity (
				uri, null, typeof (Stream));
			if (stream == null)
				return null;
			XmlTextReader xtr = new XmlTextReader (uri != null ? uri.ToString () : String.Empty, stream);
			xtr.XmlResolver = r;
			xtr.MoveToContent ();
			return xtr;
		}
	}

	public abstract class NvdlValidatorGenerator
	{
		// creates individual validator with schema
		// (which should be provided in derived constructor).
		public abstract XmlReader CreateValidator (XmlReader reader, 
			XmlResolver resolver);

		public virtual XmlReader CreateAttributeValidator (
			XmlReader reader,
			XmlResolver resolver)
		{
			throw new NotSupportedException ();
		}

		public abstract bool AddOption (string name, string arg);

		public virtual bool HandleError (Exception ex, XmlReader reader, string nvdlLocation)
		{
			return false;
		}
	}
}
