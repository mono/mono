//
// XslAttribute.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//	
// (C) 2003 Ben Maurer
// (C) 2003 Atsushi Enomoto
//

using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.IO;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl.Operations {
	public class XslAttribute : XslCompiledElement {
		XslAvt name, ns;
		string calcName, calcNs, calcPrefix;
		XmlNamespaceManager nsm;
		
		XslOperation value;
		XPathNavigator nav;

		public XslAttribute (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			nav = c.Input.Clone ();
			
			name = c.ParseAvtAttribute ("name");
			if (name == null)
				throw new XsltCompileException ("attribute \"name\" is required on XSLT attribute element.", null, c.Input);
			ns = c.ParseAvtAttribute ("namespace");

			calcName = XslAvt.AttemptPreCalc (ref name);
			calcPrefix = String.Empty;

			if (calcName != null) {
				int colonAt = calcName.IndexOf (':');
				calcPrefix = colonAt < 0 ? String.Empty : calcName.Substring (0, colonAt);
				calcName = colonAt < 0 ? calcName : calcName.Substring (colonAt + 1, calcName.Length - colonAt - 1);

				try {
					XmlConvert.VerifyNCName (calcName);
					if (calcPrefix != String.Empty)
						XmlConvert.VerifyNCName (calcPrefix);
				} catch (XmlException ex) {
					throw new XsltCompileException ("Invalid attribute name.", ex, c.Input);
				}
			}
			if (calcPrefix != String.Empty && ns == null)
				calcNs = nav.GetNamespace (calcPrefix);
			else if (ns != null)
				calcNs = XslAvt.AttemptPreCalc (ref ns);

			if (calcNs ==null && calcPrefix != String.Empty) {
				string test = c.CurrentStylesheet.PrefixInEffect (calcPrefix, null);
				if (test != null) {
					string alias = c.CurrentStylesheet.NamespaceAliases [calcPrefix] as string;
					if (alias != null)
						calcNs = c.Input.GetNamespace (alias);
					else
						calcNs = c.Input.NamespaceURI;
				}
			}

			if (ns == null && calcNs == null)
				nsm = c.GetNsm ();
				
			if (c.Input.MoveToFirstChild ()) {
				value = c.CompileTemplateContent (XPathNodeType.Attribute);
				c.Input.MoveToParent ();
			}

		}

		public override void Evaluate (XslTransformProcessor p)
		{
			string nm, nmsp, prefix;
			
			nm = calcName != null ? calcName : name.Evaluate (p);
			nmsp = calcNs != null ? calcNs : ns != null ? ns.Evaluate (p) : String.Empty;
			prefix = calcPrefix != null ? calcPrefix : String.Empty;

			if (nm == "xmlns")
				// It is an error. We must recover by not emmiting any attributes 
				// (unless we throw an exception).
				return;

			int colonAt = nm.IndexOf (':');
			// local attribute
			if (colonAt > 0) {
				prefix = nm.Substring (0, colonAt);
				nm = nm.Substring (colonAt + 1, nm.Length - colonAt - 1);

				// global attribute
				if (nmsp == null) {
					QName q = XslNameUtil.FromString (nm, nsm);
					nm = q.Name;
					nmsp = q.Namespace;
				} else
					nm = XslNameUtil.LocalNameOf (nm);
			}

			if (nmsp != String.Empty && prefix == String.Empty) {
				if (nav.MoveToFirstNamespace (XPathNamespaceScope.ExcludeXml)) {
					do {
						if (nav.Value == nmsp) {
							prefix = nav.Name;
							break;
						}
					} while (nav.MoveToNextNamespace (XPathNamespaceScope.ExcludeXml));
					nav.MoveToParent ();
				}
			}

			if (prefix == "xmlns")
				prefix = String.Empty;	// Should not be allowed.

			XmlConvert.VerifyName (nm);

			if (value == null)
				p.Out.WriteAttributeString(prefix, nm, nmsp, "");
			else {
				StringWriter sw = new StringWriter ();
				Outputter outputter = new TextOutputter (sw, true);
				p.PushOutput (outputter);
				value.Evaluate (p);			    
				p.PopOutput ();
				outputter.Done ();			        
				p.Out.WriteAttributeString (prefix, nm, nmsp, sw.ToString ());			                    			        
			}						
		}
	}
}
