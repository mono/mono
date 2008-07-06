//
// Mono.Data.Tds.Protocol.TdsConnectionParameters.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Daniel Morgan (danielmorgan@verizon.net)
//
// Copyright (C) 2002 Tim Coleman
// Portions (C) 2003 Daniel Morgan
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

namespace Mono.Data.Tds.Protocol
{
	public class TdsConnectionParameters
	{
		public string ApplicationName;
		public string Database;
		public string Charset;
		public string Hostname;
		public string Language;
		public string LibraryName;
		public string Password;
		public string ProgName;
		public string User;
		public bool DomainLogin;
		public string DefaultDomain;
		public string AttachDBFileName;

		public TdsConnectionParameters ()
		{
			Reset ();
		}

		public void Reset ()
		{
			ApplicationName = "Mono";
			Database = String.Empty;
			Charset = String.Empty;
			Hostname = System.Net.Dns.GetHostName();
			Language = String.Empty;
			LibraryName = "Mono";
			Password = String.Empty;
			ProgName = "Mono";
			User = String.Empty;
			DomainLogin = false; 
			DefaultDomain = String.Empty;
			AttachDBFileName = String.Empty;
		}
	}
}
