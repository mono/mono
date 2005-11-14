//
// System.Web.Configuration.SqlCacheDependencyDatabaseCollection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Configuration;

#if NET_2_0

namespace System.Web.Configuration {

	[ConfigurationCollection (typeof (SqlCacheDependencyDatabase), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
	public sealed class SqlCacheDependencyDatabaseCollection : ConfigurationElementCollection
	{
		public void Add (SqlCacheDependencyDatabase name)
		{
			BaseAdd (name);
		}

		public void Clear ()
		{
			BaseClear ();
		}

		public SqlCacheDependencyDatabase Get (string name)
		{
			return (SqlCacheDependencyDatabase) BaseGet (name);
		}

		public SqlCacheDependencyDatabase Get (int index)
		{
			return (SqlCacheDependencyDatabase) BaseGet (index);
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new SqlCacheDependencyDatabase ();
		}

		[MonoTODO]
		protected override object GetElementKey (ConfigurationElement element)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetKey (int index)
		{
			throw new NotImplementedException ();
		}

		public void Remove (string name)
		{
			BaseRemove (name);
		}

		public void RemoveAt (int index)
		{
			BaseRemoveAt (index);
		}

		[MonoTODO]
		public void Set (SqlCacheDependencyDatabase user)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public String[] AllKeys {
			get { throw new NotImplementedException (); }
		}

		public SqlCacheDependencyDatabase this [int index] {
			get { return (SqlCacheDependencyDatabase) BaseGet (index); }
			set { if (BaseGet (index) != null) BaseRemoveAt (index); BaseAdd (index, value); }
		}

		public new SqlCacheDependencyDatabase this [string name] {
			get { return (SqlCacheDependencyDatabase) BaseGet (name); }
		}

	}

}

#endif

