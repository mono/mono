//------------------------------------------------------------------------------
// 
// System.Security.Permissions.SecurityAttribute.cs 
//
// Copyright (C) 2001 Nick Drochak, All Rights Reserved
// 
// Author:         Nick Drochak, ndrochak@gol.com
// Created:        2002-01-06 
//
//------------------------------------------------------------------------------

using System;
using System.Security;

namespace System.Security.Permissions {
	[System.AttributeUsage(
		System.AttributeTargets.Assembly 
		| System.AttributeTargets.Class 
		| System.AttributeTargets.Struct 
		| System.AttributeTargets.Constructor 
		| System.AttributeTargets.Method, 
		AllowMultiple=true, 
		Inherited=false)
	]

	[Serializable]
	public abstract class SecurityAttribute : Attribute {

		private SecurityAction m_Action;
		private bool m_Unrestricted;

		public SecurityAttribute (SecurityAction action) 
		{
			Action = action;
		}

		public abstract IPermission CreatePermission ();

		public bool Unrestricted {
			get {
				return m_Unrestricted;
			}
			set {
				m_Unrestricted = value;
			}
		}

		public SecurityAction Action {
			get {
				return m_Action;
			}
			set {
				if (!SecurityAction.IsDefined(typeof(SecurityAction), value)) {
					throw new System.ArgumentException();
				}
				m_Action = value;
			}
		}
	} // public abstract class SecurityAttribute
}  // namespace System.Security.Permissions
