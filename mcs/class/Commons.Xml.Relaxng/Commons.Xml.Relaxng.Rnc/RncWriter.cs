//
// RELAX NG Compact Syntax writer
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2005 Novell Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.IO;
using System.Xml;
using Commons.Xml.Relaxng;

#if NET_2_0
using NSResolver = System.Xml.IXmlNamespaceResolver;
#else
using NSResolver = System.Xml.XmlNamespaceManager;
#endif

namespace Commons.Xml.Relaxng.Rnc
{
	internal class RncWriter
	{
		static readonly XmlNamespaceManager defaultNamespaceManager;

		static RncWriter ()
		{
			XmlNamespaceManager n = new XmlNamespaceManager (
				new NameTable ());
			n.AddNamespace ("xs", "http://www.w3.org/2001/XMLSchema-datatypes");
			n.AddNamespace ("xsi", "http://www.w3.org/2001/XMLSchema-instance");
			defaultNamespaceManager = n;
		}

		TextWriter w;
		NSResolver nsmgr;

		public RncWriter (TextWriter writer)
			: this (writer, defaultNamespaceManager)
		{
		}

		public RncWriter (TextWriter writer, NSResolver nsmgr)
		{
			this.w = writer;
			this.nsmgr = nsmgr;
		}

		#region Utility methods

		private void WriteNames (RelaxngNameClassList l, bool wrap)
		{
			switch (l.Count) {
			case 0:
				throw new RelaxngException ("name choice must contain at least one name class.");
			case 1:
				l [0].WriteRnc (this);
				break;
			default:
				if (wrap)
					w.Write ('(');
				l [0].WriteRnc (this);
				for (int i = 1; i < l.Count; i++) {
					w.Write ('|');
					l [i].WriteRnc (this);
				}
				if (wrap)
					w.Write (')');
				break;
			}
		}

		private void WritePatterns (RelaxngPatternList l, bool parens)
		{
			WritePatterns (l, ',', parens);
		}

		private void WritePatterns (RelaxngPatternList l,
			char sep, bool parens)
		{
			switch (l.Count) {
			case 0:
				w.Write ("empty");
				break;
			case 1:
				if (parens)
					w.Write ('(');
				l [0].WriteRnc (this);
				if (parens)
					w.Write (')');
				break;
			default:
				if (parens)
					w.Write ('(');
				l [0].WriteRnc (this);
				for (int i = 1; i < l.Count; i++) {
					w.Write (sep);
					w.Write (' ');
					l [i].WriteRnc (this);
				}
				if (parens)
					w.Write (')');
				break;
			}
		}

		private void WriteGrammarIncludeContents (
			RelaxngGrammarContentList starts,
			RelaxngGrammarContentList defines,
			RelaxngGrammarContentList divs,
			RelaxngGrammarContentList includes)
		{
			if (includes != null)
				foreach (RelaxngInclude inc in includes)
					inc.WriteRnc (this);
			if (divs != null)
				foreach (RelaxngDiv div in divs)
					div.WriteRnc (this);
			if (starts != null)
				foreach (RelaxngStart s in starts)
					s.WriteRnc (this);
			if (defines != null)
				foreach (RelaxngDefine def in defines)
					def.WriteRnc (this);
		}

		private void WriteQName (string name, string ns)
		{
			string prefix = String.Empty;
			if (ns != null && ns != String.Empty) {
#if NET_2_0
#else
				// XmlNamespaceManager sucks.
				ns = nsmgr.NameTable.Add (ns);
#endif
				prefix = nsmgr.LookupPrefix (ns);
			}
			if (prefix == null)
				throw new RelaxngException (String.Format ("Namespace '{0}' is not mapped to a prefix in argument XmlNamespaceManager.", ns));
			if (prefix != String.Empty) {
				w.Write (prefix);
				w.Write (':');
			}
			w.Write (name);
		}

		private void WriteLiteral (string value)
		{
			w.Write ('"');
			for (int i = 0; i < value.Length; i++) {
				switch (value [i]) {
				case '"':
					w.Write ("\\x{22}");
					break;
				case '\r':
					w.Write ("\\x{13}");
					break;
				case '\n':
					w.Write ("\\x{10}");
					break;
				case '\t': // It is not required, but would be better.
					w.Write ("\\x{9}");
					break;
				default:
					w.Write (value [i]);
					break;
				}
			}
			w.Write ('"');
		}

		#endregion

		#region Elements
		// Note that it might not be used directly when a grammar
		// contains more than one "start" (compact syntax does not
		// support "combine" attribute).
		public void WriteStart (RelaxngStart start)
		{
			w.Write ("start");
			if (start.Combine == null)
				w.Write (" = ");
			else
				w.Write (start.Combine.Trim () == "interleave" ?
					" &= " : " |= ");
			start.Pattern.WriteRnc (this);
			w.WriteLine ();
		}

		// Note that it might not be used directly when a grammar
		// contains more than one "define" for an identical name
		// (compact syntax does not support "combine" attribute).
		public void WriteDefine (RelaxngDefine define)
		{
			w.Write (define.Name);
			if (define.Combine == null)
				w.Write (" = ");
			else
				w.Write (define.Combine.Trim () == "interleave" ?
					" &= " : " |= ");
			if (define.Patterns.Count == 0)
				w.Write ("empty");
			else {
				define.Patterns [0].WriteRnc (this);
				for (int i = 1; i < define.Patterns.Count; i++) {
					w.Write (",");
					define.Patterns [i].WriteRnc (this);
				}
			}
			w.WriteLine ();
		}

