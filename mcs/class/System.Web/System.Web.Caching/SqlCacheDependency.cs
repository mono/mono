//
// System.Web.UI.SqlCacheDependency
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
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
using System.ComponentModel;
using System.Security.Permissions;
using System.Data.SqlClient;
using System.Web;

namespace System.Web.Caching
{
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class SqlCacheDependency: CacheDependency
	{
		string uniqueId = Guid.NewGuid().ToString();

		[MonoTODO ("What to do with the sqlCmd?")]
		public SqlCacheDependency (SqlCommand sqlCmd)
		{
		}

		[MonoTODO ("What are the params good for?")]
		public SqlCacheDependency (string databaseEntryName, string tableName)
		{
		}
		
		public override string GetUniqueID ()
		{
			return uniqueId;
		}
	}
}
#endif