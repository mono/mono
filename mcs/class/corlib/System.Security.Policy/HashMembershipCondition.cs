//
// System.Security.Policy.HashMembershipCondition
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2002 Jackson Harper, All rights reserved
//

using System.Text;
using System.Reflection;
using System.Security.Cryptography;

namespace System.Security.Policy {

	public sealed class HashMembershipCondition : IMembershipCondition, 
		ISecurityEncodable, ISecurityPolicyEncodable {

		private static readonly string XmlTag = "IMembershipCondition";

		private HashAlgorithm hash_algorithm;
		private byte[] hash_value;

		public HashMembershipCondition (HashAlgorithm hash_algorithm,
  			byte[] hash_value)
		{
			if (hash_algorithm == null || hash_value == null)
				throw new ArgumentNullException ();
				
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
					throw new ArgumentNullException ();
				hash_algorithm = value; 
			}
		}

		public byte[] HashValue {
			get { return hash_value; }
			set { 
				if (value == null)
					throw new ArgumentNullException ();
				hash_value = value; 
			} 
		}

		//
		// Public Methods
		//

		public bool Check (Evidence evidence)
		{
			if (evidence == null)
				throw new ArgumentNullException ();

			// Loop through evidence finding the first Hash object
			foreach (object obj in evidence) {
				Hash hash = obj as Hash;
				if (hash == null)
					continue;
				if (EqualsHashValue (hash.GenerateHash (hash_algorithm)))
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
			
			return (other.HashAlgorithm == hash_algorithm &&
				other.HashValue == hash_value);
		}
		
		public SecurityElement ToXml()
		{
			return ToXml (null);
		}

		public SecurityElement ToXml (PolicyLevel level)
		{
			SecurityElement se = new SecurityElement (XmlTag);
			Type type = this.GetType ();
			string classString = type.FullName + ", " + type.Assembly;
			se.AddAttribute ("class", classString);
			se.AddAttribute ("version", "1");
			se.AddAttribute ("HashValue", Encoding.Default.GetString (hash_value));
			se.AddAttribute ("HashAlgorithm", hash_algorithm.GetType ().FullName);
			return se;
		}

		public void FromXml (SecurityElement element)
		{
			FromXml (element, null);
		}
		
		public void FromXml (SecurityElement e,
			PolicyLevel level)
		{
			if (e == null)
				throw new ArgumentNullException ();
			if (e.Tag != XmlTag)
				throw new ArgumentException(
					"e","The Tag of SecurityElement must be " + XmlTag);
			
			string value = (string)e.Attributes["HashValue"];
			string algorithm = (string)e.Attributes["HashAlgorithm"];

			if (value == null || algorithm == null )
				throw new ArgumentException ();
			
			hash_value = Encoding.Default.GetBytes (value);
			hash_algorithm = (HashAlgorithm)Assembly.GetExecutingAssembly ().CreateInstance (algorithm);
			
		}

		[MonoTODO("This is not right")]
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

		private bool EqualsHashValue (byte[] value)
		{
			int len;

			if (value.Length != hash_value.Length)
				return false;
			
			len = value.Length;
			for (int i=0; i<len; i++ ) {
				if (value[i] != hash_value[i])
					return false;
			}

			return true;
		}
	}
}

