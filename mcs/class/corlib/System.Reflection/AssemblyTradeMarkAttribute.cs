//
// System.Reflection.AssemblyTrademarkAttribute.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

namespace System.Reflection
{
	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class AssemblyTrademarkAttribute : Attribute
	{
		// Field
		private string name;
		
		// Constructor
		public AssemblyTrademarkAttribute (string trademark)
		{
			name = trademark;
		}

		// Property
		public string Trademark
		{
			get { return name; }
		}
	}
}
