/*
  Copyright (C) 2009-2012 Jeroen Frijters

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
using System.IO;
using System.Security.Cryptography;

namespace IKVM.Reflection
{
	public sealed class StrongNameKeyPair
	{
		private readonly byte[] keyPairArray;
		private readonly string keyPairContainer;

		public StrongNameKeyPair(string keyPairContainer)
		{
			if (keyPairContainer == null)
			{
				throw new ArgumentNullException("keyPairContainer");
			}
			if (Universe.MonoRuntime && Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				throw new NotSupportedException("IKVM.Reflection does not support key containers when running on Mono");
			}
			this.keyPairContainer = keyPairContainer;
		}

		public StrongNameKeyPair(byte[] keyPairArray)
		{
			if (keyPairArray == null)
			{
				throw new ArgumentNullException("keyPairArray");
			}
			this.keyPairArray = (byte[])keyPairArray.Clone();
		}

		public StrongNameKeyPair(FileStream keyPairFile)
			: this(ReadAllBytes(keyPairFile))
		{
		}

		private static byte[] ReadAllBytes(FileStream keyPairFile)
		{
			if (keyPairFile == null)
			{
				throw new ArgumentNullException("keyPairFile");
			}
			byte[] buf = new byte[keyPairFile.Length - keyPairFile.Position];
			keyPairFile.Read(buf, 0, buf.Length);
			return buf;
		}

		public byte[] PublicKey
		{
			get
			{
				if (Universe.MonoRuntime)
				{
					// MONOBUG workaround for https://bugzilla.xamarin.com/show_bug.cgi?id=5299
					return MonoGetPublicKey();
				}
				using (RSACryptoServiceProvider rsa = CreateRSA())
				{
					byte[] cspBlob = rsa.ExportCspBlob(false);
					byte[] publicKey = new byte[12 + cspBlob.Length];
					Buffer.BlockCopy(cspBlob, 0, publicKey, 12, cspBlob.Length);
					publicKey[1] = 36;
					publicKey[4] = 4;
					publicKey[5] = 128;
					publicKey[8] = (byte)(cspBlob.Length >> 0);
					publicKey[9] = (byte)(cspBlob.Length >> 8);
					publicKey[10] = (byte)(cspBlob.Length >> 16);
					publicKey[11] = (byte)(cspBlob.Length >> 24);
					return publicKey;
				}
			}
		}

		internal RSACryptoServiceProvider CreateRSA()
		{
			try
			{
				if (keyPairArray != null)
				{
					RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
					rsa.ImportCspBlob(keyPairArray);
					return rsa;
				}
				else
				{
					CspParameters parm = new CspParameters();
					parm.KeyContainerName = keyPairContainer;
					// MONOBUG Mono doesn't like it when Flags or KeyNumber are set
					if (!Universe.MonoRuntime)
					{
						parm.Flags = CspProviderFlags.UseMachineKeyStore | CspProviderFlags.UseExistingKey;
						parm.KeyNumber = 2;	// Signature
					}
					return new RSACryptoServiceProvider(parm);
				}
			}
			catch
			{
				throw new ArgumentException("Unable to obtain public key for StrongNameKeyPair.");
			}
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
		private byte[] MonoGetPublicKey()
		{
			return keyPairArray != null
				? new System.Reflection.StrongNameKeyPair(keyPairArray).PublicKey
				: new System.Reflection.StrongNameKeyPair(keyPairContainer).PublicKey;
		}
	}
}
