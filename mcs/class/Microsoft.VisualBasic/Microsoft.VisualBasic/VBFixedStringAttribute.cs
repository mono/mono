//
// VBFixedStringAttribute.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//
// (C) 2002 Chris J Breisch
//

using System;

namespace Microsoft.VisualBasic 
{
	[System.AttributeUsageAttribute(System.AttributeTargets.Field)] 
	sealed public class VBFixedStringAttribute : System.Attribute {
		// Declarations
		// Constructors
		[MonoTODO]
		VBFixedStringAttribute(System.Int32 Length) { throw new NotImplementedException (); }
		// Properties
		[MonoTODO]
		public System.Int32 Length { get { throw new NotImplementedException (); } }
		// Methods
		// Events
	};
}
