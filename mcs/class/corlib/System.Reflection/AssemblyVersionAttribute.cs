//
// System.Reflection.AssemblyVersionAttribute.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

namespace System.Reflection
{
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class AssemblyVersionAttribute : Attribute
	{
		// Field
		private string name;
		
		// Constructor
		public AssemblyVersionAttribute (string version)
		{
			name = version;
		}

		// Property
		public string Version
		{
			get { return name; }
		}
	}
}
