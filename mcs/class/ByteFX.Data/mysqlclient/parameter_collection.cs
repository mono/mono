// ByteFX.Data data access components for .Net
// Copyright (C) 2002-2003  ByteFX, Inc.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Data;
using System.Collections;

namespace ByteFX.Data.MySQLClient
{
	public sealed class MySQLParameterCollection : MarshalByRefObject, IDataParameterCollection, 
		IList, ICollection, IEnumerable
	{
		private ArrayList	_parms = new ArrayList();

		#region ICollection support
		public int Count 
		{
			get { return _parms.Count; }
		}

		public void CopyTo( Array array, int index ) 
		{
/*			if (null == array) throw new ArgumentNullException("array", "Array must not be null");
			if (index < 0) throw new ArgumentOutOfRangeException("index", index, "Index must be greater than or equal to zero");
			if (array.Rank > 1) throw new ArgumentException("Array must not be multidimensional", "Array");
			if (index >= array.Length) throw new ArgumentException("Index must start within the bounds of the array", "Index");
			if ((index + Count) > array.Length) throw new ArgumentException("Not enough room to copy parameters", "Array");
*/
			_parms.CopyTo(array, index);
		}

		public bool IsSynchronized
		{
			get { return _parms.IsSynchronized; }
		}

		public object SyncRoot
		{
			get { return _parms.SyncRoot; }
		}
		#endregion

		#region IList
		public void Clear()
		{
			_parms.Clear();
		}

		public bool Contains(object value)
		{
			return _parms.Contains(value);
		}

		public int IndexOf(object value)
		{
			return _parms.IndexOf(value);
		}

		public void Insert(int index, object value)
		{
			_parms.Insert( index, value );
		}

		public bool IsFixedSize
		{
			get { return _parms.IsFixedSize; }
		}

		public bool IsReadOnly
		{
			get { return _parms.IsReadOnly; }
		}

		public void Remove( object value )
		{
			_parms.Remove( value );
		}

		public void RemoveAt( int index )
		{
			_parms.RemoveAt( index );
		}

		object IList.this[int index] 
		{
			get { return this[index]; }
			set 
			{ 
				if (! (value is MySQLParameter)) throw new MySQLException("Only MySQLParameter objects may be stored");
				this[index] = (MySQLParameter)value; 
			}
		}

		public int Add( object value )
		{
			if (! (value is MySQLParameter)) throw new MySQLException("Only MySQLParameter objects may be stored");


			if ( ((MySQLParameter)value).ParameterName == null ) 
				throw new ArgumentException("parameter must be named");

			return _parms.Add(value);
		}

		#endregion

		#region IDataParameterCollection
		public bool Contains(string name)
		{
			foreach (MySQLParameter p in _parms)
			{
				if (p.ParameterName.ToLower().Equals( name.ToLower() )) return true;
			}
			return false;
		}

		public int IndexOf( string name )
		{
			for (int x=0; x < _parms.Count; x++) 
			{
				MySQLParameter p = (MySQLParameter)_parms[x];
				if (p.ParameterName.ToLower().Equals( name.ToLower() )) return x;
			}
			throw new MySQLException("Parameter '" + name + "' not found in collection");
		}

		public void RemoveAt( string name )
		{
			int index = IndexOf( name );
			_parms.RemoveAt(index);
		}

		object IDataParameterCollection.this[string name]
		{
			get { return this[name]; }
			set 
			{ 
				if (! (value is MySQLParameter)) throw new MySQLException("Only MySQLParameter objects may be stored");
				this[name] = (MySQLParameter)value;
			}
		}
		#endregion

		#region IEnumerable
		public IEnumerator GetEnumerator()
		{
			return ((IEnumerable)_parms).GetEnumerator();
		}
		#endregion

		#region Public Methods
		public MySQLParameter this[int index]
		{
			get { return (MySQLParameter)_parms[index]; }
			set 
			{ 
				_parms[index] = value;
			}
		}

		public MySQLParameter this[string name]
		{
			get { return (MySQLParameter)_parms[ IndexOf( name ) ]; }
			set 
			{ 
				_parms[ IndexOf( name ) ] = value;
			}
		}

		public MySQLParameter Add(MySQLParameter value)
		{
			if ( value.ParameterName == null ) throw new ArgumentException("parameter must be named");

			_parms.Add(value);
			return value;
		}

		public MySQLParameter Add(string parameterName, DbType type)
		{
			return Add(new MySQLParameter(parameterName, type));
		}

		public MySQLParameter Add(string parameterName, object value)
		{
			return Add(new MySQLParameter(parameterName, value));
		}

		public MySQLParameter Add(string parameterName, MySQLDbType dbType, string sourceColumn)
		{
			return Add(new MySQLParameter(parameterName, dbType, sourceColumn));
		}

		#endregion

	}
}
