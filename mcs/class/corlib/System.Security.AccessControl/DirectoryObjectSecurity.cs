//
// System.Security.AccessControl.DirectoryObjectSecurity implementation
//
// Author:
//	Dick Porter  <dick@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

namespace System.Security.AccessControl {
	public abstract class DirectoryObjectSecurity : ObjectSecurity {
		protected DirectoryObjectSecurity ()
			: base (false, true)
		{
		}

		protected DirectoryObjectSecurity (CommonSecurityDescriptor securityDescriptor)
			: base (securityDescriptor != null && securityDescriptor.IsContainer, true)
		{
			if (securityDescriptor == null)
				throw new ArgumentNullException ("securityDescriptor");
		}

		public virtual AccessRule AccessRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type, Guid objectType, Guid inheritedObjectType)
		{
			throw new NotImplementedException ();
		}
		
		public virtual AuditRule AuditRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags, Guid objectType, Guid inheritedObjectType)
		{
			throw new NotImplementedException ();
		}
		
		public AuthorizationRuleCollection GetAccessRules (bool includeExplicit, bool includeInherited, Type targetType)
		{
			throw new NotImplementedException ();
		}
		
		public AuthorizationRuleCollection GetAuditRules (bool includeExplicit, bool includeInherited, Type targetType)
		{
			throw new NotImplementedException ();
		}
		
		protected void AddAccessRule (ObjectAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		protected void AddAuditRule (ObjectAuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		protected override bool ModifyAccess (AccessControlModification modification, AccessRule rule, out bool modified)
		{
			throw new NotImplementedException ();
		}
		
		protected override bool ModifyAudit (AccessControlModification modification, AuditRule rule, out bool modified)
		{
			throw new NotImplementedException ();
		}
		
		protected bool RemoveAccessRule (ObjectAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		protected void RemoveAccessRuleAll (ObjectAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		protected void RemoveAccessRuleSpecific (ObjectAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		protected bool RemoveAuditRule (ObjectAuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		protected void RemoveAuditRuleAll (ObjectAuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		protected void RemoveAuditRuleSpecific (ObjectAuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		protected void ResetAccessRule (ObjectAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		protected void SetAccessRule (ObjectAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		protected void SetAuditRule (ObjectAuditRule rule)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
