//
// System.Reflection.AssemblyKeyFileAttribute.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

namespace System.Reflection
{
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class AssemblyKeyFileAttribute : Attribute
	{
		// Field
		private string name;
		
		// Constructor
		public AssemblyKeyFileAttribute (string keyFile)
		{
			name = keyFile;
		}

		// Property
		public string KeyFile
		{
			get { return name; }
		}
	}
}
