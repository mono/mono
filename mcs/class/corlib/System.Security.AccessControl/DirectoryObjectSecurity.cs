//
// System.Security.AccessControl.DirectoryObjectSecurity implementation
//
// Author:
//	Dick Porter  <dick@ximian.com>
//	James Bellinger  <jfb@zer7.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

using System.Security.Principal;

namespace System.Security.AccessControl
{
	public abstract class DirectoryObjectSecurity : ObjectSecurity
	{
		protected DirectoryObjectSecurity ()
			: base (true, true)
		{
		}

		protected DirectoryObjectSecurity (CommonSecurityDescriptor securityDescriptor)
			: base (securityDescriptor)
		{
		}

		// For MoMA. NotImplementedException is correct for this base class.
		Exception GetNotImplementedException ()
		{
			return new NotImplementedException ();
		}
		
		public virtual AccessRule AccessRuleFactory (IdentityReference identityReference, int accessMask,
							     bool isInherited, InheritanceFlags inheritanceFlags,
							     PropagationFlags propagationFlags, AccessControlType type,
							     Guid objectType, Guid inheritedObjectType)
		{
			throw GetNotImplementedException ();
		}
		
		internal override AccessRule InternalAccessRuleFactory (QualifiedAce ace, Type targetType,
									AccessControlType type)
		{
			ObjectAce oace = ace as ObjectAce;
			if (null == oace || ObjectAceFlags.None == oace.ObjectAceFlags)
				return base.InternalAccessRuleFactory (ace, targetType, type);
			
			return AccessRuleFactory (ace.SecurityIdentifier.Translate (targetType),
						  ace.AccessMask, ace.IsInherited,
						  ace.InheritanceFlags, ace.PropagationFlags, type,
						  oace.ObjectAceType, oace.InheritedObjectAceType);
		}
		
		public virtual AuditRule AuditRuleFactory (IdentityReference identityReference, int accessMask,
							   bool isInherited, InheritanceFlags inheritanceFlags,
							   PropagationFlags propagationFlags, AuditFlags flags,
							   Guid objectType, Guid inheritedObjectType)
		{
			throw GetNotImplementedException ();
		}
				
		internal override AuditRule InternalAuditRuleFactory (QualifiedAce ace, Type targetType)
		{
			ObjectAce oace = ace as ObjectAce;
			if (null == oace || ObjectAceFlags.None == oace.ObjectAceFlags)
				return base.InternalAuditRuleFactory (ace, targetType);
			
			return AuditRuleFactory (ace.SecurityIdentifier.Translate (targetType),
						 ace.AccessMask, ace.IsInherited,
						 ace.InheritanceFlags, ace.PropagationFlags, ace.AuditFlags,
						 oace.ObjectAceType, oace.InheritedObjectAceType);
		}
		
		public AuthorizationRuleCollection GetAccessRules (bool includeExplicit, bool includeInherited, Type targetType)
		{
			return InternalGetAccessRules (includeExplicit, includeInherited, targetType);
		}
		
		public AuthorizationRuleCollection GetAuditRules (bool includeExplicit, bool includeInherited, Type targetType)
		{
			return InternalGetAuditRules (includeExplicit, includeInherited, targetType);
		}
		
		protected void AddAccessRule (ObjectAccessRule rule)
		{
			bool modified;
			ModifyAccess (AccessControlModification.Add, rule, out modified);
		}
		
		protected bool RemoveAccessRule (ObjectAccessRule rule)
		{
			bool modified;
			return ModifyAccess (AccessControlModification.Remove, rule, out modified);
		}
		
		protected void RemoveAccessRuleAll (ObjectAccessRule rule)
		{
			bool modified;
			ModifyAccess (AccessControlModification.RemoveAll, rule, out modified);
		}
		
		protected void RemoveAccessRuleSpecific (ObjectAccessRule rule)
		{
			bool modified;
			ModifyAccess (AccessControlModification.RemoveSpecific, rule, out modified);
		}
		
		protected void ResetAccessRule (ObjectAccessRule rule)
		{
			bool modified;
			ModifyAccess (AccessControlModification.Reset, rule, out modified);
		}
		
		protected void SetAccessRule (ObjectAccessRule rule)
		{
			bool modified;
			ModifyAccess (AccessControlModification.Set, rule, out modified);
		}
		
