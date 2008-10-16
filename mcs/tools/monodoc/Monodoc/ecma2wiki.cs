
using System;
using System.IO;
using System.Xml;
using Commons.Xml.Relaxng;
using Commons.Xml.Relaxng.Rnc;

namespace Monodoc
{
	public class Monodoc2Wiki
	{
		public static int sMain (string [] args)
		{
			if (args.Length < 1) {
				Console.WriteLine ("Usage: monodoc2wiki monodoc_xmlfile");
				return 1;
			}

			XmlDocument doc = new XmlDocument ();
#if VALIDATION
			XmlTextReader xr = new XmlTextReader (args [0]);
			RelaxngPattern rp = RncParser.ParseRnc (new StreamReader ("CLILibraryTypes.rnc"));
Console.Error.WriteLine ("**** READY ****");
rp.Compile ();
Console.Error.WriteLine ("**** DONE ****");
			RelaxngValidatingReader rvr = new RelaxngValidatingReader (xr, rp);
			doc.Load (rvr);
			rvr.Close ();
#else
			doc.Load (args [0]);
#endif
			Monodoc2Wiki instance = new Monodoc2Wiki ();
			string ret = instance.ProcessNode (doc.DocumentElement);

			Console.WriteLine (ret);

			return 0;
		}

		//StringWriter Output;
		//XmlWriter XmlOutput;
		WikiOutput Output, XmlOutput;

		class WikiOutput
		{
			StringWriter raw;
			XmlWriter output;

			public WikiOutput ()
			{
				raw = new StringWriter ();
				output = new XmlTextWriter (raw);
			}

			public void Write (string value, params object [] args)
			{
				output.WriteString (String.Format (value, args));
			}

			public void WriteLine ()
			{
				output.WriteString ("\n"); // better than Environment.NewLine
			}

			public void WriteLine (string value, params object [] args)
			{
				Write (value, args);
				WriteLine ();
			}

			public void WriteNodeTo (XmlNode node)
			{
				node.WriteTo (output);
			}

			public void WriteNodeContentTo (XmlNode node)
			{
				node.WriteContentTo (output);
			}

			public void Flush ()
			{
				output.Flush ();
			}

			public override string ToString ()
			{
				output.Flush ();
				return raw.ToString ();
			}

			public XmlWriter Writer {
				get { return output; }
			}
		}

		public string ProcessNode (XmlElement elem)
		{
			//Output = new StringWriter ();
			//XmlOutput = new XmlTextWriter (Output);
			Output = XmlOutput = new WikiOutput ();

			switch (elem.Name) {
			case "Type":
				ProcessEntireType (elem);
				break;

			case "summary":
				ProcessSummary (elem);
				break;
			case "remarks":
				ProcessRemarks (elem);
				break;
			}

			return Output.ToString ().Replace ("\r", "");
		}

		private Exception MarkupError (string msg, XmlElement el)
		{
			return new ApplicationException (msg);
		}

		private string Sanitize (string source)
		{
			string [] arr = source.Split ('\n');
			for (int i = 0; i < arr.Length; i++)
				arr [i] = arr [i].Trim ();
			return String.Join (" ", arr);
		}

		private void ProcessEntireType (XmlElement elem)
		{
			ProcessDocs (elem.SelectSingleNode ("Docs") as XmlElement);
			foreach (XmlElement member in elem.SelectNodes ("Members/Member"))
				ProcessMember (member);
		}

		private void ProcessDocs (XmlElement elem)
		{
			if (elem == null)
				return;
			ProcessSummary (elem.SelectSingleNode ("summary") as XmlElement);
			if (elem.SelectSingleNode ("param") != null) {
				Output.WriteLine ("=== Parameters ===");
				foreach (XmlElement p in elem.SelectNodes ("param"))
					ProcessParam (p);
				Output.WriteLine ();
			}
			if (elem.SelectSingleNode ("exception") != null) {
				Output.WriteLine ("=== Exceptions ===");
				foreach (XmlElement e in elem.SelectNodes ("exception"))
					ProcessException (e);
				Output.WriteLine ();
			}
			ProcessRemarks (elem.SelectSingleNode ("remarks") as XmlElement);
		}

		private void ProcessMember (XmlElement elem)
		{
			Output.Write ("== ");
			Output.Write (elem.GetAttribute ("MemberName"));
			XmlNode parameters = elem.SelectSingleNode ("Parameters");
			string memberType = elem.SelectSingleNode ("MemberType").InnerText;
			switch (memberType) {
			case "Method":
			case "Contrcutor":
				Output.Write ("(");
				break;
			}
			if (parameters != null && parameters ["Parameter"] != null) {
				if (memberType == "Property")
					Output.Write ("[");
				bool first = true;
				foreach (XmlElement p in parameters.SelectNodes ("Parameter")) {
					if (!first)
						Output.Write (", ");
					Output.Write (p.GetAttribute ("Type"));
					first = false;
				}
				if (memberType == "Property")
					Output.Write ("]");
			}
			switch (memberType) {
			case "Method":
			case "Contrcutor":
				Output.Write (")");
				break;
			}

			Output.WriteLine (" ==");

			ProcessDocs (elem.SelectSingleNode ("Docs") as XmlElement);
		}

