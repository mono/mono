//
// XslElement.cs
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

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl.Operations {	
	public class XslElement : XslCompiledElement {
		XslAvt name, ns;
		string calcName, calcNs, calcPrefix;
		XmlNamespaceManager nsm;
		bool isEmptyElement;

		XslOperation value;
		XmlQualifiedName [] useAttributeSets;

		XPathNavigator nav;
		
		public XslElement (Compiler c) : base (c) {}
		protected override void Compile (Compiler c)
		{
			nav = c.Input.Clone ();

			name = c.ParseAvtAttribute ("name");
			ns = c.ParseAvtAttribute ("namespace");
			
			calcName = XslAvt.AttemptPreCalc (ref name);
			
			if (calcName != null) {
				int colonAt = calcName.IndexOf (':');
				calcPrefix = colonAt < 0 ? String.Empty : calcName.Substring (0, colonAt);
				calcName = colonAt < 0 ? calcName : calcName.Substring (colonAt + 1, calcName.Length - colonAt - 1);
				if (ns == null)
					calcNs = c.Input.GetNamespace (calcPrefix);
			} else if (ns != null)
				calcNs = XslAvt.AttemptPreCalc (ref ns);
			
			if (ns == null && calcNs == null)
				nsm = c.GetNsm ();
			
			useAttributeSets = c.ParseQNameListAttribute ("use-attribute-sets");
			
			isEmptyElement = c.Input.IsEmptyElement;

			if (c.Input.MoveToFirstChild ()) {
				value = c.CompileTemplateContent ();
				c.Input.MoveToParent ();
			}
		}
		
		public override void Evaluate (XslTransformProcessor p)
		{
			string nm, nmsp, localName, prefix;
			
			localName = nm = calcName != null ? calcName : name.Evaluate (p);
			nmsp = calcNs != null ? calcNs : ns != null ? ns.Evaluate (p) : null;

			if (nmsp == null) {
				QName q = XslNameUtil.FromString (nm, nsm);
				localName = q.Name;
				nmsp = q.Namespace;
				int colonAt = nm.IndexOf (':');
				if (colonAt > 0)
					calcPrefix = nm.Substring (0, colonAt);
			}
			prefix = calcPrefix != null ? calcPrefix : String.Empty;

#if false
			if (calcPrefix == String.Empty) {
				if (nav.MoveToFirstNamespace (XPathNamespaceScope.ExcludeXml)) {
					do {
						if (nav.Value == nmsp) {
//							prefix = nav.Name;
							break;
						}
					} while (nav.MoveToNextNamespace (XPathNamespaceScope.ExcludeXml));
					nav.MoveToParent ();
				}
			}
#endif

			XmlConvert.VerifyName (nm);

			bool isCData = p.InsideCDataElement;
			p.PushCDataState (localName, nmsp);
			p.Out.WriteStartElement (prefix, localName, nmsp);
			p.TryStylesheetNamespaceOutput (null);

			if (useAttributeSets != null)
				foreach (XmlQualifiedName s in useAttributeSets)
					p.ResolveAttributeSet (s).Evaluate (p);

			if (value != null) value.Evaluate (p);

			if (isEmptyElement && useAttributeSets == null)
				p.Out.WriteEndElement ();
			else
				p.Out.WriteFullEndElement ();
			p.PopCDataState (isCData);
		}
	}
}
