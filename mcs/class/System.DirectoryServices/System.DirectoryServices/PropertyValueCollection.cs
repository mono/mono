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
// System.DirectoryServices.PropertyValueCollection .cs
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
	public class PropertyValueCollection : CollectionBase
	{

		private bool _Mbit;
		private DirectoryEntry _parent;

		internal PropertyValueCollection(DirectoryEntry parent):base()
		{
			_Mbit = false;
			_parent = parent;
		}

		internal bool Mbit
		{
			get
			{
				return _Mbit;
			}
			set
			{
				_Mbit = value;
			}
		}

		public object  this[ int index ]  
		{
			get  
			{
				return( (object) List[index] );
			}
			set  
			{
				List[index] = value;
				_Mbit = true;
			}
		}

		public int Add( object value )  
		{
			if(Contains(value))
			{
				return -1;
			}
			else
			{
				_Mbit=true;
				return( List.Add( value ) );
			}

		}

		public void AddRange(object[] values)
		{
			foreach (object value in values)
				Add (value);
		}

		public void AddRange (PropertyValueCollection coll)
		{
			foreach (object value in coll)
				Add (value);
		}

		public int IndexOf( object value )  
		{
			return( List.IndexOf( value ) );
		}

		public void Insert( int index, object value )  
		{
			List.Insert( index, value );
			_Mbit = true;
		}

		public void Remove( object value )  
		{
			List.Remove( value );
			_Mbit = true;
		}

		public bool Contains( object value )  
		{
			return( List.Contains( value ) );
		}

		internal bool ContainsCaselessStringValue( string value )  
		{
			for(int i=0; i< this.Count; ++i)
			{
				string lVal = (string) List[i];
				if(String.Compare(value,lVal,true)==0)
				{
					return true;
				}
			}
			return false;
		}

		public void CopyTo (object[] copy_to, int index)
		{
			foreach (object o in List)
				copy_to[index++] = o;
		}

		[MonoTODO]
		protected override void OnClearComplete ()
		{
			if (_parent != null) {
				_parent.CommitDeferred();
			}
		}

		[MonoTODO]
		protected override void OnInsertComplete (int index, object value)
		{
			if (_parent != null) {
				_parent.CommitDeferred();
			}
		}

		[MonoTODO]
		protected override void OnRemoveComplete (int index, object value)
		{
			if (_parent != null) {
				_parent.CommitDeferred();
			}
		}

		[MonoTODO]
		protected override void OnSetComplete (int index, object oldValue, object newValue)
		{
			if (_parent != null) {
				_parent.CommitDeferred();
			}
		}

		[MonoTODO]
		public string PropertyName
		{
			get
			{
				return string.Empty;
			}
		}

		public object Value 
		{
			get
			{
				switch (Count) {
					case 0 : 
						return null;
					case 1 :
						return (object) List[0];
					default :
//					System.Object[] oArray= new System.Object[this.Count];
//					object[] oArray= new object[this.Count];
//					Array.Copy((System.Array)List,0,(System.Array)oArray,0,this.Count);
						Array LArray = new object[Count];
						for ( int i = LArray.GetLowerBound(0); i <= LArray.GetUpperBound(0); i++ )
							LArray.SetValue( List[i], i );
						return LArray;
				}
			}
			set
			{
				if (value == null && List.Count == 0)
					return;

				List.Clear();
				if (value != null) {
					Add(value);
				}
				_Mbit = true;
			}
		}

	}
}
