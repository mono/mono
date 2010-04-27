//
// System.Web.Security.MembershipEncryptionHelper
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2003 Ben Maurer
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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
using System;
using System.Configuration.Provider;
using System.Security.Cryptography;
using System.Web.Configuration;

namespace System.Web.Security
{
	sealed class MembershipHelper
#if NET_4_0
	: IMembershipHelper
#endif
	{
		internal const int SALT_BYTES = 16;

		public int UserIsOnlineTimeWindow {
			get { return Membership.UserIsOnlineTimeWindow; }
		}

		public MembershipProviderCollection Providers {
			get { return Membership.Providers; }
		}
		
		static SymmetricAlgorithm GetAlg ()
		{
			MachineKeySection section = (MachineKeySection) WebConfigurationManager.GetSection ("system.web/machineKey");

			if (section.DecryptionKey.StartsWith ("AutoGenerate"))
				throw new ProviderException ("You must explicitly specify a decryption key in the <machineKey> section when using encrypted passwords.");

			string alg_type = section.Decryption;
			if (alg_type == "Auto")
				alg_type = "AES";

			SymmetricAlgorithm alg = null;
			if (alg_type == "AES")
				alg = Rijndael.Create ();
			else if (alg_type == "3DES")
				alg = TripleDES.Create ();
			else
				throw new ProviderException (String.Format ("Unsupported decryption attribute '{0}' in <machineKey> configuration section", alg_type));

			alg.Key = MachineKeySectionUtils.DecryptionKey192Bits (section);
			return alg;
		}
		
		public byte [] DecryptPassword (byte [] encodedPassword)
		{
			using (SymmetricAlgorithm alg = GetAlg ()) {
				// alg.Key is set in GetAlg based on web.config
				// iv is the first part of the encodedPassword
				byte [] iv = new byte [alg.IV.Length];
				Array.Copy (encodedPassword, 0, iv, 0, iv.Length);
				using (ICryptoTransform decryptor = alg.CreateDecryptor (alg.Key, iv)) {
					return decryptor.TransformFinalBlock (encodedPassword, iv.Length, encodedPassword.Length - iv.Length);
				}
			}
		}

		public byte[] EncryptPassword (byte[] password)
		{
			using (SymmetricAlgorithm alg = GetAlg ()) {
				// alg.Key is set in GetAlg based on web.config
				// alg.IV is randomly set (default behavior) and perfect for our needs
				byte [] iv = alg.IV;
				using (ICryptoTransform encryptor = alg.CreateEncryptor (alg.Key, iv)) {
					byte [] encrypted = encryptor.TransformFinalBlock (password, 0, password.Length);
					byte [] output = new byte [iv.Length + encrypted.Length];
					// note: the IV can be public, however it should not be based on the password
					Array.Copy (iv, 0, output, 0, iv.Length);
					Array.Copy (encrypted, 0, output, iv.Length, encrypted.Length);
					return output;
				}
			}
		}
	}
}
