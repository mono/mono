//
// XslCompiledElement.cs
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

	public abstract class XslCompiledElement : XslOperation {
		bool hasStack;
		int stackSize;
		XPathNavigator nav;
		IXmlLineInfo li;
		
		public XslCompiledElement (Compiler c)
		{
			nav = c.Input.Clone ();
			li = nav as IXmlLineInfo;
			this.Compile (c);
		}
		
		protected abstract void Compile (Compiler c);

		internal XPathNavigator InputNode {
			get { return nav; }
		}

		public int LineNumber {
			get { return li != null ? li.LineNumber : 0; }
		}

		public int LinePosition {
			get { return li != null ? li.LinePosition : 0; }
		}
	}
}