		private void ProcessSummary (XmlElement elem)
		{
			if (elem == null)
				return;
			Output.WriteLine ("=== Summary ===");
			ProcessContents (elem);
			Output.WriteLine ();
		}

		private void ProcessRemarks (XmlElement elem)
		{
			if (elem == null)
				return;
			Output.WriteLine ("=== Remarks ===");
			ProcessContents (elem);
			Output.WriteLine ();
		}

		private void ProcessException (XmlElement elem)
		{
			Output.WriteLine ();
			Output.Write (";{0}:", elem.GetAttribute ("cref").Substring (2));
			ProcessInlineContents (elem);
			Output.WriteLine ();
		}

		private void ProcessParam (XmlElement elem)
		{
			Output.WriteLine ();
			Output.Write (";{0}:", elem.GetAttribute ("name"));
			ProcessInlineContents (elem);
			Output.WriteLine ();
		}

		// Template for editable contents

		private void ProcessContents (XmlElement container)
		{
			foreach (XmlNode n in container.ChildNodes) {
				switch (n.NodeType) {
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
				case XmlNodeType.SignificantWhitespace:
					Output.Write (Sanitize (n.Value.Trim ()));
					break;
				}

				XmlElement elem = n as XmlElement;
				if (elem != null)
					ProcessBlockElement (elem);
			}
		}

		private void ProcessBlockElement (XmlElement elem)
		{
			switch (elem.Name) {
			case "block":
				ProcessBlock (elem);
				break;
			case "list":
				string listType = elem.GetAttribute ("type");
				switch (listType) {
				case "bullet":
					ProcessSimpleList (elem, "*");
					break;
				case "number":
					ProcessSimpleList (elem, "#");
					break;
				case "table":
					ProcessTable (elem);
					break;
				default:
					throw MarkupError (String.Format ("Unrecognized list type : {0}", listType), elem);
				}
				break;
			case "pre":
				ProcessAsIs (elem);
				break;
			case "para":
				ProcessPara (elem);
				break;
			case "code":
				ProcessCode (elem);
				break;
			default:
				ProcessInlineElement (elem);
				break;
			}
		}

		// Inline contents

		private void ProcessParamRef (XmlElement elem)
		{
			Output.Write (" [{0}] ", elem.GetAttribute ("name"));
		}

		private void ProcessXref (XmlElement elem)
		{
			string cref = elem.GetAttribute ("cref");
			string langword = elem.GetAttribute ("langword");
			if (cref.Length > 0)
				Output.Write (" [[{0} | {1}]] ", cref,
					cref.Substring (2));
			else if (langword.Length > 0)
				Output.Write (" " + langword + " ");
		}

		private void ProcessInlineContents (XmlElement container)
		{
			foreach (XmlNode n in container.ChildNodes) {
				switch (n.NodeType) {
				case XmlNodeType.Text:
				case XmlNodeType.CDATA:
				case XmlNodeType.SignificantWhitespace:
					Output.Write (Sanitize (n.Value.Trim ()));
					break;
				}

				XmlElement elem = n as XmlElement;
				if (elem != null)
					// actually it should be ProcessInlineContents(), but many existing documents break the rule :(
//					ProcessInlineElement (elem);
					ProcessBlockElement (elem);
			}
		}

		private void ProcessC (XmlElement elem)
		{
			Output.Write ("<code>");
			ProcessInlineContents (elem);
			Output.Write ("</code>");
		}

		private void ProcessInlineElement (XmlElement elem)
		{
			switch (elem.Name) {
			case "c":
				ProcessC (elem);
				break;
			case "block": // Actually it is always handled as block; never becomes inline
				ProcessBlock (elem);
				break;
			case "paramref":
				ProcessParamRef (elem);
				break;
			case "see":
				ProcessXref (elem);
				break;
			case "example":
				ProcessAsDiv (elem, "example");
				break;
			case "SPAN":
			case "sup":
			case "sub":
			case "superscript":
			case "subscript":
			case "permille":
			case "pi":
			case "onequarter":
				ProcessAsIs (elem);
				break;
			default:
				throw MarkupError (String.Format ("Unrecognized content element: {0}", elem.Name), elem);
			}
		}

