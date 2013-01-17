using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Collections.Generic;

using Mono.Documentation;
using BF = System.Reflection.BindingFlags;

namespace Monodoc.Generators.Html
{
	public class Ecma2Html : IHtmlExporter
	{
		static string css_ecma;
		static string js;
		static XslCompiledTransform ecma_transform;
		readonly ExtensionObject ExtObject = new ExtensionObject ();

		public Ecma2Html ()
		{
		}

		public string CssCode {
			get {
				if (css_ecma != null)
					return css_ecma;
				var assembly = typeof(Ecma2Html).Assembly;
				Stream str_css = assembly.GetManifestResourceStream ("mono-ecma.css");
				css_ecma = (new StreamReader (str_css)).ReadToEnd();
				return css_ecma;
			}
		}

		public string JsCode {
			get {
				if (js != null)
					return js;
				var assembly = typeof(Ecma2Html).Assembly;
				Stream str_js = assembly.GetManifestResourceStream ("helper.js");
				js = (new StreamReader (str_js)).ReadToEnd();
				return js;
			}
		}
		
		public string Htmlize (XmlReader ecma_xml, Dictionary<string, string> extraArgs)
		{
			var args = new XsltArgumentList ();
			args.AddExtensionObject("monodoc:///extensions", ExtObject);
			foreach (var kvp in extraArgs)
				args.AddParam (kvp.Key, string.Empty, kvp.Value);

			return Htmlize(ecma_xml, args);
		}

		public string Htmlize (XmlReader ecma_xml, XsltArgumentList args)
		{
			EnsureTransform ();
		
			var output = new StringBuilder ();
			ecma_transform.Transform (ecma_xml, 
			                          args, 
			                          XmlWriter.Create (output, ecma_transform.OutputSettings),
			                          CreateDocumentResolver ());
			return output.ToString ();
		}
		
		protected virtual XmlResolver CreateDocumentResolver ()
		{
			// results in using XmlUrlResolver
			return null;
		}

		public string Export (Stream stream, Dictionary<string, string> extraArgs)
		{
			return Htmlize (XmlReader.Create (stream), extraArgs);
		}

		public string Export (string input, Dictionary<string, string> extraArgs)
		{
			return Htmlize (XmlReader.Create (new StringReader (input)), extraArgs);
		}
		
		static void EnsureTransform ()
		{
			if (ecma_transform == null) {
				ecma_transform = new XslCompiledTransform ();
				var assembly = System.Reflection.Assembly.GetCallingAssembly ();
			
				Stream stream = assembly.GetManifestResourceStream ("mono-ecma-css.xsl");
				XmlReader xml_reader = new XmlTextReader (stream);
				XmlResolver r = new ManifestResourceResolver (".");
				ecma_transform.Load (xml_reader, XsltSettings.TrustedXslt, r);			
			}
		}

		public class ExtensionObject
		{
			bool quiet = true;

			public string Colorize(string code, string lang)
			{
				return Mono.Utilities.Colorizer.Colorize(code,lang);
			}

			// Used by stylesheet to nicely reformat the <see cref=> tags. 
			public string MakeNiceSignature(string sig, string contexttype)
			{
				if (sig.Length < 3)
					return sig;
				if (sig[1] != ':')
					return sig;

				char s = sig[0];
				sig = sig.Substring(2);
			
				switch (s) {
				case 'N': return sig;
				case 'T': return ShortTypeName (sig, contexttype);

				case 'C': case 'M': case 'P': case 'F': case 'E':
					string type, mem, arg;
					
					// Get arguments
					int paren;
					if (s == 'C' || s == 'M')
						paren = sig.IndexOf("(");
					else if (s == 'P')
						paren = sig.IndexOf("[");
					else
						paren = 0;
					
					if (paren > 0 && paren < sig.Length-1) {
						string[] args = sig.Substring(paren+1, sig.Length-paren-2).Split(',');						
						for (int i = 0; i < args.Length; i++)
							args[i] = ShortTypeName(args[i], contexttype);
						arg = "(" + String.Join(", ", args) + ")";
						sig = sig.Substring(0, paren); 
					} else {
						arg = string.Empty;
					}

					// Get type and member names
					int dot = sig.LastIndexOf(".");
					if (s == 'C' || dot <= 0 || dot == sig.Length-1) {
						mem = string.Empty;
						type = sig;
					} else {
						type = sig.Substring(0, dot);
						mem = sig.Substring(dot);
					}
						
					type = ShortTypeName(type, contexttype);
					
					return type + mem + arg;

				default:
					return sig;
				}
			}

