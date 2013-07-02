//
// System.Security.AccessControl.SemaphoreSecurity class
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Dick Porter <dick@ximian.com>
//	James Bellinger  <jfb@zer7.com>
//
// Copyright (C) 2005, 2006 Novell, Inc (http://www.novell.com)
// Copyright (C) 2012       James Bellinger
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
	[ComVisible (false)]
	public sealed class SemaphoreSecurity : NativeObjectSecurity
	{
		public SemaphoreSecurity ()
			: base (false, ResourceType.KernelObject)
		{
		}

		public SemaphoreSecurity (string name, AccessControlSections includeSections)
			: base (false, ResourceType.KernelObject, name, includeSections)
		{
		}
		
		internal SemaphoreSecurity (SafeHandle handle, AccessControlSections includeSections)
			: base (false, ResourceType.KernelObject, handle, includeSections)
		{
		}
		
		public override Type AccessRightType {
			get { return typeof (SemaphoreRights); }
		}
		
		public override Type AccessRuleType {
			get { return typeof (SemaphoreAccessRule); }
		}

		public override Type AuditRuleType {
			get { return typeof (SemaphoreAuditRule); }
		}
		
		public override AccessRule AccessRuleFactory (IdentityReference identityReference, int accessMask,
							      bool isInherited, InheritanceFlags inheritanceFlags,
							      PropagationFlags propagationFlags, AccessControlType type)
		{
			return new SemaphoreAccessRule (identityReference, (SemaphoreRights)accessMask, type);
		}
		
		public void AddAccessRule (SemaphoreAccessRule rule)
		{
			AddAccessRule ((AccessRule)rule);
		}
		
		public bool RemoveAccessRule (SemaphoreAccessRule rule)
		{
			return RemoveAccessRule ((AccessRule)rule);
		}
		
		public void RemoveAccessRuleAll (SemaphoreAccessRule rule)
		{
			RemoveAccessRuleAll ((AccessRule)rule);
		}
		
		public void RemoveAccessRuleSpecific (SemaphoreAccessRule rule)
		{
			RemoveAccessRuleSpecific ((AccessRule)rule);
		}
		
		public void ResetAccessRule (SemaphoreAccessRule rule)
		{
			ResetAccessRule ((AccessRule)rule);
		}
		
		public void SetAccessRule (SemaphoreAccessRule rule)
		{
			SetAccessRule ((AccessRule)rule);
		}
		
		public override AuditRule AuditRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited,
							    InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags,
							    AuditFlags flags)
		{
			return new SemaphoreAuditRule (identityReference, (SemaphoreRights)accessMask, flags);
		}
		
		public void AddAuditRule (SemaphoreAuditRule rule)
		{
			AddAuditRule ((AuditRule)rule);
		}
		
		public bool RemoveAuditRule (SemaphoreAuditRule rule)
		{
			return RemoveAuditRule((AuditRule)rule);
		}
		
		public void RemoveAuditRuleAll (SemaphoreAuditRule rule)
		{
			RemoveAuditRuleAll((AuditRule)rule);
		}
		
		public void RemoveAuditRuleSpecific (SemaphoreAuditRule rule)
		{
			RemoveAuditRuleSpecific((AuditRule)rule);
		}
		
		public void SetAuditRule (SemaphoreAuditRule rule)
		{
			SetAuditRule((AuditRule)rule);
		}
		
		internal new void PersistModifications (SafeHandle handle)
		{
			WriteLock();
			try {
				Persist (handle, (AccessRulesModified ? AccessControlSections.Access : 0) |
						 (AuditRulesModified  ? AccessControlSections.Audit  : 0) |
						 (OwnerModified       ? AccessControlSections.Owner  : 0) |
						 (GroupModified       ? AccessControlSections.Group  : 0), null);
			} finally {
				WriteUnlock ();
			}
		}
	}
}

