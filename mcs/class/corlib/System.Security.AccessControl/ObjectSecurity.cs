//
// System.Security.AccessControl.ObjectSecurity implementation
//
// Author:
//	Dick Porter  <dick@ximian.com>
//
// Copyright (C) 2005, 2006 Novell, Inc (http://www.novell.com)
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

namespace System.Security.AccessControl {

	public abstract class ObjectSecurity {

		internal ObjectSecurity ()
		{
			/* Give it a 0-param constructor */
		}
		
		protected ObjectSecurity (bool isContainer, bool isDS)
		{
		}

		public abstract Type AccessRightType
		{
			get;
		}
		
		public abstract Type AccessRuleType
		{
			get;
		}
		
		public bool AreAccessRulesCanonical
		{
			get {
				throw new NotImplementedException ();
			}
		}
		
		public bool AreAccessRulesProtected
		{
			get {
				throw new NotImplementedException ();
			}
		}
		
		public bool AreAuditRulesCanonical
		{
			get {
				throw new NotImplementedException ();
			}
		}
		
		public bool AreAuditRulesProtected
		{
			get {
				throw new NotImplementedException ();
			}
		}
		
		public abstract Type AuditRuleType
		{
			get;
		}
		
		protected bool AccessRulesModified
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		protected bool AuditRulesModified
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		protected bool GroupModified
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		protected bool IsContainer
		{
			get {
				throw new NotImplementedException ();
			}
		}
		
		protected bool IsDS
		{
			get {
				throw new NotImplementedException ();
			}
		}
		
		protected bool OwnerModified
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
	
		public abstract AccessRule AccessRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type);
		
		public abstract AuditRule AuditRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags);
		
		public IdentityReference GetGroup (Type targetType)
		{
			throw new NotImplementedException ();
		}
		
		public IdentityReference GetOwner (Type targetType)
		{
			throw new NotImplementedException ();
		}
		
		public byte[] GetSecurityDescriptorBinaryForm ()
		{
			throw new NotImplementedException ();
		}
		
		public string GetSecurityDescriptorSddlForm (AccessControlSections includeSections)
		{
			throw new NotImplementedException ();
		}
		
		public static bool IsSddlConversionSupported ()
		{
			throw new NotImplementedException ();
		}
		
		public virtual bool ModifyAccessRule (AccessControlModification modification, AccessRule rule, out bool modified)
		{
			throw new NotImplementedException ();
		}
		
		public virtual bool ModifyAuditRule (AccessControlModification modification, AuditRule rule, out bool modified)
		{
			throw new NotImplementedException ();
		}
		
		public virtual void PurgeAccessRules (IdentityReference identity)
		{
			throw new NotImplementedException ();
		}
		
		public virtual void PurgeAuditRules (IdentityReference identity)
		{
			throw new NotImplementedException ();
		}
		
		public void SetAccessRuleProtection (bool isProtected,
						     bool preserveInheritance)
		{
			throw new NotImplementedException ();
		}
		
		public void SetAuditRuleProtection (bool isProtected,
						    bool preserveInheritance)
		{
			throw new NotImplementedException ();
		}
		
		public void SetGroup (IdentityReference identity)
		{
			throw new NotImplementedException ();
		}
		
		public void SetOwner (IdentityReference identity)
		{
			throw new NotImplementedException ();
		}
		
		public void SetSecurityDescriptorBinaryForm (byte[] binaryForm)
		{
			throw new NotImplementedException ();
		}
		
		public void SetSecurityDescriptorBinaryForm (byte[] binaryForm, AccessControlSections includeSections)
		{
			throw new NotImplementedException ();
		}
		
		public void SetSecurityDescriptorSddlForm (string sddlForm)
		{
			throw new NotImplementedException ();
		}

		public void SetSecurityDescriptorSddlForm (string sddlForm, AccessControlSections includeSections)
		{
			throw new NotImplementedException ();
		}
		
		protected abstract bool ModifyAccess (AccessControlModification modification, AccessRule rule, out bool modified);
		
		protected abstract bool ModifyAudit (AccessControlModification modification, AuditRule rule, out bool modified);
		
		protected virtual void Persist (SafeHandle handle, AccessControlSections includeSections)
		{
			throw new NotImplementedException ();
		}
		
		protected virtual void Persist (string name, AccessControlSections includeSections)
		{
			throw new NotImplementedException ();
		}

		protected virtual void Persist (bool enableOwnershipPrivilege, string name, AccessControlSections includeSections)
		{
			throw new NotImplementedException ();
		}
		
		protected void ReadLock ()
		{
			throw new NotImplementedException ();
		}
		
		protected void ReadUnlock ()
		{
			throw new NotImplementedException ();
		}
		
		protected void WriteLock ()
		{
			throw new NotImplementedException ();
		}
		
		protected void WriteUnlock ()
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
