//
// System.Reflection.AssemblyInformationalVersionAttribute.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

namespace System.Reflection
{
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class AssemblyInformationalVersionAttribute : Attribute
	{
		// Field
		private string name;
		
		// Constructor
		public AssemblyInformationalVersionAttribute (string informationalVersion)
		{
			name = informationalVersion;
		}

		// Property
		public string InformationalVersion
		{
			get { return name; }
		}
	}
}
