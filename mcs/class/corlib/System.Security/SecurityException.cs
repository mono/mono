//
// System.Security.SecurityException.cs
//
// Author:
//   Nick Drochak(ndrochak@gol.com)
//
// (C) Nick Drochak
//

using System.Runtime.Serialization;
using System.Globalization;

namespace System.Security {
	public class SecurityException : Exception {
	
		string permissionState;
		Type permissionType;

		public string PermissionState {get { return permissionState; } }
		public Type PermissionType {get { return permissionType; } }

		// Constructors
		public SecurityException(){}
		public SecurityException(string message) 
			: base (message){}
		protected SecurityException(SerializationInfo info, StreamingContext context) 
			: base (info, context) {}
		public SecurityException(string message, Exception inner) 
			: base (message, inner) {}
		public SecurityException(string message, Type type) 
			:  base (message) 
		{
			permissionType = type;
		}
		public SecurityException(string message, Type type, string state) 
			: base (message) 
		{
			permissionType = type;
			permissionState = state;
		}

	}
}