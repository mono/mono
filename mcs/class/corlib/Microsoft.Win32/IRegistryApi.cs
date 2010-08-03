//
// Microsoft.Win32/IRegistryApi.cs
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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

#if !NET_2_1

using System;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Win32 {

	internal interface IRegistryApi {
		RegistryKey CreateSubKey (RegistryKey rkey, string keyname);
		RegistryKey OpenRemoteBaseKey (RegistryHive hKey, string machineName);
		RegistryKey OpenSubKey (RegistryKey rkey, string keyname, bool writtable);
		void Flush (RegistryKey rkey);
		void Close (RegistryKey rkey);

		object GetValue (RegistryKey rkey, string name, object default_value, RegistryValueOptions options);
		RegistryValueKind GetValueKind (RegistryKey rkey, string name);
		void SetValue (RegistryKey rkey, string name, object value);

		int SubKeyCount (RegistryKey rkey);
		int ValueCount (RegistryKey rkey);
		
		void DeleteValue (RegistryKey rkey, string value, bool throw_if_missing);
		void DeleteKey (RegistryKey rkey, string keyName, bool throw_if_missing);
		string [] GetSubKeyNames (RegistryKey rkey);
		string [] GetValueNames (RegistryKey rkey);
		string ToString (RegistryKey rkey);

		void SetValue (RegistryKey rkey, string name, object value, RegistryValueKind valueKind);

#if NET_4_0
		RegistryKey CreateSubKey (RegistryKey rkey, string keyname, RegistryOptions options);
		RegistryKey FromHandle (SafeRegistryHandle handle);
		IntPtr GetHandle (RegistryKey key);
#endif
	}
}

#endif // NET_2_1

