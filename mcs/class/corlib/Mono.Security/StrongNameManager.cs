//
// StrongNameManager.cs - StrongName Management
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Text;

using Mono.Security.Cryptography;
using Mono.Xml;

namespace Mono.Security {

	/* RUNTIME
	 *				yes
	 *	in_gac ---------------------------------\
	 *		|				|
	 *		| no				\/
	 *		|			return true
	 * CLASS LIBRARY|
	 *		|
	 * 		|
	 *		|				
	 *	bool StrongNameManager.MustVerify
	 *		|
	 *		|
	 *		\/		not found	
	 *		Token --------------------------\
	 *		|				|
	 *		| present ?			|
	 *		|				|
	 *		\/		not found	|
	 *	Assembly Name --------------------------|
	 *		|				|
	 *		| present ?			|
	 *		| or "*"			|
	 *		\/		not found	|
	 *		User ---------------------------|
	 *		|				|
	 *		| present ?			|
	 *		| or "*"			|
	 *		\/				\/
	 *	return false			return true
	 *	SKIP VERIFICATION		VERIFY ASSEMBLY
	 */

	internal class StrongNameManager {

		private class Element {
			internal Hashtable assemblies;

			public Element () 
			{
				assemblies = new Hashtable ();
			}

			public Element (string assembly, string users) : this ()
			{
				assemblies.Add (assembly, users);
			}

			public string GetUsers (string assembly) 
			{
				return (string) assemblies [assembly];
			}
		}

		static private Hashtable mappings;
		static private Hashtable tokens;

		// note: more than one configuration file can be loaded at the 
		// same time (e.g. user specific and machine specific config).
		static public void LoadConfig (string filename) 
		{
			if (File.Exists (filename)) {
				SecurityParser sp = new SecurityParser ();
				using (StreamReader sr = new StreamReader (filename)) {
					string xml = sr.ReadToEnd ();
					sp.LoadXml (xml);
				}
				SecurityElement root = sp.ToXml ();
				if ((root != null) && (root.Tag == "configuration")) {
					SecurityElement strongnames  = root.SearchForChildByTag ("strongNames");
					if ((strongnames != null) && (strongnames.Children.Count > 0)) {
						SecurityElement mapping  = strongnames.SearchForChildByTag ("pubTokenMapping");
						if ((mapping != null) && (mapping.Children.Count > 0)) {
							LoadMapping (mapping);
						}

						SecurityElement settings = strongnames.SearchForChildByTag ("verificationSettings");
						if ((settings != null) && (settings.Children.Count > 0)) {
							LoadVerificationSettings (settings);
						}
					}
				}
			}
		}

		static private void LoadMapping (SecurityElement mapping) 
		{
			if (mappings == null) {
				mappings = new Hashtable ();
			}

			lock (mappings.SyncRoot) {
				foreach (SecurityElement item in mapping.Children) {
					if (item.Tag != "map")
						continue;

					string token = item.Attribute ("Token");
					if ((token == null) || (token.Length != 16))
						continue; // invalid entry
					token = token.ToUpper (CultureInfo.InvariantCulture);

					string publicKey = item.Attribute ("PublicKey");
					if (publicKey == null)
						continue; // invalid entry
				
					// watch for duplicate entries
					if (mappings [token] == null) {
						mappings.Add (token, publicKey);
					}
					else {
						// replace existing mapping
						mappings [token] = publicKey;
					}
				}
			}
		}

		static private void LoadVerificationSettings (SecurityElement settings) 
		{
			if (tokens == null) {
				tokens = new Hashtable ();
			}

			lock (tokens.SyncRoot) {
				foreach (SecurityElement item in settings.Children) {
					if (item.Tag != "skip")
						continue;

					string token = item.Attribute ("Token");
					if (token == null)
						continue;	// bad entry
					token = token.ToUpper (CultureInfo.InvariantCulture);

					string assembly = item.Attribute ("Assembly");
					if (assembly == null)
						assembly = "*";

					string users = item.Attribute ("Users");
					if (users == null)
						users = "*";

					Element el = (Element) tokens [token];
					if (el == null) {
						// new token
						el = new Element (assembly, users);
						tokens.Add (token, el);
						continue;
					}

					// existing token
					string a = (string) el.assemblies [assembly];
					if (a == null) {
						// new assembly
						el.assemblies.Add (assembly, users);
						continue;
					}

					// existing assembly
					if (users == "*") {
						// all users (drop current users)
						el.assemblies [assembly] = "*";
						continue;
					}

					// new users, add to existing
					string existing = (string) el.assemblies [assembly];
					string newusers = String.Concat (existing, ",", users);
					el.assemblies [assembly] = newusers;
				}
			}
		}

		static public byte[] GetMappedPublicKey (byte[] token) 
		{
			if ((mappings == null) || (token == null))
				return null;

			string t = CryptoConvert.ToHex (token);
			string pk = (string) mappings [t];
			if (pk == null)
				return null;

			return CryptoConvert.FromHex (pk);
		}

		// it is possible to skip verification for assemblies 
		// or a strongname public key using the "sn" tool.
		// note: only the runtime checks if the assembly is loaded 
		// from the GAC to skip verification
		static public bool MustVerify (AssemblyName an)
		{
			if ((an == null) || (tokens == null))
				return true;

			string token = CryptoConvert.ToHex (an.GetPublicKeyToken ());
			Element el = (Element) tokens [token];
			if (el != null) {
				// look for this specific assembly first
				string users = el.GetUsers (an.Name);
				if (users == null) {
					// nothing for the specific assembly
					// so look for "*" assembly
					users = el.GetUsers ("*");
				}

				if (users != null) {
					// applicable to any user ?
					if (users == "*")
						return false;
					// applicable to the current user ?
					return (users.IndexOf (Environment.UserName) < 0);
				}
			}

			// we must check verify the strongname on the assembly
			return true;
		}

		public override string ToString () 
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("Public Key Token\tAssemblies\t\tUsers");
			sb.Append (Environment.NewLine);
			foreach (DictionaryEntry token in tokens) {
				sb.Append ((string)token.Key);
				Element t = (Element) token.Value;
				bool first = true;
				foreach (DictionaryEntry assembly in t.assemblies) {
					if (first) {
						sb.Append ("\t");
						first = false;
					}
					else {
						sb.Append ("\t\t\t");
					}
					sb.Append ((string)assembly.Key);
					sb.Append ("\t");
					string users = (string)assembly.Value;
					if (users == "*")
						users = "All users";
					sb.Append (users);
					sb.Append (Environment.NewLine);
				}
			}
			return sb.ToString ();
		}
	}
}
