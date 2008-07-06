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
// System.DirectoryServices.SearchResultCollection.cs
//
// Author:
//   Sunil Kumar (sunilk@novell.com)
//
// (C)  Novell Inc.
//

using System;
using System.Collections;

namespace System.DirectoryServices
{
	/// <summary>
	/// Contains the SearchResult instances that the Active Directory 
	/// hierarchy returned during a DirectorySearcher query.
	/// </summary>
	/// <remarks>
	/// Each time you perform a query, DirectorySearcher creates a handle to 
	/// the corresponding SearchResultCollection instance. This handle 
	/// persists until you call Dispose or until garbage collection picks up 
	/// the instance.
	/// </remarks>
	public class SearchResultCollection : MarshalByRefObject, ICollection, IEnumerable, IDisposable
	{
		private ArrayList sValues = new ArrayList();

		internal SearchResultCollection()
		{
		}
		
		public int Count
		{
			get{return sValues.Count;}
		}

		bool ICollection.IsSynchronized
		{
			get{return sValues.IsSynchronized;}
		}

		object ICollection.SyncRoot
		{
			get{return sValues.SyncRoot;}
		}
        
		void ICollection.CopyTo(System.Array oArray, int iArrayIndex)
		{
			sValues.CopyTo(oArray, iArrayIndex);
		}

		public void CopyTo(SearchResult[] results, int index)
		{
			((ICollection) this).CopyTo((System.Array)results,index);
		}

		internal void Add(object oValue)
		{
			sValues.Add(oValue);
		}
/*
		public bool IsFixedSize
		{
			get{return m_oKeys.IsFixedSize;}
		}

		public bool IsReadOnly
		{
			get{return m_oKeys.IsReadOnly;}
		}

		public ICollection Keys
		{
			get{return m_oValues.Keys;}
		}
        
		public void Clear()
		{
			m_oValues.Clear();
			m_oKeys.Clear();
		}

		public bool Contains(object oKey)
		{
			return m_oValues.Contains(oKey);
		}

		public bool ContainsKey(object oKey)
		{
			return m_oValues.ContainsKey(oKey);
		}

		public IDictionaryEnumerator GetEnumerator()
		{
			return m_oValues.GetEnumerator();
		}    

		public void Remove(object oKey)
		{
			m_oValues.Remove(oKey);
			m_oKeys.Remove(oKey);
		}
       
		public object this[object oKey]
		{
			get{return m_oValues[oKey];}
			set{m_oValues[oKey] = value;}
		}
*/
		bool Contains(object oValues)
		{
			return sValues.Contains(oValues);
		}

		public bool Contains (SearchResult result)
		{
			return sValues.Contains (result);
		}

		public SearchResult this[int index]
		{
			get{return (SearchResult)sValues[index];}
		}

/*		public ICollection Values
		{
			get{return m_oValues.Values;}
		}
*/
		public int IndexOf (SearchResult result)
		{
			return sValues.IndexOf(result);
		}

		public IEnumerator GetEnumerator()
		{
			return sValues.GetEnumerator();
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		[MonoTODO]
		protected virtual void Dispose (bool disposing)
		{
		}

		public string[] PropertiesLoaded
		{
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public IntPtr Handle
		{
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		~SearchResultCollection ()
		{
			Dispose (false);
		}
	}
}

