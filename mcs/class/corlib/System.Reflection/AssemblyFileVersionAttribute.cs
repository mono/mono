//
// System.Reflection.AssemblyFileVersionAttribute.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

namespace System.Reflection
{
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class AssemblyFileVersionAttribute : Attribute
	{
		// Field
		private string name;
		
		// Constructor
		public AssemblyFileVersionAttribute (string version)
		{
			if (version == null)
				throw new ArgumentNullException ("version");

			name = version;
		}

		// Property
		public string Version
		{
			get { return name; }
		}
	}
}
