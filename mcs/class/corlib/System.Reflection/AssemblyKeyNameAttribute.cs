//
// System.Reflection.AssemblyKeyNameAttribute.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

namespace System.Reflection
{
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class AssemblyKeyNameAttribute : Attribute
	{
		// Field
		private string name;
		
		// Constructor
		public AssemblyKeyNameAttribute (string keyName)
		{
			name = keyName;
		}

		// Property
		public string KeyName
		{
			get { return name; }
		}
	}
}
