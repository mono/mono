//
// System.Security.XmlSyntaxException.cs
//
// Author:
//   Nick Drochak(ndrochak@gol.com)
//
// (C) Nick Drochak
//

using System.Globalization;

namespace System.Security {
	[Serializable]
	public sealed class XmlSyntaxException : SystemException {
	
		// Constructors
		public XmlSyntaxException(){}
		public XmlSyntaxException(int lineNumber)
			: base (Locale.GetText("Invalid syntax on line ") + lineNumber.ToString() + "."){}
		public XmlSyntaxException(int lineNumber, string message)
			: base (Locale.GetText("Invalid syntax on line ") + lineNumber.ToString() + " - " + message ){}
		public XmlSyntaxException(string message) 
			: base (message){}
		public XmlSyntaxException(string message, Exception inner) 
			: base (message, inner) {}
	}
}
