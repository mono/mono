//
// System.Diagnostics.EventLogPermissionAttribute.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;

namespace System.Diagnostics {

	[AttributeUsage(
		AttributeTargets.Assembly | AttributeTargets.Class |
		AttributeTargets.Struct | AttributeTargets.Constructor |
		AttributeTargets.Method | AttributeTargets.Event)]
	[Serializable]
	[MonoTODO("Just Stubbed Out")]
	public class EventLogPermissionAttribute : CodeAccessSecurityAttribute {

		public EventLogPermissionAttribute(SecurityAction action)
			: base(action)
		{
		}

//		// May throw ArgumentException if computer name is invalid
//		public string MachineName {
//			get {throw new NotImplementedException();}
//			set {throw new NotImplementedException();}
//		}
//
//		public EventLogPermissionAccess PermissionAccess {
//			get {throw new NotImplementedException();}
//			set {throw new NotImplementedException();}
//		}
//
		[MonoTODO]
		public override IPermission CreatePermission()
		{
			throw new NotImplementedException();
		}
	}
}

