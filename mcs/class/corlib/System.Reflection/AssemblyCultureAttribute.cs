//
// System.Reflection.AssemblyCultureAttribute.cs
//
// Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

using System;

namespace System.Reflection
{
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class AssemblyCultureAttribute : Attribute
	{
		// Field
		private string name;
		
		// Constructor
		public AssemblyCultureAttribute (string culture)
		{
			name = culture;
		}
		
		// Properties
		public string Culture
		{
			get { return name; }
		}
	}
}
