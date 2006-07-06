//
// System.Security.AccessControl.MutexSecurity implementation
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

namespace System.Security.AccessControl {
	public sealed class MutexSecurity : NativeObjectSecurity {
		public MutexSecurity ()
		{
		}

		public MutexSecurity (string name,
				      AccessControlSections includeSections)
		{
		}
		
		public override Type AccessRightType
		{
			get {
				throw new NotImplementedException ();
			}
		}
			
		public override Type AccessRuleType
		{
			get {
				throw new NotImplementedException ();
			}
		}
			
		public override Type AuditRuleType
		{
			get {
				throw new NotImplementedException ();
			}
		}
			
		public override AccessRule AccessRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
		{
			throw new NotImplementedException ();
		}
		
		public void AddAccessRule (MutexAccessRule rule)
		{
			throw new NotImplementedException ();
		}

		public void AddAuditRule (MutexAuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		public override AuditRule AuditRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
		{
			throw new NotImplementedException ();
		}
		
		public bool RemoveAccessRule (MutexAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		public void RemoveAccessRuleAll (MutexAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		public void RemoveAccessRuleSpecific (MutexAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		public bool RemoveAuditRule (MutexAuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		public void RemoveAuditRuleAll (MutexAuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		public void RemoveAuditRuleSpecific (MutexAuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		public void ResetAccessRule (MutexAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		public void SetAccessRule (MutexAccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		public void SetAuditRule (MutexAuditRule rule)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
