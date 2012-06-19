//
// System.Security.AccessControl.ObjectSecurity implementation
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

using System.Security.Principal;
using System.Runtime.InteropServices;

namespace System.Security.AccessControl
{
	public abstract class ObjectSecurity
	{
		const ControlFlags DaclControlFlags
		  = ControlFlags.DiscretionaryAclPresent
		  | ControlFlags.DiscretionaryAclDefaulted
		  | ControlFlags.DiscretionaryAclUntrusted
		  | ControlFlags.DiscretionaryAclAutoInheritRequired
		  | ControlFlags.DiscretionaryAclAutoInherited
		  | ControlFlags.DiscretionaryAclProtected;

		const ControlFlags SaclControlFlags
		  = ControlFlags.SystemAclPresent
		  | ControlFlags.SystemAclDefaulted
		  | ControlFlags.SystemAclAutoInheritRequired
		  | ControlFlags.SystemAclAutoInherited
		  | ControlFlags.SystemAclProtected;

		const string DefaultSd = "";

		internal ObjectSecurity ()
		{
			/* Give it a 0-param constructor */
		}
		
		protected ObjectSecurity (bool isContainer, bool isDS)
		{
			is_container = isContainer;
			is_ds = isDS;
			sd = new CommonSecurityDescriptor(isContainer, isDS, DefaultSd);
		}

		bool is_container, is_ds;
		bool access_rules_modified, audit_rules_modified;
		bool group_modified, owner_modified;
		CommonSecurityDescriptor sd;

		public abstract Type AccessRightType { get; }
		
		public abstract Type AccessRuleType { get; }
		
		public abstract Type AuditRuleType { get; }
		
