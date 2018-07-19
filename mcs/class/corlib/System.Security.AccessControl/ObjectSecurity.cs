//
// System.Security.AccessControl.ObjectSecurity implementation
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
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.ExceptionServices;

namespace System.Security.AccessControl
{
	public abstract class ObjectSecurity
	{
		protected ObjectSecurity ()
		{
		}

		protected ObjectSecurity (CommonSecurityDescriptor securityDescriptor)
		{
			if (securityDescriptor == null)
				throw new ArgumentNullException ("securityDescriptor");
				
			descriptor = securityDescriptor;
			rw_lock = new ReaderWriterLock ();
		}
		
		protected ObjectSecurity (bool isContainer, bool isDS)
			: this (new CommonSecurityDescriptor
				(isContainer, isDS, ControlFlags.None, null, null, null,
				 new DiscretionaryAcl (isContainer, isDS, 0)))
		{
		}
		
		internal CommonSecurityDescriptor descriptor;
		AccessControlSections sections_modified;
		ReaderWriterLock rw_lock;

		public abstract Type AccessRightType { get; }
		
		public abstract Type AccessRuleType { get; }
		
		public abstract Type AuditRuleType { get; }
		
		public bool AreAccessRulesCanonical {
			get {
				ReadLock ();
				try {
					return descriptor.IsDiscretionaryAclCanonical;
				} finally {
					ReadUnlock ();
				}
			}
		}
		
		public bool AreAccessRulesProtected {
			get {
				ReadLock ();
				try {
					return 0 != (descriptor.ControlFlags & ControlFlags.DiscretionaryAclProtected);
				} finally {
					ReadUnlock ();
				}
			}
		}
		
		public bool AreAuditRulesCanonical {
			get {
				ReadLock ();
				try {
					return descriptor.IsSystemAclCanonical;
				} finally {
					ReadUnlock ();
				}
			}
		}
		
		public bool AreAuditRulesProtected {
			get {
				ReadLock ();
				try {
					return 0 != (descriptor.ControlFlags & ControlFlags.SystemAclProtected);
				} finally {
					ReadUnlock ();
				}
			}
		}
		
		internal AccessControlSections AccessControlSectionsModified {
			get { Reading (); return sections_modified; }
			set { Writing (); sections_modified = value; }
		}

		protected bool AccessRulesModified {
			get { return AreAccessControlSectionsModified (AccessControlSections.Access); }
			set { SetAccessControlSectionsModified (AccessControlSections.Access, value); }
		}
		
		protected bool AuditRulesModified {
			get { return AreAccessControlSectionsModified (AccessControlSections.Audit); }
			set { SetAccessControlSectionsModified (AccessControlSections.Audit, value); }
		}
		
		protected bool GroupModified {
			get { return AreAccessControlSectionsModified (AccessControlSections.Group); }
			set { SetAccessControlSectionsModified (AccessControlSections.Group, value); }
		}
		
		protected bool IsContainer {
			get { return descriptor.IsContainer; }
		}
		
		protected bool IsDS {
			get { return descriptor.IsDS; }
		}
		
		protected bool OwnerModified {
			get { return AreAccessControlSectionsModified (AccessControlSections.Owner); }
			set { SetAccessControlSectionsModified (AccessControlSections.Owner, value); }
		}
		
		public abstract AccessRule AccessRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type);
		
