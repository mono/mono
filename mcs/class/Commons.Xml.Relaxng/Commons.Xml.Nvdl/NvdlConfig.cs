using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using Commons.Xml.Nvdl.Simplified;
using Commons.Xml;

namespace Commons.Xml.Nvdl
{
	public class NvdlConfig
	{
		XmlResolver resolver = new XmlUrlResolver ();
		ArrayList providers = new ArrayList ();

		public NvdlConfig ()
		{
			providers.Add (new NvdlBuiltInValidationProvider ());
		}

		public void AddProvider (NvdlValidationProvider provider)
		{
			providers.Add (provider);
		}

		internal XmlResolver XmlResolverInternal {
			get { return resolver; }
		}

		public XmlResolver XmlResolver {
			set { resolver = value; }
		}

		public NvdlValidatorGenerator GetGenerator (NvdlValidate validate, string inheritSchemaType)
		{
			this.resolver = this.XmlResolverInternal;

			string schemaType = validate.SchemaType;
			if (schemaType == null)
				schemaType = inheritSchemaType;
			if (schemaType == null)
				schemaType = "text/xml";

			foreach (NvdlValidationProvider p in providers) {
				NvdlValidatorGenerator g =
					p.CreateGenerator (validate, schemaType, this);
				if (g != null)
					return g;
			}

			throw new NvdlCompileException (String.Format ("Either schema type '{0}' or the target schema document is not supported in this configuration. Add custom provider that supports this schema type.", schemaType), validate);
		}

/*
		public virtual NvdlValidatorGenerator GetGenerator (string mimeType, string schemaAttribute, XmlElement schemaElement, NvdlValidate validate)
		{
			XmlReader schemaReader = null;
			if (schemaType == "text/xml") {
				if (schemaAttribute != null) {
					if (schemaElement != null)
						throw new NvdlCompileException ("Both 'schema' attribute and 'schema' element are specified in a 'validate' element.", validate);
				return GetSchemaXmlStream (schemaUri, config, validate);
				else if (validate.SchemaBody != null) {
					XmlReader r = new XmlNodeReader (validate.SchemaBody);
					r.MoveToContent ();
					r.Read (); // Skip "schema" element
					r.MoveToContent ();
					if (r.NodeType == XmlNodeType.Element)
						return r;
					else
						return GetSchemaXmlStream (r.ReadString (), config, validate);
				}
				else
					return null;
			}
			else
				throw new NvdlCompileException (String.Format ("Either MIME type '{0}' or the target schema document is not supported.", schemaType), validate);

			schemaReader.MoveToContent ();

			NvdlValidationProvider provider =
				ctx.Config.GetProvider (schemaReader.NamespaceURI);

			generator = provider.CreateGenerator (schemaReader, ctx.Config.XmlResolverInternal);

			foreach (NvdlOption option in validate.Options) {
				bool mustSupport = option.MustSupport != null ?
					XmlConvert.ToBoolean (option.MustSupport) : false;
				if (!generator.AddOption (option.Name,
					option.Arg) && mustSupport)
					throw new NvdlCompileException (String.Format ("Option '{0}' with argument '{1}' is not supported for schema type '{2}'.",
						option.Name, option.Arg,
						schemaType), validate);
			}
		}

		public static XmlReader GetSchemaXmlStream (string schemaUri, NvdlConfig config, NvdlValidate validate)
		{
			XmlResolver r = config.Resolver;
			if (r == null)
				return null;
			Uri uri = r.ResolveUri (null, validate.schemaUri);
			Stream stream = r.GetEntity (uri, null, typeof (Stream));
			if (stream == null)
				return null;
			XmlTextReader xtr = new XmlTextReader (uri != null ? uri.ToString () : String.Empty, stream);
			xtr.XmlResolver = r;
			xtr.MoveToContent ();
			return xtr;
		}

		private bool ElementHasElementChild (XmlElement el)
		{
			foreach (XmlNode n in el.ChildNodes)
				if (n.NodeType == XmlNodeType.Element)
					return true;
			return false;
		}
*/
	}
}
