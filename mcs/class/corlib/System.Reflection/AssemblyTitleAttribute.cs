//
// System.Reflection.AssemblyTitleAttribute.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

namespace System.Reflection
{
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class AssemblyTitleAttribute : Attribute
	{
		// Field
		private string name;
		
		// Constructor
		public AssemblyTitleAttribute (string title)
		{
			name = title;
		}

		// Property
		public string Title
		{
			get { return name; }
		}
	}
}
