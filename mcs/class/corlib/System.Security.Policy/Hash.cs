//
// System.Security.Policy.Hash
//
// Authors:
//	Jackson Harper (Jackson@LatitudeGeo.com)
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Jackson Harper, All rights reserved.
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;

namespace System.Security.Policy {

[Serializable]
public sealed class Hash : ISerializable {

	private Assembly assembly;
	private byte[] data = null;

	public Hash (Assembly assembly) 
	{
		if (assembly == null)
			throw new ArgumentNullException ("assembly");
		this.assembly = assembly;
	}

	//
	// Public Properties
	//

	public byte[] MD5 {
		get {
			// fully named to avoid conflit between MD5 property and class name
			HashAlgorithm hash = System.Security.Cryptography.MD5.Create ();
			return GenerateHash (hash);
		}
	}

	public byte[] SHA1 {
		get {
			// fully named to avoid conflit between SHA1 property and class name
			HashAlgorithm hash = System.Security.Cryptography.SHA1.Create ();
			return GenerateHash (hash);
		}
	}

	//
	// Public Methods
	//

	public byte[] GenerateHash (HashAlgorithm hashAlg) 
	{
		if (hashAlg == null)
			throw new ArgumentNullException ("hashAlg");
		return hashAlg.ComputeHash (GetData ());
	}

	[MonoTODO]
	public void GetObjectData (SerializationInfo info, StreamingContext context) 
	{
		if (info == null)
			throw new ArgumentNullException ("info");
		throw new NotImplementedException ();
	}

	[MonoTODO("The Raw data seems to be different than the raw data I have")]
	public override string ToString () 
	{
		SecurityElement se = new SecurityElement (GetType ().FullName);
		se.AddAttribute ("version", "1");
		
		StringBuilder sb = new StringBuilder ();
		byte[] raw = GetData ();
		for (int i=0; i < raw.Length; i++)
			sb.Append (raw [i].ToString ("X2"));

		se.AddChild (new SecurityElement ("RawData", sb.ToString ()));
		return se.ToString ();
	}

	//
	// Private Methods
	//

	[MonoTODO("This doesn't match the MS version perfectly.")]
	private byte[] GetData () 
	{
		if (null == data) {
			// TODO we mustn't hash the complete assembly!
			// ---- Look at ToString (MS version) for what to hash (and what not to)
			// TODO we must drop the authenticode signature (if present)
			FileStream stream = new 
				FileStream (assembly.Location, FileMode.Open, FileAccess.Read);
			data = new byte [stream.Length];
			stream.Read (data, 0, (int)stream.Length);
		}

		return data;
	}
}

}
