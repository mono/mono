//
// MSXslScriptManager.cs
//
// Author:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C)2003 Atsushi Enomoto
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
using System.Diagnostics;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Mono.Xml.Xsl {

	// FIXME: Correct evidence handling; test other than simple string case
	internal class MSXslScriptManager {
		Hashtable scripts = new Hashtable ();

		public MSXslScriptManager () {}
		
		public void AddScript (Compiler c)
		{
			MSXslScript s = new MSXslScript (c.Input, c.Evidence);
			string ns = c.Input.GetNamespace (s.ImplementsPrefix);
			if (ns == null)
				throw new XsltCompileException ("Specified prefix for msxsl:script was not found: " + s.ImplementsPrefix, null, c.Input);
			scripts.Add (ns, s.Compile (c.Input));
		}
		
		enum ScriptingLanguage {
			JScript,
			VisualBasic,
			CSharp
		}
		
		public object GetExtensionObject (string ns)
		{
			if (!scripts.ContainsKey (ns))
				return null;
			return Activator.CreateInstance ((Type) scripts [ns]);
		}

		class MSXslScript {
			ScriptingLanguage language = ScriptingLanguage.JScript; // default = JScript.
			string implementsPrefix = null;
			string code = null;
			Evidence evidence;

			public MSXslScript (XPathNavigator nav, Evidence evidence)
			{
				this.evidence = evidence;
				code = nav.Value;
				if (nav.MoveToFirstAttribute ()) {
					do {
						switch (nav.LocalName) {
						case "language":
							switch (nav.Value.ToLower (CultureInfo.InvariantCulture)) {
							case "jscript":
							case "javascript":
								language = ScriptingLanguage.JScript; break;
							case "vb":
							case "visualbasic":
								language = ScriptingLanguage.VisualBasic;
								break;
							case "c#":
							case "csharp":
								language = ScriptingLanguage.CSharp;
								break;
							default:
								throw new XsltException ("Invalid scripting language!", null);
							}
							break;
						case "implements-prefix":
							implementsPrefix = nav.Value;
							break;
						}
					} while (nav.MoveToNextAttribute ());
					nav.MoveToParent ();
				}
				
				if (implementsPrefix == null)
					throw new XsltException ("need implements-prefix attr", null);
			}
	
			public ScriptingLanguage Language {
				get { return language; }
			}
	
			public string ImplementsPrefix {
				get { return implementsPrefix; }
			}
	
			public string Code {
				get { return code; }
			}
			
			public object Compile (XPathNavigator node)
			{
#if TARGET_JVM || MOBILE
				throw new NotImplementedException ();
#else
				string suffix = "";
				foreach (byte b in MD5.Create ().ComputeHash (Encoding.Unicode.GetBytes (code))) {
					suffix += b.ToString ("x2");
				}
				switch (this.language) {
				case ScriptingLanguage.CSharp:
					return new CSharpCompilerInfo ().GetScriptClass (Code, suffix, node, evidence);
				case ScriptingLanguage.JScript:
					return new JScriptCompilerInfo ().GetScriptClass (Code, suffix, node, evidence);
				case ScriptingLanguage.VisualBasic:
					return new VBCompilerInfo ().GetScriptClass (Code, suffix, node, evidence);
				default:
					return null;
				}
#endif
			}
		}
	}
}

