//
// Microsoft.Win32/UnixRegistryApi.cs
//
// Authors:
//	Miguel de Icaza (miguel@gnome.org)
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2005, 2006 Novell, Inc (http://www.novell.com)
// 
// MISSING:
//   It would be useful if we do case-insensitive expansion of variables,
//   the registry is very windows specific, so we probably should default to
//   those semantics in expanding environment variables, for example %path%
//
//   We should use an ordered collection for storing the values (instead of
//   a Hashtable).
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

#if !NET_2_1

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Security;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Win32 {

	class ExpandString {
		string value;
		
		public ExpandString (string s)
		{
			value = s;
		}

		public override string ToString ()
		{
			return value;
		}

		public string Expand ()
		{
			StringBuilder sb = new StringBuilder ();

			for (int i = 0; i < value.Length; i++){
				if (value [i] == '%'){
					int j = i + 1;
					for (; j < value.Length; j++){
						if (value [j] == '%'){
							string key = value.Substring (i + 1, j - i - 1);

							sb.Append (Environment.GetEnvironmentVariable (key));
							i += j;
							break;
						}
					}
					if (j == value.Length){
						sb.Append ('%');
					}
				} else {
					sb.Append (value [i]);
				}
			}
			return sb.ToString ();
		}
	}

	class KeyHandler
	{
		static Hashtable key_to_handler = new Hashtable ();
		static Hashtable dir_to_handler = new Hashtable (
			new CaseInsensitiveHashCodeProvider (), new CaseInsensitiveComparer ());
		const string VolatileDirectoryName = "volatile-keys";

		public string Dir;
		string ActualDir; // Lets keep this one private.
		public bool IsVolatile;

		Hashtable values;
		string file;
		bool dirty;

		static KeyHandler ()
		{
			CleanVolatileKeys ();
		}

		KeyHandler (RegistryKey rkey, string basedir) : this (rkey, basedir, false)
		{
		}

		KeyHandler (RegistryKey rkey, string basedir, bool is_volatile)
		{
			// Force ourselved to reuse the key, if any.
			string volatile_basedir = GetVolatileDir (basedir);
			string actual_basedir = basedir;

			if (Directory.Exists (basedir))
				is_volatile = false;
			else if (Directory.Exists (volatile_basedir)) {
				actual_basedir = volatile_basedir;
				is_volatile = true;
			} else if (is_volatile)
				actual_basedir = volatile_basedir;

			if (!Directory.Exists (actual_basedir)) {
				try {
					Directory.CreateDirectory (actual_basedir);
				} catch (UnauthorizedAccessException){
					throw new SecurityException ("No access to the given key");
				}
			}
			Dir = basedir; // This is our identifier.
			ActualDir = actual_basedir; // This our actual location.
			IsVolatile = is_volatile;
			file = Path.Combine (ActualDir, "values.xml");
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
					if (tree.Tag == "values" && tree.Children != null){
						foreach (SecurityElement value in tree.Children){
							if (value.Tag == "value"){
								LoadKey (value);
							}
						}
					}
				}
			} catch (UnauthorizedAccessException){
				values.Clear ();
				throw new SecurityException ("No access to the given key");
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
					values [name] = Convert.FromBase64String (se.Text);
					break;
				case "string":
					values [name] = se.Text == null ? String.Empty : se.Text;
					break;
				case "expand":
					values [name] = new ExpandString (se.Text);
					break;
				case "qword":
					values [name] = Int64.Parse (se.Text);
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

		public RegistryKey Ensure (RegistryKey rkey, string extra, bool writable)
		{
			return Ensure (rkey, extra, writable, false);
		}

		// 'is_volatile' is used only if the key hasn't been created already.
		public RegistryKey Ensure (RegistryKey rkey, string extra, bool writable, bool is_volatile)
		{
			lock (typeof (KeyHandler)){
				string f = Path.Combine (Dir, extra);
				KeyHandler kh = (KeyHandler) dir_to_handler [f];
				if (kh == null)
					kh = new KeyHandler (rkey, f, is_volatile);
				RegistryKey rk = new RegistryKey (kh, CombineName (rkey, extra), writable);
				key_to_handler [rk] = kh;
				dir_to_handler [f] = kh;
				return rk;
			}
		}

		public RegistryKey Probe (RegistryKey rkey, string extra, bool writable)
		{
			RegistryKey rk = null;

			lock (typeof (KeyHandler)){
				string f = Path.Combine (Dir, extra);
				KeyHandler kh = (KeyHandler) dir_to_handler [f];
				if (kh != null) {
					rk = new RegistryKey (kh, CombineName (rkey,
						extra), writable);
					key_to_handler [rk] = kh;
				} else if (Directory.Exists (f) || VolatileKeyExists (f)) {
					kh = new KeyHandler (rkey, f);
					rk = new RegistryKey (kh, CombineName (rkey, extra),
						writable);
					dir_to_handler [f] = kh;
					key_to_handler [rk] = kh;
				}
				return rk;
			}
		}

		static string CombineName (RegistryKey rkey, string extra)
		{
			if (extra.IndexOf ('/') != -1)
				extra = extra.Replace ('/', '\\');
			
			return String.Concat (rkey.Name, "\\", extra);
		}

		static long GetSystemBootTime ()
		{
			if (!File.Exists ("/proc/stat"))
				return -1;

			string btime = null;
			string line;

			try {
				using (StreamReader stat_file = new StreamReader ("/proc/stat", Encoding.ASCII)) {
					while ((line = stat_file.ReadLine ()) != null)
						if (line.StartsWith ("btime")) {
							btime = line;
							break;
						}
				}
			} catch (Exception e) {
				Console.Error.WriteLine ("While reading system info {0}", e);
			}

			if (btime == null)
				return -1;

			int space = btime.IndexOf (' ');
			long res;
			if (!Int64.TryParse (btime.Substring (space, btime.Length - space), out res))
				return -1;

			return res;
		}

		// The registered boot time it's a simple line containing the last system btime.
		static long GetRegisteredBootTime (string path)
		{
			if (!File.Exists (path))
				return -1;

			string line = null;
			try {
				using (StreamReader reader = new StreamReader (path, Encoding.ASCII))
					line = reader.ReadLine ();
			} catch (Exception e) {
				Console.Error.WriteLine ("While reading registry data at {0}: {1}", path, e);
			}

			if (line == null)
				return -1;

			long res;
			if (!Int64.TryParse (line, out res))
				return -1;

			return res;
		}

		static void SaveRegisteredBootTime (string path, long btime)
		{
			try {
				using (StreamWriter writer = new StreamWriter (path, false, Encoding.ASCII))
					writer.WriteLine (btime.ToString ());
			} catch (Exception e) {
				Console.Error.WriteLine ("While saving registry data at {0}: {1}", path, e);
			}
		}
			
		// We save the last boot time in a last-btime file in every root, and we use it
		// to clean the volatile keys directory in case the system btime changed.
		static void CleanVolatileKeys ()
		{
			long system_btime = GetSystemBootTime ();

			string [] roots = new string [] {
				UserStore,
				MachineStore
			};

			foreach (string root in roots) {
				if (!Directory.Exists (root))
					continue;

				string btime_file = Path.Combine (root, "last-btime");
				string volatile_dir = Path.Combine (root, VolatileDirectoryName);

				if (Directory.Exists (volatile_dir)) {
					long registered_btime = GetRegisteredBootTime (btime_file);
					if (system_btime < 0 || registered_btime < 0 || registered_btime != system_btime)
						Directory.Delete (volatile_dir, true);
				}

				SaveRegisteredBootTime (btime_file, system_btime);
			}
		}
	
		public static bool VolatileKeyExists (string dir)
		{
			lock (typeof (KeyHandler)) {
				KeyHandler kh = (KeyHandler) dir_to_handler [dir];
				if (kh != null)
					return kh.IsVolatile;
			}

			if (Directory.Exists (dir)) // Non-volatile key exists.
				return false;

			return Directory.Exists (GetVolatileDir (dir));
		}

		public static string GetVolatileDir (string dir)
		{
			string root = GetRootFromDir (dir);
			string volatile_dir = dir.Replace (root, Path.Combine (root, VolatileDirectoryName));
			return volatile_dir;
		}

		public static KeyHandler Lookup (RegistryKey rkey, bool createNonExisting)
		{
			lock (typeof (KeyHandler)){
				KeyHandler k = (KeyHandler) key_to_handler [rkey];
				if (k != null)
					return k;

				// when a non-root key is requested for no keyhandler exist
				// then that key must have been marked for deletion
				if (!rkey.IsRoot || !createNonExisting)
					return null;

				RegistryHive x = (RegistryHive) rkey.Hive;
				switch (x){
				case RegistryHive.CurrentUser:
					string userDir = Path.Combine (UserStore, x.ToString ());
					k = new KeyHandler (rkey, userDir);
					dir_to_handler [userDir] = k;
					break;
				case RegistryHive.CurrentConfig:
				case RegistryHive.ClassesRoot:
				case RegistryHive.DynData:
				case RegistryHive.LocalMachine:
				case RegistryHive.PerformanceData:
				case RegistryHive.Users:
					string machine_dir = Path.Combine (MachineStore, x.ToString ());
					k = new KeyHandler (rkey, machine_dir);
					dir_to_handler [machine_dir] = k;
					break;
				default:
					throw new Exception ("Unknown RegistryHive");
				}
				key_to_handler [rkey] = k;
				return k;
			}
		}

		static string GetRootFromDir (string dir)
		{
			if (dir.IndexOf (UserStore) > -1)
				return UserStore;
			else if (dir.IndexOf (MachineStore) > -1)
				return MachineStore;

			throw new Exception ("Could not get root for dir " + dir);
		}

		public static void Drop (RegistryKey rkey)
		{
			lock (typeof (KeyHandler)) {
				KeyHandler k = (KeyHandler) key_to_handler [rkey];
				if (k == null)
					return;
				key_to_handler.Remove (rkey);

				// remove cached KeyHandler if no other keys reference it
				int refCount = 0;
				foreach (DictionaryEntry de in key_to_handler)
					if (de.Value == k)
						refCount++;
				if (refCount == 0)
					dir_to_handler.Remove (k.Dir);
			}
		}

		public static void Drop (string dir)
		{
			lock (typeof (KeyHandler)) {
				KeyHandler kh = (KeyHandler) dir_to_handler [dir];
				if (kh == null)
					return;

				dir_to_handler.Remove (dir);

				// remove (other) references to keyhandler
				ArrayList keys = new ArrayList ();
				foreach (DictionaryEntry de in key_to_handler)
					if (de.Value == kh)
						keys.Add (de.Key);

				foreach (object key in keys)
					key_to_handler.Remove (key);
			}
		}

		public static bool Delete (string dir)
		{
			if (!Directory.Exists (dir)) {
				string volatile_dir = GetVolatileDir (dir);
				if (!Directory.Exists (volatile_dir))
					return false;

				dir = volatile_dir;
			}

			Directory.Delete (dir, true);
			Drop (dir);
			return true;
		}

		public RegistryValueKind GetValueKind (string name)
		{
			if (name == null)
				return RegistryValueKind.Unknown;
			object value = values [name];
			if (value == null)
				return RegistryValueKind.Unknown;

			if (value is int)
				return RegistryValueKind.DWord;
			if (value is string [])
				return RegistryValueKind.MultiString;
			if (value is long)
				return RegistryValueKind.QWord;
			if (value is byte [])
				return RegistryValueKind.Binary;
			if (value is string)
				return RegistryValueKind.String;
			if (value is ExpandString)
				return RegistryValueKind.ExpandString;
			return RegistryValueKind.Unknown;
		}
		
		public object GetValue (string name, RegistryValueOptions options)
		{
			if (IsMarkedForDeletion)
				return null;

			if (name == null)
				name = string.Empty;
			object value = values [name];
			ExpandString exp = value as ExpandString;
			if (exp == null)
				return value;
			if ((options & RegistryValueOptions.DoNotExpandEnvironmentNames) == 0)
				return exp.Expand ();

			return exp.ToString ();
		}

		public void SetValue (string name, object value)
		{
			AssertNotMarkedForDeletion ();

			if (name == null)
				name = string.Empty;

			// immediately convert non-native registry values to string to avoid
			// returning it unmodified in calls to UnixRegistryApi.GetValue
			if (value is int || value is string || value is byte[] || value is string[])
				values[name] = value;
			else
				values[name] = value.ToString ();
			SetDirty ();
		}

		public string [] GetValueNames ()
		{
			AssertNotMarkedForDeletion ();

			ICollection keys = values.Keys;

			string [] vals = new string [keys.Count];
			keys.CopyTo (vals, 0);
			return vals;
		}

		public int GetSubKeyCount ()
		{
			return GetSubKeyNames ().Length;
		}

		public string [] GetSubKeyNames ()
		{
			DirectoryInfo selfDir = new DirectoryInfo (ActualDir);
			DirectoryInfo[] subDirs = selfDir.GetDirectories ();
			string[] subKeyNames;

			// for volatile keys (cannot contain non-volatile subkeys) or keys
			// without *any* presence in the volatile key section, we can do it simple.
			if (IsVolatile || !Directory.Exists (GetVolatileDir (Dir))) {
				subKeyNames = new string[subDirs.Length];
				for (int i = 0; i < subDirs.Length; i++) {
					DirectoryInfo subDir = subDirs[i];
					subKeyNames[i] = subDir.Name;
				}
				return subKeyNames;
			}

			// We may have the entries repeated, so keep just one of each one.
			DirectoryInfo volatileDir = new DirectoryInfo (GetVolatileDir (Dir));
			DirectoryInfo [] volatileSubDirs = volatileDir.GetDirectories ();
			Dictionary<string,string> dirs = new Dictionary<string,string> ();

			foreach (DirectoryInfo dir in subDirs)
				dirs [dir.Name] = dir.Name;
			foreach (DirectoryInfo volDir in volatileSubDirs)
				dirs [volDir.Name] = volDir.Name;

			subKeyNames = new string [dirs.Count];
			int j = 0;
			foreach (KeyValuePair<string,string> entry in dirs)
				subKeyNames[j++] = entry.Value;

			return subKeyNames;
		}

		//
		// This version has to do argument validation based on the valueKind
		//
		public void SetValue (string name, object value, RegistryValueKind valueKind)
		{
			SetDirty ();

			if (name == null)
				name = string.Empty;

			switch (valueKind){
			case RegistryValueKind.String:
				if (value is string){
					values [name] = value;
					return;
				}
				break;
			case RegistryValueKind.ExpandString:
				if (value is string){
					values [name] = new ExpandString ((string)value);
					return;
				}
				break;
				
			case RegistryValueKind.Binary:
				if (value is byte []){
					values [name] = value;
					return;
				}
				break;
				
			case RegistryValueKind.DWord:
				if (value is long &&
				    (((long) value) < Int32.MaxValue) &&
				    (((long) value) > Int32.MinValue)){
					values [name] = (int) ((long)value);
					return;
				}
				if (value is int){
					values [name] = value;
					return;
				}
				break;
				
			case RegistryValueKind.MultiString:
				if (value is string []){
					values [name] = value;
					return;
				}
				break;
				
			case RegistryValueKind.QWord:
				if (value is int){
					values [name] = (long) ((int) value);
					return;
				}
				if (value is long){
					values [name] = value;
					return;
				}
				break;
			default:
				throw new ArgumentException ("unknown value", "valueKind");
			}
			throw new ArgumentException ("Value could not be converted to specified type", "valueKind");
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
			lock (typeof (KeyHandler)) {
				if (dirty) {
					Save ();
					dirty = false;
				}
			}
		}

		public bool ValueExists (string name)
		{
			if (name == null)
				name = string.Empty;

			return values.Contains (name);
		}

		public int ValueCount {
			get {
				return values.Keys.Count;
			}
		}

		public bool IsMarkedForDeletion {
			get {
				return !dir_to_handler.Contains (Dir);
			}
		}

		public void RemoveValue (string name)
		{
			AssertNotMarkedForDeletion ();

			values.Remove (name);
			SetDirty ();
		}

		~KeyHandler ()
		{
			Flush ();
		}
		
		void Save ()
		{
			if (IsMarkedForDeletion)
				return;

			if (!File.Exists (file) && values.Count == 0)
				return;

			SecurityElement se = new SecurityElement ("values");
			
			// With SecurityElement.Text = value, and SecurityElement.AddAttribute(key, value)
			// the values must be escaped prior to being assigned. 
			foreach (DictionaryEntry de in values){
				object val = de.Value;
				SecurityElement value = new SecurityElement ("value");
				value.AddAttribute ("name", SecurityElement.Escape ((string) de.Key));
				
				if (val is string){
					value.AddAttribute ("type", "string");
					value.Text = SecurityElement.Escape ((string) val);
				} else if (val is int){
					value.AddAttribute ("type", "int");
					value.Text = val.ToString ();
				} else if (val is long) {
					value.AddAttribute ("type", "qword");
					value.Text = val.ToString ();
				} else if (val is byte []){
					value.AddAttribute ("type", "bytearray");
					value.Text = Convert.ToBase64String ((byte[]) val);
				} else if (val is ExpandString){
					value.AddAttribute ("type", "expand");
					value.Text = SecurityElement.Escape (val.ToString ());
				} else if (val is string []){
					value.AddAttribute ("type", "string-array");

					foreach (string ss in (string[]) val){
						SecurityElement str = new SecurityElement ("string");
						str.Text = SecurityElement.Escape (ss); 
						value.AddChild (str);
					}
				}
				se.AddChild (value);
			}

			using (FileStream fs = File.Create (file)){
				StreamWriter sw = new StreamWriter (fs);

				sw.Write (se.ToString ());
				sw.Flush ();
			}
		}

		private void AssertNotMarkedForDeletion ()
		{
			if (IsMarkedForDeletion)
				throw RegistryKey.CreateMarkedForDeletionException ();
		}

		static string user_store;
		static string machine_store;

		private static string UserStore {
			get {
				if (user_store == null)
					user_store = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal),
					".mono/registry");

				return user_store;
			}
		}

		private static string MachineStore {
			get {
				if (machine_store == null) {
					machine_store = Environment.GetEnvironmentVariable ("MONO_REGISTRY_PATH");
					if (machine_store == null) {
						string s = Environment.GetMachineConfigPath ();
						int p = s.IndexOf ("machine.config");
						machine_store = Path.Combine (Path.Combine (s.Substring (0, p-1), ".."), "registry");
					}
				}

				return machine_store;
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

		static bool IsWellKnownKey (string parentKeyName, string keyname)
		{
			// FIXME: Add more keys if needed
			if (parentKeyName == Registry.CurrentUser.Name ||
				parentKeyName == Registry.LocalMachine.Name)
				return (0 == String.Compare ("software", keyname, true, CultureInfo.InvariantCulture));

			return false;
		}

		public RegistryKey CreateSubKey (RegistryKey rkey, string keyname)
		{
			return CreateSubKey (rkey, keyname, true);
		}

#if NET_4_0
		public RegistryKey CreateSubKey (RegistryKey rkey, string keyname, RegistryOptions options)
		{
			return CreateSubKey (rkey, keyname, true, options == RegistryOptions.Volatile);
		}
#endif

		public RegistryKey OpenRemoteBaseKey (RegistryHive hKey, string machineName)
		{
			throw new NotImplementedException ();
		}

		public RegistryKey OpenSubKey (RegistryKey rkey, string keyname, bool writable)
		{
			KeyHandler self = KeyHandler.Lookup (rkey, true);
			if (self == null) {
				// return null if parent is marked for deletion
				return null;
			}

			RegistryKey result = self.Probe (rkey, ToUnix (keyname), writable);
			if (result == null && IsWellKnownKey (rkey.Name, keyname)) {
				// create the subkey even if its parent was opened read-only
				result = CreateSubKey (rkey, keyname, writable);
			}

			return result;
		}

#if NET_4_0
		public RegistryKey FromHandle (SafeRegistryHandle handle)
		{
			throw new NotImplementedException ();
		}
#endif
		
		public void Flush (RegistryKey rkey)
		{
			KeyHandler self = KeyHandler.Lookup (rkey, false);
			if (self == null) {
				// we do not need to flush changes as key is marked for deletion
				return;
			}
			self.Flush ();
		}
		
		public void Close (RegistryKey rkey)
		{
			KeyHandler.Drop (rkey);
		}

		public object GetValue (RegistryKey rkey, string name, object default_value, RegistryValueOptions options)
		{
			KeyHandler self = KeyHandler.Lookup (rkey, true);
			if (self == null) {
				// key was removed since it was opened
				return default_value;
			}

			if (self.ValueExists (name))
				return self.GetValue (name, options);
			return default_value;
		}
		
		public void SetValue (RegistryKey rkey, string name, object value)
		{
			KeyHandler self = KeyHandler.Lookup (rkey, true);
			if (self == null)
				throw RegistryKey.CreateMarkedForDeletionException ();
			self.SetValue (name, value);
		}

		public void SetValue (RegistryKey rkey, string name, object value, RegistryValueKind valueKind)
		{
			KeyHandler self = KeyHandler.Lookup (rkey, true);
			if (self == null)
				throw RegistryKey.CreateMarkedForDeletionException ();
			self.SetValue (name, value, valueKind);
		}

		public int SubKeyCount (RegistryKey rkey)
		{
			KeyHandler self = KeyHandler.Lookup (rkey, true);
			if (self == null)
				throw RegistryKey.CreateMarkedForDeletionException ();
			return self.GetSubKeyCount ();
		}
		
		public int ValueCount (RegistryKey rkey)
		{
			KeyHandler self = KeyHandler.Lookup (rkey, true);
			if (self == null)
				throw RegistryKey.CreateMarkedForDeletionException ();
			return self.ValueCount;
		}
		
		public void DeleteValue (RegistryKey rkey, string name, bool throw_if_missing)
		{
			KeyHandler self = KeyHandler.Lookup (rkey, true);
			if (self == null) {
				// if key is marked for deletion, report success regardless of
				// throw_if_missing
				return;
			}

			if (throw_if_missing && !self.ValueExists (name))
				throw new ArgumentException ("the given value does not exist");

			self.RemoveValue (name);
		}
		
		public void DeleteKey (RegistryKey rkey, string keyname, bool throw_if_missing)
		{
			KeyHandler self = KeyHandler.Lookup (rkey, true);
			if (self == null) {
				// key is marked for deletion
				if (!throw_if_missing)
					return;
				throw new ArgumentException ("the given value does not exist");
			}

			string dir = Path.Combine (self.Dir, ToUnix (keyname));
			
			if (!KeyHandler.Delete (dir) && throw_if_missing)
				throw new ArgumentException ("the given value does not exist");
		}
		
		public string [] GetSubKeyNames (RegistryKey rkey)
		{
			KeyHandler self = KeyHandler.Lookup (rkey, true);
			return self.GetSubKeyNames ();
		}
		
		public string [] GetValueNames (RegistryKey rkey)
		{
			KeyHandler self = KeyHandler.Lookup (rkey, true);
			if (self == null)
				throw RegistryKey.CreateMarkedForDeletionException ();
			return self.GetValueNames ();
		}

		public string ToString (RegistryKey rkey)
		{
			return rkey.Name;
		}

		private RegistryKey CreateSubKey (RegistryKey rkey, string keyname, bool writable)
		{
			return CreateSubKey (rkey, keyname, writable, false);
		}

		private RegistryKey CreateSubKey (RegistryKey rkey, string keyname, bool writable, bool is_volatile)
		{
			KeyHandler self = KeyHandler.Lookup (rkey, true);
			if (self == null)
				throw RegistryKey.CreateMarkedForDeletionException ();
			if (KeyHandler.VolatileKeyExists (self.Dir) && !is_volatile)
				throw new IOException ("Cannot create a non volatile subkey under a volatile key.");

			return self.Ensure (rkey, ToUnix (keyname), writable, is_volatile);
		}

		public RegistryValueKind GetValueKind (RegistryKey rkey, string name)
		{
			KeyHandler self = KeyHandler.Lookup (rkey, true);
			if (self != null) 
				return self.GetValueKind (name);

			// key was removed since it was opened or it does not exist.
			return RegistryValueKind.Unknown;
		}

#if NET_4_0
		public IntPtr GetHandle (RegistryKey key)
		{
			throw new NotImplementedException ();
		}
#endif
		
	}
}

#endif // NET_2_1

