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

namespace System.IO
{
	/// <summary>
	/// A utility class to assist with various permission validation in System.IO
	/// </summary>
	internal sealed class CheckPermission
	{
		/// <summary>
		/// Generates and exception if caller doesn't have flags access to filesystem item specified by path
		/// </summary>
		public static void Demand(FileIOPermissionAccess flags, string path)
		{
			FileIOPermission ioPerm = new FileIOPermission(flags, path);
			// FIXME: FileIOPermission is not yet implemented
			//ioPerm.Demand();
		}		
		
		public static void Access(FileAccess access, string path)
		{
			switch(access)
			{
			case FileAccess.Read:
				Demand(FileIOPermissionAccess.Read, path);
				break;
			case FileAccess.Write:
				Demand(FileIOPermissionAccess.Write, path);
				break;
			case FileAccess.ReadWrite:
				Demand(FileIOPermissionAccess.Read | FileIOPermissionAccess.Write, path);
				break;
			default:
				// TODO: determine what best to do here
				throw new ArgumentException("Invalid FileAccess parameter");
			}
		}
		
		public static void ModeAccess(FileMode mode, FileAccess access, string path, bool exists)
		{
			return;
			// TODO: this logic isn't entirely complete and accurate, yet
			if((mode & (FileMode.CreateNew | FileMode.Create)) != 0)
			{
				CheckPermission.Demand(FileIOPermissionAccess.Write, Path.GetDirectoryName(path));
			}
			else if((mode & FileMode.OpenOrCreate) != 0)
			{
				if(!exists)
				{
					CheckPermission.Demand(FileIOPermissionAccess.Write, Path.GetDirectoryName(path));
				}
				else
				{
					CheckPermission.Access(access, path);
				}
			}
			else if(exists)
			{
				CheckPermission.Access(access, path);
			}
			else
			{
				throw new FileNotFoundException();
			}
		}
	}
}	// namespace System.IO.Private
