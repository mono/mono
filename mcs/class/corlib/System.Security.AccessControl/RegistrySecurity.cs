//
// System.Security.AccessControl.RegistrySecurity implementation
//
// Authors:
//	Dick Porter  <dick@ximian.com>
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006-2007 Novell, Inc (http://www.novell.com)
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
	public sealed class RegistrySecurity : NativeObjectSecurity {
		public RegistrySecurity ()
		{
		}
		
		public override Type AccessRightType {
			get { return typeof (RegistryRights); }
		}
		
		public override Type AccessRuleType {
			get { return typeof (RegistryAccessRule); }
		}

		public override Type AuditRuleType {
			get { return typeof (RegistryAuditRule); }
		}
		
		public override AccessRule AccessRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
		{
			// FIXME: isInherited is unused
			return new RegistryAccessRule (identityReference, (RegistryRights) accessMask, inheritanceFlags, propagationFlags, type);
		}
		
		public void AddAccessRule (RegistryAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		public void AddAuditRule (RegistryAuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		public override AuditRule AuditRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
		{
			// FIXME: isInherited is unused
			return new RegistryAuditRule (identityReference, (RegistryRights) accessMask, inheritanceFlags, propagationFlags, flags);
		}
		
		public bool RemoveAccessRule (RegistryAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		public void RemoveAccessRuleAll (RegistryAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		public void RemoveAccessRuleSpecific (RegistryAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		public bool RemoveAuditRule (RegistryAuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		public void RemoveAuditRuleAll (RegistryAuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		public void RemoveAuditRuleSpecific (RegistryAuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		public void ResetAccessRule (RegistryAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		public void SetAccessRule (RegistryAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		public void SetAuditRule (RegistryAuditRule rule)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
