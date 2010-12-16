/*
  Copyright (C) 2008 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.Serialization;

namespace IKVM.Reflection.Impl
{
	static class CryptoHack
	{
		internal static RSA CreateRSA(StrongNameKeyPair keyPair)
		{
			// HACK use serialization to get at the private key or key container name,
			// this should be more future proof than using reflection to access the fields directly.
			SerializationInfo ser = new SerializationInfo(typeof(StrongNameKeyPair), new FormatterConverter());
			((ISerializable)keyPair.keyPair).GetObjectData(ser, new StreamingContext());
			byte[] key = (byte[])ser.GetValue("_keyPairArray", typeof(byte[]));
			string keycontainer = ser.GetString("_keyPairContainer");
			if (keycontainer != null)
			{
				CspParameters parm = new CspParameters();
				parm.Flags = CspProviderFlags.UseMachineKeyStore;
				parm.KeyContainerName = keycontainer;
				parm.KeyNumber = 2;	// Signature
				return new RSACryptoServiceProvider(parm);
			}
			else
			{
				return Mono.Security.Cryptography.CryptoConvert.FromCapiKeyBlob(key);
			}
		}
	}
}
