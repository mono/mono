//
// XslApplyImports.cs
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

namespace Mono.Xml.Xsl.Operations {
	public class XslApplyImports : XslCompiledElement {
		ArrayList withParams;
		public XslApplyImports (Compiler c) : base (c) {}
		protected override void Compile (Compiler c)
		{
		}
		
		public override void Evaluate (XslTransformProcessor p)
		{
			p.ApplyImports ();
		}
	}
}
