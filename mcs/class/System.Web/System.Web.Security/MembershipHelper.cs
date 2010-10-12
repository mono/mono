//
// System.Web.Security.MembershipHelper
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

using System.Configuration.Provider;
using System.Security.Cryptography;
using System.Web.Configuration;
using System.Web.Util;

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
		
		static SymmetricAlgorithm GetAlgorithm ()
		{
			MachineKeySection section = MachineKeySection.Config;

			if (section.DecryptionKey.StartsWith ("AutoGenerate"))
				throw new ProviderException ("You must explicitly specify a decryption key in the <machineKey> section when using encrypted passwords.");

			SymmetricAlgorithm sa = section.GetDecryptionAlgorithm ();
			if (sa == null)
				throw new ProviderException (String.Format ("Unsupported decryption attribute '{0}' in <machineKey> configuration section", section.Decryption));

			sa.Key = section.GetDecryptionKey ();
			return sa;
		}
		
		public byte [] DecryptPassword (byte [] encodedPassword)
		{
			using (SymmetricAlgorithm sa = GetAlgorithm ()) {
				return MachineKeySectionUtils.Decrypt (sa, encodedPassword, 0, encodedPassword.Length);
			}
		}

		public byte[] EncryptPassword (byte[] password)
		{
			using (SymmetricAlgorithm sa = GetAlgorithm ()) {
				return MachineKeySectionUtils.Encrypt (sa, password);
			}
		}
	}
}
