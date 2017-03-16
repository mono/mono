//
// WindowsClaimSet.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Principal;

namespace System.IdentityModel.Claims
{
	public class WindowsClaimSet : ClaimSet, IDisposable
	{
		WindowsIdentity identity;
		DateTime expiration_time;

		// Constructors

		[MonoTODO]
		public WindowsClaimSet (WindowsIdentity windowsIdentity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public WindowsClaimSet (WindowsIdentity windowsIdentity, bool includeWindowsGroups)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public WindowsClaimSet (WindowsIdentity windowsIdentity, DateTime expirationTime)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public WindowsClaimSet (WindowsIdentity windowsIdentity, bool includeWindowsGroups, DateTime expirationTime)
		{
			throw new NotImplementedException ();
		}

		// Properties

		public override int Count {
			get { throw new NotImplementedException (); }
		}

		public override ClaimSet Issuer {
			get { throw new NotImplementedException (); }
		}

		public override Claim this [int index] {
			get { throw new NotImplementedException (); }
		}

		public DateTime ExpirationTime {
			get { return expiration_time; }
		}

		public WindowsIdentity WindowsIdentity {
			get { return identity; }
		}

		// Methods

		[MonoTODO]
		public void Dispose ()
		{
		}

		[MonoTODO]
		public override IEnumerable<Claim> FindClaims (
			string claimType, string right)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IEnumerator<Claim> GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			return base.ToString ();
		}
	}
}
