// System.Security.Policy.Hash
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;

namespace System.Security.Policy {

	[MonoTODO]
	public sealed class Hash {
		
		private Assembly assembly;

		public Hash(Assembly assembly)
		{
			this.assembly = assembly;
		}

		//
		// Public Properties
		//
		
		public byte[] MD5 
		{
			get {
				return GenerateHash (new MD5CryptoServiceProvider ());
			}
		}

		public byte[] SHA1
		{
			get {
				return GenerateHash (new SHA1CryptoServiceProvider ());
			}
		}

		//
		// Public Methods
		//
		
		public byte[] GenerateHash(HashAlgorithm hashAlg)
		{
			return hashAlg.ComputeHash (GetData ());
		}

		[MonoTODO]
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append ("<System.Security.Policy.Hash version=\"1\">");
			builder.AppendFormat ("<RawData>{0}</RawData>", GetData ());
			builder.Append ("</System.Security.Policy.Hash>");

			return builder.ToString ();
		}

		//
		// Private Methods
		//

		[MonoTODO("This doesn't match the MS version perfectly.")]
		private byte[] GetData()
		{
			FileStream stream = new FileStream( assembly.Location, FileMode.Open, FileAccess.Read );
			byte[] buf = new byte[stream.Length];

			stream.Read( buf, 0, (int)stream.Length );

			return buf;
		}
	}

}

