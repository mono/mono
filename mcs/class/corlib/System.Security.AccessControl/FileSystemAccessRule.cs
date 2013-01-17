//
// System.Security.AccessControl.FileSystemAccessRule implementation
//
// Authors:
//	Dick Porter  <dick@ximian.com>
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006-2007 Novell, Inc (http://www.novell.com)
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

using System.Security.Principal;

namespace System.Security.AccessControl
{
	public sealed class FileSystemAccessRule : AccessRule
	{
		public FileSystemAccessRule (IdentityReference identity,
					     FileSystemRights fileSystemRights,
					     AccessControlType type)
			: this (identity, fileSystemRights, InheritanceFlags.None, PropagationFlags.None, type)
		{
		}

		public FileSystemAccessRule (string identity,
					     FileSystemRights fileSystemRights,
					     AccessControlType type)
			: this (new NTAccount (identity), fileSystemRights, InheritanceFlags.None, PropagationFlags.None, type)
		{
		}

		public FileSystemAccessRule (IdentityReference identity,
					     FileSystemRights fileSystemRights,
					     InheritanceFlags inheritanceFlags,
					     PropagationFlags propagationFlags,
					     AccessControlType type)
			: this (identity, fileSystemRights, false, inheritanceFlags, propagationFlags, type)
		{
		}
		
		internal FileSystemAccessRule (IdentityReference identity,
					       FileSystemRights fileSystemRights,
					       bool isInherited,
					       InheritanceFlags inheritanceFlags,
					       PropagationFlags propagationFlags,
					       AccessControlType type)
			: base (identity, (int) fileSystemRights, isInherited, inheritanceFlags, propagationFlags, type)
		{
		}
		
		public FileSystemAccessRule (string identity,
					     FileSystemRights fileSystemRights,
					     InheritanceFlags inheritanceFlags,
					     PropagationFlags propagationFlags,
					     AccessControlType type)
			: this (new NTAccount (identity), fileSystemRights, inheritanceFlags, propagationFlags, type)
		{
		}
		
		public FileSystemRights FileSystemRights {
			get { return (FileSystemRights)AccessMask; }
		}
	}
}

