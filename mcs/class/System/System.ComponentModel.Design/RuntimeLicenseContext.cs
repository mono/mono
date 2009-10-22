//
// System.ComponentModel.Design.RuntimeLicenseContext.cs
//
// Authors:
//   Ivan Hamilton (ivan@chimerical.com.au)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Carlo Kok  (ck@remobjects.com)
//
// (C) 2004 Ivan Hamilton
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
// (C) 2009 Carlo Kok
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

using System.ComponentModel;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.ComponentModel.Design
{
	internal class RuntimeLicenseContext : LicenseContext
	{
        private Hashtable extraassemblies;
		private Hashtable keys;

		public RuntimeLicenseContext () : base ()
		{
		}

		void LoadKeys ()
		{
			if (keys != null)
				return;
			keys = new Hashtable ();

			Assembly asm = Assembly.GetEntryAssembly ();
			if (asm != null)
				LoadAssemblyLicenses (keys, asm);
			else {
				foreach (Assembly asmnode in AppDomain.CurrentDomain.GetAssemblies ()) {
						LoadAssemblyLicenses (keys, asmnode);
				}
			}
		}

		void LoadAssemblyLicenses (Hashtable targetkeys, Assembly asm)
		{
            if (asm is System.Reflection.Emit.AssemblyBuilder) return; 
			string asmname = Path.GetFileName (asm.Location);
			string resourcename = asmname + ".licenses";
			try {
				foreach (string name in asm.GetManifestResourceNames ()) {
					if (name != resourcename)
						continue;
					using (Stream stream = asm.GetManifestResourceStream (name)) {
						BinaryFormatter formatter = new BinaryFormatter ();
						object[] res = formatter.Deserialize (stream) as object[];
						if (String.Compare ((string) res[0], asmname, true) == 0) {
							Hashtable table = (Hashtable) res[1];
							foreach (DictionaryEntry et in table)
                                targetkeys.Add (et.Key, et.Value);
						}
					}
				}

			} catch (InvalidCastException) {
			}
		}

		public override string GetSavedLicenseKey (Type type, Assembly resourceAssembly)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
            if (resourceAssembly != null) {
                if (extraassemblies == null) 
					extraassemblies = new Hashtable ();
                Hashtable ht = extraassemblies [resourceAssembly.FullName] as Hashtable;
                if (ht == null) {
                    ht = new Hashtable ();
                    LoadAssemblyLicenses (ht, resourceAssembly);
                    extraassemblies [resourceAssembly.FullName] = ht;
                }
                return (string) ht [type.AssemblyQualifiedName];
            }
			LoadKeys ();
			return (string) keys [type.AssemblyQualifiedName];
		}

		public override void SetSavedLicenseKey (Type type, string key)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			LoadKeys ();
			keys [type.AssemblyQualifiedName] = key;
		}
	}
}
