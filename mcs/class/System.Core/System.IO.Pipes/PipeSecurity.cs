//
// PipeSecurity.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	James Bellinger <jfb@zer7.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
// Copyright (C) 2012 James Bellinger
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;

namespace System.IO.Pipes
{
	[HostProtection (SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	public class PipeSecurity : NativeObjectSecurity
	{
		public PipeSecurity ()
			: base (false, ResourceType.FileObject)
		{
		}
		
		internal PipeSecurity (SafeHandle handle, AccessControlSections includeSections)
			: base (false, ResourceType.FileObject, handle, includeSections)
		{
		}
		
		public override Type AccessRightType {
			get { return typeof (PipeAccessRights); }
		}

		public override Type AccessRuleType {
			get { return typeof (PipeAccessRule); }
		}

		public override Type AuditRuleType {
			get { return typeof (PipeAuditRule); }
		}

		public override AccessRule AccessRuleFactory (IdentityReference identityReference,
							      int accessMask, bool isInherited,
							      InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags,
							      AccessControlType type)
		{
			return new PipeAccessRule (identityReference, (PipeAccessRights)accessMask, type);
		}

		public void AddAccessRule (PipeAccessRule rule)
		{
			AddAccessRule ((AccessRule)rule);
		}

		public void AddAuditRule (PipeAuditRule rule)
		{
			AddAuditRule ((AuditRule) rule);
		}

		public override sealed AuditRule AuditRuleFactory (IdentityReference identityReference,
								   int accessMask, bool isInherited,
								   InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags,
								   AuditFlags flags)
		{
			return new PipeAuditRule (identityReference, (PipeAccessRights)accessMask, flags);
		}

		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		protected internal void Persist (SafeHandle handle)
		{
			WriteLock();
			try {
				Persist (handle, AccessControlSectionsModified, null);
			} finally {
				WriteUnlock ();
			}
		}

		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		protected internal void Persist (string name)
		{
			WriteLock();
			try {
				Persist (name, AccessControlSectionsModified, null);
			} finally {
				WriteUnlock ();
			}
		}

#if BOOTSTRAP_BASIC		
		AccessControlSections AccessControlSectionsModified {
			get {
				return (AccessRulesModified ? AccessControlSections.Access : 0) |
				       (AuditRulesModified  ? AccessControlSections.Audit  : 0) |
				       (OwnerModified       ? AccessControlSections.Owner  : 0) |
				       (GroupModified       ? AccessControlSections.Group  : 0);
			}
		}
#endif
		public bool RemoveAccessRule (PipeAccessRule rule)
		{
			return RemoveAccessRule ((AccessRule)rule);
		}

		public void RemoveAccessRuleSpecific (PipeAccessRule rule)
		{
			RemoveAccessRuleSpecific ((AccessRule)rule);
		}

		public bool RemoveAuditRule (PipeAuditRule rule)
		{
			return RemoveAuditRule ((AuditRule)rule);
		}

		public void RemoveAuditRuleAll (PipeAuditRule rule)
		{
			RemoveAuditRuleAll ((AuditRule)rule);
		}

		public void RemoveAuditRuleSpecific (PipeAuditRule rule)
		{
			RemoveAuditRuleSpecific ((AuditRule)rule);
		}

		public void ResetAccessRule (PipeAccessRule rule)
		{
			ResetAccessRule ((AccessRule)rule);
		}

		public void SetAccessRule (PipeAccessRule rule)
		{
			SetAccessRule ((AccessRule)rule);
		}

		public void SetAuditRule (PipeAuditRule rule)
		{
			SetAuditRule ((AuditRule)rule);
		}
	}
}
