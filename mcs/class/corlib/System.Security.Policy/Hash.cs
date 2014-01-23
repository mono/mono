//
// System.Security.Policy.Hash
//
// Authors:
//	Jackson Harper (Jackson@LatitudeGeo.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Jackson Harper, All rights reserved.
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
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

using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;

namespace System.Security.Policy {

[Serializable]
[ComVisible (true)]
public sealed class Hash :
#if NET_4_0
		EvidenceBase,
#endif
		ISerializable, IBuiltInEvidence {

	private Assembly assembly;
	private byte[] data;

	internal byte[] _md5;
	internal byte[] _sha1;

	public Hash (Assembly assembly) 
	{
		if (assembly == null)
			throw new ArgumentNullException ("assembly");
		this.assembly = assembly;
	}

	internal Hash () 
	{
	}

	internal Hash (SerializationInfo info, StreamingContext context)
	{
		data = (byte[]) info.GetValue ("RawData", typeof (byte[]));
	}

	//
	// Public Properties
	//

	public byte[] MD5 {
		get {
			// Case 1: we have a MD5 value - either precalculated or static (FX 2.0)
			if (_md5 != null)
				return _md5;

			// Case 2: we don't have a MD5 value precalculated so we either
			// (a): have an assembly reference - and can calculate the hash; or
			// (b): have been initialized with a static MD5 value (FX 2.0)
			if ((assembly == null) && (_sha1 != null)) {
				string msg = Locale.GetText ("No assembly data. This instance was initialized with an MSHA1 digest value.");
				throw new SecurityException (msg);
			}
			// fully named to avoid conflit between MD5 property and class name
			HashAlgorithm hash = System.Security.Cryptography.MD5.Create ();
			_md5 = GenerateHash (hash);
			return _md5;
		}
	}

	public byte[] SHA1 {
		get {
			// Case 1: we have a SHA1 value - either precalculated or static (FX 2.0)
			if (_sha1 != null)
				return _sha1;
			
			// Case 2: we don't have a SHA1 value precalculated so we either
			// (a): have an assembly reference - and can calculate the hash; or
			// (b): have been initialized with a static MD5 value (FX 2.0)
			if ((assembly == null) && (_md5 != null)) {
				string msg = Locale.GetText ("No assembly data. This instance was initialized with an MD5 digest value.");
				throw new SecurityException (msg);
			}
			// fully named to avoid conflit between SHA1 property and class name
			HashAlgorithm hash = System.Security.Cryptography.SHA1.Create ();
			_sha1 = GenerateHash (hash);
			return _sha1;
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

	public void GetObjectData (SerializationInfo info, StreamingContext context) 
	{
		if (info == null)
			throw new ArgumentNullException ("info");
		info.AddValue ("RawData", GetData ());
	}

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

	// FIXME: be more restrictive when imperative security is implemented
	[FileIOPermission (SecurityAction.Assert, Unrestricted = true)]
	private byte[] GetData () 
	{
		if ((assembly == null) && (data == null)) {
			string msg = Locale.GetText ("No assembly data.");
			throw new SecurityException (msg);
		}
		if (null == data) {
			// TODO (Pre-Fx-2.0) we mustn't hash the complete assembly!
			// ---- Look at ToString (MS version) for what to hash (and what not to)
			// TODO we must drop the authenticode signature (if present)
			FileStream stream = new 
				FileStream (assembly.Location, FileMode.Open, FileAccess.Read);
			data = new byte [stream.Length];
			stream.Read (data, 0, (int)stream.Length);
		}

		return data;
	}

	// interface IBuiltInEvidence

	int IBuiltInEvidence.GetRequiredSize (bool verbose) 
	{
		return (verbose ? 5 : 0);	// as documented
	}

	[MonoTODO ("IBuiltInEvidence")]
	int IBuiltInEvidence.InitFromBuffer (char [] buffer, int position) 
	{
		return 0;
	}

	[MonoTODO ("IBuiltInEvidence")]
	int IBuiltInEvidence.OutputToBuffer (char [] buffer, int position, bool verbose) 
	{
		return 0;
	}

	static public Hash CreateMD5 (byte[] md5)
	{
		if (md5 == null)
			throw new ArgumentNullException ("md5");
		Hash h = new Hash ();
		h._md5 = md5;
		return h;
	}

	static public Hash CreateSHA1 (byte[] sha1)
	{
		if (sha1 == null)
			throw new ArgumentNullException ("sha1");
		Hash h = new Hash ();
		h._sha1 = sha1;
		return h;
	}
}

}
