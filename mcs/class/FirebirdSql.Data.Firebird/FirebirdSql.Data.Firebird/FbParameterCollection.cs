/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Data;
using System.ComponentModel;
using System.Collections;
using System.Globalization;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/overview/*'/>
#if	(NET)
	[ListBindable(false)]
	[Editor(typeof(Design.FbParameterCollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
#endif
	public sealed class FbParameterCollection : MarshalByRefObject, IDataParameterCollection, IList, ICollection, IEnumerable
	{
		#region Fields

		private ArrayList parameters;

		#endregion

		#region Indexers

		/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/indexer[@name="Item(System.String)"]/*'/>
#if	(!NETCF)
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
		public FbParameter this[string parameterName]
		{
			get { return (FbParameter)this[this.IndexOf(parameterName)]; }
			set { this[this.IndexOf(parameterName)] = (FbParameter)value; }
		}

		object IDataParameterCollection.this[string parameterName]
		{
			get { return this[parameterName]; }
			set { this[parameterName] = (FbParameter)value; }
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/indexer[@name="Item(System.Int32)"]/*'/>
#if	(!NETCF)
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
		public FbParameter this[int index]
		{
			get { return (FbParameter)this.parameters[index]; }
			set { this.parameters[index] = (FbParameter)value; }
		}

		object IList.this[int index]
		{
			get { return (FbParameter)this[index]; }
			set { this[index] = (FbParameter)value; }
		}

		#endregion

		#region Constructors

		internal FbParameterCollection()
		{
			this.parameters = ArrayList.Synchronized(new ArrayList());
		}

		#endregion

		#region IList Properties

		bool IList.IsFixedSize
		{
			get { return this.parameters.IsFixedSize; }
		}

		bool IList.IsReadOnly
		{
			get { return this.parameters.IsReadOnly; }
		}

		#endregion

		#region ICollection	Properties

		/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/property[@name="Count"]/*'/>
#if	(!NETCF)
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
		public int Count
		{
			get { return this.parameters.Count; }
		}

		bool ICollection.IsSynchronized
		{
			get { return this.parameters.IsSynchronized; }
		}

		object ICollection.SyncRoot
		{
			get { return this.parameters.SyncRoot; }
		}

		#endregion

		#region ICollection	Methods

		/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/method[@name="CopyTo(System.Array,System.Int32)"]/*'/>
		public void CopyTo(Array array, int index)
		{
			this.parameters.CopyTo(array, index);
		}

		#endregion

		#region IList Methods

		/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/method[@name="Clear"]/*'/>
		public void Clear()
		{
			this.parameters.Clear();
		}

		#endregion

		#region IEnumerable	Methods

		/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/method[@name="GetEnumerator"]/*'/>
		public IEnumerator GetEnumerator()
		{
			return this.parameters.GetEnumerator();
		}

		#endregion

		#region Methods

		/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/method[@name="Contains(System.Object)"]/*'/>
		public bool Contains(object value)
		{
			return this.parameters.Contains(value);
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/method[@name="Contains(System.String)"]/*'/>
		public bool Contains(string parameterName)
		{
			return (-1 != this.IndexOf(parameterName));
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/method[@name="IndexOf(System.Object)"]/*'/>
		public int IndexOf(object value)
		{
			return this.parameters.IndexOf(value);
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/method[@name="IndexOf(System.String)"]/*'/>
		public int IndexOf(string parameterName)
		{
			int index = 0;
			foreach (FbParameter item in this.parameters)
			{
				if (GlobalizationHelper.CultureAwareCompare(item.ParameterName, parameterName))
				{
					return index;
				}
				index++;
			}
			return -1;
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/method[@name="Insert(System.Int32,System.Object)"]/*'/>
		public void Insert(int index, object value)
		{
			this.parameters.Insert(index, value);
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/method[@name="Remove(System.Object)"]/*'/>
		public void Remove(object value)
		{
			if (!(value is FbParameter))
			{
				throw new InvalidCastException("The parameter passed was not a FbParameter.");
			}
			if (!this.Contains(value))
			{
				throw new SystemException("The parameter does not exist in the collection.");
			}

			this.parameters.Remove(value);

			((FbParameter)value).Parent = null;
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/method[@name="RemoveAt(System.Int32)"]/*'/>
		public void RemoveAt(int index)
		{
			if (index < 0 || index > this.Count)
			{
				throw new IndexOutOfRangeException("The specified index does not exist.");
			}

			FbParameter parameter = this[index];
			this.parameters.RemoveAt(index);
			parameter.Parent = null;
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/method[@name="RemoveAt(System.String)"]/*'/>
		public void RemoveAt(string parameterName)
		{
			this.RemoveAt(this.IndexOf(parameterName));
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/method[@name="Add(System.String,System.Object)"]/*'/>
		public FbParameter Add(string parameterName, object value)
		{
			FbParameter param = new FbParameter(parameterName, value);

			return this.Add(param);
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/method[@name="Add(System.String,FbDbType)"]/*'/>
		public FbParameter Add(string parameterName, FbDbType type)
		{
			FbParameter param = new FbParameter(parameterName, type);

			return this.Add(param);
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/method[@name="Add(System.String,FbDbType,System.Int32)"]/*'/>
		public FbParameter Add(string parameterName, FbDbType fbType, int size)
		{
			FbParameter param = new FbParameter(parameterName, fbType, size);

			return this.Add(param);
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/method[@name="Add(System.String,FbDbType,System.Int32,System.String)"]/*'/>
		public FbParameter Add(
			string parameterName, FbDbType fbType, int size, string sourceColumn)
		{
			FbParameter param = new FbParameter(parameterName, fbType, size, sourceColumn);

			return this.Add(param);
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/method[@name="Add(System.Object)"]/*'/>
		public int Add(object value)
		{
			if (!(value is FbParameter))
			{
				throw new InvalidCastException("The parameter passed was not a FbParameter.");
			}

			return this.IndexOf(this.Add(value as FbParameter));
		}

		/// <include file='Doc/en_EN/FbParameterCollection.xml'	path='doc/class[@name="FbParameterCollection"]/method[@name="Add(FbParameter)"]/*'/>
		public FbParameter Add(FbParameter value)
		{
			lock (this.parameters.SyncRoot)
			{
				if (value == null)
				{
					throw new ArgumentException("The value parameter is null.");
				}
				if (value.Parent != null)
				{
					throw new ArgumentException("The FbParameter specified in the value parameter is already added to this or another FbParameterCollection.");
				}
				if (value.ParameterName == null ||
					value.ParameterName.Length == 0)
				{
					value.ParameterName = this.GenerateParameterName();
				}
				else
				{
					if (this.IndexOf(value) != -1)
					{
						throw new ArgumentException("FbParameterCollection already contains FbParameter with ParameterName '" + value.ParameterName + "'.");
					}
				}

				this.parameters.Add(value);

				return value;
			}
		}

		#endregion

		#region Private	Methods

		private string GenerateParameterName()
		{
			int index = this.Count + 1;
			string name = String.Empty;

			while (index > 0)
			{
				name = "Parameter" + index.ToString(CultureInfo.InvariantCulture);

				if (this.IndexOf(name) == -1)
				{
					index = -1;
				}
				else
				{
					index++;
				}
			}

			return name;
		}

		#endregion
	}
}