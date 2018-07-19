//
// System.Reflection.StrongNameKeyPair.cs
//
// Authors:
//	Kevin Winchester (kwin@ns.sympatico.ca)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002 Kevin Winchester
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
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

using Mono.Security;
using Mono.Security.Cryptography;

namespace System.Reflection {

	[ComVisible (true)]
[Serializable]
public class StrongNameKeyPair : ISerializable, IDeserializationCallback
{		
	private byte[] _publicKey;
	private string _keyPairContainer;
	private bool _keyPairExported;
	private byte[] _keyPairArray;
	
	[NonSerialized]
	private RSA _rsa;

	// note: we ask for UnmanagedCode because we do not want everyone
	// to be able to generate strongnamed assemblies

	[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
	public StrongNameKeyPair (byte[] keyPairArray) 
	{
		if (keyPairArray == null)
			throw new ArgumentNullException ("keyPairArray");

		LoadKey (keyPairArray);
		GetRSA ();
	}
	
	[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
	public StrongNameKeyPair (FileStream keyPairFile) 
	{
		if (keyPairFile == null)
			throw new ArgumentNullException ("keyPairFile");

		byte[] input = new byte [keyPairFile.Length];
		keyPairFile.Read (input, 0, input.Length);
		LoadKey (input);
		GetRSA ();
	}
	
	[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
	public StrongNameKeyPair (string keyPairContainer) 
	{
		// named key container
		if (keyPairContainer == null)
			throw new ArgumentNullException ("keyPairContainer");

		_keyPairContainer = keyPairContainer;
		GetRSA ();
	}
	protected StrongNameKeyPair (SerializationInfo info, StreamingContext context)
	{
		_publicKey = (byte []) info.GetValue ("_publicKey", typeof (byte []));
		_keyPairContainer = info.GetString ("_keyPairContainer");
		_keyPairExported = info.GetBoolean ("_keyPairExported");
		_keyPairArray = (byte []) info.GetValue ("_keyPairArray", typeof (byte []));
	}

	void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
	{
		info.AddValue ("_publicKey", _publicKey, typeof (byte []));
		info.AddValue ("_keyPairContainer", _keyPairContainer);
		info.AddValue ("_keyPairExported", _keyPairExported);
		info.AddValue ("_keyPairArray", _keyPairArray, typeof (byte []));
	}

	void IDeserializationCallback.OnDeserialization (object sender)
	{
	}

	private RSA GetRSA ()
	{
		if (_rsa != null) return _rsa;
		
		if (_keyPairArray != null) {
			try {
				_rsa = CryptoConvert.FromCapiKeyBlob (_keyPairArray);
			}
			catch {
				// exception is thrown when getting PublicKey
				// to match MS implementation
				_keyPairArray = null;
			}
		}
#if !MOBILE
		else if (_keyPairContainer != null) {
			CspParameters csp = new CspParameters ();
			csp.KeyContainerName = _keyPairContainer;
			_rsa = new RSACryptoServiceProvider (csp);
		}
#endif
		return _rsa;
	}

	private void LoadKey (byte[] key) 
	{
		try {
			// check for ECMA key
			if (key.Length == 16) {
				int i = 0;
				int sum = 0;
				while (i < key.Length)
					sum += key [i++];
				if (sum == 4) {
					// it is the ECMA key
					_publicKey = (byte[]) key.Clone ();
				}
			}
			else
				_keyPairArray = key;
		}
		catch
		{
			// exception is thrown when getting PublicKey
			// to match MS implementation
		}
	}

	public byte[] PublicKey {
		get {
			if (_publicKey == null) {
				RSA rsa = GetRSA ();
				// ECMA "key" is valid but doesn't produce a RSA instance
				if (rsa == null)
					throw new ArgumentException ("invalid keypair");

				byte[] blob = CryptoConvert.ToCapiKeyBlob (rsa, false);
				_publicKey = new byte [blob.Length + 12];
				// The first 12 bytes are documented at:
				// http://msdn.microsoft.com/library/en-us/cprefadd/html/grfungethashfromfile.asp
				// ALG_ID - Signature
				_publicKey[0] = 0x00;
				_publicKey[1] = 0x24;	
				_publicKey[2] = 0x00;	
				_publicKey[3] = 0x00;	
				// ALG_ID - Hash
				_publicKey[4] = 0x04;
				_publicKey[5] = 0x80;
				_publicKey[6] = 0x00;
				_publicKey[7] = 0x00;
				// Length of Public Key (in bytes)
				int lastPart = blob.Length;
				_publicKey[8] = (byte)(lastPart % 256);
				_publicKey[9] = (byte)(lastPart / 256); // just in case
				_publicKey[10] = 0x00;
				_publicKey[11] = 0x00;

				Buffer.BlockCopy (blob, 0, _publicKey, 12, blob.Length);
			}
			return _publicKey;
		}
	}

	internal StrongName StrongName () 
	{
		RSA rsa = GetRSA ();
		if (rsa != null)
			return new StrongName (rsa);
		if (_publicKey != null)
			return new StrongName (_publicKey);
		return null;
	}
}

}
