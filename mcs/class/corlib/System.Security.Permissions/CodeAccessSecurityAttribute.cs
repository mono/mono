//------------------------------------------------------------------------------
// 
// System.Security.Permissions.SecurityPermissionAttribute.cs 
//
// Copyright (C) 2001 Nick Drochak, All Rights Reserved
// 
// Author:         Nick Drochak, ndrochak@gol.com
// Created:        2002-01-06 
//
//------------------------------------------------------------------------------

using System;
using System.Security.Permissions;

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
	public abstract class CodeAccessSecurityAttribute : SecurityAttribute {

		public CodeAccessSecurityAttribute (SecurityAction action) : base (action) {}

	}  // public abstract class CodeAccessSecurityAttribute
}  // namespace System.Security.Permissions

