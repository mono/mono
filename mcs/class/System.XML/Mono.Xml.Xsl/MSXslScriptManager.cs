//
// MSXslScriptManager.cs
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C)2003 Atsushi Enomoto
//
using System;
using System.CodeDom;
using System.Collections;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Mono.Xml.Xsl {

	public class MSXslScriptManager {
		ArrayList jsScripts = new ArrayList ();
		ArrayList vbScripts = new ArrayList ();
		ArrayList csScripts = new ArrayList ();
		
		public MSXslScriptManager () {}
		
		public void AddScript (XPathNavigator nav)
		{
			MSXslScript s = new MSXslScript (nav);
			switch (s.Language) {
				case ScriptingLanguage.JScript:
					jsScripts.Add (s); break;
				case ScriptingLanguage.VisualBasic:
					vbScripts.Add (s); break;
				case ScriptingLanguage.CSharp:
					csScripts.Add (s); break;
			}
		}
		
		enum ScriptingLanguage {
			JScript,
			VisualBasic,
			CSharp
		}
		
		class MSXslScript {
			ScriptingLanguage language = ScriptingLanguage.JScript;  // i think this is the default
			string implementsPrefix = null;
			string code = null;
			
			public MSXslScript (XPathNavigator nav)
			{
				code = nav.Value;
				if (nav.MoveToFirstAttribute ()) {
					do {
						switch (nav.LocalName) {
						case "language":	
							switch (nav.Value.ToLower (CultureInfo.InvariantCulture)) {
							case "jscript": case "javascript":
								language = ScriptingLanguage.JScript; break;
							case "vb": case "visualbasic":
								language = ScriptingLanguage.VisualBasic; break;
							case "c#": case "csharp":
								language = ScriptingLanguage.CSharp; break;
							default:
								throw new XsltException ("Invalid scripting language!", null);
							}
							break;
						case "implements-prefix":
							implementsPrefix = nav.Value;
							break;
						}
				
					} while (nav.MoveToNextAttribute ());
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
		}
	}
}
