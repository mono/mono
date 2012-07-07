//
// System.Security.AccessControl.EventWaitHandleSecurity implementation
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

namespace System.Security.AccessControl
{
	public sealed class EventWaitHandleSecurity : NativeObjectSecurity
	{
		public EventWaitHandleSecurity ()
			: base (false, ResourceType.KernelObject)
		{
		}

		internal EventWaitHandleSecurity (SafeHandle handle,
						  AccessControlSections includeSections)
			: base (false, ResourceType.KernelObject, handle, includeSections)
		{
		}
		
		public override Type AccessRightType {
			get { return typeof (EventWaitHandleRights); }
		}
		
		public override Type AccessRuleType {
			get { return typeof (EventWaitHandleAccessRule); }
		}
		
		public override Type AuditRuleType {
			get { return typeof (EventWaitHandleAuditRule); }
		}
		
		public override AccessRule AccessRuleFactory (IdentityReference identityReference, int accessMask,
							      bool isInherited, InheritanceFlags inheritanceFlags,
							      PropagationFlags propagationFlags, AccessControlType type)
		{
			return new EventWaitHandleAccessRule (identityReference, (EventWaitHandleRights) accessMask, type);
		}
		
		public void AddAccessRule (EventWaitHandleAccessRule rule)
		{
			AddAccessRule ((AccessRule)rule);
		}
		
		public bool RemoveAccessRule (EventWaitHandleAccessRule rule)
		{
			return RemoveAccessRule ((AccessRule)rule);
		}
		
		public void RemoveAccessRuleAll (EventWaitHandleAccessRule rule)
		{
			RemoveAccessRuleAll ((AccessRule)rule);
		}
		
		public void RemoveAccessRuleSpecific (EventWaitHandleAccessRule rule)
		{
			RemoveAccessRuleSpecific ((AccessRule)rule);
		}
		
		public void ResetAccessRule (EventWaitHandleAccessRule rule)
		{
			ResetAccessRule ((AccessRule)rule);
		}
		
		public void SetAccessRule (EventWaitHandleAccessRule rule)
		{
			SetAccessRule ((AccessRule)rule);
		}
		
		public override AuditRule AuditRuleFactory (IdentityReference identityReference, int accessMask,
							    bool isInherited, InheritanceFlags inheritanceFlags,
							    PropagationFlags propagationFlags, AuditFlags flags)
		{
			return new EventWaitHandleAuditRule (identityReference, (EventWaitHandleRights) accessMask, flags);
		}
		
		public void AddAuditRule (EventWaitHandleAuditRule rule)
		{
			AddAuditRule ((AuditRule)rule);
		}
		
		public bool RemoveAuditRule (EventWaitHandleAuditRule rule)
		{
			return RemoveAuditRule((AuditRule)rule);
		}
		
		public void RemoveAuditRuleAll (EventWaitHandleAuditRule rule)
		{
			RemoveAuditRuleAll((AuditRule)rule);
		}
		
		public void RemoveAuditRuleSpecific (EventWaitHandleAuditRule rule)
		{
			RemoveAuditRuleSpecific((AuditRule)rule);
		}
		
		public void SetAuditRule (EventWaitHandleAuditRule rule)
		{
			SetAuditRule((AuditRule)rule);
		}
	}
}

