//
// PipeSecurity.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
	[MonoNotSupported ("ACL is not supported in Mono")]
	[HostProtection (SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	public class PipeSecurity : NativeObjectSecurity
	{
		[MonoNotSupported ("ACL is not supported in Mono")]
		public PipeSecurity ()
			: base (false, ResourceType.FileObject)
		{
			throw new NotImplementedException ("ACL is not supported in Mono");
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

		[MonoNotSupported ("ACL is not supported in Mono")]
		public override AccessRule AccessRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
		{
			throw new NotImplementedException ("ACL is not supported in Mono");
		}

		[MonoNotSupported ("ACL is not supported in Mono")]
		public void AddAccessRule (PipeAccessRule rule)
		{
			throw new NotImplementedException ("ACL is not supported in Mono");
		}

		[MonoNotSupported ("ACL is not supported in Mono")]
		public void AddAuditRule (PipeAuditRule rule)
		{
			throw new NotImplementedException ("ACL is not supported in Mono");
		}

		[MonoNotSupported ("ACL is not supported in Mono")]
		public override sealed AuditRule AuditRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
		{
			throw new NotImplementedException ("ACL is not supported in Mono");
		}

		[MonoNotSupported ("ACL is not supported in Mono")]
		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		protected internal void Persist (SafeHandle handle)
		{
			throw new NotImplementedException ("ACL is not supported in Mono");
		}

		[MonoNotSupported ("ACL is not supported in Mono")]
		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		protected internal void Persist (string name)
		{
			throw new NotImplementedException ("ACL is not supported in Mono");
		}

		[MonoNotSupported ("ACL is not supported in Mono")]
		public bool RemoveAccessRule (PipeAccessRule rule)
		{
			throw new NotImplementedException ("ACL is not supported in Mono");
		}

		[MonoNotSupported ("ACL is not supported in Mono")]
		public void RemoveAccessRuleSpecific (PipeAccessRule rule)
		{
			throw new NotImplementedException ("ACL is not supported in Mono");
		}

		[MonoNotSupported ("ACL is not supported in Mono")]
		public bool RemoveAuditRule (PipeAuditRule rule)
		{
			throw new NotImplementedException ("ACL is not supported in Mono");
		}

		[MonoNotSupported ("ACL is not supported in Mono")]
		public void RemoveAuditRuleAll (PipeAuditRule rule)
		{
			throw new NotImplementedException ("ACL is not supported in Mono");
		}

		[MonoNotSupported ("ACL is not supported in Mono")]
		public void RemoveAuditRuleSpecific (PipeAuditRule rule)
		{
			throw new NotImplementedException ("ACL is not supported in Mono");
		}

		[MonoNotSupported ("ACL is not supported in Mono")]
		public void ResetAccessRule (PipeAccessRule rule)
		{
			throw new NotImplementedException ("ACL is not supported in Mono");
		}

		[MonoNotSupported ("ACL is not supported in Mono")]
		public void SetAccessRule (PipeAccessRule rule)
		{
			throw new NotImplementedException ("ACL is not supported in Mono");
		}

		[MonoNotSupported ("ACL is not supported in Mono")]
		public void SetAuditRule (PipeAuditRule rule)
		{
			throw new NotImplementedException ("ACL is not supported in Mono");
		}
	}
}
