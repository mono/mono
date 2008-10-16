using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Monodoc
{
	public class WikiStyleDocConverter
	{
		public static void Main (string [] args)
		{
			if (args.Length < 1) {
				Console.Error.WriteLine ("usage: wiki2ecma sourcefile [--full]");
				return;
			}

			bool full = args.Length > 1 && args [1] == "--full";

			bool isXml = false;
			using (Stream s = File.OpenRead (args [0])) {
				isXml = (s.ReadByte () == '<');
			}

			string text;
			if (isXml) {
				XmlDocument doc = new XmlDocument ();
				doc.Load (args [0]);
				XmlNode node = doc.SelectSingleNode ("//text");
				text = node.InnerText;
			} else {
				StreamReader sr = new StreamReader (args [0], 
					Encoding.UTF8);
				text = sr.ReadToEnd ();
			}

			// Pass the input Wiki-like content as the .ctor()
			// parameter.
			WikiStyleDocConverter p = new WikiStyleDocConverter (text);
			XmlNode result;
			if (full)
				result = p.ParseEntireDoc ();
			else
				result = p.ParseContent ();

			XmlTextWriter xw = new XmlTextWriter (Console.Out);
			xw.Formatting = Formatting.Indented;
			result.WriteTo (xw);
			xw.Close ();
		}

		string [] lines;
		int lineno = 0;
		XmlDocument doc;
		string current_member;

		public WikiStyleDocConverter (string source)
		{
			source = TransformElements (source);

			lines = source.Split ('\n');
			doc = new XmlDocument ();
			doc.AppendChild (doc.CreateElement ("root"));
		}

		private string TransformElements (string source)
		{
			XPathDocument doc = new XPathDocument (
				new StringReader ("<root>" + source + "</root>"));
			XslTransform tr = new XslTransform ();
			tr.Load ("wiki2ecmahelper.xsl");
			XmlReader reader = tr.Transform (doc, null, (XmlResolver) null);
			reader.Read (); // should consume <root> start tag

			return reader.ReadInnerXml ();
		}

		public XmlNode ParseContent ()
		{
			ProcessContent (doc.DocumentElement);
			return doc.DocumentElement;
		}

		public XmlNode ParseEntireDoc ()
		{
			XmlElement el = doc.DocumentElement;

			while (lineno < lines.Length) {
				string line = lines [lineno].Trim ();
				if (line.Length == 0) {
					lineno++;
					continue;
				}
				XmlNode node = null;
				switch (line) {
				case "=== Summary ===":
					node = ProcessTaggedContent (EditTarget.Summary);
					el.AppendChild (node);
					break;
				case "=== Remarks ===":
					node = ProcessTaggedContent (EditTarget.Remarks);
					el.AppendChild (node);
					break;
				case "=== Parameters ===":
					ProcessList (el, "param", "name");
					break;
				case "=== Exceptions ===":
					ProcessList (el, "exception", "type");
					break;
				default:
					if (StrUtil.StartsWith (line, "==")) {
						current_member = line.Substring (
							3, line.Length - 6).Trim ();
						el = doc.CreateElement ("Member");
						el.SetAttribute ("MemberName", current_member);
						doc.DocumentElement.AppendChild (el);
						lineno++;
						break;
					}
					throw MarkupError ("Unexpected line format: " + line);
				}
			}
			return doc.DocumentElement;
		}

		void ProcessList (XmlNode parent, string elemName, string defAttr)
		{
			lineno++;
			for (; lineno < lines.Length; lineno++) {
				string line = lines [lineno];
				if (line.Length == 0)
					continue;
				if (line [0] != ';')
					break;
				int idx = line.IndexOf (':');
				XmlElement el = doc.CreateElement (elemName);
				parent.AppendChild (el);
				el.SetAttribute (defAttr, line.Substring (1, idx - 1));
				ProcessSimpleLine (el, line, idx + 1);
			}
		}

		XmlNode ProcessTaggedContent (string target)
		{
			XmlElement el = doc.CreateElement (target);
			lineno++;
			ProcessContent (el);
			return el;
		}

		void ProcessContent (XmlNode container)
		{
			while (lineno < lines.Length) {
				string line = lines [lineno];
				if (line.Length == 0) {
					lineno++;
					continue;
				}

				switch (line [0]) {
				case '=':
					return;
				case '{':
					ProcessTable (container);
					break;
				case ':':
					XmlElement el = doc.CreateElement ("block");
					el.SetAttribute ("subset", "none");
					el.SetAttribute ("type", "note");
					container.AppendChild (el);
					ProcessSimple (el, true);
					break;
				default:
					el = doc.CreateElement ("para");
					container.AppendChild (el);
					ProcessSimple (el, false);
					break;
				}
			}
		}

		void ProcessTable (XmlNode container)
		{
			lineno++;
			XmlElement list = doc.CreateElement ("list");
			container.AppendChild (list);
			list.SetAttribute ("type", "table");
			XmlElement tline = null;
			for (; lineno < lines.Length; lineno++) {
				string line = lines [lineno];
				if (line == "|}") {
					lineno++;
					return;
				}

				if (line.Length == 0)
					continue;
				if (line == "|-") {
					tline = doc.CreateElement ("item");
					continue;
				}
				switch (line [0]) {
				case '!':
					tline = doc.CreateElement ("listheader");
					int endTerm = line.IndexOf ('!', 1);
					int beginDesc = endTerm < 0 ? -1 : line.IndexOf ('!', endTerm + 1);
					if (beginDesc < 0)
						throw MarkupError ("list table header has incorrect markup : " + line);
					XmlElement term = doc.CreateElement ("term");
					term.InnerText = line.Substring (1, endTerm - 1);
					tline.AppendChild (term);
					XmlElement desc = doc.CreateElement ("description");
					desc.InnerText = line.Substring (beginDesc + 1);
					tline.AppendChild (desc);
					list.AppendChild (tline);
					break;
				case '|':
					if (tline == null)
						throw MarkupError ("Specify '|-' to begin new table line");
					endTerm = line.IndexOf ('|', 1);
					if (endTerm < 0)
						throw MarkupError ("Missing list table separator '|'");
					beginDesc = endTerm < 0 ? -1 : line.IndexOf ('|', endTerm + 1);
					term = doc.CreateElement ("term");
					term.InnerText = line.Substring (1, endTerm - 1);
					tline.AppendChild (term);
					desc = doc.CreateElement ("description");
					ProcessSimpleLine (desc, line, beginDesc + 1);
					tline.AppendChild (desc);
					list.AppendChild (tline);
					break;
				}
				tline = null;
			}
			// there is already "return" statement above.
			throw MarkupError ("End of list table is missing");
		}

		void ProcessSimple (XmlNode container, bool allowColon)
		{
			for (;lineno < lines.Length; lineno++) {
				string line = lines [lineno];
				if (line.Length == 0) {
					if (lineno + 1 < lines.Length &&
					    lines [lineno + 1] == String.Empty) {
						lineno++;
						return;
					}
					continue;
				}
				switch (line [0]) {
				case '=':
				case '{':
					return;
				case ':':
					if (!allowColon)
						return;
					ProcessSimpleLine (container, line, 1);
					break;
				default:
					ProcessSimpleLine (container, line, 0);
					break;
				}
			}
		}

		void ProcessSimpleLine (XmlNode container, string line, int from)
		{
			int idx;
			while ((idx = line.IndexOf ('[', from)) >= 0) {
				if (idx + 1 < line.Length && line [idx + 1] == '[')
					from = ProcessSee (
						container, line, idx, from);
				else
					from = ProcessParamRef (
						container, line, idx, from);
			}
			if (from != line.Length)
				container.AppendChild (doc.CreateTextNode (line.Substring (from) + '\n'));
		}

		int ProcessSee (XmlNode container, string line, int idx, int from)
		{
			int end = line.IndexOf ("]]", idx);
			int sep = end < idx ? - 1: line.IndexOf ('|', idx, end - idx);
			if (sep < 0)
				throw MarkupError (String.Format ("There is no matching '|' and ']' to close link at position {1} : {0}", line, idx));
			if (idx > from) {
				XmlText text = doc.CreateTextNode (
					line.Substring (from, idx - from));
				container.AppendChild (text);
			}

			XmlElement el = doc.CreateElement ("see");
			el.SetAttribute ("cref", line.Substring (
				idx + 2, sep - idx - 2).Trim ());
			container.AppendChild (el);

			end += 2;
			return end;
		}

		int ProcessParamRef (XmlNode container, string line, int idx, int from)
		{
			int end = line.IndexOf (']', idx);
			if (end < idx)
				throw MarkupError (String.Format ("There is no matching ']' to close link at position {1} : {0}", line, idx));
			if (idx > from) {
				XmlText text = doc.CreateTextNode (
					line.Substring (from, idx - from));
				container.AppendChild (text);
			}

			XmlElement el = doc.CreateElement ("paramref");
			el.SetAttribute ("name", line.Substring (
				idx + 1, end - idx - 1));
			container.AppendChild (el);
			end += 1;
			return end;
		}

		Exception MarkupError (string message)
		{
			throw new Exception (String.Format (
				"At line {1} : {0}", message, lineno));
		}
	}

	class EditTarget
	{
		public const string Summary = "summary";
		public const string Remarks = "remarks";
	}

	class StrUtil
	{
		static CompareInfo ci = CultureInfo.CurrentCulture.CompareInfo;

		public static bool StartsWith (string s, string target)
		{
			return ci.IsPrefix (s, target, CompareOptions.Ordinal);
		}
	}
}
