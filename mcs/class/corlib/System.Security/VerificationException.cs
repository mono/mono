//
// System.Security.VerificationException.cs
//
// Author:
//   Nick Drochak(ndrochak@gol.com)
//
// (C) Nick Drochak
//

using System.Runtime.Serialization;
using System.Globalization;

namespace System.Security {
	[Serializable]
	public class VerificationException : SystemException {
	
		// Constructors
		public VerificationException(){}
		public VerificationException(string message) 
			: base (message){}
		protected VerificationException(SerializationInfo info, StreamingContext context) 
			: base (info, context) {}
		public VerificationException(string message, Exception inner) 
			: base (message, inner) {}
	}
}
