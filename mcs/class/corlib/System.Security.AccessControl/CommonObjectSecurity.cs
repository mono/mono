//
// System.Security.AccessControl.CommonObjectSecurity implementation
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

namespace System.Security.AccessControl {

	[MonoTODO ("required for NativeObjectSecurity - implementation is missing")]
	public abstract class CommonObjectSecurity : ObjectSecurity {

		internal CommonObjectSecurity ()
		{
			/* Give it a 0-param constructor */
		}
		
		protected CommonObjectSecurity (bool isContainer)
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
		
		protected void AddAccessRule (AccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		protected void AddAuditRule (AuditRule rule)
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
		
		protected bool RemoveAccessRule (AccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		protected void RemoveAccessRuleAll (AccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		protected void RemoveAccessRuleSpecific (AccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		protected bool RemoveAuditRule (AuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		protected void RemoveAuditRuleAll (AuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		protected void RemoveAuditRuleSpecific (AuditRule rule)
		{
			throw new NotImplementedException ();
		}
		
		protected void ResetAccessRule (AccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		protected void SetAccessRule (AccessRule rule)
		{
			throw new NotImplementedException ();
		}
		
		protected void SetAuditRule (AuditRule rule)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
