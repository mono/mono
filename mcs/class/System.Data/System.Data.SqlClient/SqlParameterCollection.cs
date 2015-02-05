//
// System.Data.SqlClient.SqlParameterCollection.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//   Diego Caravana (diego@toth.it)
//   Umadevi S (sumadevi@novell.com)
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

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

using Mono.Data.Tds;

namespace System.Data.SqlClient
{
	[ListBindable (false)]
	[Editor ("Microsoft.VSDesigner.Data.Design.DBParametersEditor, " + Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
	public sealed class SqlParameterCollection : DbParameterCollection, IDataParameterCollection, IList, ICollection, IEnumerable
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

		public
		override
		int Count {
			get { return list.Count; }
		}

		public override bool IsFixedSize {
			get {
				return list.IsFixedSize;
			}
		}

		public override bool IsReadOnly {
			get {
				return list.IsReadOnly;
			}
		}

		public override bool IsSynchronized {
			get {
				return list.IsSynchronized;
			}
		}

		public override object SyncRoot {
			get {
				return list.SyncRoot;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]	
		public
		new
		SqlParameter this [int index] {
			get {
				if (index < 0 || index >= list.Count)
					throw new IndexOutOfRangeException ("The specified index is out of range.");
				return (SqlParameter) list [index];
			}
			set {
				if (index < 0 || index >= list.Count)
					throw new IndexOutOfRangeException ("The specified index is out of range.");
				list [index] = (SqlParameter) value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]	
		public
		new
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

		protected  override DbParameter GetParameter (int index)
		{
			return this [index];
		}

		protected override DbParameter GetParameter (string parameterName)
		{
			return this [parameterName];
		}
	
		protected  override void SetParameter (int index, DbParameter value)
		{
			this [index] = (SqlParameter) value;
		}

		protected override void SetParameter (string parameterName, DbParameter value)
		{
			this [parameterName] = (SqlParameter) value;
		}

		internal TdsMetaParameterCollection MetaParameters {
			get { return metaParameters; }
		}

		#endregion // Properties

		#region Methods

		[EditorBrowsable (EditorBrowsableState.Never)]
		public
		override
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
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Do not call this method.")]
		public SqlParameter Add (string parameterName, object value)
		{
			return Add (new SqlParameter (parameterName, value));
		}

		public SqlParameter AddWithValue (string parameterName, object value)
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
		override
		void Clear()
		{
			metaParameters.Clear ();

			foreach (SqlParameter p in list)
				p.Container = null;

			list.Clear ();
		}
		
		public
		override
		bool Contains (object value)
		{
			if (!(value is SqlParameter))
				throw new InvalidCastException ("The parameter was not an SqlParameter.");
			return Contains (((SqlParameter) value).ParameterName);
		}

		public
		override
		bool Contains (string value)
		{
			foreach (SqlParameter p in list)
				if (p.ParameterName.Equals (value))
					return true;
			return false;
		}

		public bool Contains (SqlParameter value)
		{
			return (this.IndexOf(value) != -1);
		}

		public
		override
		void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public
		override
		IEnumerator GetEnumerator()
		{
			return list.GetEnumerator ();
		}
		
		public
		override
		int IndexOf (object value)
		{
			if (!(value is SqlParameter))
				throw new InvalidCastException ("The parameter was not an SqlParameter.");
			return IndexOf (((SqlParameter) value).ParameterName);
		}
		
		public
		override
		int IndexOf (string parameterName)
		{
			for (int i = 0; i < Count; i += 1)
				if (this [i].ParameterName.Equals (parameterName))
					return i;
			return -1;
		}

		public int IndexOf (SqlParameter value)
		{
			return list.IndexOf(value);
		}

		public
		override
		void Insert (int index, object value)
		{
			list.Insert (index, value);
		}

		public void Insert (int index, SqlParameter value)
		{
			list.Insert (index,value);
		}

		public
		override
		void Remove (object value)
		{
			//TODO : this neds validation to check if the object is a 
			// sqlparameter.

			((SqlParameter) value).Container = null;
			
			metaParameters.Remove (((SqlParameter) value).MetaParameter);
			list.Remove (value);
		}

		public void Remove (SqlParameter value)
		{
			//both this and the above code are the same. but need to work with
			// 1.1!
			value.Container = null;
			metaParameters.Remove (value.MetaParameter);
			list.Remove (value);
		}

		public
		override
		void RemoveAt (int index)
		{
			this [index].Container = null;
			metaParameters.RemoveAt (index);
			list.RemoveAt (index);
		}

		public
		override
		void RemoveAt (string parameterName)
		{
			RemoveAt (IndexOf (parameterName));
		}

		public override void AddRange (Array values)
		{
			if (values == null)
				throw new ArgumentNullException("The argument passed was null");
			foreach (object value in values) {
				if (!(value is SqlParameter))
					throw new InvalidCastException ("Element in the array parameter was not an SqlParameter.");
				SqlParameter param = (SqlParameter) value;
				if (param.Container != null)
					throw new ArgumentException ("An SqlParameter specified in the array is already added to this or another SqlParameterCollection.");
				param.Container = this;
				list.Add (param);
				metaParameters.Add (param.MetaParameter);
			}
		}

		public void AddRange (SqlParameter[] values)
		{
			AddRange((Array) values);
		}
		
		public void CopyTo (SqlParameter[] array, int index)
		{
			list.CopyTo (array, index);
		}

		#endregion // Methods
	}
}
