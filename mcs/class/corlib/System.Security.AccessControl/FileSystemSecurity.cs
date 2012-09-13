//
// System.Security.AccessControl.FileSystemSecurity implementation
//
// Authors:
//	Dick Porter  <dick@ximian.com>
//	Atsushi Enomoto  <atsushi@ximian.com>
//	James Bellinger  <jfb@zer7.com>
//
// Copyright (C) 2006-2007 Novell, Inc (http://www.novell.com)
// Copyright (C) 2012      James Bellinger
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

using System.Runtime.InteropServices;
using System.Security.Principal;

namespace System.Security.AccessControl
{
	public abstract class FileSystemSecurity : NativeObjectSecurity
	{
		internal FileSystemSecurity (bool isContainer)
			: base (isContainer, ResourceType.FileObject)
		{
		}

		internal FileSystemSecurity (bool isContainer, string name, AccessControlSections includeSections)
			: base (isContainer, ResourceType.FileObject, name, includeSections)
		{
		}

		internal FileSystemSecurity (bool isContainer, SafeHandle handle, AccessControlSections includeSections)
			: base (isContainer, ResourceType.FileObject, handle, includeSections)
		{
		}
		
		public override Type AccessRightType {
			get { return typeof (FileSystemRights); }
		}
		
		public override Type AccessRuleType {
			get { return typeof (FileSystemAccessRule); }
		}

		public override Type AuditRuleType {
			get { return typeof (FileSystemAuditRule); }
		}

		public override sealed AccessRule AccessRuleFactory (IdentityReference identityReference, int accessMask,
								     bool isInherited, InheritanceFlags inheritanceFlags,
								     PropagationFlags propagationFlags, AccessControlType type)
		{
			return new FileSystemAccessRule (identityReference, (FileSystemRights) accessMask, isInherited,
							 inheritanceFlags, propagationFlags, type);
		}
		
		public void AddAccessRule (FileSystemAccessRule rule)
		{
			AddAccessRule ((AccessRule)rule);
		}
		
		public bool RemoveAccessRule (FileSystemAccessRule rule)
		{
			return RemoveAccessRule ((AccessRule)rule);
		}
		
		public void RemoveAccessRuleAll (FileSystemAccessRule rule)
		{
			RemoveAccessRuleAll ((AccessRule)rule);
		}
		
		public void RemoveAccessRuleSpecific (FileSystemAccessRule rule)
		{
			RemoveAccessRuleSpecific ((AccessRule)rule);
		}
		
		public void ResetAccessRule (FileSystemAccessRule rule)
		{
			ResetAccessRule ((AccessRule)rule);
		}
		
		public void SetAccessRule (FileSystemAccessRule rule)
		{
			SetAccessRule ((AccessRule)rule);
		}
		
		public override sealed AuditRule AuditRuleFactory (IdentityReference identityReference, int accessMask,
								   bool isInherited, InheritanceFlags inheritanceFlags,
								   PropagationFlags propagationFlags, AuditFlags flags)
		{
			return new FileSystemAuditRule (identityReference, (FileSystemRights) accessMask, isInherited,
							inheritanceFlags, propagationFlags, flags);
		}
		
		public void AddAuditRule (FileSystemAuditRule rule)
		{
			AddAuditRule ((AuditRule)rule);
		}
		
		public bool RemoveAuditRule (FileSystemAuditRule rule)
		{
			return RemoveAuditRule((AuditRule)rule);
		}
		
		public void RemoveAuditRuleAll (FileSystemAuditRule rule)
		{
			RemoveAuditRuleAll((AuditRule)rule);
		}
		
		public void RemoveAuditRuleSpecific (FileSystemAuditRule rule)
		{
			RemoveAuditRuleSpecific((AuditRule)rule);
		}
		
		public void SetAuditRule (FileSystemAuditRule rule)
		{
			SetAuditRule((AuditRule)rule);
		}
	}
}