			static string ShortTypeName(string name, string contexttype)
			{
				int dot = contexttype.LastIndexOf(".");
				if (dot < 0) return name;
				string contextns = contexttype.Substring(0, dot+1);

				if (name == contexttype)
					return name.Substring(dot+1);
			
				if (name.StartsWith(contextns))
					return name.Substring(contextns.Length);
			
				return name.Replace("+", ".");
			}

			string MonoImpInfo(string assemblyname, string typename, string membername, string arglist, bool strlong)
			{
				if (quiet)
					return string.Empty;
				
				var a = new List<string> ();
				if (!string.IsNullOrEmpty (arglist)) a.Add (arglist);
				return MonoImpInfo(assemblyname, typename, membername, a, strlong);
			}

			string MonoImpInfo(string assemblyname, string typename, string membername, XPathNodeIterator itr, bool strlong)
			{
				if (quiet)
					return string.Empty;
				
				var rgs = itr.Cast<XPathNavigator> ().Select (nav => nav.Value).ToList ();
			
				return MonoImpInfo (assemblyname, typename, membername, rgs, strlong);
			}
		
			string MonoImpInfo(string assemblyname, string typename, string membername, List<string> arglist, bool strlong)
			{
				try {
					System.Reflection.Assembly assembly = null;
				
					try {
						assembly = System.Reflection.Assembly.LoadWithPartialName(assemblyname);
					} catch (Exception) {
						// nothing.
					}
				
					if (assembly == null) {
						/*if (strlong) return "The assembly " + assemblyname + " is not available to MonoDoc.";
						  else return string.Empty;*/
						return string.Empty; // silently ignore
					}

					Type t = assembly.GetType(typename, false);
					if (t == null) {
						if (strlong)
							return typename + " has not been implemented.";
						else
							return "Not implemented.";
					}

					// The following code is flakey and fails to find existing members
					return string.Empty;
				} catch (Exception) {
					return string.Empty;
				}
			}
		
			string MonoImpInfo(System.Reflection.MemberInfo mi, string itemtype, bool strlong)
			{
				if (quiet)
					return string.Empty;
				
				string s = string.Empty;

				object[] atts = mi.GetCustomAttributes(true);
				int todoctr = 0;
				foreach (object att in atts) if (att.GetType().Name == "MonoTODOAttribute") todoctr++;

				if (todoctr > 0) {
					if (strlong)
						s = "This " + itemtype + " is marked as being unfinished.<BR/>\n";
					else 
						s = "Unfinished.";
				}

				return s;
			}

			public string MonoImpInfo(string assemblyname, string typename, bool strlong)
			{
				if (quiet)
					return string.Empty;
				
				try {
					if (assemblyname == string.Empty)
						return string.Empty;

					var assembly = System.Reflection.Assembly.LoadWithPartialName(assemblyname);
					if (assembly == null)
						return string.Empty;

					Type t = assembly.GetType(typename, false);
					if (t == null) {
						if (strlong)
							return typename + " has not been implemented.";
						else
							return "Not implemented.";
					}

					string s = MonoImpInfo(t, "type", strlong);

					if (strlong) {
						var mis = t.GetMembers (BF.Static | BF.Instance | BF.Public | BF.NonPublic);

						// Scan members for MonoTODO attributes
						int mctr = 0;
						foreach (var mi in mis) {
							string mii = MonoImpInfo(mi, null, false);
							if (mii != string.Empty) mctr++; 
						}
						if (mctr > 0) {
							s += "This type has " + mctr + " members that are marked as unfinished.<BR/>";
						}
					}

					return s;

				} catch (Exception) {
					return string.Empty;
				}			
			}

			public bool MonoEditing ()
			{
				return false;
			}
		
			public bool IsToBeAdded(string text)
			{
				return text.StartsWith ("To be added");
			}
		}
	}
}
