//
// System.Security.AccessControl.AuthorizationRule implementation
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

using System.Security.Principal;

namespace System.Security.AccessControl {
	public abstract class AuthorizationRule
	{
		IdentityReference identity;
		int accessMask;
		bool isInherited;
		InheritanceFlags inheritanceFlags;
		PropagationFlags propagationFlags;
		
		internal AuthorizationRule ()
		{
			/* Give it a 0-param constructor */
		}
		
		protected internal AuthorizationRule (IdentityReference identity,
						      int accessMask, bool isInherited,
						      InheritanceFlags inheritanceFlags,
						      PropagationFlags propagationFlags)
		{
			if (null == identity)
				throw new ArgumentNullException ("identity");
				
			if (!(identity is SecurityIdentifier) && !(identity is NTAccount))
				throw new ArgumentException ("identity");

			// Unit testing showed that MS.NET 4.0 actually throws ArgumentException
			// for accessMask == 0, not the ArgumentOutOfRangeException specified.			
			if (accessMask == 0)
				throw new ArgumentException ("accessMask");

			if (0 != (inheritanceFlags & ~(InheritanceFlags.ContainerInherit|InheritanceFlags.ObjectInherit)))
				throw new ArgumentOutOfRangeException ();

			if (0 != (propagationFlags & ~(PropagationFlags.NoPropagateInherit|PropagationFlags.InheritOnly)))
				throw new ArgumentOutOfRangeException ();
			
			this.identity = identity;
			this.accessMask = accessMask;
			this.isInherited = isInherited;
			this.inheritanceFlags = inheritanceFlags;
			this.propagationFlags = propagationFlags;
		}

		public IdentityReference IdentityReference
		{
			get {
				return(identity);
			}
		}
		
		public InheritanceFlags InheritanceFlags
		{
			get {
				return(inheritanceFlags);
			}
		}

		public bool IsInherited
		{
			get {
				return(isInherited);
			}
		}

		public PropagationFlags PropagationFlags
		{
			get {
				return(propagationFlags);
			}
		}

		protected internal int AccessMask
		{
			get {
				return(accessMask);
			}
		}
	}
}

