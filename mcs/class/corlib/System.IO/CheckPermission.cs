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

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


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
		[MonoTODO]
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

		[MonoTODO]
		public static void ModeAccess(FileMode mode, FileAccess access, string path, bool exists)
		{
#if false
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
				throw new FileNotFoundException("File not found", path);
			}
#endif
		}
	}
}	// namespace System.IO.Private
