//
// ComClassAttribute.cs
//
// Authors:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2002 Chris J Breisch
// (C) 2004 Rafael Teixeira
//

using System;

namespace Microsoft.VisualBasic {
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)] 
	sealed public class ComClassAttribute : Attribute {
		// Declarations
		private string classID;
		private string interfaceID;
		private string eventID;
		private bool interfaceShadows;

		// Constructors

		public ComClassAttribute() { }

		public ComClassAttribute(string _ClassID) { 
			classID = _ClassID;
		}

		public ComClassAttribute(string _ClassID, string _InterfaceID) { 
			classID = _ClassID;
			interfaceID = _InterfaceID;
		}

		public ComClassAttribute(string _ClassID, string _InterfaceID, string _EventID) { 
			classID = _ClassID;
			interfaceID = _InterfaceID;
			eventID = _EventID;
		}

		// Properties
		public string EventID { get { return eventID; } }

		public bool InterfaceShadows { 
			get { return interfaceShadows; } 
			set { interfaceShadows = value; } 
		}

		public string ClassID { get { return classID; } }

		public string InterfaceID { get { return interfaceID; } }
	};
}
