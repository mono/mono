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

	[Serializable]
	public class SecurityException : SystemException {

		// Fields
		string permissionState;
		Type permissionType;
		private string _granted;
		private string _refused;

		// Properties
		public string PermissionState
		{
			get { return permissionState; }
		}

		public Type PermissionType
		{
			get { return permissionType; }
		}
#if ! NET_1_0
		public string GrantedSet {
			get { return _granted; }
		}

		public string RefusedSet {
			get { return _refused; }
		}
#endif
		// Constructors
		public SecurityException ()
			: base (Locale.GetText ("A security error has been detected."))
		{
			base.HResult = unchecked ((int)0x8013150A);
		}

		public SecurityException (string message) 
			: base (message)
		{
			base.HResult = unchecked ((int)0x8013150A);
		}
		
		protected SecurityException (SerializationInfo info, StreamingContext context) 
			: base (info, context)
		{
			base.HResult = unchecked ((int)0x8013150A);
			permissionState = info.GetString ("permissionState");
		}
		
		public SecurityException (string message, Exception inner) 
			: base (message, inner)
		{
			base.HResult = unchecked ((int)0x8013150A);
		}
		
		public SecurityException (string message, Type type) 
			:  base (message) 
		{
			base.HResult = unchecked ((int)0x8013150A);
			permissionType = type;
		}
		
		public SecurityException (string message, Type type, string state) 
			: base (message) 
		{
			base.HResult = unchecked ((int)0x8013150A);
			permissionType = type;
			permissionState = state;
		}

		internal SecurityException (string message, PermissionSet granted, PermissionSet refused) 
			: base (message)
		{
			base.HResult = unchecked ((int)0x8013150A);
			_granted = granted.ToString ();
			_refused = refused.ToString ();
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
