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
		public readonly ArrayList AttrParameters;
		
		public Attribute (string target, ArrayList attrs)
		{
			Target = target;
			AttrParameters  = attrs;
		}

	}

	public class Attributes {

		ArrayList attributes;

		public Attributes (Attribute a)
		{
			attributes = new ArrayList ();
			attributes.Add (a);

		}

		public void AddAttribute (Attribute a)
		{
			if (a != null)
				attributes.Add (a);
		}
		
	}
}
