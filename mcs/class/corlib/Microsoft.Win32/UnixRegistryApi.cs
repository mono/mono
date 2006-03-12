//
// Microsoft.Win32/IRegistryApi.cs
//
// Authors:
//	Miguel de Icaza (miguel@gnome.org)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
// 
// MISSING:
//   Someone could the same subkey twice: once read/write once readonly,
//   currently since we use a unique hash based on the file name, we are unable
//   to have two versions of the same key and hence unable to throw an exception
//   if the user tries to write to a read-only key.
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
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Security;
using System.Threading;

namespace Microsoft.Win32 {

	class KeyHandler {
		static Hashtable key_to_handler = new Hashtable ();
		static Hashtable dir_to_key = new Hashtable ();
		public string Dir;
		public IntPtr Handle;

		public Hashtable values;
		string file;
		bool dirty;
		
		KeyHandler (RegistryKey rkey, string basedir)
		{
			if (!Directory.Exists (basedir)){
				try {
					Directory.CreateDirectory (basedir);
				} catch (Exception e){
					Console.Error.WriteLine ("KeyHandler error while creating directory {0}:\n{1}", basedir, e);
				}
			}
			Dir = basedir;
			file = Path.Combine (Dir, "values.xml");
			Load ();
		}

		public void Load ()
		{
			values = new Hashtable ();
			if (!File.Exists (file))
				return;
			
			try {
				using (FileStream fs = File.OpenRead (file)){
					StreamReader r = new StreamReader (fs);
					string xml = r.ReadToEnd ();
					if (xml.Length == 0)
						return;
					
					SecurityElement tree = SecurityElement.FromString (xml);
					if (tree.Tag == "values"){
						foreach (SecurityElement value in tree.Children){
							if (value.Tag == "value"){
								LoadKey (value);
							}
						}
					}
				}
			} catch (Exception e){
				Console.Error.WriteLine ("While loading registry key at {0}: {1}", file, e);
				values.Clear ();
			}
		}

		void LoadKey (SecurityElement se)
		{
			Hashtable h = se.Attributes;
			try {
				string name = (string) h ["name"];
				if (name == null)
					return;
				string type = (string) h ["type"];
				if (type == null)
					return;
				
				switch (type){
				case "int":
					values [name] = Int32.Parse (se.Text);
					break;
				case "bytearray":
					Convert.FromBase64String (se.Text);
					break;
				case "string":
					values [name] = se.Text;
					break;
				case "string-array":
					ArrayList sa = new ArrayList ();
					if (se.Children != null){
						foreach (SecurityElement stre in se.Children){
							sa.Add (stre.Text);
						}
					}
					values [name] = sa.ToArray (typeof (string));
					break;
				}
			} catch {
				// We ignore individual errors in the file.
			}
		}
		
		public RegistryKey Ensure (RegistryKey rkey, string extra)
		{
			lock (typeof (KeyHandler)){
				string f = Path.Combine (Dir, extra);
				if (dir_to_key.Contains (f))
					return (RegistryKey) dir_to_key [f];

				KeyHandler kh = new KeyHandler (rkey, f);
				RegistryKey rk = new RegistryKey (kh, CombineName (rkey, extra));
				key_to_handler [rk] = kh;
				dir_to_key [f] = rk;
				return rk;
			}
		}

		public RegistryKey Probe (RegistryKey rkey, string extra, bool write)
		{
			lock (typeof (KeyHandler)){
				string f = Path.Combine (Dir, extra);
				if (dir_to_key.Contains (f))
					return (RegistryKey) dir_to_key [f];
				if (Directory.Exists (f)){
					KeyHandler kh = new KeyHandler (rkey, f);
					RegistryKey rk = new RegistryKey (kh, CombineName (rkey, extra));
					dir_to_key [f] = rk;
					key_to_handler [rk] = kh;
					return rk;
				}
				return null;
			}
		}

		static string CombineName (RegistryKey rkey, string extra)
		{
			if (extra.IndexOf ('/') != -1)
				extra = extra.Replace ('/', '\\');
			
			return String.Concat (rkey.Name, "\\", extra);
		}
		
