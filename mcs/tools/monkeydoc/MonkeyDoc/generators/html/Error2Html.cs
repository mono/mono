using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using System.Collections.Generic;

namespace Monodoc.Generators.Html
{
	public class Error2Html : IHtmlExporter
	{
		public string Export (string input, Dictionary<string, string> extraArgs)
		{
			return Htmlize (new XPathDocument (new StringReader (input)));
		}

		public string Export (Stream input, Dictionary<string, string> extraArgs)
		{
			return Htmlize (new XPathDocument (input));
		}

		public string CssCode {
			get {
				return @"
					 #error_ref { 
					    background: #debcb0; 
					    border: 2px solid #782609; 
					 }
					 div.summary {
						 font-size: 110%;
						 font-weight: bolder;
					 }
					 div.details {
						 font-size: 110%;
						 font-weight: bolder;
					 }
					 div.code_example {
						background: #f5f5dd;
						border: 1px solid black;
						padding-left: 1em;
						padding-bottom: 1em;
						margin-top: 1em;
						white-space: pre;
						margin-bottom: 1em;
					 }
					 div.code_ex_title {
						position: relative;
						top: -1em;
						left: 30%;
						background: #cdcd82;
						border: 1px solid black;
						color: black;
						font-size: 65%;
						text-transform: uppercase;
						width: 40%;
						padding: 0.3em;
						text-align: center;
					 }";
			}
		}

		public string Htmlize (IXPathNavigable doc)
		{
			var navigator = doc.CreateNavigator ();
			var errorName = navigator.SelectSingleNode ("//ErrorDocumentation/ErrorName");
			var details = navigator.SelectSingleNode ("//ErrorDocumentation/Details");

			StringWriter sw = new StringWriter ();
			XmlWriter w = new XmlTextWriter (sw);
			
			WriteElementWithClass (w, "div", "header");
			w.WriteAttributeString ("id", "error_ref");
			WriteElementWithClass (w, "div", "subtitle", "Compiler Error Reference");
			WriteElementWithClass (w, "div", "title", "Error " + (errorName == null ? string.Empty : errorName.Value));
			w.WriteEndElement ();

			if (details != null) {
				WriteElementWithClass (w, "div", "summary", "Summary");

				var summary = details.SelectSingleNode ("/Summary");
				w.WriteValue (summary == null ? string.Empty : summary.Value);
				
				WriteElementWithClass (w, "div", "details", "Details");
				var de = details.SelectSingleNode ("/Details");
				w.WriteValue (de == null ? string.Empty : de.Value);
			}
			
			foreach (XPathNavigator xmp in navigator.Select ("//ErrorDocumentation/Examples/string")) {
				WriteElementWithClass (w, "div", "code_example");
				WriteElementWithClass (w, "div", "code_ex_title", "Example");
				w.WriteRaw (Mono.Utilities.Colorizer.Colorize (xmp.Value, "c#"));;
				w.WriteEndElement ();
			}
			
			w.Close ();
			
			return sw.ToString ();
		}

		void WriteElementWithClass (XmlWriter w, string element, string cls, string content = null)
		{
			w.WriteStartElement (element);
			w.WriteAttributeString ("class", cls);
			if (!string.IsNullOrEmpty (content)) {
				w.WriteValue (content);
				w.WriteEndElement ();
			}
		}
	}
}
