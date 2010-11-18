//
// System.Web.Configuration.MachineKeyRegistryStorage
//
// Authors:
//	Marek Habersack <mhabersack@novell.com>
//
// (C) 2007 Novell, Inc
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

using Microsoft.Win32;

namespace System.Web.Configuration
{
	internal class MachineKeyRegistryStorage
	{
		public enum KeyType
		{
			Validation,
			Encryption
		};

		static string keyEncryption;
		static string keyValidation;
		
		static MachineKeyRegistryStorage ()
		{
			string appName = AppDomain.CurrentDomain.SetupInformation.ApplicationName;
			if (appName == null)
				return;
			
			string hash = appName.GetHashCode ().ToString ("x");
			keyEncryption = "software\\mono\\asp.net\\" + Environment.Version.ToString () +
				"\\autogenkeys\\" + hash + "-" + ((int)KeyType.Encryption).ToString ();
			keyValidation = "software\\mono\\asp.net\\" + Environment.Version.ToString () +
				"\\autogenkeys\\" + hash + "-" + ((int)KeyType.Validation).ToString ();
		}
		
		public static byte[] Retrieve (KeyType kt)
		{
			string key = null;
			
			switch (kt) {
				case KeyType.Validation:
					key = keyValidation;
					break;

				case KeyType.Encryption:
					key = keyEncryption;
					break;

				default:
					throw new ArgumentException ("Unknown key type.");
			}

			if (key == null)
				return null;
			
			object o = null;

			try {
				RegistryKey v = OpenRegistryKey (key, false);
				o = v.GetValue ("AutoGenKey", null);
			} catch (Exception) {
				return null;
			}

			if (o == null || o.GetType () != typeof (byte[]))
				return null;
			return (byte[]) o;
		}

		static RegistryKey OpenRegistryKey (string path, bool write)
		{
			RegistryKey ret, tmp;
			string[] keys = path.Split ('\\');
			int klen = keys.Length;

			ret = Registry.CurrentUser;
			for (int i = 0; i < klen; i++) {
				tmp = ret.OpenSubKey (keys [i], true);
				if (tmp == null) {
					if (!write)
						return null;
					tmp = ret.CreateSubKey (keys [i]);
				}
				ret = tmp;
			}

			return ret;
		}
		
		public static void Store (byte[] buf, KeyType kt)
		{
			if (buf == null)
				return;
			
			string key = null;
			switch (kt) {
				case KeyType.Validation:
					key = keyValidation;
					break;

				case KeyType.Encryption:
					key = keyEncryption;
					break;

				default:
					throw new ArgumentException ("Unknown key type.");
			}

			if (key == null)
				return;

			try {
				using (RegistryKey rk = OpenRegistryKey (key, true)) {
#if NET_2_0
					rk.SetValue ("AutoGenKey", buf, RegistryValueKind.Binary);
					rk.SetValue ("AutoGenKeyCreationTime", DateTime.Now.Ticks, RegistryValueKind.QWord);
					rk.SetValue ("AutoGenKeyFormat", 2, RegistryValueKind.DWord);
#else
					rk.SetValue ("AutoGenKey", buf);
					rk.SetValue ("AutoGenKeyCreationTime", DateTime.Now.Ticks);
					rk.SetValue ("AutoGenKeyFormat", 2);
#endif
					rk.Flush (); // we want it synchronous
				}
			} catch (Exception ex) {
				Console.Error.WriteLine ("(info) Auto generated encryption keys not saved: {0}", ex);
			}
		}
	}
}
