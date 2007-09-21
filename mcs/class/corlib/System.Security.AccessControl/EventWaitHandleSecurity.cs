//
// System.Security.AccessControl.EventWaitHandleSecurity implementation
//
// Authors:
//	Dick Porter  <dick@ximian.com>
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005-2007 Novell, Inc (http://www.novell.com)
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

namespace System.Security.AccessControl
{
	public sealed class EventWaitHandleSecurity : NativeObjectSecurity
	{
		public EventWaitHandleSecurity ()
		{
			throw new NotImplementedException ();
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
		
		// AccessRule
		
		public override AccessRule AccessRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
		{
			return new EventWaitHandleAccessRule (identityReference, (EventWaitHandleRights) accessMask, type);
		}
		
		[MonoTODO]
		public void AddAccessRule (EventWaitHandleAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool RemoveAccessRule (EventWaitHandleAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void RemoveAccessRuleAll (EventWaitHandleAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void RemoveAccessRuleSpecific (EventWaitHandleAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ResetAccessRule (EventWaitHandleAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void SetAccessRule (EventWaitHandleAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		// AuditRule
		
		public override AuditRule AuditRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
		{
			return new EventWaitHandleAuditRule (identityReference, (EventWaitHandleRights) accessMask, flags);
		}
		
		[MonoTODO]
		public void AddAuditRule (EventWaitHandleAuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool RemoveAuditRule (EventWaitHandleAuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void RemoveAuditRuleAll (EventWaitHandleAuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void RemoveAuditRuleSpecific (EventWaitHandleAuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void SetAuditRule (EventWaitHandleAuditRule rule)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
