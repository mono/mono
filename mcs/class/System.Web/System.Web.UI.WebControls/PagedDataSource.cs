
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
/**
 * Namespace: System.Web.UI.WebControls
 * Class:     PagedDataSource
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.ComponentModel;
using System.Collections;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public sealed class PagedDataSource : ICollection, IEnumerable, ITypedList
	{
		private int  pageSize;
		private bool allowPaging;
		private int  currentPageIndex;
		private bool allowCustomPaging;
		private int  virtualCount;
		
#if NET_2_0
		DataSourceSelectArguments arguments;
		DataSourceView view;
		bool serverPaging;
#endif

		private IEnumerable dataSource;

		public PagedDataSource()
		{
			Initialize();
		}

		private void Initialize()
		{
			pageSize          = 10;
			allowPaging       = false;
			currentPageIndex  = 0;
			allowCustomPaging = false;
			virtualCount      = 0;
		}

#if NET_2_0
		public DataSourceSelectArguments DataSourceSelectArguments {
			get { return arguments; }
			set { arguments = value; }
		}

		public DataSourceView DataSourceView {
			get { return view; }
			set { view = value; }
		}

		public bool AllowServerPaging {
			get { return serverPaging; }
			set { serverPaging = value; }
		}
		
		public bool IsServerPagingEnabled {
			get { return allowPaging && serverPaging; }
		}
		
		public void SetItemCountFromPageIndex (int highestPageIndex)
		{
			arguments.StartRowIndex = CurrentPageIndex * PageSize;
			arguments.MaximumRows = (highestPageIndex - CurrentPageIndex) * PageSize + 1;
			IEnumerable data = view.ExecuteSelect (arguments);
			
			virtualCount = CurrentPageIndex * PageSize;
			if (data is ICollection) {
				virtualCount += ((ICollection)data).Count;
			} else {
				IEnumerator e = data.GetEnumerator ();
				while (e.MoveNext())
					virtualCount++;
			}
		}

#endif

		public bool AllowCustomPaging
		{
			get
			{
				return allowCustomPaging;
			}
			set
			{
				allowCustomPaging = value;
			}
		}

		public bool AllowPaging
		{
			get
			{
				return allowPaging;
			}
			set
			{
				allowPaging = value;
			}
		}

		public int Count
		{
			get
			{
				if(dataSource != null)
				{
					if(!IsPagingEnabled)
					{
						return DataSourceCount;
					}
					if(IsCustomPagingEnabled)
					{
						return pageSize;
					}
					if(IsLastPage)
					{
						return (DataSourceCount - FirstIndexInPage);
					}
					return pageSize;
				}
				return 0;
			}
		}

		public int CurrentPageIndex
		{
			get
			{
				return currentPageIndex;
			}

			set
			{
				currentPageIndex = value;
			}
		}

		public IEnumerable DataSource
		{
			get
			{
				return dataSource;
			}
			set
			{
				dataSource = value;
			}
		}

		public int DataSourceCount
		{
			get
			{
				if(dataSource != null)
				{
#if NET_2_0
					if (serverPaging)
					{
						return virtualCount;
					}
#endif
					if(IsCustomPagingEnabled)
					{
						return virtualCount;
					}
					if(dataSource is ICollection)
					{
						return ((ICollection)dataSource).Count;
					}
					throw new HttpException(HttpRuntime.FormatResourceString("PagedDataSource_Cannot_Get_Count"));
				}
				return 0;
			}
		}

		public int FirstIndexInPage
		{
			get
			{
				if(dataSource != null && IsPagingEnabled && !IsCustomPagingEnabled)
				{
					return (currentPageIndex * pageSize);
				}
				return 0;
			}
		}

		public bool IsCustomPagingEnabled
		{
			get
			{
				return (IsPagingEnabled && allowCustomPaging);
			}
		}

		public bool IsFirstPage
		{
			get
			{
				return (!IsPagingEnabled || (CurrentPageIndex == 0));
			}
		}

		public bool IsLastPage
		{
			get
			{
				return (!IsPagingEnabled || (CurrentPageIndex == (PageCount - 1)));
			}
		}

		public bool IsPagingEnabled
		{
			get
			{
				return (allowPaging && pageSize != 0);
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		public int PageCount
		{
			get
			{
				if(dataSource != null) {
					if(!IsPagingEnabled)
						return 1;
                                        
					int total = DataSourceCount;
					return (total + pageSize - 1)/pageSize;
				}
				return 0;
			}
		}

		public int PageSize
		{
			get
			{
				return pageSize;
			}
			set
			{
				pageSize = value;
			}
		}

		public object SyncRoot
		{
			get
			{
				return this;
			}
		}

		public int VirtualCount
		{
			get
			{
				return virtualCount;
			}
			set
			{
				virtualCount = value;
			}
		}
		
		public void CopyTo(Array array, int index)
		{
			IEnumerator enumerator = this.GetEnumerator();
			if(enumerator == null) return;

			while(enumerator.MoveNext())
				array.SetValue(enumerator.Current, index++);
		}

		public IEnumerator GetEnumerator()
		{
#if NET_2_0
			int fInd = serverPaging ? 0 : FirstIndexInPage;
#else
			int fInd = FirstIndexInPage;
#endif
			int count = -1;

			if(dataSource is ICollection)
			{
				count = Count;
			}

			if(dataSource is IList)
			{
				return (new PrivateListEnumerator((IList)dataSource, fInd, count));
			}
			if(dataSource is Array)
			{
				return (new PrivateArrayEnumerator((object[])dataSource, fInd, count));
			}
			if(dataSource is ICollection)
			{
				return (new PrivateICollectionEnumerator((ICollection)dataSource, fInd, count));
			}
			if(allowCustomPaging)
			{
				return (new PrivateIEnumeratorEnumerator(dataSource.GetEnumerator(), Count));
			}
			return dataSource.GetEnumerator();
		}

		class PrivateIEnumeratorEnumerator : IEnumerator
		{
			private int index;
			private int max;

			private IEnumerator enumerator;

			public PrivateIEnumeratorEnumerator(IEnumerator enumerator, int count)
			{
				this.enumerator = enumerator;
				index = -1;
				max   = count;
			}

			public bool MoveNext()
			{
				enumerator.MoveNext();
				index++;
				return (index < max);
			}

			public void Reset()
			{
				index = -1;
				enumerator.Reset();
			}

			public object Current
			{
				get
				{
					return enumerator.Current;
				}
			}
		}

		class PrivateICollectionEnumerator : IEnumerator
		{
			private int index;
			private int start;
			private int max;

			private ICollection collection;
			private IEnumerator collEnum;

			public PrivateICollectionEnumerator(ICollection collection, int start, int count)
			{
				this.collection = collection;
				this.start  = start;
				index = -1;
				max = start + count;
				if(max > collection.Count)
				{
					max = collection.Count;
				}
			}

			public bool MoveNext()
			{
				if(collEnum == null)
				{
					int cIndex = 0;
					collEnum = collection.GetEnumerator();
					while(cIndex < start)
					{
						collEnum.MoveNext();
						cIndex++;
					}
				}
				collEnum.MoveNext();
				index++;
				return (start + index < max);
			}

			public void Reset()
			{
				index = -1;
				collEnum = null;
			}

			public object Current
			{
				get
				{
					return collEnum.Current;
				}
			}
		}

		class PrivateArrayEnumerator : IEnumerator
		{
			private int index;
			private int start;
			private int max;
			private object[] values;

			public PrivateArrayEnumerator(object[] values, int start, int count)
			{
				this.values = values;
				this.start  = start;
				index = -1;
				max = start + count;
				if(max > this.values.Length)
				{
					max = this.values.Length;
				}
			}

			public bool MoveNext()
			{
				index++;
				return (index + start < max);
			}

			public void Reset()
			{
				index = -1;
			}

			public object Current
			{
				get
				{
					if(index >= 0)
					{
						return values[index + start];
					}
					throw new InvalidOperationException("Enumerator_MoveNext_Not_Called");
				}
			}
		}

		class PrivateListEnumerator : IEnumerator
		{
			private int   index;
			private int   start;
			private int   max;
			private IList collection;

			public PrivateListEnumerator(IList list, int start, int count)
			{
				collection = list;
				this.start = start;
				index = -1;
				max = start + count;
				if(max > list.Count)
				{
					max = list.Count;
				}
			}

			public bool MoveNext()
			{
				index++;
				return (index + start < max);
			}

			public void Reset()
			{
				index = -1;
			}

			public object Current
			{
				get
				{
					if(index >= 0)
					{
						return collection[index + start];
					}
					throw new InvalidOperationException("Enumerator_MoveNext_Not_Called");
				}
			}
		}

		public string GetListName(PropertyDescriptor[] listAccessors)
		{
			return String.Empty;
		}

		public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
		{
			if(dataSource != null)
			{
				if(dataSource is ITypedList)
				{
					return ((ITypedList)dataSource).GetItemProperties(listAccessors);
				}
			}
			return null;
		}
	}
}
