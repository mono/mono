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

		internal PropertyValueCollection():base()
		{
			_Mbit = false;
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

		public void AddRange(object[] value)
		{
			int i=0;
			while(true)
			{
				try
				{
					if(Add(value[i])!= -1)
					{
						++i;
					}
				}
				catch(IndexOutOfRangeException ex)
				{
					break;
				}
				catch(Exception ex)
				{
					throw ex;
				}
			}
			return;
		}

		public int IndexOf( object value )  
		{
			return( List.IndexOf( value ) );
		}

		public void Insert( int index, object value )  
		{
			List.Insert( index, value );
		}

		public void Remove( object value )  
		{
			List.Remove( value );
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

		protected override void OnInsert( int index, Object value )  
		{
			// Insert additional code to be run only when inserting values.
		}

		protected override void OnRemove( int index, Object value )  
		{
			// Insert additional code to be run only when removing values.
		}

		protected override void OnSet( int index, Object oldValue, Object newValue )  
		{
			// Insert additional code to be run only when setting values.
		}

		protected override void OnValidate( Object value )  
		{
//			if ( value.GetType() != Type.GetType("System.Object") )
//				throw new ArgumentException( "value must be of type Object.", "value" );
		}

		public object Value 
		{
			get
			{
				if(this.Count==1)
				{
					return (object) List[0];
				}
				else
				{
//					System.Object[] oArray= new System.Object[this.Count];
//					object[] oArray= new object[this.Count];
//					Array.Copy((System.Array)List,0,(System.Array)oArray,0,this.Count);
					Array LArray = Array.CreateInstance( Type.GetType("System.Object"), this.Count );
					for ( int i = LArray.GetLowerBound(0); i <= LArray.GetUpperBound(0); i++ )
						LArray.SetValue( List[i], i );
					return LArray;
				}
			}
			set
			{
				List.Clear();
				Add(value);
//				List[0] =  (object)value;
//				_Mbit=true;
			}
		}

	}
}

