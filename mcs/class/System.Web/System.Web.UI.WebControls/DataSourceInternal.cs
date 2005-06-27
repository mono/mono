
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
 * Namespace:   System.Web.UI.WebControls
 * Class:       DataSourceInternal
 *
 * Author:      Gaurav Vaish
 * Maintainer:  gvaish_mono@lycos.com
 * Contact:     gvaish_mono@lycos.com
 * Implementation: Yes
 * Status:      100%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Collections;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	internal class DataSourceInternal : ICollection, IEnumerable
	{
		private int itemCount;

		public DataSourceInternal(int itemCount)
		{
			this.itemCount = itemCount;
		}

		public int Count
		{
			get
			{
				return itemCount;
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

		public object SyncRoot
		{
			get
			{
				return this;
			}
		}

		public void CopyTo(Array array, int index)
		{
			IEnumerator e = GetEnumerator();
			while(e.MoveNext())
			{
				array.SetValue(e.Current, index);
				index++;
			}
		}
		
		public IEnumerator GetEnumerator()
		{
			return new DataSourceEnumeratorInternal(itemCount);
		}
		
		private class DataSourceEnumeratorInternal : IEnumerator
		{
			private int count;
			private int index;
			
			public DataSourceEnumeratorInternal(int count)
			{
				this.count = count;
				this.index = -1;
			}
			
			public bool MoveNext()
			{
				index++;
				return (index < count);
			}
			
			public object Current
			{
				get
				{
					return null;
				}
			}
			
			public void Reset()
			{
				this.index = -1;
			}
		}
	}
}
