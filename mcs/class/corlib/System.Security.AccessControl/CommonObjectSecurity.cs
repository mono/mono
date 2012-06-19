//
// System.Security.AccessControl.CommonObjectSecurity implementation
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

using System.Collections.Generic;
using System.Security.Principal;

namespace System.Security.AccessControl {

	[MonoTODO ("required for NativeObjectSecurity - implementation is missing")]
	public abstract class CommonObjectSecurity : ObjectSecurity {

		protected CommonObjectSecurity (bool isContainer)
			: base (isContainer, false)
		{
		}
		
		public AuthorizationRuleCollection GetAccessRules (bool includeExplicit, bool includeInherited, Type targetType)
		{
			throw new NotImplementedException ();
		}
		
		public AuthorizationRuleCollection GetAuditRules (bool includeExplicit, bool includeInherited, Type targetType)
		{
			throw new NotImplementedException ();
		}
		
		// Access
		
		protected void AddAccessRule (AccessRule rule)
		{
			bool modified;
			ModifyAccessRule(AccessControlModification.Add, rule, out modified);
		}
		
		protected bool RemoveAccessRule (AccessRule rule)
		{
			bool modified;
			return ModifyAccessRule(AccessControlModification.Remove, rule, out modified);
		}
		
		protected void RemoveAccessRuleAll (AccessRule rule)
		{
			bool modified;
			ModifyAccessRule(AccessControlModification.RemoveAll, rule, out modified);
		}
		
		protected void RemoveAccessRuleSpecific (AccessRule rule)
		{
			bool modified;
			ModifyAccessRule(AccessControlModification.RemoveSpecific, rule, out modified);
		}
		
		protected void ResetAccessRule (AccessRule rule)
		{
			bool modified;
			ModifyAccessRule(AccessControlModification.Reset, rule, out modified);
		}
		
		protected void SetAccessRule (AccessRule rule)
		{
			bool modified;
			ModifyAccessRule(AccessControlModification.Set, rule, out modified);
		}
		
		protected override bool ModifyAccess (AccessControlModification modification, AccessRule rule, out bool modified)
		{
			if (rule == null)
				throw new ArgumentNullException("rule");

			SecurityIdentifier sid = rule.IdentityReference.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;

			switch (modification)
			{
			case AccessControlModification.Add:
				SecurityDescriptor.DiscretionaryAcl.AddAccess(rule.AccessControlType, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags);
				break;
			default:
				throw new NotImplementedException();
			}

			modified = true;
			return true;
		}
		
		protected void AddAuditRule (AuditRule rule)
		{
			bool modified;
			ModifyAuditRule(AccessControlModification.Add, rule, out modified);
		}
		
		protected bool RemoveAuditRule (AuditRule rule)
		{
			bool modified;
			return ModifyAuditRule(AccessControlModification.Remove, rule, out modified);
		}
		
		protected void RemoveAuditRuleAll (AuditRule rule)
		{
			bool modified;
			ModifyAuditRule(AccessControlModification.RemoveAll, rule, out modified);
		}
		
		protected void RemoveAuditRuleSpecific (AuditRule rule)
		{
			bool modified;
			ModifyAuditRule(AccessControlModification.RemoveSpecific, rule, out modified);
		}
		
		protected void SetAuditRule (AuditRule rule)
		{
			bool modified;
			ModifyAuditRule(AccessControlModification.Set, rule, out modified);
		}
		
		protected override bool ModifyAudit (AccessControlModification modification, AuditRule rule, out bool modified)
		{
			if (rule == null)
				throw new ArgumentNullException("rule");

			SecurityIdentifier sid = rule.IdentityReference.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;

			switch (modification)
			{
			case AccessControlModification.Add:
				SecurityDescriptor.SystemAcl.AddAudit(rule.AuditFlags, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags);
				break;
			default:
				throw new NotImplementedException();
			}

			modified = true;
			return true;
		}
	}
}

