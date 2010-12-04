//
// System.Security.Policy.NTAccount.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Kenneth Bell
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


using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Security.Principal {

	[ComVisible (false)]
	public sealed class NTAccount : IdentityReference {

		private string _value;

		public NTAccount (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name.Length == 0)
				throw new ArgumentException (Locale.GetText ("Empty"), "name");
			_value = name;
		}

		public NTAccount (string domainName, string accountName)
		{
			if (accountName == null)
				throw new ArgumentNullException ("accountName");
			if (accountName.Length == 0)
				throw new ArgumentException (Locale.GetText ("Empty"), "accountName");
			if (domainName == null)
				_value = accountName;
			else
				_value = domainName + "\\" + accountName;
		}


		public override string Value { 
			get { return _value; }
		}


		public override bool Equals (object o)
		{
			NTAccount nt = (o as NTAccount);
			if (nt == null)
				return false;
			return (nt.Value == Value);
		}

		public override int GetHashCode ()
		{
			return Value.GetHashCode ();
		}

		public override bool IsValidTargetType (Type targetType)
		{
			if (targetType == typeof (NTAccount))
				return true;
			if (targetType == typeof (SecurityIdentifier))
				return true;
			return false;
		}

		public override string ToString ()
		{
			return Value;
		}

		public override IdentityReference Translate (Type targetType)
		{
			if (targetType == typeof (NTAccount))
				return this; // ? copy
			
			if(targetType == typeof(SecurityIdentifier)) {
				WellKnownAccount acct = WellKnownAccount.LookupByName(this.Value);
				if (acct == null || acct.Sid == null)
					throw new IdentityNotMappedException("Cannot map account name: " + this.Value);

				return new SecurityIdentifier(acct.Sid);
			}
			
			throw new ArgumentException("Unknown type", "targetType");
		}

		public static bool operator == (NTAccount left, NTAccount right)
		{
			if (((object)left) == null)
				return (((object)right) == null);
			if (((object)right) == null)
				return false;
			return (left.Value == right.Value);
		}

		public static bool operator != (NTAccount left, NTAccount right)
		{
			if (((object)left) == null)
				return (((object)right) != null);
			if (((object)right) == null)
				return true;
			return (left.Value != right.Value);
		}
	}
}