		public abstract AuditRule AuditRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags);		
				
		public IdentityReference GetGroup (Type targetType)
		{
			ReadLock ();
			try {
				if (descriptor.Group == null)
					return null;
				
				return descriptor.Group.Translate (targetType);
			} finally {
				ReadUnlock ();
			}
		}
		
		public IdentityReference GetOwner (Type targetType)
		{
			ReadLock ();
			try {
				if (descriptor.Owner == null)
					return null;
				
				return descriptor.Owner.Translate (targetType);
			} finally {
				ReadUnlock ();
			}
		}
		
		public byte[] GetSecurityDescriptorBinaryForm ()
		{
			ReadLock ();
			try {
				byte[] binaryForm = new byte[descriptor.BinaryLength];
				descriptor.GetBinaryForm (binaryForm, 0);
				return binaryForm;
			} finally {
				ReadUnlock ();
			}
		}
		
		public string GetSecurityDescriptorSddlForm (AccessControlSections includeSections)
		{
			ReadLock ();
			try {
				return descriptor.GetSddlForm (includeSections);
			} finally {
				ReadUnlock ();
			}
		}

		public static bool IsSddlConversionSupported ()
		{
			return GenericSecurityDescriptor.IsSddlConversionSupported ();
		}
		
		public virtual bool ModifyAccessRule (AccessControlModification modification, AccessRule rule, out bool modified)
		{
			if (rule == null)
				throw new ArgumentNullException ("rule");
				
			if (!AccessRuleType.IsAssignableFrom (rule.GetType()))
				throw new ArgumentException ("rule");

			return ModifyAccess (modification, rule, out modified);
		}
		
		public virtual bool ModifyAuditRule (AccessControlModification modification, AuditRule rule, out bool modified)
		{
			if (rule == null)
				throw new ArgumentNullException ("rule");

			if (!AuditRuleType.IsAssignableFrom (rule.GetType()))
				throw new ArgumentException ("rule");
				
			return ModifyAudit (modification, rule, out modified);
		}
		
		public virtual void PurgeAccessRules (IdentityReference identity)
		{
			if (null == identity)
				throw new ArgumentNullException ("identity");
				
			WriteLock ();
			try {
				descriptor.PurgeAccessControl (SidFromIR (identity));
			} finally {
				WriteUnlock ();
			}
		}
		
		public virtual void PurgeAuditRules (IdentityReference identity)
		{
			if (null == identity)
				throw new ArgumentNullException ("identity");
				
			WriteLock ();
			try {
				descriptor.PurgeAudit (SidFromIR (identity));
			} finally {
				WriteUnlock ();
			}
		}
		
		public void SetAccessRuleProtection (bool isProtected,
						     bool preserveInheritance)
		{
			WriteLock ();
			try {
				descriptor.SetDiscretionaryAclProtection (isProtected, preserveInheritance);
			} finally {
				WriteUnlock();
			}
		}
		
		public void SetAuditRuleProtection (bool isProtected,
						    bool preserveInheritance)
		{
			WriteLock ();
			try {
				descriptor.SetSystemAclProtection (isProtected, preserveInheritance);
			} finally {
				WriteUnlock ();
			}
		}
		
		public void SetGroup (IdentityReference identity)
		{
			WriteLock ();
			try {
				descriptor.Group = SidFromIR (identity);
				GroupModified = true;
			} finally {
				WriteUnlock ();
			}
		}
		
		public void SetOwner (IdentityReference identity)
		{
			WriteLock ();
			try {
				descriptor.Owner = SidFromIR (identity);
				OwnerModified = true;
			} finally {
				WriteUnlock ();
			}
		}
		
		public void SetSecurityDescriptorBinaryForm (byte[] binaryForm)
		{
			SetSecurityDescriptorBinaryForm (binaryForm, AccessControlSections.All);
		}
		
		public void SetSecurityDescriptorBinaryForm (byte[] binaryForm, AccessControlSections includeSections)
		{
			CopySddlForm (new CommonSecurityDescriptor (IsContainer, IsDS, binaryForm, 0), includeSections);
		}
		
		public void SetSecurityDescriptorSddlForm (string sddlForm)
		{
			SetSecurityDescriptorSddlForm (sddlForm, AccessControlSections.All);
		}

		public void SetSecurityDescriptorSddlForm (string sddlForm, AccessControlSections includeSections)
		{
			CopySddlForm (new CommonSecurityDescriptor (IsContainer, IsDS, sddlForm), includeSections);
		}
		
		void CopySddlForm (CommonSecurityDescriptor sourceDescriptor, AccessControlSections includeSections)
		{
			WriteLock ();
			try {
				AccessControlSectionsModified |= includeSections;
				if (0 != (includeSections & AccessControlSections.Audit))
					descriptor.SystemAcl = sourceDescriptor.SystemAcl;
				if (0 != (includeSections & AccessControlSections.Access))
					descriptor.DiscretionaryAcl = sourceDescriptor.DiscretionaryAcl;
				if (0 != (includeSections & AccessControlSections.Owner))
					descriptor.Owner = sourceDescriptor.Owner;
				if (0 != (includeSections & AccessControlSections.Group))
					descriptor.Group = sourceDescriptor.Group;
			} finally {
				WriteUnlock ();
			}
		}
		
		protected abstract bool ModifyAccess (AccessControlModification modification, AccessRule rule, out bool modified);
		
		protected abstract bool ModifyAudit (AccessControlModification modification, AuditRule rule, out bool modified);
		
		// For MoMA. NotImplementedException is correct for this base class.
		Exception GetNotImplementedException ()
		{
			return new NotImplementedException ();
		}
		
		protected virtual void Persist (SafeHandle handle, AccessControlSections includeSections)
		{
			throw GetNotImplementedException ();
		}
		
		protected virtual void Persist (string name, AccessControlSections includeSections)
		{
			throw GetNotImplementedException ();
		}
		
		[MonoTODO]
		[HandleProcessCorruptedStateExceptions]
		protected virtual void Persist (bool enableOwnershipPrivilege, string name, AccessControlSections includeSections)
		{
			throw new NotImplementedException ();
		}
		
		void Reading ()
		{
			if (!rw_lock.IsReaderLockHeld && !rw_lock.IsWriterLockHeld)
				throw new InvalidOperationException ("Either a read or a write lock must be held.");
		}
		
		protected void ReadLock ()
		{
			rw_lock.AcquireReaderLock (Timeout.Infinite);
		}
		
		protected void ReadUnlock ()
		{
			rw_lock.ReleaseReaderLock ();
		}
		
		void Writing ()
		{
			if (!rw_lock.IsWriterLockHeld)
				throw new InvalidOperationException ("Write lock must be held.");
		}
		
		protected void WriteLock ()
		{
			rw_lock.AcquireWriterLock (Timeout.Infinite);
		}
		
		protected void WriteUnlock ()
		{
			rw_lock.ReleaseWriterLock ();
		}
		
		internal AuthorizationRuleCollection InternalGetAccessRules (bool includeExplicit,
									     bool includeInherited,
									     Type targetType)
		{
			List<AuthorizationRule> rules = new List<AuthorizationRule> ();
			
			ReadLock ();
			try {
				foreach (GenericAce genericAce in descriptor.DiscretionaryAcl) {
					QualifiedAce ace = genericAce as QualifiedAce;
					if (null == ace) continue;
					if (ace.IsInherited && !includeInherited) continue;
					if (!ace.IsInherited && !includeExplicit) continue;
							
					AccessControlType type;
					if (AceQualifier.AccessAllowed == ace.AceQualifier)
						type = AccessControlType.Allow;
					else if (AceQualifier.AccessDenied == ace.AceQualifier)
						type = AccessControlType.Deny;
					else
						continue;
						
					AccessRule rule = InternalAccessRuleFactory (ace, targetType, type);
					rules.Add (rule);
				}
			} finally {
				ReadUnlock ();
			}
			
			return new AuthorizationRuleCollection (rules.ToArray ());
		}
		
		internal virtual AccessRule InternalAccessRuleFactory (QualifiedAce ace, Type targetType,
								       AccessControlType type)
		{
			return AccessRuleFactory (ace.SecurityIdentifier.Translate (targetType),
						  ace.AccessMask, ace.IsInherited,
						  ace.InheritanceFlags, ace.PropagationFlags, type);
		}
 
		internal AuthorizationRuleCollection InternalGetAuditRules (bool includeExplicit,
									    bool includeInherited,
									    Type targetType)
		{
			List<AuthorizationRule> rules = new List<AuthorizationRule> ();
			
			ReadLock ();
			try {
				if (null != descriptor.SystemAcl) {
					foreach (GenericAce genericAce in descriptor.SystemAcl) {
						QualifiedAce ace = genericAce as QualifiedAce;
						if (null == ace) continue;
						if (ace.IsInherited && !includeInherited) continue;
						if (!ace.IsInherited && !includeExplicit) continue;
				
						if (AceQualifier.SystemAudit != ace.AceQualifier) continue;
						
						AuditRule rule = InternalAuditRuleFactory (ace, targetType);
						rules.Add (rule);
					}
				}
			} finally {
				ReadUnlock ();
			}
			
			return new AuthorizationRuleCollection (rules.ToArray ());
		}
		
		internal virtual AuditRule InternalAuditRuleFactory (QualifiedAce ace, Type targetType)
		{
			return AuditRuleFactory (ace.SecurityIdentifier.Translate (targetType),
						 ace.AccessMask, ace.IsInherited,
						 ace.InheritanceFlags, ace.PropagationFlags, ace.AuditFlags);
		}
				
		internal static SecurityIdentifier SidFromIR (IdentityReference identity)
		{
			if (null == identity)
				throw new ArgumentNullException ("identity");
		
			return (SecurityIdentifier)identity.Translate (typeof (SecurityIdentifier));
		}
		
		bool AreAccessControlSectionsModified (AccessControlSections mask)
		{
			return 0 != (AccessControlSectionsModified & mask);
		}
		
		void SetAccessControlSectionsModified(AccessControlSections mask, bool modified)
		{
			if (modified)
				AccessControlSectionsModified |= mask;
			else
				AccessControlSectionsModified &= ~mask;
		}
	}
}

