//
// System.Security.AccessControl.ObjectSecurity<T>
//
// Authors:
//      ?
//	James Bellinger  <jfb@zer7.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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

#if NET_4_0

using System.Runtime.InteropServices;
using System.Security.Principal;

namespace System.Security.AccessControl
{
	public abstract class ObjectSecurity<T> : NativeObjectSecurity where T : struct
	{
		protected ObjectSecurity (bool isContainer,
					  ResourceType resourceType)
			: base (isContainer, resourceType)
		{

		}
		
		protected ObjectSecurity (bool isContainer,
					  ResourceType resourceType,
					  SafeHandle safeHandle,
					  AccessControlSections includeSections)
			: base (isContainer, resourceType, safeHandle, includeSections)
		{

		}
		
		protected ObjectSecurity (bool isContainer,
					  ResourceType resourceType,
					  string name,
					  AccessControlSections includeSections)
			: base (isContainer, resourceType, name, includeSections)
		{

		}
		
		protected ObjectSecurity (bool isContainer,
					  ResourceType resourceType,
					  SafeHandle safeHandle,
					  AccessControlSections includeSections,
					  ExceptionFromErrorCode exceptionFromErrorCode,
					  object exceptionContext)
			: base (isContainer, resourceType, safeHandle, includeSections,
				exceptionFromErrorCode, exceptionContext)
		{

		}
		
		protected ObjectSecurity (bool isContainer,
					  ResourceType resourceType,
					  string name,
					  AccessControlSections includeSections,
					  ExceptionFromErrorCode exceptionFromErrorCode,
					  object exceptionContext)
			: base (isContainer, resourceType, name, includeSections,
				exceptionFromErrorCode, exceptionContext)
		{

		}

		public override Type AccessRightType {
			get { return typeof (T); }
		}
		
		public override Type AccessRuleType {
			get { return typeof (AccessRule<T>); }
		}
		
		public override Type AuditRuleType {
			get { return typeof (AuditRule<T>); }
		}
		
		public override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask,
							     bool isInherited, InheritanceFlags inheritanceFlags,
							     PropagationFlags propagationFlags, AccessControlType type)
		{
			return new AccessRule<T> (identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, type);
		}
		
		public virtual void AddAccessRule (AccessRule<T> rule)
		{
			AddAccessRule ((AccessRule)rule);
		}
		
		public virtual bool RemoveAccessRule (AccessRule<T> rule)
		{
			return RemoveAccessRule ((AccessRule)rule);
		}
		
		public virtual void RemoveAccessRuleAll (AccessRule<T> rule)
		{
			RemoveAccessRuleAll ((AccessRule)rule);
		}
		
		public virtual void RemoveAccessRuleSpecific (AccessRule<T> rule)
		{
			RemoveAccessRuleSpecific ((AccessRule)rule);
		}
		
		public virtual void ResetAccessRule (AccessRule<T> rule)
		{
			ResetAccessRule ((AccessRule)rule);
		}
		
		public virtual void SetAccessRule (AccessRule<T> rule)
		{
			SetAccessRule ((AccessRule)rule);
		}
		
		public override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask,
							   bool isInherited, InheritanceFlags inheritanceFlags,
							   PropagationFlags propagationFlags, AuditFlags flags)
		{
			return new AuditRule<T> (identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, flags);
		}
		
		public virtual void AddAuditRule (AuditRule<T> rule)
		{
			AddAuditRule ((AuditRule)rule);
		}
		
		public virtual bool RemoveAuditRule (AuditRule<T> rule)
		{
			return RemoveAuditRule((AuditRule)rule);
		}
		
		public virtual void RemoveAuditRuleAll (AuditRule<T> rule)
		{
			RemoveAuditRuleAll((AuditRule)rule);
		}
		
		public virtual void RemoveAuditRuleSpecific (AuditRule<T> rule)
		{
			RemoveAuditRuleSpecific((AuditRule)rule);
		}
		
		public virtual void SetAuditRule (AuditRule<T> rule)
		{
			SetAuditRule((AuditRule)rule);
		}
		
		protected void Persist (SafeHandle handle)
		{
			WriteLock ();
			try {
				Persist (handle, AccessControlSectionsModified);
			} finally {
				WriteUnlock ();
			}
		}
		
		protected void Persist (string name)
		{
			WriteLock ();
			try {
				Persist (name, AccessControlSectionsModified);
			} finally {
				WriteUnlock ();
			}
		}
	}
}
	
#endif

