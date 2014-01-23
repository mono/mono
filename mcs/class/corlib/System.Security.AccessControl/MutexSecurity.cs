//
// System.Security.AccessControl.MutexSecurity implementation
//
// Authors:
//	Dick Porter  <dick@ximian.com>
//	Atsushi Enomoto  <atsushi@ximian.com>
//	James Bellinger  <jfb@zer7.com>
//
// Copyright (C) 2005-2007 Novell, Inc (http://www.novell.com)
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
using System.Threading;

namespace System.Security.AccessControl
{
	public sealed class MutexSecurity : NativeObjectSecurity
	{
		public MutexSecurity ()
			: base (false, ResourceType.KernelObject)
		{
		}

		public MutexSecurity (string name,
				      AccessControlSections includeSections)
			: base (false, ResourceType.KernelObject, name, includeSections,
				MutexExceptionFromErrorCode, null)
		{
		}
		
		internal MutexSecurity (SafeHandle handle,
					AccessControlSections includeSections)
			: base (false, ResourceType.KernelObject, handle, includeSections,
				MutexExceptionFromErrorCode, null)
		{
		}

		public override Type AccessRightType {
			get { return typeof (MutexRights); }
		}
			
		public override Type AccessRuleType {
			get { return typeof (MutexAccessRule); }
		}
			
		public override Type AuditRuleType {
			get { return typeof (MutexAuditRule); }
		}
		
		public override AccessRule AccessRuleFactory (IdentityReference identityReference, int accessMask,
							      bool isInherited, InheritanceFlags inheritanceFlags,
							      PropagationFlags propagationFlags, AccessControlType type)
		{
			return new MutexAccessRule (identityReference, (MutexRights) accessMask, type);
		}
		
		public void AddAccessRule (MutexAccessRule rule)
		{
			AddAccessRule ((AccessRule)rule);
		}
		
		public bool RemoveAccessRule (MutexAccessRule rule)
		{
			return RemoveAccessRule ((AccessRule)rule);
		}
		
		public void RemoveAccessRuleAll (MutexAccessRule rule)
		{
			RemoveAccessRuleAll ((AccessRule)rule);
		}
		
		public void RemoveAccessRuleSpecific (MutexAccessRule rule)
		{
			RemoveAccessRuleSpecific ((AccessRule)rule);
		}
		
		public void ResetAccessRule (MutexAccessRule rule)
		{
			ResetAccessRule ((AccessRule)rule);
		}
		
		public void SetAccessRule (MutexAccessRule rule)
		{
			SetAccessRule ((AccessRule)rule);
		}

		public override AuditRule AuditRuleFactory (IdentityReference identityReference, int accessMask,
							    bool isInherited, InheritanceFlags inheritanceFlags,
							    PropagationFlags propagationFlags, AuditFlags flags)
		{
			return new MutexAuditRule (identityReference, (MutexRights) accessMask, flags);
		}
		
		public void AddAuditRule (MutexAuditRule rule)
		{
			AddAuditRule ((AuditRule)rule);
		}
		
		public bool RemoveAuditRule (MutexAuditRule rule)
		{
			return RemoveAuditRule((AuditRule)rule);
		}
		
		public void RemoveAuditRuleAll (MutexAuditRule rule)
		{
			RemoveAuditRuleAll((AuditRule)rule);
		}
		
		public void RemoveAuditRuleSpecific (MutexAuditRule rule)
		{
			RemoveAuditRuleSpecific((AuditRule)rule);
		}
		
		public void SetAuditRule (MutexAuditRule rule)
		{
			SetAuditRule((AuditRule)rule);
		}
		
		static Exception MutexExceptionFromErrorCode (int errorCode,
							      string name, SafeHandle handle,
							      object context)
		{
			switch (errorCode) {
				case 2: return new WaitHandleCannotBeOpenedException ();
				default: return DefaultExceptionFromErrorCode (errorCode, name, handle, context);
			}
		}
	}
}

