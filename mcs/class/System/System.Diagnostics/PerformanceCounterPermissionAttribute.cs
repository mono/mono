//
// System.Diagnostics.PerformanceCounterPermissionAttribute.cs
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
		AttributeTargets.Assembly |
		AttributeTargets.Class |
		AttributeTargets.Struct |
		AttributeTargets.Constructor |
		AttributeTargets.Method |
		AttributeTargets.Event )]
	[MonoTODO]
	public class PerformanceCounterPermissionAttribute 
		: CodeAccessSecurityAttribute {

		[MonoTODO]
		public PerformanceCounterPermissionAttribute (
			SecurityAction action) 
			: base (action)
		{
			throw new NotImplementedException ();
		}

//		[MonoTODO]
//		public string CategoryName {
//			get {throw new NotImplementedException ();}
//			set {throw new NotImplementedException ();}
//		}
//
//		[MonoTODO]
//		public string MachineName {
//			get {throw new NotImplementedException ();}
//			set {throw new NotImplementedException ();}
//		}
//
//		[MonoTODO]
//		public PerformanceCounterPermissionAccess PermissionAccess {
//			get {throw new NotImplementedException ();}
//			set {throw new NotImplementedException ();}
//		}
//
		[MonoTODO]
		public override IPermission CreatePermission ()
		{
			throw new NotImplementedException ();
		}
	}
}

