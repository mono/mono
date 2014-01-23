using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Collections.Generic;

namespace Monodoc.Generators.Html
{
	public class Ecmaspec2Html : IHtmlExporter
	{
		static string css_ecmaspec;
		static XslTransform ecma_transform;
		static XsltArgumentList args = new XsltArgumentList();

		public string CssCode {
			get {
				if (css_ecmaspec != null)
					return css_ecmaspec;
				System.Reflection.Assembly assembly = System.Reflection.Assembly.GetCallingAssembly ();
				Stream str_css = assembly.GetManifestResourceStream ("ecmaspec.css");
				css_ecmaspec = (new StreamReader (str_css)).ReadToEnd ();
				return css_ecmaspec;
			}
		}

		class ExtObj
		{
			public string Colorize (string code, string lang)
			{
				return Mono.Utilities.Colorizer.Colorize (code, lang);
			}
		}

		public string Export (Stream stream, Dictionary<string, string> extraArgs)
		{
			return Htmlize (new XPathDocument (stream));
		}

		public string Export (string input, Dictionary<string, string> extraArgs)
		{
			return Htmlize (new XPathDocument (new StringReader (input)));
		}

		static string Htmlize (XPathDocument ecma_xml)
		{
			if (ecma_transform == null){
				ecma_transform = new XslTransform ();
				System.Reflection.Assembly assembly = System.Reflection.Assembly.GetCallingAssembly ();
				Stream stream;
				stream = assembly.GetManifestResourceStream ("ecmaspec-html-css.xsl");

				XmlReader xml_reader = new XmlTextReader (stream);
				ecma_transform.Load (xml_reader, null, null);
				args.AddExtensionObject ("monodoc:///extensions", new ExtObj ()); 
			}
		
			if (ecma_xml == null) return "";

			StringWriter output = new StringWriter ();
			ecma_transform.Transform (ecma_xml, args, output, null);
		
			return output.ToString ();
		}
	}
}
