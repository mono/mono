using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Monodoc;
using Monodoc.Generators;

namespace Monodoc.Generators.Html
{
	public class Man2Html : IHtmlExporter
	{
		public string CssCode {
			get {
				return string.Empty;
			}
		}

		public string Export (Stream input, Dictionary<string, string> extraArgs)
		{
			if (input == null)
				return null;
			return GetTextFromReader (new StreamReader (input));
		}

		public string Export (string input, Dictionary<string, string> extraArgs)
		{
			if (string.IsNullOrEmpty (input))
				return null;
			return GetTextFromReader (new StringReader (input));
		}

		public static string GetTextFromReader (TextReader file)
		{
			string line;
			StateInfo s = new StateInfo ();

			while ((line = file.ReadLine ()) != null)
				ProcessLine (line, s);

			return s.output.ToString ();
		}

		enum ListState {
			None,
			Start,
			Title,
		}

		class StateInfo {
			public ListState ls;
			public Stack<string> tags = new Stack<string> ();
			public StringBuilder output = new StringBuilder ();
		}

		static void ProcessLine (string line, StateInfo s)
		{
			string[] parts = SplitLine (line);
			switch (parts [0]) {
			case ".\\\"": // comments
			case ".de":   // define macro
			case ".if":   // if
			case ".ne":   // ???
			case "..":    // end macro
				// ignore
				break;
			case ".I":
				s.output.Append ("<i>");
				Translate (parts, 1, s.output);
				s.output.Append ("</i>");
				break;
			case ".B":
				s.output.Append ("<b>");
				Translate (parts, 1, s.output);
				s.output.Append ("</b>");
				break;
			case ".br":
				Translate (parts, 1, s.output);
				s.output.Append ("<br />");
				break;
			case ".nf":
				Expect (s, "</p>");
				s.output.Append ("<pre>\n");
				s.tags.Push ("</pre>");
				break;
			case ".fi":
				Expect (s, "</pre>");
				break;
			case ".PP":
				Expect (s, "</p>", "</dd>", "</dl>");
				goto case ".Sp";
			case ".Sp":
				Expect (s, "</p>");
				s.output.Append ("<p>");
				Translate (parts, 1, s.output);
				s.tags.Push ("</p>");
				break;
			case ".RS":
				Expect (s, "</p>");
				s.output.Append ("<blockquote>");
				s.tags.Push ("</blockquote>");
				break;
			case ".RE":
				ClearUntil (s, "</blockquote>");
				break;
			case ".SH":
				ClearAll (s);
				s.output.Append ("<h2>");
				Translate (parts, 1, s.output);
				s.output.Append ("</h2>")
					.Append ("<blockquote>");
				s.tags.Push ("</blockquote>");
				break;
			case ".SS":
				s.output.Append ("<h3>");
				Translate (parts, 1, s.output);
				s.output.Append ("</h3>");
				break;
			case ".TH": {
				ClearAll (s);
				string name = "", extra = "";
				if (parts.Length >= 4 && parts [2].Trim ().Length == 0) {
					name = parts [1] + "(" + parts [3] + ")";
					if (parts.Length > 4) {
						int start = 4;
						if (parts [start].Trim ().Length == 0)
							++start;
						extra = string.Join ("", parts, start, parts.Length-start);
					}
				}
				else
					name = string.Join ("", parts, 1, parts.Length-1);
				s.output.Append ("<table width=\"100%\" bgcolor=\"#b0c4da\">" + 
				                 "<tr colspan=\"2\"><td>Manual Pages</td></tr>\n" +
				                 "<tr><td><h3>");
				Translate (name, s.output);
				s.output.Append ("</h3></td><td align=\"right\">");
				Translate (extra, s.output);
				s.output.Append ("</td></tr></table>");
				break;
			}
			case ".TP":
				Expect (s, "</p>");
				if (s.tags.Count > 0 && s.tags.Peek ().ToString () != "</dd>") {
					s.output.Append ("<dl>");
					s.tags.Push ("</dl>");
				}
				else
					Expect (s, "</dd>");
				s.output.Append ("<dt>");
				s.tags.Push ("</dt>");
				s.ls = ListState.Start;
				break;
			default:
				Translate (line, s.output);
				break;
			}
			if (s.ls == ListState.Start)
				s.ls = ListState.Title;
			else if (s.ls == ListState.Title) {
				Expect (s, "</dt>");
				s.output.Append ("<dd>");
				s.tags.Push ("</dd>");
				s.ls = ListState.None;
			}
			s.output.Append ("\n");
		}

