//
// VBFixedStringAttribute.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2002 Chris J Breisch
// (C) 2004 Rafael Teixeira
//

using System;

namespace Microsoft.VisualBasic 
{
	[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)] 
	sealed public class VBFixedStringAttribute : Attribute {

		// Declarations
		private int length;

		// Constructors
		public VBFixedStringAttribute(int Length) { length = Length; }

		// Properties
		public int Length { get { return length; } }

	};
}
