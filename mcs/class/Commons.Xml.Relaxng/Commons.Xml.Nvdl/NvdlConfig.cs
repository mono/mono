using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
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
				schemaType = "application/xml";

			foreach (NvdlValidationProvider p in providers) {
				NvdlValidatorGenerator g =
					p.CreateGenerator (validate, schemaType, this);
				if (g != null)
					return g;
			}

			throw new NvdlCompileException (String.Format ("Either schema type '{0}' or the target schema document is not supported in this configuration. Add custom provider that supports this schema type.", schemaType), validate);
		}
	}
}
