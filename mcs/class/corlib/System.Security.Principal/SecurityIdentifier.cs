//
// System.Security.Policy.SecurityIdentifier class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Security.Principal {

	[MonoTODO ("not implemented")]
	[ComVisible (false)]
	public sealed class SecurityIdentifier : IdentityReference, IComparable<SecurityIdentifier> {

		private string _value;

		public static readonly int MaxBinaryLength = 0;
		public static readonly int MinBinaryLength = 0;


		public SecurityIdentifier (string sddlForm)
		{
			if (sddlForm == null)
				throw new ArgumentNullException ("sddlForm");

			_value = sddlForm.ToUpperInvariant ();
		}

		public SecurityIdentifier (byte[] binaryForm, int offset)
		{
			if (binaryForm == null)
				throw new ArgumentNullException ("binaryForm");
			if ((offset < 0) || (offset > binaryForm.Length - 1))
				throw new ArgumentException ("offset");

			throw new NotImplementedException ();
		}

		public SecurityIdentifier (IntPtr binaryForm)
		{
			throw new NotImplementedException ();
		}

		public SecurityIdentifier (WellKnownSidType sidType, SecurityIdentifier domainSid)
		{
			switch (sidType) {
			case WellKnownSidType.LogonIdsSid:
				throw new ArgumentException ("sidType");
			case WellKnownSidType.AccountAdministratorSid:
			case WellKnownSidType.AccountGuestSid:
			case WellKnownSidType.AccountKrbtgtSid:
			case WellKnownSidType.AccountDomainAdminsSid:
			case WellKnownSidType.AccountDomainUsersSid:
			case WellKnownSidType.AccountDomainGuestsSid:
			case WellKnownSidType.AccountComputersSid:
			case WellKnownSidType.AccountControllersSid:
			case WellKnownSidType.AccountCertAdminsSid:
			case WellKnownSidType.AccountSchemaAdminsSid:
			case WellKnownSidType.AccountEnterpriseAdminsSid:
			case WellKnownSidType.AccountPolicyAdminsSid:
			case WellKnownSidType.AccountRasAndIasServersSid:
				if (domainSid == null)
					throw new ArgumentNullException ("domainSid");
				// TODO
				break;
			default:
				// TODO
				break;
			}
		}

		public SecurityIdentifier AccountDomainSid {
			get { throw new ArgumentNullException ("AccountDomainSid"); }
		}

		public int BinaryLength {
			get { return -1; }
		}

		public override string Value { 
			get { return _value; }
		}

		public int CompareTo (SecurityIdentifier sid)
		{
			return Value.CompareTo (sid.Value);
		}

		public override bool Equals (object o)
		{
			return Equals (o as SecurityIdentifier);
		}

		public bool Equals (SecurityIdentifier sid)
		{
			if (sid == null)
				return false;
			return (sid.Value == Value);
		}

		public void GetBinaryForm (byte[] binaryForm, int offset)
		{
			if (binaryForm == null)
				throw new ArgumentNullException ("binaryForm");
			if ((offset < 0) || (offset > binaryForm.Length - 1 - this.BinaryLength))
				throw new ArgumentException ("offset");

			// TODO
		}

		public override int GetHashCode ()
		{
			return Value.GetHashCode ();
		}

		public bool IsAccountSid ()
		{
			throw new NotImplementedException ();
		}

		public bool IsEqualDomainSid (SecurityIdentifier sid)
		{
			throw new NotImplementedException ();
		}

		public override bool IsValidTargetType (Type targetType)
		{
			if (targetType == typeof (SecurityIdentifier))
				return true;
			if (targetType == typeof (NTAccount))
				return true;
			return false;
		}

		public bool IsWellKnown (WellKnownSidType type)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			return Value;
		}

		public override IdentityReference Translate (Type targetType)
		{
			if (targetType == typeof (SecurityIdentifier))
				return this; // ? copy
			return null;
		}

		public static bool operator == (SecurityIdentifier left, SecurityIdentifier right)
		{
			if (((object)left) == null)
				return (((object)right) == null);
			if (((object)right) == null)
				return false;
			return (left.Value == right.Value);
		}

		public static bool operator != (SecurityIdentifier left, SecurityIdentifier right)
		{
			if (((object)left) == null)
				return (((object)right) != null);
			if (((object)right) == null)
				return true;
			return (left.Value != right.Value);
		}
	}
}

#endif
