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

//
// Copyright (c) 2002-2003 Mainsoft Corporation.
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
	}
}