		[MonoTODO]
		public bool AreAccessRulesCanonical
		{
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool AreAccessRulesProtected
		{
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool AreAuditRulesCanonical
		{
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public bool AreAuditRulesProtected
		{
			get {
				throw new NotImplementedException ();
			}
		}
		
		protected bool AccessRulesModified {
			get { return access_rules_modified; }
			set { access_rules_modified = value; }
		}
		
		protected bool AuditRulesModified {
			get { return audit_rules_modified; }
			set { audit_rules_modified = value; }
		}
		
		protected bool GroupModified {
			get { return group_modified; }
			set { group_modified = value; }
		}
		
		protected bool IsContainer {
			get { return is_container; }
		}
		
		protected bool IsDS {
			get { return is_ds; }
		}
		
		protected bool OwnerModified {
			get { return owner_modified; }
			set { owner_modified = value; }
		}
	
		public abstract AccessRule AccessRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type);
		
		public abstract AuditRule AuditRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags);
		
		public IdentityReference GetGroup (Type targetType)
		{
			if (targetType == typeof(SecurityIdentifier)) {
				return sd.Group;
			} else {
				return sd.Group.Translate (targetType);
			}
		}
		
		public IdentityReference GetOwner (Type targetType)
		{
			if (targetType == typeof(SecurityIdentifier)) {
				return sd.Owner;
			} else {
				return sd.Owner.Translate (targetType);
			}
		}
		
		public byte[] GetSecurityDescriptorBinaryForm ()
		{
			byte[] binForm = new byte[sd.BinaryLength];
			sd.GetBinaryForm (binForm, 0);
			return binForm;
		}
		
		public string GetSecurityDescriptorSddlForm (AccessControlSections includeSections)
		{
			return sd.GetSddlForm (includeSections);
		}
		
		public static bool IsSddlConversionSupported ()
		{
			return true;
		}
		
		public virtual bool ModifyAccessRule (AccessControlModification modification, AccessRule rule, out bool modified)
		{
			return ModifyAccess(modification, rule, out modified);
		}
		
		public virtual bool ModifyAuditRule (AccessControlModification modification, AuditRule rule, out bool modified)
		{
			return ModifyAudit(modification, rule, out modified);
		}
		
		[MonoTODO]
		public virtual void PurgeAccessRules (IdentityReference identity)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual void PurgeAuditRules (IdentityReference identity)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void SetAccessRuleProtection (bool isProtected,
						     bool preserveInheritance)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void SetAuditRuleProtection (bool isProtected,
						    bool preserveInheritance)
		{
			throw new NotImplementedException ();
		}
		
		public void SetGroup (IdentityReference identity)
		{
			group_modified = true;
			sd.Group = (SecurityIdentifier)identity.Translate (typeof(SecurityIdentifier));
		}
		
		public void SetOwner (IdentityReference identity)
		{
			owner_modified = true;
			sd.Owner = (SecurityIdentifier)identity.Translate (typeof(SecurityIdentifier));
		}
		
		public void SetSecurityDescriptorBinaryForm (byte[] binaryForm)
		{
			SetSecurityDescriptorBinaryForm (binaryForm, AccessControlSections.All);
		}
		
		public void SetSecurityDescriptorBinaryForm (byte[] binaryForm, AccessControlSections includeSections)
		{
			RawSecurityDescriptor raw_sd = new RawSecurityDescriptor (binaryForm, 0);

			if ((includeSections & AccessControlSections.Owner) != 0) {
				owner_modified = true;
				sd.Owner = raw_sd.Owner;
				SetFlags(raw_sd.ControlFlags, ControlFlags.OwnerDefaulted);
			}

			if ((includeSections & AccessControlSections.Group) != 0) {
				group_modified = true;
				sd.Group = raw_sd.Group;
				SetFlags(raw_sd.ControlFlags, ControlFlags.GroupDefaulted);
			}
			
			if ((includeSections & AccessControlSections.Audit) != 0) {
				audit_rules_modified = true;
				sd.SystemAcl = new SystemAcl(IsContainer, IsDS, raw_sd.SystemAcl);
				SetFlags(raw_sd.ControlFlags, SaclControlFlags);
			}
			
			if ((includeSections & AccessControlSections.Access) != 0) {
				access_rules_modified = true;
				sd.DiscretionaryAcl = new DiscretionaryAcl(IsContainer, IsDS, raw_sd.DiscretionaryAcl);
				SetFlags(raw_sd.ControlFlags, DaclControlFlags);
			}
		}
		
		public void SetSecurityDescriptorSddlForm (string sddlForm)
		{
			SetSecurityDescriptorSddlForm (sddlForm, AccessControlSections.All);
		}

		public void SetSecurityDescriptorSddlForm (string sddlForm, AccessControlSections includeSections)
		{
			RawSecurityDescriptor raw_sd = new RawSecurityDescriptor (sddlForm);

			if ((includeSections & AccessControlSections.Owner) != 0) {
				owner_modified = true;
				sd.Owner = raw_sd.Owner;
				SetFlags(raw_sd.ControlFlags, ControlFlags.OwnerDefaulted);
			}

			if ((includeSections & AccessControlSections.Group) != 0) {
				group_modified = true;
				sd.Group = raw_sd.Group;
				SetFlags(raw_sd.ControlFlags, ControlFlags.GroupDefaulted);
			}
			
			if ((includeSections & AccessControlSections.Audit) != 0) {
				audit_rules_modified = true;
				sd.SystemAcl = new SystemAcl(IsContainer, IsDS, raw_sd.SystemAcl);
				SetFlags(raw_sd.ControlFlags, SaclControlFlags);
			}
			
			if ((includeSections & AccessControlSections.Access) != 0) {
				access_rules_modified = true;
				sd.DiscretionaryAcl = new DiscretionaryAcl(IsContainer, IsDS, raw_sd.DiscretionaryAcl);
				SetFlags(raw_sd.ControlFlags, DaclControlFlags);
			}
		}
		
		protected abstract bool ModifyAccess (AccessControlModification modification, AccessRule rule, out bool modified);
		
		protected abstract bool ModifyAudit (AccessControlModification modification, AuditRule rule, out bool modified);
		
		[MonoTODO]
		protected virtual void Persist (SafeHandle handle, AccessControlSections includeSections)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void Persist (string name, AccessControlSections includeSections)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void Persist (bool enableOwnershipPrivilege, string name, AccessControlSections includeSections)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected void ReadLock ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected void ReadUnlock ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected void WriteLock ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected void WriteUnlock ()
		{
			throw new NotImplementedException ();
		}

		protected CommonSecurityDescriptor SecurityDescriptor
	  	{
			get { return sd; }
		}

		private void SetFlags(ControlFlags flags, ControlFlags which)
		{
			sd.SetControlFlags((flags & which) | (sd.ControlFlags & ~which));
		}
	}
}
