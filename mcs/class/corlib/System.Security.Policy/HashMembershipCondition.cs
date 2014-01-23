//
// System.Security.Policy.HashMembershipCondition.cs
//
// Authors:
//	Jackson Harper (Jackson@LatitudeGeo.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Jackson Harper, All rights reserved
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
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;

using Mono.Security.Cryptography;

namespace System.Security.Policy {

	[Serializable]
	[ComVisible (true)]
	public sealed class HashMembershipCondition : IMembershipCondition, IDeserializationCallback, ISerializable {
		private readonly int version = 1;

		private HashAlgorithm hash_algorithm;
		private byte[] hash_value;

		// so System.Activator.CreateInstance can create an instance...
		internal HashMembershipCondition ()
		{
		}

		public HashMembershipCondition (HashAlgorithm hashAlg, byte[] value)
		{
			if (hashAlg == null)
				throw new ArgumentNullException ("hashAlg");
			if (value == null)
				throw new ArgumentNullException ("value");
				
			this.hash_algorithm = hashAlg;
			this.hash_value = (byte[]) value.Clone ();
		}

		//
		// Public Properties
		//
		
		public HashAlgorithm HashAlgorithm {
			get {
				if (hash_algorithm == null)
					hash_algorithm = new SHA1Managed ();
				return hash_algorithm;
			}
			set { 
				if (value == null)
					throw new ArgumentNullException ("HashAlgorithm");
				hash_algorithm = value; 
			}
		}

		public byte[] HashValue {
			get {
				if (hash_value == null)
					throw new ArgumentException (Locale.GetText ("No HashValue available."));
				return (byte[]) hash_value.Clone ();
			}
			set { 
				if (value == null)
					throw new ArgumentNullException ("HashValue");
				hash_value = (byte[]) value.Clone ();
			} 
		}

		//
		// Public Methods
		//

		public bool Check (Evidence evidence)
		{
			if (evidence == null)
				return false;

			IEnumerator e = evidence.GetHostEnumerator ();
			while (e.MoveNext ()) {
				Hash hash = (e.Current as Hash);
				if (hash == null)
					continue;
				if (Compare (hash_value, hash.GenerateHash (hash_algorithm)))
					return true;
				break;
			}
			return false;
		}

		public IMembershipCondition Copy ()
		{
			return new HashMembershipCondition (hash_algorithm, hash_value);
		}

		public override bool Equals (object o)
		{
			HashMembershipCondition other = (o as HashMembershipCondition);
			if (other == null)
				return false;

			return ((other.HashAlgorithm == hash_algorithm) &&
				Compare (hash_value, other.hash_value));
		}
		
		public SecurityElement ToXml ()
		{
			return ToXml (null);
		}

		public SecurityElement ToXml (PolicyLevel level)
		{
			SecurityElement se = MembershipConditionHelper.Element (typeof (HashMembershipCondition), version);
			se.AddAttribute ("HashValue", CryptoConvert.ToHex (HashValue));
			se.AddAttribute ("HashAlgorithm", hash_algorithm.GetType ().FullName);
			return se;
		}

		public void FromXml (SecurityElement e)
		{
			FromXml (e, null);
		}
		
		public void FromXml (SecurityElement e, PolicyLevel level)
		{
			MembershipConditionHelper.CheckSecurityElement (e, "e", version, version);
			
			hash_value = CryptoConvert.FromHex (e.Attribute ("HashValue"));

			string algorithm = e.Attribute ("HashAlgorithm");
			hash_algorithm = (algorithm == null) ? null : HashAlgorithm.Create (algorithm);
		}

		public override int GetHashCode ()
		{
			// note: a Copy must have the same hash code
			int code = hash_algorithm.GetType ().GetHashCode ();
			if (hash_value != null) {
				foreach (byte b in hash_value) {
					code ^= b;
				}
			}
			return code;
		}
		
		public override string ToString ()
		{
			Type alg_type = this.HashAlgorithm.GetType ();
			return String.Format ("Hash - {0} {1} = {2}", alg_type.FullName, 
				alg_type.Assembly, CryptoConvert.ToHex (HashValue));
		}

		//
		// Private Methods
		//

		private bool Compare (byte[] expected, byte[] actual)
		{
			if (expected.Length != actual.Length)
				return false;
			
			int len = expected.Length;
			for (int i = 0; i < len; i++) {
				if (expected [i] != actual [i])
					return false;
			}
			return true;
		}

		[MonoTODO ("fx 2.0")]
		void IDeserializationCallback.OnDeserialization (object sender)
		{
		}

		[MonoTODO ("fx 2.0")]
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context) 
		{
		}
	}
}