		public void WriteInclude (RelaxngInclude include)
		{
			w.Write ("include ");
			w.Write (include.Href);

			// FIXME: optInherit?

			if (include.Starts.Count > 0 ||
				include.Defines.Count > 0 ||
				include.Divs.Count > 0) {
				w.Write ('(');
				WriteGrammarIncludeContents (include.Starts,
					include.Defines, include.Divs, null);
				w.Write (')');
			}
			w.WriteLine ();
		}

		public void WriteDiv (RelaxngDiv div)
		{
			w.Write ("div { ");
			WriteGrammarIncludeContents (div.Starts,
				div.Defines, div.Divs, div.Includes);
			w.WriteLine ('}');
		}

		public void WriteNotAllowed (RelaxngNotAllowed na)
		{
			w.Write ("notAllowed ");
		}

		public void WriteEmpty (RelaxngEmpty empty)
		{
			w.Write ("empty ");
		}

		public void WriteText (RelaxngText text)
		{
			w.Write ("text ");
		}

		public void WriteData (RelaxngData data)
		{
			WriteQName (data.Type, data.DatatypeLibrary);
			w.Write (' ');
			if (data.ParamList.Count > 0) {
				w.Write ('{');
				foreach (RelaxngParam p in data.ParamList)
					p.WriteRnc (this);
				w.Write ("} ");
			}
			if (data.Except != null)
				data.Except.WriteRnc (this);
		}

		public void WriteValue (RelaxngValue v)
		{
			WriteQName (v.Type, v.DatatypeLibrary);
			w.Write (' ');
			WriteLiteral (v.Value);
			w.Write (' ');
		}

		public void WriteList (RelaxngList p)
		{
			w.Write ("list { ");
			WritePatterns (p.Patterns, false);
			w.Write ("} ");
		}

		public void WriteMixed (RelaxngMixed p)
		{
			w.Write ("mixed { ");
			WritePatterns (p.Patterns, false);
			w.Write ("} ");
		}

		public void WriteElement (RelaxngElement element)
		{
			w.Write ("element ");
			element.NameClass.WriteRnc (this);
			w.Write (" { ");
			WritePatterns (element.Patterns, false);
			w.Write ("} ");
		}

		public void WriteAttribute (RelaxngAttribute attribute)
		{
			w.Write ("attribute ");
			attribute.NameClass.WriteRnc (this);
			w.Write (" { ");
			if (attribute.Pattern == null)
				w.Write ("empty");
			else
				attribute.Pattern.WriteRnc (this);
			w.Write (" } ");
		}

		public void WriteRef (RelaxngRef r)
		{
			w.Write (r.Name);
			w.Write (' ');
		}

		public void WriteParentRef (RelaxngParentRef r)
		{
			w.Write ("parent ");
			w.Write (r.Name);
			w.Write (' ');
		}

		public void WriteExternalRef (RelaxngExternalRef r)
		{
			w.Write ("external ");
			w.Write (r.Href);
			// FIXME: optInherit?
			w.Write (' ');
		}

		public void WriteOneOrMore (RelaxngOneOrMore p)
		{
			WritePatterns (p.Patterns, true);
			w.Write ('+');
		}

		public void WriteZeroOrMore (RelaxngZeroOrMore p)
		{
			WritePatterns (p.Patterns, true);
			w.Write ('*');
		}

		public void WriteOptional (RelaxngOptional p)
		{
			WritePatterns (p.Patterns, true);
			w.Write ('?');
		}

		public void WriteChoice (RelaxngChoice p)
		{
			WritePatterns (p.Patterns, '|', false);
		}

		public void WriteGroup (RelaxngGroup p)
		{
			WritePatterns (p.Patterns, ',', false);
		}

		public void WriteInterleave (RelaxngInterleave p)
		{
			WritePatterns (p.Patterns, '&', false);
		}

		public void WriteParam (RelaxngParam p)
		{
			w.Write (p.Name);
			w.Write (" = ");
			WriteLiteral (p.Value);
		}

		public void WriteDataExcept (RelaxngExcept e)
		{
			w.Write ("- ");
			WritePatterns (e.Patterns, true);
		}

		public void WriteGrammar (RelaxngGrammar g)
		{
			w.WriteLine ("grammar {");
			WriteGrammarIncludeContents (g.Starts,
				g.Defines, g.Divs, g.Includes);
			w.WriteLine ('}');
		}

		public void WriteAnyName (RelaxngAnyName n)
		{
			w.Write ('*');
			if (n.Except != null)
				n.Except.WriteRnc (this);
		}

		public void WriteNsName (RelaxngNsName n)
		{
			WriteQName ("*", n.Namespace);
			if (n.Except != null)
				n.Except.WriteRnc (this);
		}

		public void WriteName (RelaxngName n)
		{
			WriteQName (n.LocalName, n.Namespace);
		}

		public void WriteNameChoice (RelaxngNameChoice c)
		{
			WriteNames (c.Children, false);
		}

		public void WriteNameExcept (RelaxngExceptNameClass e)
		{
			w.Write ("- ");
			WriteNames (e.Names, true);
		}
		#endregion
	}
}
