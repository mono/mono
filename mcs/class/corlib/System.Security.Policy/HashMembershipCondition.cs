//
// System.Security.Policy.HashMembershipCondition
//
// Authors:
//	Jackson Harper (Jackson@LatitudeGeo.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Jackson Harper, All rights reserved
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

using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace System.Security.Policy {

	[Serializable]
#if NET_2_0
	public sealed class HashMembershipCondition : IMembershipCondition, IDeserializationCallback, ISerializable {
#else
	public sealed class HashMembershipCondition : IMembershipCondition {
#endif
		private readonly int version = 1;

		private HashAlgorithm hash_algorithm;
		private byte[] hash_value;

		// so System.Activator.CreateInstance can create an instance...
		internal HashMembershipCondition ()
		{
		}

		public HashMembershipCondition (HashAlgorithm hash_algorithm, byte[] hash_value)
		{
			if (hash_algorithm == null)
				throw new ArgumentNullException ("hash_algorithm");
			if (hash_value == null)
				throw new ArgumentNullException ("hash_value");
				
			this.hash_algorithm = hash_algorithm;
			this.hash_value = hash_value;
		}

		//
		// Public Properties
		//
		
		public HashAlgorithm HashAlgorithm {
			get { return hash_algorithm; }
			set { 
				if (value == null)
					throw new ArgumentNullException ("HashAlgorithm");
				hash_algorithm = value; 
			}
		}

		public byte[] HashValue {
			get { return hash_value; }
			set { 
				if (value == null)
					throw new ArgumentNullException ("HashValue");
				hash_value = value; 
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
			HashMembershipCondition other;
			if (!(o is HashMembershipCondition))
				return false;

			other = (HashMembershipCondition)o;
			
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
			se.AddAttribute ("HashValue", Encoding.Default.GetString (hash_value));
			se.AddAttribute ("HashAlgorithm", hash_algorithm.GetType ().FullName);
			return se;
		}

		public void FromXml (SecurityElement element)
		{
			FromXml (element, null);
		}
		
		public void FromXml (SecurityElement e, PolicyLevel level)
		{
			MembershipConditionHelper.CheckSecurityElement (e, "e", version, version);
			
			string value = (string)e.Attributes ["HashValue"];
			string algorithm = (string)e.Attributes ["HashAlgorithm"];

			if (value == null || algorithm == null ) {
				throw new ArgumentException (Locale.GetText (
					"Missing either HashValue or HashAlgorithm"), "e");
			}
			
			hash_value = Encoding.Default.GetBytes (value);
			hash_algorithm = (HashAlgorithm)Assembly.GetExecutingAssembly ().CreateInstance (algorithm);
		}

		public override int GetHashCode ()
		{
			return hash_value.GetHashCode ();
		}
		
		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();
			Type alg_type = hash_algorithm.GetType ();

			builder.Append ("Hash -");
			builder.AppendFormat ("{0} {1}", alg_type.FullName, 
				alg_type.Assembly);
			builder.AppendFormat (" = ",  Encoding.Default.GetString (hash_value));

			return builder.ToString ();
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

#if NET_2_0
		[MonoTODO ("fx 2.0")]
		void IDeserializationCallback.OnDeserialization (object sender)
		{
		}

		[MonoTODO ("fx 2.0")]
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context) 
		{
		}
#endif
	}
}

