//
// System.Reflection.AssemblyDefaultAliasAttribute.cs
//
// Author: Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

using System;

namespace System.Reflection
{
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class AssemblyDefaultAliasAttribute : Attribute
	{
		// Field
		private string name;
		
		// Constructor
		public AssemblyDefaultAliasAttribute (string defaultAlias)
		{
			name = defaultAlias;
		}
		
		// Properties
		public string DefaultAlias
		{
			get { return name; }
		}
	}
}
