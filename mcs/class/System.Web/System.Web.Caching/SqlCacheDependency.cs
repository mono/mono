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

using System;
using System.Collections.Generic;
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
			if (sqlCmd == null)
				throw new ArgumentNullException ("sqlCmd");
		}

		[MonoTODO ("What are the params good for?")]
		public SqlCacheDependency (string databaseEntryName, string tableName)
		{
			if (databaseEntryName == null)
				throw new ArgumentNullException ("databaseEntryName");

			if (tableName == null)
				throw new ArgumentNullException ("tableName");
		}

		[MonoTODO ("Needs more testing - especially the return value and database+table lookup.")]
		public static CacheDependency CreateOutputCacheDependency (string dependency)
		{
			if (dependency == null)
				throw new HttpException (InvalidDependencyFormatMessage (dependency));

			if (dependency.Length == 0)
				throw new ArgumentException (InvalidDependencyFormatMessage (dependency), "dependency");

			int colon;
			string[] pairs = dependency.Split (';');
			var dependencies = new List <SqlCacheDependency> ();

			foreach (string pair in pairs) {
				colon = pair.IndexOf (':');
				if (colon == -1)
					throw new ArgumentException (InvalidDependencyFormatMessage (dependency), "dependency");

				dependencies.Add (new SqlCacheDependency (pair.Substring (0, colon), pair.Substring (colon + 1)));
			}

			switch (dependencies.Count) {
				case 0:
					return null;

				case 1:
					return dependencies [0];

				default:
					var acd = new AggregateCacheDependency ();
					acd.Add (dependencies.ToArray ());
					return acd;
			}
		}

		static string InvalidDependencyFormatMessage (string dependency)
		{
			return String.Format (@"The '' SqlDependency attribute for OutputCache directive is invalid.

For SQL Server 7.0 and SQL Server 2000, the valid format is ""database:tablename"", and table name must conform to the format of regular identifiers in SQL. To specify multiple pairs of values, use the ';' separator between pairs. (To specify ':', '\' or ';', prefix it with the '\' escape character.)

For dependencies that use SQL Server 9.0 notifications, specify the value 'CommandNotification'.", dependency);
		}
		
		protected override void DependencyDispose ()
		{
			// MSDN doesn't document it as being part of the class, but assembly
			// comparison shows that it does exist in this type, so we're just calling
			// the base class here
			base.DependencyDispose ();
		}
		
		public override string GetUniqueID ()
		{
			return uniqueId;
		}
	}
}
