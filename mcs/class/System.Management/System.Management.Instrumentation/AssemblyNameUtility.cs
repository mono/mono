//
// AssemblyRef
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
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
using System.Globalization;
using System.Reflection;

namespace System.Management.Instrumentation
{
	internal sealed class AssemblyNameUtility
	{
		public AssemblyNameUtility()
		{
		}

		private static string BinToString(byte[] rg)
		{
			if (rg != null)
			{
				string str = "";
				for (int i = 0; i < rg.GetLength(0); i++)
				{
					str = string.Concat(str, string.Format("{0:x2}", rg[i]));
				}
				return str;
			}
			else
			{
				return "";
			}
		}

		public static string UniqueToAssemblyBuild(Assembly assembly)
		{
			Guid mvid = MetaDataInfo.GetMvid(assembly);
			return string.Concat(AssemblyNameUtility.UniqueToAssemblyVersion(assembly), "_Mvid_", mvid.ToString().ToLower(CultureInfo.InvariantCulture));
		}

		public static string UniqueToAssemblyFullVersion(Assembly assembly)
		{
			AssemblyName name = assembly.GetName(true);
			object[] str = new object[11];
			str[0] = name.Name;
			str[1] = "_SN_";
			str[2] = AssemblyNameUtility.BinToString(name.GetPublicKeyToken());
			str[3] = "_Version_";
			str[4] = name.Version.Major;
			str[5] = ".";
			str[6] = name.Version.Minor;
			str[7] = ".";
			str[8] = name.Version.Build;
			str[9] = ".";
			str[10] = name.Version.Revision;
			return string.Concat(str);
		}

		public static string UniqueToAssemblyMinorVersion(Assembly assembly)
		{
			AssemblyName name = assembly.GetName(true);
			object[] str = new object[7];
			str[0] = name.Name;
			str[1] = "_SN_";
			str[2] = AssemblyNameUtility.BinToString(name.GetPublicKeyToken());
			str[3] = "_Version_";
			str[4] = name.Version.Major;
			str[5] = ".";
			str[6] = name.Version.Minor;
			return string.Concat(str);
		}

		private static string UniqueToAssemblyVersion(Assembly assembly)
		{
			AssemblyName name = assembly.GetName(true);
			object[] str = new object[11];
			str[0] = name.Name;
			str[1] = "_SN_";
			str[2] = AssemblyNameUtility.BinToString(name.GetPublicKeyToken());
			str[3] = "_Version_";
			str[4] = name.Version.Major;
			str[5] = ".";
			str[6] = name.Version.Minor;
			str[7] = ".";
			str[8] = name.Version.Build;
			str[9] = ".";
			str[10] = name.Version.Revision;
			return string.Concat(str);
		}
	}
}