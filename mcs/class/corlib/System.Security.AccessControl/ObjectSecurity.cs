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

#if NET_2_0

using System.Security.Principal;
using System.Runtime.InteropServices;

namespace System.Security.AccessControl
{
	public abstract class ObjectSecurity
	{
		internal ObjectSecurity ()
		{
			/* Give it a 0-param constructor */
		}
		
		protected ObjectSecurity (bool isContainer, bool isDS)
		{
			is_container = isContainer;
			is_ds = isDS;
		}

		bool is_container, is_ds;
		bool access_rules_modified, audit_rules_modified;
		bool group_modified, owner_modified;

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
		
		[MonoTODO]
		public IdentityReference GetGroup (Type targetType)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public IdentityReference GetOwner (Type targetType)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public byte[] GetSecurityDescriptorBinaryForm ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string GetSecurityDescriptorSddlForm (AccessControlSections includeSections)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static bool IsSddlConversionSupported ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual bool ModifyAccessRule (AccessControlModification modification, AccessRule rule, out bool modified)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual bool ModifyAuditRule (AccessControlModification modification, AuditRule rule, out bool modified)
		{
			throw new NotImplementedException ();
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
		
		[MonoTODO]
		public void SetGroup (IdentityReference identity)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void SetOwner (IdentityReference identity)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void SetSecurityDescriptorBinaryForm (byte[] binaryForm)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void SetSecurityDescriptorBinaryForm (byte[] binaryForm, AccessControlSections includeSections)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void SetSecurityDescriptorSddlForm (string sddlForm)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSecurityDescriptorSddlForm (string sddlForm, AccessControlSections includeSections)
		{
			throw new NotImplementedException ();
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
	}
}

#endif
