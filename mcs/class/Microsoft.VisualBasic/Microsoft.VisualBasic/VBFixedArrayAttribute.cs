//
// VBFixedArrayAttribute.cs
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
	sealed public class VBFixedArrayAttribute : System.Attribute {
		// Declarations
		// Constructors
		[MonoTODO]
		VBFixedArrayAttribute(System.Int32 UpperBound1) { throw new NotImplementedException (); }
		[MonoTODO]
		VBFixedArrayAttribute(System.Int32 UpperBound1, System.Int32 UpperBound2) { throw new NotImplementedException (); }
		// Properties
		[MonoTODO]
		public System.Int32 Length { get { throw new NotImplementedException (); } }
		[MonoTODO]
		public System.Int32[] Bounds { get { throw new NotImplementedException (); } }
		// Methods
		// Events
	};
}
