//
// Microsoft.Win32.Registry.cs
//
// Author:
//   Miguel de Icaza (miguel@novell.com)
//   stubbed out by Alexandre Pigolkine (pigolkine@gmx.de)
//

//
// Copyright (C) 2004, 2005 Novell, Inc (http://www.novell.com)
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
using System.Runtime.InteropServices;

namespace Microsoft.Win32
{
	[ComVisible (true)]
	public static class Registry
	{
		public static readonly RegistryKey ClassesRoot = new RegistryKey (
				RegistryHive.ClassesRoot);
		public static readonly RegistryKey CurrentConfig = new RegistryKey (
				RegistryHive.CurrentConfig);
		public static readonly RegistryKey CurrentUser = new RegistryKey (
				RegistryHive.CurrentUser);

#if NET_4_0
		[Obsolete ("Use PerformanceData instead")]
#endif
		public static readonly RegistryKey DynData = new RegistryKey (
				RegistryHive.DynData);
		public static readonly RegistryKey LocalMachine = new RegistryKey (
				RegistryHive.LocalMachine);
		public static readonly RegistryKey PerformanceData = new RegistryKey (
				RegistryHive.PerformanceData);
		public static readonly RegistryKey Users = new RegistryKey (
				RegistryHive.Users);

		static RegistryKey ToKey (string keyName, bool setting)
		{
			if (keyName == null)
				throw new ArgumentException ("Not a valid registry key name", "keyName");

			RegistryKey key = null;
			string [] keys = keyName.Split ('\\');

			switch (keys [0]){
			case "HKEY_CLASSES_ROOT":
				key = ClassesRoot;
				break;
			case "HKEY_CURRENT_CONFIG":
				key = CurrentConfig;
				break;
			case "HKEY_CURRENT_USER":
				key = CurrentUser;
				break;
			case "HKEY_DYN_DATA":
				key = DynData;
				break;
			case "HKEY_LOCAL_MACHINE":
				key = LocalMachine;
				break;
			case "HKEY_PERFORMANCE_DATA":
				key = PerformanceData;
				break;
			case "HKEY_USERS":
				key = Users;
				break;
			default:
				throw new ArgumentException ("Keyname does not start with a valid registry root", "keyName");
			}

			for (int i = 1; i < keys.Length; i++){
				RegistryKey nkey = key.OpenSubKey (keys [i], true);
				if (nkey == null){
					if (!setting)
						return null;
					nkey = key.CreateSubKey (keys [i]);
				}
				key = nkey;
			}
			return key;
		}
		
		public static void SetValue (string keyName, string valueName, object value)
		{
			RegistryKey key = ToKey (keyName, true);
			if (valueName.Length > 255)
				throw new ArgumentException ("valueName is larger than 255 characters", "valueName");

			if (key == null)
				throw new ArgumentException ("cant locate that keyName", "keyName");

			key.SetValue (valueName, value);
		}

		public static void SetValue (string keyName, string valueName, object value, RegistryValueKind valueKind)
		{
			RegistryKey key = ToKey (keyName, true);
			if (valueName.Length > 255)
				throw new ArgumentException ("valueName is larger than 255 characters", "valueName");

			if (key == null)
				throw new ArgumentException ("cant locate that keyName", "keyName");

			key.SetValue (valueName, value, valueKind);
		}
		
		public static object GetValue (string keyName, string valueName, object defaultValue)
		{
			RegistryKey key = ToKey (keyName, false);
			if (key == null)
				return defaultValue;
			
			return key.GetValue (valueName, defaultValue);
		}
	}
}

#endif // NET_2_1

