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
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace System.Management
{
	internal static class WbemClientFactory
	{
		public static IWbemClassObject_DoNotMarshal Get(string strObjectPath)
		{
			return new UnixWbemClassObject(new UnixWbemClass (null));
		}

		public static IEnumerable<IWbemClassObject_DoNotMarshal> Get(string nameSpace, string strQuery)
		{
			strQuery = Regex.Replace (strQuery, "Win32", "UNIX", RegexOptions.IgnoreCase);
			strQuery = Regex.Replace (strQuery, "unix_", "UNIX_", RegexOptions.IgnoreCase);

			System.Management.Internal.WbemClient client = new System.Management.Internal.WbemClient ("localhost", null, nameSpace);
			client.IsSecure = false;
			var obj = client.ExecQuery (new System.Management.Internal.ExecQueryOpSettings ("WQL", strQuery));
			System.Management.Internal.CimInstancePathList list = obj as System.Management.Internal.CimInstancePathList;
			foreach (var item in list) {
				yield return new UnixWbemClassObject(new UnixWbemClass (item));
			}
		}

	}
}
