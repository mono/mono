//
// System.Security.AccessControl.CommonObjectSecurity implementation
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

using System.Collections.Generic;

namespace System.Security.AccessControl
{
	public abstract class CommonObjectSecurity : ObjectSecurity
	{
		protected CommonObjectSecurity (bool isContainer)
			: base (isContainer, false)
		{
		}
		
		internal CommonObjectSecurity (CommonSecurityDescriptor securityDescriptor)
			: base (securityDescriptor)
		{
		}
		
		public AuthorizationRuleCollection GetAccessRules (bool includeExplicit, bool includeInherited, Type targetType)
		{
			return InternalGetAccessRules (includeExplicit, includeInherited, targetType);
		}
		
		public AuthorizationRuleCollection GetAuditRules (bool includeExplicit, bool includeInherited, Type targetType)
		{
			return InternalGetAuditRules (includeExplicit, includeInherited, targetType);
		}
		
		protected void AddAccessRule (AccessRule rule)
		{
			bool modified;
			ModifyAccess (AccessControlModification.Add, rule, out modified);
		}
		
		protected bool RemoveAccessRule (AccessRule rule)
		{
			bool modified;
			return ModifyAccess (AccessControlModification.Remove, rule, out modified);
		}
		
		protected void RemoveAccessRuleAll (AccessRule rule)
		{
			bool modified;
			ModifyAccess (AccessControlModification.RemoveAll, rule, out modified);
		}
		
		protected void RemoveAccessRuleSpecific (AccessRule rule)
		{
			bool modified;
			ModifyAccess (AccessControlModification.RemoveSpecific, rule, out modified);
		}
		
		protected void ResetAccessRule (AccessRule rule)
		{
			bool modified;
			ModifyAccess (AccessControlModification.Reset, rule, out modified);
		}
		
		protected void SetAccessRule (AccessRule rule)
		{
			bool modified;
			ModifyAccess (AccessControlModification.Set, rule, out modified);
		}		
		
		protected override bool ModifyAccess (AccessControlModification modification, AccessRule rule, out bool modified)
		{		
			if (null == rule)
				throw new ArgumentNullException ("rule");
				
			modified = true;
			
			WriteLock ();
			try {
				switch (modification) {
				case AccessControlModification.Add:
					descriptor.DiscretionaryAcl.AddAccess (rule.AccessControlType,
									       SidFromIR (rule.IdentityReference),
									       rule.AccessMask,
									       rule.InheritanceFlags,
									       rule.PropagationFlags);
					break;
				case AccessControlModification.Set:
					descriptor.DiscretionaryAcl.SetAccess (rule.AccessControlType,
									       SidFromIR (rule.IdentityReference),
									       rule.AccessMask,
									       rule.InheritanceFlags,
									       rule.PropagationFlags);
					break;
				case AccessControlModification.Reset:
					PurgeAccessRules (rule.IdentityReference);
					goto case AccessControlModification.Add;
				case AccessControlModification.Remove:
					modified = descriptor.DiscretionaryAcl.RemoveAccess (rule.AccessControlType,
											     SidFromIR (rule.IdentityReference),
											     rule.AccessMask,
											     rule.InheritanceFlags,
											     rule.PropagationFlags);
					break;
				case AccessControlModification.RemoveAll:
					PurgeAccessRules (rule.IdentityReference);
					break;
				case AccessControlModification.RemoveSpecific:
					descriptor.DiscretionaryAcl.RemoveAccessSpecific (rule.AccessControlType,
											  SidFromIR (rule.IdentityReference),
											  rule.AccessMask,
											  rule.InheritanceFlags,
											  rule.PropagationFlags);
					break;
				default:
					throw new ArgumentOutOfRangeException ("modification");
				}
				
				if (modified) AccessRulesModified = true;
			} finally {
				WriteUnlock ();
			}
			
			return modified;
		}
		
		protected void AddAuditRule (AuditRule rule)
		{
			bool modified;
			ModifyAudit (AccessControlModification.Add, rule, out modified);
		}
		
		protected bool RemoveAuditRule (AuditRule rule)
		{
			bool modified;
			return ModifyAudit (AccessControlModification.Remove, rule, out modified);
		}
		
		protected void RemoveAuditRuleAll (AuditRule rule)
		{
			bool modified;
			ModifyAudit (AccessControlModification.RemoveAll, rule, out modified);
		}
		
		protected void RemoveAuditRuleSpecific (AuditRule rule)
		{
			bool modified;
			ModifyAudit (AccessControlModification.RemoveSpecific, rule, out modified);
		}
		
		protected void SetAuditRule (AuditRule rule)
		{
			bool modified;
			ModifyAudit (AccessControlModification.Set, rule, out modified);
		}
		
		protected override bool ModifyAudit (AccessControlModification modification, AuditRule rule, out bool modified)
		{
			if (null == rule)
				throw new ArgumentNullException ("rule");

			modified = true;
			
			WriteLock ();
			try {
				switch (modification) {
				case AccessControlModification.Add:
					if (null == descriptor.SystemAcl)
						descriptor.SystemAcl = new SystemAcl (IsContainer, IsDS, 1);
					
					descriptor.SystemAcl.AddAudit (rule.AuditFlags,
								       SidFromIR (rule.IdentityReference),
								       rule.AccessMask,
								       rule.InheritanceFlags,
								       rule.PropagationFlags);
					break;
				case AccessControlModification.Set:
					if (null == descriptor.SystemAcl)
						descriptor.SystemAcl = new SystemAcl (IsContainer, IsDS, 1);

					descriptor.SystemAcl.SetAudit (rule.AuditFlags,
								       SidFromIR (rule.IdentityReference),
								       rule.AccessMask,
								       rule.InheritanceFlags,
								       rule.PropagationFlags);
					break;
				case AccessControlModification.Reset:
					break;
				case AccessControlModification.Remove:
					if (null == descriptor.SystemAcl)
						modified = false;
					else
						modified = descriptor.SystemAcl.RemoveAudit (rule.AuditFlags,
											     SidFromIR (rule.IdentityReference),
											     rule.AccessMask,
											     rule.InheritanceFlags,
											     rule.PropagationFlags);
					break;
				case AccessControlModification.RemoveAll:
					PurgeAuditRules (rule.IdentityReference);
					break;
				case AccessControlModification.RemoveSpecific:
					if (null != descriptor.SystemAcl)
						descriptor.SystemAcl.RemoveAuditSpecific (rule.AuditFlags,
											  SidFromIR (rule.IdentityReference),
											  rule.AccessMask,
											  rule.InheritanceFlags,
											  rule.PropagationFlags);
					break;
				default:
					throw new ArgumentOutOfRangeException ("modification");
				}
				
				if (modified) AuditRulesModified = true;
			} finally {
				WriteUnlock ();
			}
			
			return modified;
		}
	}
}

