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

		// Fields
		string permissionState;
		Type permissionType;

		// Properties
		public string PermissionState
		{
			get { return permissionState; }
		}

		public Type PermissionType
		{
			get { return permissionType; }
		}

		// Constructors
		public SecurityException ()
			: base (Locale.GetText ("A security error has been detected."))
		{
		}

		public SecurityException (string message) 
			: base (message)
		{
		}
		
		protected SecurityException (SerializationInfo info, StreamingContext context) 
			: base (info, context)
		{
			permissionState = info.GetString ("permissionState");
		}
		
		public SecurityException (string message, Exception inner) 
			: base (message, inner)
		{
		}
		
		public SecurityException (string message, Type type) 
			:  base (message) 
		{
			permissionType = type;
		}
		
		public SecurityException (string message, Type type, string state) 
			: base (message) 
		{
			permissionType = type;
			permissionState = state;
		}

		// Methods
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("PermissionState", permissionState);
		}

		public override string ToString ()
		{
			return permissionType.FullName + ": " + permissionState;
		}
	}
}
