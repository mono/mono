//
// attribute.cs: Attribute Handler
//
// Author: Ravi Pratap (ravi@ximian.com)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace CIR {

	public class Attribute {

		public readonly string    Target;
		public readonly ArrayList Attrs;
		
		public Attribute (string target, ArrayList attrs)
		{
			Target = target;
			Attrs  = attrs;
		}

	}
}