		static string[] SplitLine (string line)
		{
			if (line.Length > 1 && line [0] != '.')
				return new string[]{null, line};

			int i;
			for (i = 0; i < line.Length; ++i) {
				if (char.IsWhiteSpace (line, i))
					break;
			}

			if (i == line.Length)
				return new string[]{line};

			var pieces = new List<string> ();
			pieces.Add (line.Substring (0, i));
			bool inQuotes = false;
			bool prevWs   = true;
			++i;
			int start = i;
			for ( ; i < line.Length; ++i) {
				char c = line [i];
				if (inQuotes) {
					if (c == '"') {
						Add (pieces, line, start, i);
						start = i+1;
						inQuotes = false;
					}
				}
				else {
					if (prevWs && c == '"') {
						Add (pieces, line, start, i);
						start = i+1;
						inQuotes = true;
					}
					else if (char.IsWhiteSpace (c)) {
						if (!prevWs) {
							Add (pieces, line, start, i);
							start = i;
						}
						prevWs = true;
					}
					else {
						if (prevWs) {
							Add (pieces, line, start, i);
							start = i;
						}
						prevWs = false;
					}
				}
			}
			if (start > 0 && start != line.Length)
				pieces.Add (line.Substring (start, line.Length-start));
			return pieces.ToArray ();
		}

		static void Add (List<string> pieces, string line, int start, int end)
		{
			if (start == end)
				return;
			pieces.Add (line.Substring (start, end-start));
		}

		static void Expect (StateInfo s, params string[] expected)
		{
			string e;
			while (s.tags.Count > 0 && 
			       Array.IndexOf (expected, (e = s.tags.Peek ().ToString ())) >= 0) {
				s.output.Append (s.tags.Pop ().ToString ());
			}
		}

		static void ClearUntil (StateInfo s, string required)
		{
			string e;
			while (s.tags.Count > 0 && 
			       (e = s.tags.Peek ().ToString ()) != required) {
				s.output.Append (s.tags.Pop ().ToString ());
			}
			if (e == required)
				s.output.Append (s.tags.Pop ().ToString ());
		}

		static void ClearAll (StateInfo s)
		{
			while (s.tags.Count > 0)
				s.output.Append (s.tags.Pop ().ToString ());
		}

		static void Translate (string[] lines, int startIndex, StringBuilder output)
		{
			if (lines.Length <= startIndex)
				return;
			do {
				Translate (lines [startIndex++], output);
				if (startIndex == lines.Length)
					break;
			} while (startIndex < lines.Length);
		}

		static void Translate (string line, StringBuilder output)
		{
			string span = null;
			int start = output.Length;
			for (int i = 0; i < line.Length; ++i) {
				switch (line [i]) {
				case '\\': {
					if ((i+2) < line.Length && line [i+1] == 'f') {
						if (line [i+2] == 'I') {
							output.Append ("<i>");
							span = "</i>";
						}
						else if (line [i+2] == 'B') {
							output.Append ("<b>");
							span = "</b>";
						}
						else if (line [i+2] == 'R' || line [i+2] == 'P') {
							output.Append (span);
						}
						else
							goto default;
						i += 2;
					}
					else if ((i+1) < line.Length) {
						output.Append (line [i+1]);
						++i;
					}
					else
						goto default;
					break;
				}
				case '<':
					output.Append ("&lt;");
					break;
				case '>':
					output.Append ("&gt;");
					break;
				case '&':
					output.Append ("&amp;");
					break;
				default:
					output.Append (line [i]);
					break;
				}
			}
		}
	}
}
