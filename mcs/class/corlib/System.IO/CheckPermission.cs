//------------------------------------------------------------------------------
// 
// System.IO.CheckPermission.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
// Created:        Saturday, August 25, 2001 
//
// NOTE: All contributors can freely add to this class or make modifications
//       that do not break existing usage of methods 
//------------------------------------------------------------------------------


using System;
using System.Security;
using System.Security.Permissions;

namespace System.IO.Private
{
	/// <summary>
	/// A utility class to assist with various permission validation in System.IO
	/// </summary>
	public sealed class CheckPermission
	{
		/// <summary>
		/// Generates and exception if caller doesn't have flags access to filesystem item specified by path
		/// </summary>
		public static void Demand(FileIOPermissionAccess flags, string path)
		{
			FileIOPermission ioPerm = new FileIOPermission(flags, path);
			ioPerm.Demand();
		}		
	}
}	// namespace System.IO.Private