		public static KeyHandler Lookup (RegistryKey rkey)
		{
			lock (typeof (KeyHandler)){
				KeyHandler k = (KeyHandler) key_to_handler [rkey];
				if (k != null)
					return k;

				RegistryHive x = (RegistryHive) rkey.Data;
				switch (x){
				case RegistryHive.ClassesRoot:
				case RegistryHive.CurrentConfig:
				case RegistryHive.CurrentUser:
				case RegistryHive.DynData:
				case RegistryHive.LocalMachine:
				case RegistryHive.PerformanceData:
				case RegistryHive.Users:
					string d = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), ".mono/registry");
					d = Path.Combine (d, x.ToString ());
					
					k = new KeyHandler (rkey, d);
					key_to_handler [rkey] = k;
					break;
				default:
					throw new Exception ("Unknown RegistryHive");
				}
				key_to_handler [rkey] = k;
				return k;
			}
		}

		public static void Drop (RegistryKey rkey)
		{
			KeyHandler k = (KeyHandler) key_to_handler [rkey];
			if (k == null)
				return;
			dir_to_key.Remove (k.Dir);
			key_to_handler.Remove (rkey);
		}

		public static void Drop (string dir)
		{
			if (dir_to_key.Contains (dir)){
				key_to_handler.Remove (dir_to_key [dir]); 
				dir_to_key.Remove (dir);
			}
		}

		public void SetValue (string name, object value)
		{
			values [name] = value;
			SetDirty ();
		}

		void SetDirty ()
		{
			lock (typeof (KeyHandler)){
				if (dirty)
					return;
				dirty = true;
				new Timer (DirtyTimeout, null, 3000, Timeout.Infinite);
			}
		}

		public void DirtyTimeout (object state)
		{
			Flush ();
		}
		
		public void Flush ()
		{
			lock (typeof (KeyHandler)){
				dirty = false;
				Save ();
			}
		}

		~KeyHandler ()
		{
			Flush ();
		}
		
		void Save ()
		{
			if (!File.Exists (file) && values.Count == 0)
				return;
			
			SecurityElement se = new SecurityElement ("values");
			
			foreach (DictionaryEntry de in values){
				object val = de.Value;
				SecurityElement value = new SecurityElement ("value");
				value.AddAttribute ("name", (string) de.Key);
				
				if (val is string){
					value.AddAttribute ("type", "string");
					value.Text = (string) val;
				} else if (val is int){
					value.AddAttribute ("type", "int");
					value.Text = val.ToString ();
				} else if (val is byte []){
					value.AddAttribute ("type", "bytearray");
					value.Text = Convert.ToBase64String ((byte[]) val);
				} else if (val is string []){
					value.AddAttribute ("type", "string-array");

					foreach (string ss in (string[]) val){
						SecurityElement str = new SecurityElement ("string");
						str.Text = ss; 
						value.AddChild (str);
					}
				}
				se.AddChild (value);
			}

			try {
				using (FileStream fs = File.Create (file)){
					StreamWriter sw = new StreamWriter (fs);

					sw.Write (se.ToString ());
					sw.Flush ();
				}
			} catch (Exception e){
				Console.Error.WriteLine ("When saving {0} got {1}", file, e);
			}
		}
	}
	
	internal class UnixRegistryApi : IRegistryApi {

		static string ToUnix (string keyname)
		{
			if (keyname.IndexOf ('\\') != -1)
				keyname = keyname.Replace ('\\', '/');
			return keyname.ToLower ();
		}
		
		public RegistryKey CreateSubKey (RegistryKey rkey, string keyname)
		{
			KeyHandler self = KeyHandler.Lookup (rkey);
			return self.Ensure (rkey, ToUnix (keyname));
		}

		public RegistryKey OpenSubKey (RegistryKey rkey, string keyname, bool writtable)
		{
			KeyHandler self = KeyHandler.Lookup (rkey);
			return self.Probe (rkey, ToUnix (keyname), writtable);
		}
		
		public void Flush (RegistryKey rkey)
		{
			KeyHandler self = KeyHandler.Lookup (rkey);
			self.Flush ();
		}
		
		public void Close (RegistryKey rkey)
		{
			KeyHandler.Drop (rkey);
		}
		
		public object GetValue (RegistryKey rkey, string name, bool return_default_value, object default_value)
		{
			KeyHandler self = KeyHandler.Lookup (rkey);

			if (self.values.Contains (name))
				return self.values [name];
			if (return_default_value)
				return default_value;
			return null;
		}
		
		public void SetValue (RegistryKey rkey, string name, object value)
		{
			if (!((value is int) || (value is string) || (value is string []) || (value is byte [])))
				throw new ArgumentException ("The value is not int, string, string[] or byte[]", "value");
			
			KeyHandler self = KeyHandler.Lookup (rkey);
			self.SetValue (name, value);
		}

		public int SubKeyCount (RegistryKey rkey)
		{
			KeyHandler self = KeyHandler.Lookup (rkey);

			return Directory.GetDirectories (self.Dir).Length;
		}
		
		public int ValueCount (RegistryKey rkey)
		{
			KeyHandler self = KeyHandler.Lookup (rkey);

			return self.values.Keys.Count;
		}
		
		public void DeleteValue (RegistryKey rkey, string name, bool throw_if_missing)
		{
			KeyHandler self = KeyHandler.Lookup (rkey);

			if (throw_if_missing && !self.values.Contains (name))
				throw new ArgumentException ("the given value does not exist", "name");

			self.values.Remove (name);
		}
		
		public void DeleteKey (RegistryKey rkey, string keyname, bool throw_if_missing)
		{
			KeyHandler self = KeyHandler.Lookup (rkey);
			string dir = Path.Combine (self.Dir, keyname);
			
			if (Directory.Exists (dir)){
				Directory.Delete (dir, true);
				KeyHandler.Drop (dir);
			} else if (throw_if_missing)
				throw new ArgumentException ("the given value does not exist", "value");
		}
		
		public string [] GetSubKeyNames (RegistryKey rkey)
		{
			KeyHandler self = KeyHandler.Lookup (rkey);
			return Directory.GetDirectories (self.Dir);
		}
		
		public string [] GetValueNames (RegistryKey rkey)
		{
			KeyHandler self = KeyHandler.Lookup (rkey);
			ICollection keys = self.values.Keys;

			string [] vals = new string [keys.Count];
			keys.CopyTo (vals, 0);
			return vals;
		}

		public string ToString (RegistryKey rkey)
		{
			return rkey.Name;
		}
	}
}