		private void ProcessAsIs (XmlElement elem)
		{
			Output.Flush ();
			Output.WriteNodeTo (elem);
			XmlOutput.Flush ();
		}

		private void ProcessAsDiv (XmlElement elem, string name)
		{
			Output.Flush ();
			XmlOutput.Writer.WriteStartElement ("div");
			XmlOutput.Writer.WriteAttributeString ("class", name);
			Output.WriteNodeContentTo (elem);
			XmlOutput.Writer.WriteEndElement ();
			XmlOutput.Flush ();
		}

		private void ProcessPara (XmlElement elem)
		{
			Output.WriteLine ();
			ProcessInlineContents (elem);
			Output.WriteLine ();
		}

		private void ProcessCode (XmlElement elem)
		{
			string lang = elem.GetAttribute ("lang");
			if (lang == "c#")
				Output.WriteLine ("<csharp>");
			else
				Output.WriteLine ("<pre class='{0}'>", lang);
			ProcessContents (elem);
			if (lang == "c#")
				Output.WriteLine ("</csharp>");
			else
				Output.WriteLine ("</pre>", lang);
		}

		private void ProcessBlock (XmlElement elem)
		{
			string subset = elem.GetAttribute ("subset");
			string type = elem.GetAttribute ("type");
			if (subset == "none") {
				switch (type) {
				case "note":
					Output.WriteLine ();
					Output.Write (":");
					ProcessInlineContents (elem);
					Output.WriteLine ();
					return;
				case "behaviors":
					ProcessAsDiv (elem, "behaviors");
					return;
				case "default":
					ProcessAsDiv (elem, "default");
					return;
				case "example":
					ProcessAsDiv (elem, "example-block");
					return;
				case "overrides":
					ProcessAsDiv (elem, "overrides");
					return;
				case "usage":
					ProcessAsDiv (elem, "usage");
					return;
				}
			}

			throw MarkupError (String.Format ("Unrecognized block element: subset is {0} and type is {1}", subset, type), elem);
		}

		/* It expects that all table cells contain at most one content
		private void ProcessTable (XmlElement elem)
		{
			Output.WriteLine ();
			Output.WriteLine ("{| border='1' cellspacing='2'");
			XmlElement h = elem.SelectSingleNode ("listheader") as XmlElement;
			if (h != null) {
				Output.Write ("! ");
				ProcessInlineContents (h.SelectSingleNode ("term"));
				foreach (XmlElement desc in h.SelectNodes ("description")) {
					Output.Write (" !! ");
					ProcessInlineContents (desc);
				}
				Output.WriteLine ();
			}
			foreach (XmlElement item in elem.SelectNodes ("item")) {
				Output.WriteLine ("|-");
				Output.Write ("| ");
				ProcessInlineContents (item.SelectSingleNode ("term"));
				foreach (XmlElement desc in item.SelectNodes ("description")) {
					Output.Write (" || ");
					ProcessInlineContents (desc);
				}
				Output.WriteLine ();
			}
			Output.WriteLine ("|}");
			Output.WriteLine ();
		}
		*/

		private void ProcessSimpleList (XmlElement elem, string type)
		{
			Output.WriteLine ();
			foreach (XmlElement item in elem.SelectNodes ("item")) {
				Output.Write (type);
				Output.Write (" ");
				ProcessInlineContents (item.SelectSingleNode ("term") as XmlElement);
				Output.WriteLine ();
			}
			Output.WriteLine ();
		}

		private void ProcessTable (XmlElement elem)
		{
			// Since mediawiki does not support multi line tables
			// we have to use <table> tags here.
			Output.WriteLine ();
			Output.WriteLine ("<table border='1' cellspacing='2'>");
			XmlElement h = elem.SelectSingleNode ("listheader") as XmlElement;
			if (h != null) {
				Output.Write ("<tr><th>");
				ProcessInlineContents (h.SelectSingleNode ("term") as XmlElement);
				Output.WriteLine ("</th>");
				foreach (XmlElement desc in h.SelectNodes ("description")) {
					Output.Write ("<th>");
					ProcessContents (desc);
					Output.WriteLine ("</th>");
				}
				Output.WriteLine ("</tr>");
			}
			foreach (XmlElement item in elem.SelectNodes ("item")) {
				Output.Write ("<tr><td>");
				ProcessInlineContents (item.SelectSingleNode ("term") as XmlElement);
				Output.Write ("</td>");
				foreach (XmlElement desc in item.SelectNodes ("description")) {
					Output.Write ("<td>");
					ProcessContents (desc);
					Output.WriteLine ("</td>");
				}
				Output.WriteLine ("</tr>");
			}
			Output.WriteLine ("</table>");
			Output.WriteLine ();
		}
	}
}
