// System.Security.Policy.Hash
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2002 Jackson Harper, All rights reserved.

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
		private byte[] data = null;

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

		[MonoTODO("The Raw data seems to be different than the raw data I have")]
		public override string ToString()
		{
			SecurityElement se = new SecurityElement (GetType ().FullName);
			se.AddAttribute ("version", "1");
			se.AddChild (new SecurityElement ("RawData", GetData ().ToString ()));

			return se.ToString ();
		}

		//
		// Private Methods
		//

		[MonoTODO("This doesn't match the MS version perfectly.")]
		private byte[] GetData()
		{
			if (null == data) {
				FileStream stream = new 
					FileStream( assembly.Location, FileMode.Open, FileAccess.Read );
				data = new byte[stream.Length];
				stream.Read( data, 0, (int)stream.Length );
			}

			return data;
		}
	}
}

