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
		int lineNumber;
		int linePosition;
		XPathNodeType parentType;
		
		public XslCompiledElement (Compiler c) : this (c, XPathNodeType.Root)
		{
		}

		public XslCompiledElement (Compiler c, XPathNodeType parentType)
		{
			this.parentType = parentType;
			IXmlLineInfo li = c.Input as IXmlLineInfo;
			if (li != null) {
				lineNumber = li.LineNumber;
				linePosition = li.LinePosition;
			}
			this.Compile (c);
		}
		
		protected abstract void Compile (Compiler c);

		public int LineNumber {
			get { return lineNumber; }
		}

		public int LinePosition {
			get { return linePosition; }
		}

		public XPathNodeType ParentType {
			get { return parentType; }
		}
	}
}
