//
// System.Reflection.AssemblyCompanyAttribute.cs
//
// Author: Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

using System;

namespace System.Reflection
{
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class AssemblyCompanyAttribute : Attribute
	{
		// Field
		private string name;
		
		// Constructor
		public AssemblyCompanyAttribute (string company)
		{
			name = company;
		}
		
		// Properties
		public string Company
		{
			get { return name; }
		}
	}
}
