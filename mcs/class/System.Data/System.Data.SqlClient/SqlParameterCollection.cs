//
// System.Data.SqlClient.SqlParameterCollection.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//   Diego Caravana (diego@toth.it)
//
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Mono.Data.Tds;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

#if NET_2_0
using System.Data.ProviderBase;
#endif // NET_2_0

using System.Collections;

namespace System.Data.SqlClient {
	[ListBindable (false)]
	[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DataParametersEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
#if NET_2_0
	public sealed class SqlParameterCollection : DbParameterBaseCollection, IDataParameterCollection, IList, ICollection, IEnumerable
#else
	public sealed class SqlParameterCollection : MarshalByRefObject, IDataParameterCollection, IList, ICollection, IEnumerable
#endif // NET_2_0
	{
		#region Fields

		ArrayList list = new ArrayList();
		TdsMetaParameterCollection metaParameters;
		SqlCommand command;

		#endregion // Fields

		#region Constructors

		internal SqlParameterCollection (SqlCommand command)
		{
			this.command = command;
			metaParameters = new TdsMetaParameterCollection ();
		}

		#endregion // Constructors

		#region Properties

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]	
		public 
#if NET_2_0
		override
#endif // NET_2_0
	 int Count {
			get { return list.Count; }			  
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]	
		public 
#if NET_2_0
		new
#endif // NET_2_0
	 SqlParameter this [int index] {
			get { return (SqlParameter) list [index]; }			  
			set { list [index] = (SqlParameter) value; }			  
		}

		object IDataParameterCollection.this [string parameterName] {
			get { return this[parameterName]; }
			set { 
				if (!(value is SqlParameter))
					throw new InvalidCastException ("Only SQLParameter objects can be used.");
				this [parameterName] = (SqlParameter) value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]	
		public 
#if NET_2_0
		new
#endif // NET_2_0
	 SqlParameter this [string parameterName] {
			get {
				foreach (SqlParameter p in list)
					if (p.ParameterName.Equals (parameterName))
						return p;
				throw new IndexOutOfRangeException ("The specified name does not exist: " + parameterName);
			}	  
			set {	
				if (!Contains (parameterName))
					throw new IndexOutOfRangeException("The specified name does not exist: " + parameterName);
				this [IndexOf (parameterName)] = value;
			}			  
		}

		object IList.this [int index] {
			get { return (SqlParameter) this [index]; }
			set { this [index] = (SqlParameter) value; }
		}

		bool IList.IsFixedSize {
			get { return list.IsFixedSize; }
		}

		bool IList.IsReadOnly {
			get { return list.IsReadOnly; }
		}

		bool ICollection.IsSynchronized {
			get { return list.IsSynchronized; }
		}

		object ICollection.SyncRoot {
			get { return list.SyncRoot; }
		}

		internal TdsMetaParameterCollection MetaParameters {
			get { return metaParameters; }
		}
		
		#endregion // Properties

		#region Methods

		public  
#if NET_2_0
		override
#endif // NET_2_0
	 int Add (object value)
		{
			if (!(value is SqlParameter))
				throw new InvalidCastException ("The parameter was not an SqlParameter.");
			Add ((SqlParameter) value);
			return IndexOf (value);
		}
		
		public SqlParameter Add (SqlParameter value)
		{
			if (value.Container != null)
				throw new ArgumentException ("The SqlParameter specified in the value parameter is already added to this or another SqlParameterCollection.");
			
			value.Container = this;
			list.Add (value);
			metaParameters.Add (value.MetaParameter);
			return value;
		}
		
		public SqlParameter Add (string parameterName, object value)
		{
			return Add (new SqlParameter (parameterName, value));
		}
		
		public SqlParameter Add (string parameterName, SqlDbType sqlDbType)
		{
			return Add (new SqlParameter (parameterName, sqlDbType));
		}

		public SqlParameter Add (string parameterName, SqlDbType sqlDbType, int size)
		{
			return Add (new SqlParameter (parameterName, sqlDbType, size));
		}

		public SqlParameter Add (string parameterName, SqlDbType sqlDbType, int size, string sourceColumn)
		{
			return Add (new SqlParameter (parameterName, sqlDbType, size, sourceColumn));
		}

		public 
#if NET_2_0
		override
#endif // NET_2_0
	 void Clear()
		{
			metaParameters.Clear ();
			
			foreach (SqlParameter p in list)
				p.Container = null;
			
			list.Clear ();
		}
		
		public 
#if NET_2_0
		override
#endif // NET_2_0
	 bool Contains (object value)
		{
			if (!(value is SqlParameter))
				throw new InvalidCastException ("The parameter was not an SqlParameter.");
			return Contains (((SqlParameter) value).ParameterName);
		}

		public 
#if NET_2_0
		override
#endif // NET_2_0
	 bool Contains (string value)
		{
			foreach (SqlParameter p in list)
				if (p.ParameterName.Equals (value))
					return true;
			return false;
		}

		public 
#if NET_2_0
		override
#endif // NET_2_0
	 void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public 
#if NET_2_0
		override
#endif // NET_2_0
	 IEnumerator GetEnumerator()
		{
			return list.GetEnumerator ();
		}
		
		public 
#if NET_2_0
		override
#endif // NET_2_0
	 int IndexOf (object value)
		{
			if (!(value is SqlParameter))
				throw new InvalidCastException ("The parameter was not an SqlParameter.");
			return IndexOf (((SqlParameter) value).ParameterName);
		}
		
		public 
#if NET_2_0
		override
#endif // NET_2_0
	 int IndexOf (string parameterName)
		{
			for (int i = 0; i < Count; i += 1)
                                if (this [i].ParameterName.Equals (parameterName))
                                        return i;
                        return -1;

		}

		public 
#if NET_2_0
		override
#endif // NET_2_0
	 void Insert (int index, object value)
		{
			list.Insert (index, value);
		}

		public 
#if NET_2_0
		override
#endif // NET_2_0
	 void Remove (object value)
		{
			((SqlParameter) value).Container = null;
			
			metaParameters.Remove (((SqlParameter) value).MetaParameter);
			list.Remove (value);
		}

		public 
#if NET_2_0
		override
#endif // NET_2_0
	 void RemoveAt (int index)
		{
			this [index].Container = null;
			metaParameters.RemoveAt (index);
			list.RemoveAt (index);
		}

		public 
#if NET_2_0
		override
#endif // NET_2_0
	 void RemoveAt (string parameterName)
		{
			RemoveAt (IndexOf (parameterName));
		}

                [MonoTODO]
                protected 
#if NET_2_0
		override
#endif // NET_2_0
	 Type ItemType
                {
                        get {throw new NotImplementedException ();}
                        
                }


		#endregion // Methods	
	}
}
