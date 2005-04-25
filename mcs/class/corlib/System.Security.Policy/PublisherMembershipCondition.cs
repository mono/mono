//
// PublisherMembershipCondition.cs: Publisher Membership Condition
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;

using Mono.Security.Cryptography;

namespace System.Security.Policy {

	[Serializable]
#if NET_2_0
	[ComVisible (true)]
#endif
	public sealed class PublisherMembershipCondition : IConstantMembershipCondition, IMembershipCondition {

		private readonly int version = 1;

		private X509Certificate x509;

		// so System.Activator.CreateInstance can create an instance...
		internal PublisherMembershipCondition ()
		{
		}

		public PublisherMembershipCondition (X509Certificate certificate) 
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");
			// needed to match MS implementation
			if (certificate.GetHashCode () == 0) {
#if NET_2_0
				throw new ArgumentException ("certificate");
#else
				throw new NullReferenceException ("certificate");
#endif
			}
			x509 = certificate;
		}
	
		public X509Certificate Certificate {
			get { return x509; }
			set { 
				if (value == null)
					throw new ArgumentNullException ("value");
				x509 = value; 
			}
		}
	
		public bool Check (Evidence evidence) 
		{
			if (evidence == null)
				return false;

			IEnumerator e = evidence.GetHostEnumerator ();
			while (e.MoveNext ()) {
				if (e.Current is Publisher) {
					if (x509.Equals ((e.Current as Publisher).Certificate))
						return true;
				}
			}
			return false;
		}
	
		public IMembershipCondition Copy () 
		{
			return new PublisherMembershipCondition (x509);
		}
	
		public override bool Equals (object o) 
		{
			PublisherMembershipCondition pmc = (o as PublisherMembershipCondition);
			if (pmc == null)
				return false;
			return x509.Equals (pmc.Certificate);
		}
	
		public void FromXml (SecurityElement e) 
		{
			FromXml (e, null);
		}
	
		public void FromXml (SecurityElement e, PolicyLevel level) 
		{
			MembershipConditionHelper.CheckSecurityElement (e, "e", version, version);
			string cert = e.Attribute ("X509Certificate");
			if (cert != null) {
				byte[] rawcert = CryptoConvert.FromHex (cert);
				x509 = new X509Certificate (rawcert);
			}
			// PolicyLevel isn't used as there's no need to resolve NamedPermissionSet references
		}
	
		public override int GetHashCode () 
		{
			return x509.GetHashCode ();
		}
	
		public override string ToString () 
		{
			return "Publisher - " + x509.GetPublicKeyString ();
		}

		public SecurityElement ToXml () 
		{
			return ToXml (null);
		}
	
		public SecurityElement ToXml (PolicyLevel level) 
		{
			// PolicyLevel isn't used as there's no need to resolve NamedPermissionSet references
			SecurityElement se = MembershipConditionHelper.Element (typeof (PublisherMembershipCondition), version);
			se.AddAttribute ("X509Certificate", x509.GetRawCertDataString ());
			return se;
		}
	}
}
