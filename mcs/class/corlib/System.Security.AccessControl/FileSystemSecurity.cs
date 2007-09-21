//
// System.Security.AccessControl.FileSystemSecurity implementation
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

#if NET_2_0

using System.Security.Principal;

namespace System.Security.AccessControl {

	public abstract class FileSystemSecurity : NativeObjectSecurity {

		internal FileSystemSecurity (bool isContainer)
			: base (isContainer, ResourceType.FileObject)
		{
		}

		internal FileSystemSecurity (bool isContainer, string name, AccessControlSections includeSections)
			: base (isContainer, ResourceType.FileObject, name, includeSections)
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
		
		// AccessRule
		
		[MonoTODO]
		public override sealed AccessRule AccessRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
		{
			// FIXME: isInherited is unused
			return new FileSystemAccessRule (identityReference, (FileSystemRights) accessMask, inheritanceFlags, propagationFlags, type);
		}
		
		[MonoTODO]
		public void AddAccessRule (FileSystemAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool RemoveAccessRule (FileSystemAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void RemoveAccessRuleAll (FileSystemAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void RemoveAccessRuleSpecific (FileSystemAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ResetAccessRule (FileSystemAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void SetAccessRule (FileSystemAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		// AuditRule
		
		[MonoTODO]
		public override sealed AuditRule AuditRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
		{
			// FIXME: isInherited is unused
			return new FileSystemAuditRule (identityReference, (FileSystemRights) accessMask, inheritanceFlags, propagationFlags, flags);
		}
		
		[MonoTODO]
		public void AddAuditRule (FileSystemAuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool RemoveAuditRule (FileSystemAuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void RemoveAuditRuleAll (FileSystemAuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void RemoveAuditRuleSpecific (FileSystemAuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void SetAuditRule (FileSystemAuditRule rule)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
