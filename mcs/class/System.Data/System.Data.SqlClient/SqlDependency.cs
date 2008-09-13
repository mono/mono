//
// System.Data.SqlClient.SqlDependency.cs
//
// Authors:
//   Veerapuram Varadhan (vvaradhan@novell.com)
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System;
using System.Security.Permissions;
using System.Data;
using System.Data.SqlClient;

namespace System.Data.SqlClient
{
	public sealed class SqlDependency
	{
		string uniqueId =  Guid.NewGuid().ToString();

			[MonoTODO]
		public SqlDependency()
		{
			
		}
		[MonoTODO]
		public SqlDependency (SqlCommand command)
		{
			
		}
		
		[MonoTODO]
		public SqlDependency (SqlCommand command, string options, int timeout)
		{
			
		}
		
		public string Id {
			get { return uniqueId; }
		}
		
		[MonoTODO]
		public bool HasChanges {
			get { return true; }
		}
		
		public event OnChangeEventHandler OnChange;

		[MonoTODO]
		public void AddCommandDependency(SqlCommand command)
		{
			
		}
		
		[MonoTODO]
		[HostProtectionAttribute(SecurityAction.LinkDemand, ExternalThreading = true)]
		public static bool Start(string connectionString)
		{
			return true;
		}

		[MonoTODO]
		[HostProtectionAttribute(SecurityAction.LinkDemand, ExternalThreading = true)]
		public static bool Start(string connectionString, string queue)
		{
			return true;
		}

		[MonoTODO]
		[HostProtectionAttribute(SecurityAction.LinkDemand, ExternalThreading = true)]
		public static bool Stop(string connectionString)
		{
			return true;
		}

		[MonoTODO]
		[HostProtectionAttribute(SecurityAction.LinkDemand, ExternalThreading = true)]
		public static bool Stop(string connectionString, string queue)
		{
			return true;
		}
		
	}
}
#endif 