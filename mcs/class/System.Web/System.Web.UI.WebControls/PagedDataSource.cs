// System.Web.UI.WebControls.PagedDataSource.cs
//
// Author: Duncan Mak (duncan@novell.com)
//	   Jackson Harper  (jackson@ximian.com)	
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.ComponentModel;

namespace System.Web.UI.WebControls {
	public sealed class PagedDataSource : ICollection, IEnumerable, ITypedList
	{
		int page_size, current_page_index, virtual_count;
		bool allow_paging, allow_custom_paging;
		IEnumerable source;
		
		public PagedDataSource ()
		{
			page_size = 10;
			allow_paging = false;
			current_page_index = 0;
			allow_custom_paging = false;
			virtual_count = 0;
		}

		public bool AllowCustomPaging {
			get { return allow_custom_paging; }
			set { allow_custom_paging = value; }
		}

		public bool AllowPaging {
			get { return allow_paging; }
			set { allow_paging = value; }
		}

		public int Count {
			get {
				if (DataSource == null)
					return 0;
				
				if (IsPagingEnabled == true) {
					if (IsCustomPagingEnabled == true || IsLastPage == false)
						return PageSize;

					if (IsCustomPagingEnabled == false && IsLastPage == true)
						return FirstIndexInPage - DataSourceCount;
				}

				return DataSourceCount;
			}
		}						

		public int CurrentPageIndex {
			get { return current_page_index; }
			set { current_page_index = value; }
		}

		public IEnumerable DataSource {
			get { return source; }
			set { source = value; }
		}

		public int DataSourceCount {
			get {
				if (source == null)
					return 0;
				
				if (IsCustomPagingEnabled)
					return virtual_count;

				if (source is ICollection)
					return ((ICollection) source).Count;

				throw new HttpException ("The data source must implement ICollection");
			}
		}

		public int FirstIndexInPage {
			get {
				if (!IsPagingEnabled || IsCustomPagingEnabled || DataSource == null)
					return 0;
				
				return current_page_index * page_size;
			}
		}

		public bool IsCustomPagingEnabled {
			get { return IsPagingEnabled && AllowCustomPaging; }
		}

		public bool IsFirstPage {
			get { 
				if (!AllowPaging)
					return true;
				
				return CurrentPageIndex == 0; 
			}
		}

		public bool IsLastPage {
			get {
				if (!AllowPaging)
					return true;

				if (PageCount == 0)
					return false;

				return CurrentPageIndex == PageCount - 1;
			}
		}

		public bool IsPagingEnabled {
			get { return AllowPaging && (PageSize != 0); }
		}

		public bool IsReadOnly {
			get { return false; } // as documented
		}

		public bool IsSynchronized {
			get { return false; } // as documented
		}

		public int PageCount {
			get {
				if (DataSource == null)
					return 0;

				if (!IsPagingEnabled || DataSourceCount == 0 || PageSize == 0)
					return 1;

				return (DataSourceCount + PageSize - 1) / PageSize;
			}
		}
		
		public int PageSize {
			get { return page_size; }
			set { page_size = value; }
		}

		public object SyncRoot {
			get { return this; }
		}

		public int VirtualCount {
			get { return virtual_count; }
			set { virtual_count = value; }
		}
#if NET_2_0
		[MonoTODO]
		public bool AllowServerPaging {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public DataSourceSelectArguments DataSourceSelectArguments {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public DataSourceView DataSourceView {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public void SetItemCountFromPageIndex (int highestPageIndex)
		{
			throw new NotImplementedException ();
		}
#endif

		public void CopyTo (Array array, int index)
		{
			foreach (object o in source)
				array.SetValue (o, index++);
		}

		public IEnumerator GetEnumerator ()
		{
			ICollection col = source as ICollection;
			if (col != null)
				return GetEnumeratorEnum (col.GetEnumerator (),
						FirstIndexInPage, FirstIndexInPage + PageSize);

			IList list = source as IList;
			if (list != null)
				return GetListEnum (list, FirstIndexInPage,
						FirstIndexInPage + PageSize);

			return source.GetEnumerator ();
		}

		public PropertyDescriptorCollection GetItemProperties (
			PropertyDescriptor [] list_accessors)
		{
			ITypedList typed = DataSource as ITypedList;
			if (typed == null)
				return null;
			return typed.GetItemProperties (list_accessors);
		}

		public string GetListName (PropertyDescriptor [] list_accessors)
		{
			return String.Empty; // as documented
		}

		private IEnumerator GetListEnum (IList list, int start, int end)
		{
			if (!AllowPaging)
				end = list.Count;
			for (int i = start; i < end; i++)
				yield return list [i];
		}

		private IEnumerator GetEnumeratorEnum (IEnumerator e, int start, int end)
		{
			for (int i = 0; i < start; i++)
				e.MoveNext ();
			for (int i = start; (!allow_paging || i < end) && e.MoveNext (); i++)
				yield return e.Current;
		}
	}
}
