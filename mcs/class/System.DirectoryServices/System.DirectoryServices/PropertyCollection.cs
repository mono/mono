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
// System.DirectoryServices.PropertyCollection.cs
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
	public class PropertyCollection : IDictionary, ICollection,IEnumerable
	{
		protected ArrayList m_oKeys = new ArrayList();
		protected Hashtable m_oValues = new Hashtable();

		internal PropertyCollection()
		{
		}
		public int Count
		{
			get{return m_oValues.Count;}
		}

		bool ICollection.IsSynchronized
		{
			get{return m_oValues.IsSynchronized;}
		}

		object ICollection.SyncRoot
		{
			get{return m_oValues.SyncRoot;}
		}
        
		void CopyTo(System.Array oArray, int iArrayIndex)
		{
			m_oValues.CopyTo(oArray, iArrayIndex);
		}

		void ICollection.CopyTo(System.Array oArray, int iArrayIndex)
		{
			CopyTo(oArray,iArrayIndex);
		}

		public void CopyTo(PropertyValueCollection[] array, int index)
		{
			CopyTo(array,index);
		}

		void Add(object oKey, object oValue)
		{
			m_oKeys.Add(oKey);
			m_oValues.Add(oKey, oValue);
		}

		void IDictionary.Add(object oKey, object oValue){
			Add(oKey,oValue);
		}
		
		bool IDictionary.IsFixedSize
		{
			get{return m_oKeys.IsFixedSize;}
		}

		bool IDictionary.IsReadOnly
		{
			get{return m_oKeys.IsReadOnly;}
		}

		ICollection IDictionary.Keys
		{
			get{return m_oValues.Keys;}
		}
       
		public ICollection PropertyNames
		{
			get{return m_oValues.Keys;}
		}
 
		void IDictionary.Clear()
		{
			m_oValues.Clear();
			m_oKeys.Clear();
		}

		bool IDictionary.Contains(object oKey)
		{
			return m_oValues.Contains(oKey);
		}

		public bool Contains (string propertyName)
		{
			return m_oValues.Contains(propertyName);
		}

		public bool ContainsKey(object oKey)
		{
			return m_oValues.ContainsKey(oKey);
		}

		public IDictionaryEnumerator GetEnumerator()
		{
			return m_oValues.GetEnumerator();
		}    

		void IDictionary.Remove(object oKey)
		{
			m_oValues.Remove(oKey);
			m_oKeys.Remove(oKey);
		}
        
		object IDictionary.this[object oKey]
		{
			get{return m_oValues[oKey];}
			set{m_oValues[oKey] = value;}
		}

		public ICollection Values
		{
			get{return m_oValues.Values;}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_oValues.GetEnumerator();
		}

		public  PropertyValueCollection this[string propertyName] 
		{
			get 
			{
				if(Contains(propertyName))
				{
					return (PropertyValueCollection)m_oValues[propertyName];
				}
				else
				{
					PropertyValueCollection _pValColl=new PropertyValueCollection();
					Add((string)propertyName, (PropertyValueCollection)_pValColl);
					return _pValColl;					
				}
//				throw new InvalidOperationException();
			}
		}

	}
}

