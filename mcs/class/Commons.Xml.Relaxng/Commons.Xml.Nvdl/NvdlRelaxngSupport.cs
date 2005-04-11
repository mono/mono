using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Xml;
using Commons.Xml.Relaxng;
using Commons.Xml.Relaxng.Rnc;

namespace Commons.Xml.Nvdl
{
	public class NvdlRelaxngValidatorProvider : NvdlValidationProvider
	{
		public const string RncMimeType = "application/vnd.oasis-open.relax-ng.rnc";

		public override NvdlValidatorGenerator CreateGenerator (
			NvdlValidate validate, string schemaType, NvdlConfig config)
		{
			if (schemaType == RncMimeType)
				return CreateRncGenerator (validate, config);
			return base.CreateGenerator (validate, schemaType, config);
		}

		private NvdlValidatorGenerator CreateRncGenerator (NvdlValidate validate, NvdlConfig config)
		{
			XmlResolver resolver = config.XmlResolverInternal;
			string schemaUri = GetSchemaUri (validate);
			if (schemaUri == null)
				return null;
			RelaxngPattern p = RncParser.ParseRnc (
				new StreamReader ((Stream) resolver.GetEntity (
					resolver.ResolveUri (null, schemaUri),
					null,
					typeof (Stream))));
			return new NvdlRelaxngValidatorGenerator (p, config);
		}

		public override NvdlValidatorGenerator CreateGenerator (
			XmlReader reader, NvdlConfig config)
		{
			if (reader.NamespaceURI != RelaxngGrammar.NamespaceURI)
				return null;

			return new NvdlRelaxngValidatorGenerator (RelaxngPattern.Read (reader), config);
		}
	}

	internal class NvdlRelaxngValidatorGenerator : NvdlValidatorGenerator
	{
		RelaxngPattern pattern;
		RelaxngPattern attributePattern;

		public NvdlRelaxngValidatorGenerator (RelaxngPattern p,
			NvdlConfig config)
		{
			// FIXME: use XmlResolver
			pattern = p;
		}

		public override XmlReader CreateValidator (XmlReader reader,
			XmlResolver resolver)
		{
			// XmlResolver is never used.
			RelaxngValidatingReader rvr = 
				new RelaxngValidatingReader (
					reader, pattern);
			rvr.ReportDetails = true;
			return rvr;
		}

		public override XmlReader CreateAttributeValidator (
			XmlReader reader,
			XmlResolver resolver)
		{
			if (attributePattern == null) {
				RelaxngElement el = new RelaxngElement ();
				el.NameClass = new RelaxngAnyName ();
				el.Patterns.Add (pattern);
			}
			// XmlResolver is never used.
			RelaxngValidatingReader rvr = 
				new RelaxngValidatingReader (
					reader, attributePattern);
			rvr.ReportDetails = true;
			return rvr;
		}

		public override bool AddOption (string name, string arg)
		{
			return false;
		}
	}
}
