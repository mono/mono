//
// System.Reflection.AssemblyDescriptionAttribute.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

namespace System.Reflection
{
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class AssemblyDescriptionAttribute : Attribute
	{
		// Field
		private string name;
		
		// Constructor
		public AssemblyDescriptionAttribute (string description)
		{
			name = description;
		}

		// Property
		public string Description
		{
			get { return name; }
		}
	}
		
}
