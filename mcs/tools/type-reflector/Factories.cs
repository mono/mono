//
// Factory.cs: Declares finder and formatter policy factories
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

using System;
using System.Collections;
using System.Diagnostics;
using System.Text;

namespace Mono.TypeReflector
{
	public sealed class Factories
	{
		public sealed class FinderTypeFactory : TypeFactory
		{
			public new INodeFinder Create (object key)
			{
				return (INodeFinder) base.Create (key);
			}
		}

		public sealed class FormatterTypeFactory : TypeFactory
		{
			public new INodeFormatter Create (object key)
			{
				return (INodeFormatter) base.Create (key);
			}
		}

		public static FinderTypeFactory FinderFactory = new FinderTypeFactory ();
		public static FormatterTypeFactory FormatterFactory = new FormatterTypeFactory ();
	}
}