		protected override bool ModifyAccess (AccessControlModification modification, AccessRule rule, out bool modified)
		{
			if (null == rule)
				throw new ArgumentNullException ("rule");
				
			ObjectAccessRule orule = rule as ObjectAccessRule;
			if (null == orule)
				throw new ArgumentException ("rule");
				
			modified = true;
			
			WriteLock ();
			try {
				switch (modification) {
				case AccessControlModification.Add:
					descriptor.DiscretionaryAcl.AddAccess (orule.AccessControlType,
									       SidFromIR (orule.IdentityReference),
									       orule.AccessMask,
									       orule.InheritanceFlags,
									       orule.PropagationFlags,
									       orule.ObjectFlags,
									       orule.ObjectType,
									       orule.InheritedObjectType);
					break;
				case AccessControlModification.Set:
					descriptor.DiscretionaryAcl.SetAccess (orule.AccessControlType,
									       SidFromIR (orule.IdentityReference),
									       orule.AccessMask,
									       orule.InheritanceFlags,
									       orule.PropagationFlags,
									       orule.ObjectFlags,
									       orule.ObjectType,
									       orule.InheritedObjectType);
					break;
				case AccessControlModification.Reset:
					PurgeAccessRules (orule.IdentityReference);
					goto case AccessControlModification.Add;
				case AccessControlModification.Remove:
					modified = descriptor.DiscretionaryAcl.RemoveAccess (orule.AccessControlType,
											     SidFromIR (orule.IdentityReference),
											     rule.AccessMask,
											     orule.InheritanceFlags,
											     orule.PropagationFlags,
											     orule.ObjectFlags,
											     orule.ObjectType,
											     orule.InheritedObjectType);
					break;
				case AccessControlModification.RemoveAll:
					PurgeAccessRules (orule.IdentityReference);
					break;
				case AccessControlModification.RemoveSpecific:
					descriptor.DiscretionaryAcl.RemoveAccessSpecific (orule.AccessControlType,
											  SidFromIR (orule.IdentityReference),
											  orule.AccessMask,
											  orule.InheritanceFlags,
											  orule.PropagationFlags,
											  orule.ObjectFlags,
											  orule.ObjectType,
											  orule.InheritedObjectType);
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
						
		protected void AddAuditRule (ObjectAuditRule rule)
		{
			bool modified;
			ModifyAudit (AccessControlModification.Add, rule, out modified);
		}
		
		protected bool RemoveAuditRule (ObjectAuditRule rule)
		{
			bool modified;
			return ModifyAudit (AccessControlModification.Remove, rule, out modified);
		}
		
		protected void RemoveAuditRuleAll (ObjectAuditRule rule)
		{
			bool modified;
			ModifyAudit (AccessControlModification.RemoveAll, rule, out modified);
		}
		
		protected void RemoveAuditRuleSpecific (ObjectAuditRule rule)
		{
			bool modified;
			ModifyAudit (AccessControlModification.RemoveSpecific, rule, out modified);
		}
		
		protected void SetAuditRule (ObjectAuditRule rule)
		{
			bool modified;
			ModifyAudit (AccessControlModification.Set, rule, out modified);
		}
		
		protected override bool ModifyAudit (AccessControlModification modification, AuditRule rule, out bool modified)
		{
			if (null == rule)
				throw new ArgumentNullException ("rule");

			ObjectAuditRule orule = rule as ObjectAuditRule;
			if (null == orule)
				throw new ArgumentException ("rule");

			modified = true;
			
			WriteLock ();
			try {
				switch (modification) {
				case AccessControlModification.Add:
					if (null == descriptor.SystemAcl)
						descriptor.SystemAcl = new SystemAcl (IsContainer, IsDS, 1);
					
					descriptor.SystemAcl.AddAudit (orule.AuditFlags,
								       SidFromIR (orule.IdentityReference),
								       orule.AccessMask,
								       orule.InheritanceFlags,
								       orule.PropagationFlags,
								       orule.ObjectFlags,
								       orule.ObjectType,
								       orule.InheritedObjectType);
					break;
				case AccessControlModification.Set:
					if (null == descriptor.SystemAcl)
						descriptor.SystemAcl = new SystemAcl (IsContainer, IsDS, 1);

					descriptor.SystemAcl.SetAudit (orule.AuditFlags,
								       SidFromIR (orule.IdentityReference),
								       orule.AccessMask,
								       orule.InheritanceFlags,
								       orule.PropagationFlags,
								       orule.ObjectFlags,
								       orule.ObjectType,
								       orule.InheritedObjectType);
					break;
				case AccessControlModification.Reset:
					break;
				case AccessControlModification.Remove:
					if (null == descriptor.SystemAcl)
						modified = false;
					else
						modified = descriptor.SystemAcl.RemoveAudit (orule.AuditFlags,
											     SidFromIR (orule.IdentityReference),
											     orule.AccessMask,
											     orule.InheritanceFlags,
											     orule.PropagationFlags,
											     orule.ObjectFlags,
											     orule.ObjectType,
											     orule.InheritedObjectType);
					break;
				case AccessControlModification.RemoveAll:
					PurgeAuditRules (orule.IdentityReference);
					break;
				case AccessControlModification.RemoveSpecific:
					if (null != descriptor.SystemAcl)
						descriptor.SystemAcl.RemoveAuditSpecific (orule.AuditFlags,
											  SidFromIR (orule.IdentityReference),
											  orule.AccessMask,
											  orule.InheritanceFlags,
											  orule.PropagationFlags,
											  orule.ObjectFlags,
											  orule.ObjectType,
											  orule.InheritedObjectType);
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

