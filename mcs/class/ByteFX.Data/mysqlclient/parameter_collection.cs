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
using System.ComponentModel;

namespace ByteFX.Data.MySqlClient
{
	[Editor(typeof(ByteFX.Data.Common.DBParametersEditor), typeof(System.Drawing.Design.UITypeEditor))]
	[ListBindable(true)]
	public sealed class MySqlParameterCollection : MarshalByRefObject, IDataParameterCollection, 
		IList, ICollection, IEnumerable
	{
		private ArrayList	_parms = new ArrayList();

		#region ICollection support

		/// <summary>
		/// Gets the number of MySqlParameter objects in the collection.
		/// </summary>
		public int Count 
		{
			get { return _parms.Count; }
		}

		/// <summary>
		/// Copies MySqlParameter objects from the MySqlParameterCollection to the specified array.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
		public void CopyTo( Array array, int index ) 
		{
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

		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
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

		/// <summary>
		/// Inserts a MySqlParameter into the collection at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
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

		/// <summary>
		/// Removes the specified MySqlParameter from the collection.
		/// </summary>
		/// <param name="value"></param>
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
				if (! (value is MySqlParameter)) throw new MySqlException("Only MySqlParameter objects may be stored");
				this[index] = (MySqlParameter)value; 
			}
		}

		public int Add( object value )
		{
			if (! (value is MySqlParameter)) throw new MySqlException("Only MySqlParameter objects may be stored");

			MySqlParameter p = (MySqlParameter)value;

			if (p.ParameterName == null || p.ParameterName == String.Empty)
				throw new MySqlException("Parameters must be named");

			return _parms.Add(value);
		}

		#endregion

		#region IDataParameterCollection
		public bool Contains(string name)
		{
			if (name[0] == '@')
				name = name.Substring(1, name.Length-1);
			foreach (MySqlParameter p in _parms)
			{
				if (p.ParameterName.ToLower().Equals( name.ToLower() )) return true;
			}
			return false;
		}

		public int IndexOf( string name )
		{
			if (name[0] == '@')
				name = name.Substring(1, name.Length-1);
			for (int x=0; x < _parms.Count; x++) 
			{
				MySqlParameter p = (MySqlParameter)_parms[x];
				if (p.ParameterName.ToLower().Equals( name.ToLower() )) return x;
			}
			throw new MySqlException("Parameter '" + name + "' not found in collection");
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
				if (! (value is MySqlParameter)) throw new MySqlException("Only MySqlParameter objects may be stored");
				this[name] = (MySqlParameter)value;
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
		public MySqlParameter this[int index]
		{
			get { return (MySqlParameter)_parms[index]; }
			set { _parms[index] = value; }
		}

		public MySqlParameter this[string name]
		{
			get { return (MySqlParameter)_parms[ IndexOf( name ) ]; }
			set { _parms[ IndexOf( name ) ] = value; }
		}

		public MySqlParameter Add(MySqlParameter value)
		{
			if ( value.ParameterName == null ) throw new ArgumentException("parameter must be named");

			_parms.Add(value);
			return value;
		}

		public MySqlParameter Add( string parameterName, object value )
		{
			return Add( new MySqlParameter( parameterName, value ) );
		}

		public MySqlParameter Add(string parameterName, MySqlDbType type)
		{
			return Add(new MySqlParameter(parameterName, type));
		}

		public MySqlParameter Add(string parameterName, MySqlDbType dbType, int size)
		{
			return Add(new MySqlParameter(parameterName, dbType, size ));
		}

		public MySqlParameter Add(string parameterName, MySqlDbType dbType, int size, string sourceColumn)
		{
			return Add(new MySqlParameter(parameterName, dbType, size, sourceColumn));
		}

		#endregion

	}
}
