//
// System.ComponentModel.Design.Data.DesignerDataConnection
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2007 Novell, Inc.
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

#if NET_2_0

using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Security.Permissions;

namespace System.ComponentModel.Design.Data
{
	public sealed class DesignerDataConnection
	{
		string name, provider_name, connection_string;
		bool is_configured;

		[MonoTODO]
		public DesignerDataConnection (string name, string providerName, string connectionString)
			: this (name, providerName, connectionString, false)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DesignerDataConnection (string name, string providerName, string connectionString, bool isConfigured)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string Name {
			get { return name; }
		}

		[MonoTODO]
		public string ProviderName{
			get { return provider_name; }
		}

		[MonoTODO]
		public string ConnectionString {
			get { return connection_string; }
		}

		[MonoTODO]
		public bool IsConfigured {
			get { return is_configured; }
		}
	}
}

#endif
