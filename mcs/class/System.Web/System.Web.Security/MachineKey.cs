//
// Authors:
//   Marek Habersack <grendel@twistedcode.net>
//
// (C) 2011 Novell, Inc (http://novell.com/)
//

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
using System.Web;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web.Security 
{
	public static class MachineKey
	{
#if NET_4_5
		[Obsolete ("This method is obsolete and is only provided for compatibility with existing code. It is recommended that new code use the Protect and Unprotect methods instead.")]
#endif
		public static byte[] Decode (string encodedData, MachineKeyProtection protectionOption)
		{
			if (encodedData == null)
				throw new ArgumentNullException ("encodedData");

			int dlen = encodedData.Length;
			if (dlen == 0 || dlen % 2 == 1)
				throw new ArgumentException ("encodedData");

			byte[] data = MachineKeySectionUtils.GetBytes (encodedData, dlen);
			if (data == null || data.Length == 0)
				throw new ArgumentException ("encodedData");
			
			var config = WebConfigurationManager.GetWebApplicationSection ("system.web/machineKey") as MachineKeySection;
			byte[] result = null;
			Exception ex = null;
			try {
				switch (protectionOption) {
					case MachineKeyProtection.All:
						result = MachineKeySectionUtils.VerifyDecrypt (config, data);
						break;

					case MachineKeyProtection.Encryption:
						result = MachineKeySectionUtils.Decrypt (config, data);
						break;

					case MachineKeyProtection.Validation:
						result = MachineKeySectionUtils.Verify (config, data);
						break;

					default:
						return MachineKeySectionUtils.GetBytes (encodedData, dlen);
				}
			} catch (Exception e) {
				ex = e;
			}
			
			if (result == null || ex != null)
				throw new HttpException ("Unable to verify passed data.", ex);
			
			return result;
		}
#if NET_4_5
		[Obsolete ("This method is obsolete and is only provided for compatibility with existing code. It is recommended that new code use the Protect and Unprotect methods instead.")]
#endif
		public static string Encode (byte[] data, MachineKeyProtection protectionOption)
		{
			if (data == null)
				throw new ArgumentNullException ("data");

			var config = WebConfigurationManager.GetWebApplicationSection ("system.web/machineKey") as MachineKeySection;
			byte[] result;
			switch (protectionOption) {
				case MachineKeyProtection.All:
					result = MachineKeySectionUtils.EncryptSign (config, data);
					break;

				case MachineKeyProtection.Encryption:
					result = MachineKeySectionUtils.Encrypt (config, data);
					break;

				case MachineKeyProtection.Validation:
					result = MachineKeySectionUtils.Sign (config, data);
					break;

				default:
					return String.Empty;
			}
			
			return MachineKeySectionUtils.GetHexString (result);
		}

#if NET_4_5
		[MonoTODO ("We ignore the purposes parameter for now, this allowes Unprotect to be called on any data")]
		public static byte [] Protect (byte [] userData, params string [] purposes)
		{
			// TODO: generate key from purposes and use it
			if (userData == null)
				throw new ArgumentNullException ("userData");
			var config = WebConfigurationManager.GetWebApplicationSection ("system.web/machineKey") as MachineKeySection;
			return MachineKeySectionUtils.EncryptSign (config, userData);
		}

		[MonoTODO ("We ignore the purposes parameter for now, this allowes Unprotect to be called on any data")]
		public static byte [] Unprotect (byte [] userData, params string [] purposes)
		{
			// TODO: generate key from purposes and use it
			if (userData == null)
				throw new ArgumentNullException ("userData");

			var config = WebConfigurationManager.GetWebApplicationSection ("system.web/machineKey") as MachineKeySection;
			return MachineKeySectionUtils.VerifyDecrypt (config, userData);
		}
#endif
	}
}
