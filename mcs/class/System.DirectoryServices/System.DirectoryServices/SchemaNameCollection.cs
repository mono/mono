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
		internal SchemaNameCollection ()
		{
		}
		
		[MonoTODO]
		int IList.Add(object avalue)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public int Add (string value)
		{
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public void Clear()
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		bool IList.Contains(object cvalue)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		int IList.IndexOf(object ivalue)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		void IList.Insert(int index,object ivalue)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		void IList.Remove(object rvalue)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public void RemoveAt(int index)
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
			[MonoTODO]
			get 
			{
				throw new InvalidOperationException();
			}
			[MonoTODO]
			set
			{
				throw new InvalidOperationException();
			}
		}

		public string this[int index]
		{
			[MonoTODO]
			get { throw new NotImplementedException (); }
			[MonoTODO]
			set { throw new NotImplementedException (); }

		}

		public int Count
		{
			get
			{
				return 0;
			}
		}
		bool ICollection.IsSynchronized 
		{
			[MonoTODO]
			get
			{
				return true;
			}
		}

		object ICollection.SyncRoot { 
			[MonoTODO]
			get {
				// FIXME:
				return this;
			}
		}

		[MonoTODO]
		public IEnumerator GetEnumerator() 
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void AddRange (SchemaNameCollection value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void AddRange (string[] value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public bool Contains (string value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		void ICollection.CopyTo (Array arr, int pos)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo (string[] stringArray, int index)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public int IndexOf (string value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void Insert (int index, string value)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void Remove (string value)
		{
			throw new NotImplementedException();
		}
	}
}

