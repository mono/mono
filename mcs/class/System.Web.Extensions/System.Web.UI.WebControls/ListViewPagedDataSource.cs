//
// System.Web.UI.WebControls.ListView
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007-2008 Novell, Inc
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
#if NET_3_5
using System;
using System.Collections;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	public class ListViewPagedDataSource : ICollection, IEnumerable, ITypedList
	{
		sealed class ListEnumerator : IEnumerator
		{
			int index;
			int startIndex;
			int end;
			IList list;
			
			public object Current {
				get { return list [startIndex + index]; }
			}
			
			public ListEnumerator (IList list, int startIndex, int end)
			{
				this.list = list;
				this.index = -1;
				this.startIndex = startIndex;
				this.end = startIndex + end;
			}
			
			public bool MoveNext ()
			{
				index++;
				return (startIndex + index) < end;
			}

			// See:
			// http://msdn.microsoft.com/en-us/library/system.web.ui.webcontrols.listviewpageddatasource.getenumerator.aspx
			// (Note 1)
			public void Reset ()
			{
				index = -1;
			}
		}

		sealed class CollectionEnumerator : IEnumerator
		{
			int index;
			int startIndex;
			int end;
			ICollection collection;
			IEnumerator enumerator;
			
			public object Current {
				get {
					if (enumerator != null)
						return enumerator.Current;

					return null;
				}
			}
			
			public CollectionEnumerator (ICollection collection, int startIndex, int end)
			{
				this.collection = collection;
				this.index = -1;
				this.startIndex = startIndex;
				this.end = end;
			}
			
			public bool MoveNext ()
			{
				if (enumerator == null) {
					enumerator = collection.GetEnumerator ();
					for (int i = 0; i < startIndex; i++)
						enumerator.MoveNext ();
				}
				
				index++;
				enumerator.MoveNext ();
				return (startIndex + index) < end;
			}

			// See:
			// http://msdn.microsoft.com/en-us/library/system.web.ui.webcontrols.listviewpageddatasource.getenumerator.aspx
			// (Note 1)
			public void Reset ()
			{
				index = -1;
				enumerator = null;
			}
		}
		
		public ListViewPagedDataSource ()
		{
			StartRowIndex = 0;
			MaximumRows = 0;
			AllowServerPaging = false;
			TotalRowCount = 0;
		}
		
		public void CopyTo (Array array, int index)
		{
		}
		
		public IEnumerator GetEnumerator ()
		{
			IEnumerable ds = DataSource;

			if (ds == null)
				return null;

			IList list = ds as IList;
			if (list != null)
				return new ListEnumerator (list, AllowServerPaging ? 0 : StartRowIndex, Count);

			ICollection collection = ds as ICollection;
			if (collection != null)
				return new CollectionEnumerator (collection, AllowServerPaging ? 0 : StartRowIndex, Count);
			
			return ds.GetEnumerator ();
		}
		
		public PropertyDescriptorCollection GetItemProperties (PropertyDescriptor [] listAccessors)
		{
			IEnumerable ds = DataSource;

			if (ds == null || !(ds is ITypedList))
				return null;

			return ((ITypedList) ds).GetItemProperties (listAccessors);
		}
		
		public string GetListName (PropertyDescriptor [] listAccessors)
		{
			return String.Empty;
		}
		
		public bool AllowServerPaging {
			get;
			set;
		}
		
		public int Count {
			get {
				IEnumerable ds = DataSource;
				if (ds == null)
					return 0;

				bool onLastPage = OnLastPage;
				int maxRows = MaximumRows;
				if (!onLastPage && maxRows >= 0)
					return maxRows;

				// LAMESPEC: MSDN says that DataSourceCount should be subtracted
				// from StartRowIndex
				return DataSourceCount - StartRowIndex;
			}
		}
		
		public IEnumerable DataSource {
			get;
			set;
		}
		
		public int DataSourceCount {
			get {
				IEnumerable ds = DataSource;
				if (ds == null)
					return 0;

				if (!(ds is ICollection))
					throw new InvalidOperationException ("The data source object does not implement the System.Collections..::.ICollection interface.");

				if (IsServerPagingEnabled)
					return TotalRowCount;				

				return ((ICollection) ds).Count;
			}
		}
		
		public bool IsReadOnly {
			get { return false; }
		}
		
		public bool IsServerPagingEnabled {
			get { return AllowServerPaging; }
		}
		
		public bool IsSynchronized {
			get { return false; }
		}
		
		public int MaximumRows {
			get;
			set;
		}
		
		public int StartRowIndex {
			get;
			set;
		}
		
		public object SyncRoot {
			get { return this; }
		}
		
		public int TotalRowCount {
			get;
			set;
		}

		bool OnLastPage {
			get { return ((StartRowIndex + MaximumRows) >= DataSourceCount); }
		}
		
	}
}
#endif
