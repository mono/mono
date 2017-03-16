//
// Claim.cs
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
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace System.IdentityModel.Claims
{
	[DataContract (Namespace="http://schemas.xmlsoap.org/ws/2005/05/identity")]
	public class Claim
	{
		class ClaimComparer : IEqualityComparer<Claim>
		{
			public bool Equals (Claim c1, Claim c2)
			{
				if (c1 == null)
					return c2 == null;
				else if (c2 == null)
					return false;
				return  c1.ClaimType == c2.ClaimType &&
					c1.Right == c2.Right &&
					c1.Resource != null ? c1.Resource.Equals (c2.Resource) : c2.Resource == null;
			}

			public int GetHashCode (Claim c)
			{
				return  c.ClaimType.GetHashCode () << 24 +
					c.Right.GetHashCode () << 16 +
					(c.Resource != null ? c.Resource.GetHashCode () << 8 : 0);
			}
		}

		// static members
		static readonly ClaimComparer default_comparer = new ClaimComparer ();
		static readonly Claim system = new Claim (ClaimTypes.System, "System", Rights.Identity);

		public static IEqualityComparer<Claim> DefaultComparer {
			get { return default_comparer; }
		}

		public static Claim System {
			get { return system; }
		}

		public static Claim CreateDnsClaim (string dns)
		{
			return new Claim (ClaimTypes.Dns, dns, Rights.PossessProperty);
		}

		[MonoTODO]
		public static Claim CreateDenyOnlyWindowsSidClaim (SecurityIdentifier sid)
		{
			throw new NotImplementedException ();
		}

		public static Claim CreateHashClaim (byte [] hash)
		{
			return new Claim (ClaimTypes.Hash, hash, Rights.PossessProperty);
		}

		public static Claim CreateMailAddressClaim (
			MailAddress mailAddress)
		{
			return new Claim (ClaimTypes.Email, mailAddress, Rights.PossessProperty);
		}

		public static Claim CreateNameClaim (string name)
		{
			return new Claim (ClaimTypes.Name, name, Rights.PossessProperty);
		}

		public static Claim CreateRsaClaim (RSA rsa)
		{
			return new Claim (ClaimTypes.Rsa, rsa, Rights.PossessProperty);
		}

		public static Claim CreateSpnClaim (string spn)
		{
			return new Claim (ClaimTypes.Spn, spn, Rights.PossessProperty);
		}

		public static Claim CreateThumbprintClaim (byte [] thumbprint)
		{
			return new Claim (ClaimTypes.Thumbprint, thumbprint, Rights.PossessProperty);
		}

		public static Claim CreateUpnClaim (string upn)
		{
			return new Claim (ClaimTypes.Upn, upn, Rights.PossessProperty);
		}

		public static Claim CreateUriClaim (Uri uri)
		{
			return new Claim (ClaimTypes.Uri, uri, Rights.PossessProperty);
		}

		public static Claim CreateWindowsSidClaim (
			SecurityIdentifier sid)
		{
			return new Claim (ClaimTypes.Sid, sid, Rights.PossessProperty);
		}

		public static Claim CreateX500DistinguishedNameClaim (
			X500DistinguishedName x500DistinguishedName)
		{
			return new Claim (ClaimTypes.X500DistinguishedName, x500DistinguishedName, Rights.PossessProperty);
		}

		// since those public properties do not expose attributes,
		// here I use them on the fields instead ...
		[DataMember (Name = "ClaimType")]
		string claim_type;
		[DataMember (Name = "Resource")]
		object resource;
		[DataMember (Name = "Right")]
		string right;

		public Claim (string claimType, object resource,
			string right)
		{
			this.claim_type = claimType;
			this.resource = resource;
			this.right = right;
		}

		public object Resource {
			get { return resource; }
		}

		public string ClaimType {
			get { return claim_type; }
		}

		public string Right {
			get { return right; }
		}

		public override string ToString ()
		{
			return String.Concat (Right, ": ", ClaimType);
		}

		public override bool Equals (object obj)
		{
			Claim c = obj as Claim;
			return c != null && DefaultComparer.Equals (this, c);
		}

		public override int GetHashCode ()
		{
			return DefaultComparer.GetHashCode (this);
		}
	}
}
