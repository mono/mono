//
// ComClassAttribute.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//
// (C) 2002 Chris J Breisch
//

using System;

namespace Microsoft.VisualBasic {
	[System.AttributeUsageAttribute(System.AttributeTargets.Class)] 
	sealed public class ComClassAttribute : System.Attribute {
		// Declarations
		// Constructors
		[MonoTODO]
		ComClassAttribute(System.String _ClassID) { throw new NotImplementedException (); }
		[MonoTODO]
		ComClassAttribute(System.String _ClassID, System.String _InterfaceID) { throw new NotImplementedException (); }
		[MonoTODO]
		ComClassAttribute(System.String _ClassID, System.String _InterfaceID, System.String _EventId) { throw new NotImplementedException (); }
		// Properties
		[MonoTODO]
		public System.String EventID { get { throw new NotImplementedException (); } }
		[MonoTODO]
		public System.Boolean InterfaceShadows { get { throw new NotImplementedException (); } }
		[MonoTODO]
		public System.String ClassID { get { throw new NotImplementedException (); } }
		[MonoTODO]
		public System.String InterfaceID { get { throw new NotImplementedException (); } }
		// Methods
		// Events
	};
}
