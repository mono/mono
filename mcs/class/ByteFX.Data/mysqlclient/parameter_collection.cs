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
	/// <summary>
	/// Represents a collection of parameters relevant to a <see cref="MySqlCommand"/> as well as their respective mappings to columns in a <see cref="DataSet"/>. This class cannot be inherited.
	/// </summary>
	/// <include file='docs/MySqlParameterCollection.xml' path='MyDocs/MyMembers[@name="Class"]/*'/>
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

		bool ICollection.IsSynchronized
		{
			get { return _parms.IsSynchronized; }
		}

		object ICollection.SyncRoot
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

		/// <summary>
		/// Gets a value indicating whether a MySqlParameter exists in the collection.
		/// </summary>
		/// <param name="value">The value of the <see cref="MySqlParameter"/> object to find. </param>
		/// <returns>true if the collection contains the <see cref="MySqlParameter"/> object; otherwise, false.</returns>
		/// <overloads>Gets a value indicating whether a <see cref="MySqlParameter"/> exists in the collection.</overloads>
		public bool Contains(object value)
		{
			return _parms.Contains(value);
		}

		/// <summary>
		/// Gets the location of a <see cref="MySqlParameter"/> in the collection.
		/// </summary>
		/// <param name="value">The <see cref="MySqlParameter"/> object to locate. </param>
		/// <returns>The zero-based location of the <see cref="MySqlParameter"/> in the collection.</returns>
		/// <overloads>Gets the location of a <see cref="MySqlParameter"/> in the collection.</overloads>
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

		bool IList.IsFixedSize
		{
			get { return _parms.IsFixedSize; }
		}

		bool IList.IsReadOnly
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

		/// <summary>
		/// Removes the specified <see cref="MySqlParameter"/> from the collection using a specific index.
		/// </summary>
		/// <param name="index">The zero-based index of the parameter. </param>
		/// <overloads>Removes the specified <see cref="MySqlParameter"/> from the collection.</overloads>
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

		/// <summary>
		/// Adds the specified <see cref="MySqlParameter"/> object to the <see cref="MySqlParameterCollection"/>.
		/// </summary>
		/// <param name="value">The <see cref="MySqlParameter"/> to add to the collection.</param>
		/// <returns>The index of the new <see cref="MySqlParameter"/> object.</returns>
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

		/// <summary>
		/// Gets a value indicating whether a <see cref="MySqlParameter"/> with the specified parameter name exists in the collection.
		/// </summary>
		/// <param name="name">The name of the <see cref="MySqlParameter"/> object to find.</param>
		/// <returns>true if the collection contains the parameter; otherwise, false.</returns>
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

		/// <summary>
		/// Gets the location of the <see cref="MySqlParameter"/> in the collection with a specific parameter name.
		/// </summary>
		/// <param name="parameterName">The name of the <see cref="MySqlParameter"/> object to retrieve. </param>
		/// <returns>The zero-based location of the <see cref="MySqlParameter"/> in the collection.</returns>
		public int IndexOf( string parameterName )
		{
			if (parameterName[0] == '@')
				parameterName = parameterName.Substring(1, parameterName.Length-1);
			for (int x=0; x < _parms.Count; x++) 
			{
				MySqlParameter p = (MySqlParameter)_parms[x];
				if (p.ParameterName.ToLower().Equals( parameterName.ToLower() )) return x;
			}
			throw new MySqlException("Parameter '" + parameterName + "' not found in collection");
		}

		/// <summary>
		/// Removes the specified <see cref="MySqlParameter"/> from the collection using the parameter name.
		/// </summary>
		/// <param name="name">The name of the <see cref="MySqlParameter"/> object to retrieve. </param>
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
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_parms).GetEnumerator();
		}
		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the <see cref="MySqlParameter"/> at the specified index.
		/// </summary>
		/// <overloads>Gets the <see cref="MySqlParameter"/> with a specified attribute.
		/// [C#] In C#, this property is the indexer for the <see cref="MySqlParameterCollection"/> class.
		/// </overloads>
		public MySqlParameter this[int index]
		{
			get { return (MySqlParameter)_parms[index]; }
			set { _parms[index] = value; }
		}

		/// <summary>
		/// Gets the <see cref="MySqlParameter"/> with the specified name.
		/// </summary>
		public MySqlParameter this[string name]
		{
			get { return (MySqlParameter)_parms[ IndexOf( name ) ]; }
			set { _parms[ IndexOf( name ) ] = value; }
		}

		/// <summary>
		/// Adds the specified <see cref="MySqlParameter"/> object to the <see cref="MySqlParameterCollection"/>.
		/// </summary>
		/// <param name="value">The <see cref="MySqlParameter"/> to add to the collection.</param>
		/// <returns>The index of the new <see cref="MySqlParameter"/> object.</returns>
		public MySqlParameter Add(MySqlParameter value)
		{
			if ( value.ParameterName == null ) throw new ArgumentException("parameter must be named");

			_parms.Add(value);
			return value;
		}

		/// <summary>
		/// Adds a <see cref="MySqlParameter"/> to the <see cref="MySqlParameterCollection"/> given the specified parameter name and value.
		/// </summary>
		/// <param name="parameterName">The name of the parameter.</param>
		/// <param name="value">The <see cref="MySqlParameter.Value"/> of the <see cref="MySqlParameter"/> to add to the collection.</param>
		/// <returns>The index of the new <see cref="MySqlParameter"/> object.</returns>
		public MySqlParameter Add( string parameterName, object value )
		{
			return Add( new MySqlParameter( parameterName, value ) );
		}

		/// <summary>
		/// Adds a <see cref="MySqlParameter"/> to the <see cref="MySqlParameterCollection"/> given the parameter name and the data type.
		/// </summary>
		/// <param name="parameterName">The name of the parameter.</param>
		/// <param name="dbType">One of the <see cref="MySqlDbType"/> values. </param>
		/// <returns>The index of the new <see cref="MySqlParameter"/> object.</returns>
		public MySqlParameter Add(string parameterName, MySqlDbType dbType)
		{
			return Add(new MySqlParameter(parameterName, dbType));
		}

		/// <summary>
		/// Adds a <see cref="MySqlParameter"/> to the <see cref="MySqlParameterCollection"/> with the parameter name, the data type, and the column length.
		/// </summary>
		/// <param name="parameterName">The name of the parameter.</param>
		/// <param name="dbType">One of the <see cref="MySqlDbType"/> values. </param>
		/// <param name="size">The length of the column.</param>
		/// <returns>The index of the new <see cref="MySqlParameter"/> object.</returns>
		public MySqlParameter Add(string parameterName, MySqlDbType dbType, int size)
		{
			return Add(new MySqlParameter(parameterName, dbType, size ));
		}

		/// <summary>
		/// Adds a <see cref="MySqlParameter"/> to the <see cref="MySqlParameterCollection"/> with the parameter name, the data type, the column length, and the source column name.
		/// </summary>
		/// <param name="parameterName">The name of the parameter.</param>
		/// <param name="dbType">One of the <see cref="MySqlDbType"/> values. </param>
		/// <param name="size">The length of the column.</param>
		/// <param name="sourceColumn">The name of the source column.</param>
		/// <returns>The index of the new <see cref="MySqlParameter"/> object.</returns>
		public MySqlParameter Add(string parameterName, MySqlDbType dbType, int size, string sourceColumn)
		{
			return Add(new MySqlParameter(parameterName, dbType, size, sourceColumn));
		}

		#endregion

	}
}
