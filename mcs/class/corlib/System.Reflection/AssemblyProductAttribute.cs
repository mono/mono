//
// System.Reflection.AssemblyProductAttribute.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

namespace System.Reflection
{
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class AssemblyProductAttribute : Attribute
	{
		// Field
		private string name;
		
		// Constructor
		public AssemblyProductAttribute (string product)
		{
			name = product;
		}

		// Property
		public string Product
		{
			get { return name; }
		}
	}
}
