//
// MSXslScriptManager.cs
//
// Author:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C)2003 Atsushi Enomoto
//
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Mono.Xml.Xsl {

	[MonoTODO ("Correct evedence handling; use JScript compiler in future versions; test other than simple string case")]
	public class MSXslScriptManager {
		Hashtable scripts = new Hashtable ();
		Evidence evidence;

		public MSXslScriptManager () {}
		
		public void AddScript (Compiler c)
		{
			MSXslScript s = new MSXslScript (c.Input, c.Evidence, scripts.Count);
			this.evidence = c.Evidence;
			scripts.Add (c.Input.GetNamespace (s.ImplementsPrefix), s.Compile ());
		}
		
		enum ScriptingLanguage {
			JScript,
			VisualBasic,
			CSharp
		}
		
		public object GetExtensionObject (string ns)
		{
			return Activator.CreateInstance ((Type) scripts [ns]);
		}

		class MSXslScript {
			ScriptingLanguage language = ScriptingLanguage.JScript; // default = JScript.
			string implementsPrefix = null;
			string code = null;
			string suffix;
			XPathNavigator node;
			Evidence evidence;

			public MSXslScript (XPathNavigator nav, Evidence evidence, int suffix)
			{
				node = nav.Clone ();
				this.evidence = evidence;
				this.suffix = suffix.ToString ();
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
			
			public object Compile ()
			{
				string suffix = Guid.NewGuid ().ToString ().Replace ("-", String.Empty);
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
			}
		}
	}
}

