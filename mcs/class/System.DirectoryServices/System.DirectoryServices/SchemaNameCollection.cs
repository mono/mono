/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.,  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

//
// System.DirectoryServices.SchemaNameCollection.cs
//
// Author:
//   Sunil Kumar (sunilk@novell.com)
//
// (C)  Novell Inc.
//

using System.Collections;

namespace System.DirectoryServices
{
	
	/// <summary>
	///Contains a list of the schema names that the
	/// SchemaFilter property of a DirectoryEntries
	///  object can use.
	/// </summary>
	public class SchemaNameCollection : IList, ICollection, IEnumerable
	{
		private SchemaNameCollection ()
		{
		}
		
		int IList.Add(object avalue)
		{
			throw new NotImplementedException();
		}

		int Add (string value)
		{
			throw new NotImplementedException();
		}
		
		void IList.Clear()
		{
				throw new NotImplementedException();
		}
		bool IList.Contains(object cvalue)
		{
				throw new NotImplementedException();
		}
		int IList.IndexOf(object ivalue)
		{
				throw new NotImplementedException();
		}
		void IList.Insert(int index,object ivalue)
		{
				throw new NotImplementedException();
		}
		void IList.Remove(object rvalue)
		{
				throw new NotImplementedException();
		}
		void IList.RemoveAt(int index)
		{
					throw new NotImplementedException();
		}
		
	    bool IList.IsFixedSize 
		{
			get
			{
				return true;
			}
		}

		bool IList.IsReadOnly 
		{
			get
			{
				return true;
			}
		}
		object IList.this[int recordIndex] 
		{
			get 
			{
				throw new InvalidOperationException();
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public void CopyTo(Array array,int index)
		{
				throw new NotImplementedException();
		}

		public int Count 
		{
			get
			{
				return 0;
			}
		}
		public bool IsSynchronized 
		{
			get
			{
				return true;
			}
		}

		object ICollection.SyncRoot { 
			get {
				// FIXME:
				return this;
			}
		}

		public IEnumerator GetEnumerator() 
		{
				throw new NotImplementedException();
		}

	
	}

}

