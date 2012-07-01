//
// System.Security.AccessControl.ObjectSecurity<T>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
		internal ObjectSecurity ()
		{

		}
		
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
			get {
				return typeof(T);
			}
		}
		
		public override Type AccessRuleType {
			get {
				return typeof (AccessRule<T>);
			}
		}
		
		public override Type AuditRuleType {
			get {
				return typeof (AuditRule<T>);
			}
		}
		
		public override AccessRule AccessRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
		{
			return new AccessRule<T> (identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, type);
		}
		
		public override AuditRule AuditRuleFactory(IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
		{
			return new AuditRule<T> (identityReference, accessMask, isInherited, inheritanceFlags, propagationFlags, flags);
		}
	}
}
	
#endif

