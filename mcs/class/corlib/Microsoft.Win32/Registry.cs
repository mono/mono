//
// Microsoft.Win32.Registry.cs
//
// Author:
//   stubbed out by Alexandre Pigolkine (pigolkine@gmx.de)
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

namespace Microsoft.Win32
{
	public sealed class Registry
	{
		private Registry () { }
		public static readonly RegistryKey ClassesRoot = new RegistryKey (
				RegistryHive.ClassesRoot, "HKEY_CLASSES_ROOT");
		public static readonly RegistryKey CurrentConfig = new RegistryKey (
				RegistryHive.CurrentConfig, "HKEY_CURRENT_CONFIG");
		public static readonly RegistryKey CurrentUser = new RegistryKey (
				RegistryHive.CurrentUser, "HKEY_CURRENT_USER");
		public static readonly RegistryKey DynData = new RegistryKey (
				RegistryHive.DynData, "HKEY_DYN_DATA");
		public static readonly RegistryKey LocalMachine = new RegistryKey (
				RegistryHive.LocalMachine, "HKEY_DYN_DATA");
		public static readonly RegistryKey PerformanceData = new RegistryKey (
				RegistryHive.PerformanceData, "HKEY_PERFORMANCE_DATA");
		public static readonly RegistryKey Users = new RegistryKey (
				RegistryHive.Users, "HKEY_USERS");
	}
}
