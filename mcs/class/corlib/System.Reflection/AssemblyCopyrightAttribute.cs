//
// System.Reflection.AssemblyCopyrightAttribute.cs
//
// Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

using System;

namespace System.Reflection
{
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class AssemblyCopyrightAttribute : Attribute
	{
		// Field
		private string name;
		
		// Constructor
		public AssemblyCopyrightAttribute (string copyright)
		{
			name = copyright;
		}
		
		// Properties
		public string Copyright
		{
			get { return name; }
		}
	}
}
