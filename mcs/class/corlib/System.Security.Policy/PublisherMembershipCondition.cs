//
// PublisherMembershipCondition.cs: Publisher Membership Condition
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Security.Cryptography.X509Certificates;
using System.Text;

using Mono.Security.Cryptography;

namespace System.Security.Policy {

	[Serializable]
	public sealed class PublisherMembershipCondition
                : IConstantMembershipCondition, IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable {
	
		private X509Certificate x509;

		// so System.Activator.CreateInstance can create an instance...
		internal PublisherMembershipCondition ()
		{
		}

		// LAMESPEC: Undocumented ArgumentNullException exception
		public PublisherMembershipCondition (X509Certificate certificate) 
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");
			// needed to match MS implementation
			if (certificate.GetRawCertData () == null)
				throw new NullReferenceException ("certificate");
			x509 = certificate;
		}
	
		// LAMESPEC: Undocumented ArgumentNullException exception
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
			if (!(o is PublisherMembershipCondition))
				throw new ArgumentException ("not a PublisherMembershipCondition");
			return x509.Equals ((o as PublisherMembershipCondition).Certificate);
		}
	
		public void FromXml (SecurityElement e) 
		{
			FromXml (e, null);
		}
	
		public void FromXml (SecurityElement e, PolicyLevel level) 
		{
			if (e == null)
				throw new ArgumentNullException ("e");
			if (e.Tag != "IMembershipCondition")
				throw new ArgumentException ("Not IMembershipCondition", "e");
			// PolicyLevel isn't used as there's no need to resolve NamedPermissionSet references
			string cert = e.Attribute ("X509Certificate");
			if (cert != null) {
				byte[] rawcert = CryptoConvert.FromHex (cert);
				x509 = new X509Certificate (rawcert);
			}
		}
	
		public override int GetHashCode () 
		{
			return x509.GetHashCode ();
		}
	
		public override string ToString () 
		{
			return "Publisher - " + x509.GetPublicKeyString ();
		}

		// snippet moved from FileIOPermission (nickd) to be reused in all derived classes
		internal SecurityElement Element (object o, int version) 
		{
			SecurityElement se = new SecurityElement ("IMembershipCondition");
			Type type = this.GetType ();
			StringBuilder asmName = new StringBuilder (type.Assembly.ToString ());
			asmName.Replace ('\"', '\'');
			se.AddAttribute ("class", type.FullName + ", " + asmName);
			se.AddAttribute ("version", version.ToString ());
			return se;
		}
	
		public SecurityElement ToXml () 
		{
			return ToXml (null);
		}
	
		public SecurityElement ToXml (PolicyLevel level) 
		{
			// PolicyLevel isn't used as there's no need to resolve NamedPermissionSet references
			SecurityElement se = Element (this, 1);
			se.AddAttribute ("X509Certificate", x509.GetRawCertDataString ());
			return se;
		}
	}
}
