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
using System.Globalization;

namespace System.DirectoryServices
{
	public class PropertyCollection : IDictionary, ICollection,IEnumerable
	{
		private ArrayList m_oKeys = new ArrayList();
		private Hashtable m_oValues = new Hashtable();
		private DirectoryEntry _parent;

		internal PropertyCollection(): this(null)
		{
		}

		internal PropertyCollection(DirectoryEntry parent)
		{
			_parent=parent;
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
        
		void ICopyTo(System.Array oArray, int iArrayIndex)
		{
			m_oValues.CopyTo(oArray, iArrayIndex);
		}

		void ICollection.CopyTo(System.Array oArray, int iArrayIndex)
		{
			ICopyTo(oArray,iArrayIndex);
		}

		public void CopyTo(PropertyValueCollection[] array, int index)
		{
			ICopyTo(array,index);
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

		bool IContains(object oKey)
		{
			return m_oValues.Contains(oKey);
		}
		bool IDictionary.Contains(object oKey)
		{
			return IContains(oKey);
		}

		public bool Contains (string propertyName)
		{
//			String tstr=new String(propertyName.ToCharArray());
//			return IContains(tstr.ToLower());
			return IContains(propertyName.ToLower());
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
//					String tstr=new String(propertyName.ToCharArray());
//					return (PropertyValueCollection)m_oValues[tstr.ToLower()];
					return (PropertyValueCollection)m_oValues[propertyName.ToLower()];
				}
				else
				{
					PropertyValueCollection _pValColl=new PropertyValueCollection(_parent);
//					String tstr=new String(propertyName.ToCharArray());
//					Add((string)tstr.ToLower(), (PropertyValueCollection)_pValColl);
					Add((string)propertyName.ToLower(), (PropertyValueCollection)_pValColl);
					return _pValColl;					
				}
//				throw new InvalidOperationException();
			}
		}

	}
}

