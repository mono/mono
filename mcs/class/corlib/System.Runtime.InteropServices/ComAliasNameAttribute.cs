//
// System.Runtime.InteropServices.ComAliasNameAttribute.cs
//
// Name: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Runtime.InteropServices {
	[AttributeUsage (AttributeTargets.Property | AttributeTargets.Field |
			 AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
	public sealed class ComAliasNameAttribute : Attribute
	{
		string val;
		
		public ComAliasNameAttribute (string alias)
		{
			val = alias;
		}

		public string Value {
			get { return val; }
		}
	}
}
