//
// System.Data.OleDb.OleDbParameterCollection
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;
using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	public sealed class OleDbParameterCollection : MarshalByRefObject,
		IDataParameterCollection, IList, ICollection, IEnumerable
	{
		#region Fields

		ArrayList list = new ArrayList ();

		#endregion // Fields

		#region Properties

		public int Count {
			get { return list.Count; }
		}

		public OleDbParameter this[int index] {
			get { return (OleDbParameter) list[index]; }
			set { list[index] = value; }
		}

		public OleDbParameter this[string parameterName] {
			[MonoTODO]
			get { throw new NotImplementedException (); }
			[MonoTODO]
			set { throw new NotImplementedException (); }
		}

		int ICollection.Count {
			get { return list.Count; }
		}

		bool IList.IsFixedSize {
			get { return false; }
		}
		
		bool IList.IsReadOnly {
			get { return false; }
		}
		
		bool ICollection.IsSynchronized {
			get { return list.IsSynchronized; }
		}

		object ICollection.SyncRoot {
			get { return list.SyncRoot; }
		}
		
		object IList.this[int index] {
			get { return list[index]; }
			set { list[index] = value; }
		}

		object IDataParameterCollection.this[string name]
		{
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		internal IntPtr GdaParameterList {
			[MonoTODO]
			get {
				IntPtr param_list;

				param_list = libgda.gda_parameter_list_new ();
				// FIXME: add parameters to list
				
				return param_list;
			}
		}

		#endregion // Properties

		#region Methods


		public OleDbParameter Add (OleDbParameter parameter)
		{
			list.Add (parameter);
			return parameter;
		}

		public OleDbParameter Add (string name, object value)
		{
			OleDbParameter parameter = new OleDbParameter (name, value);
			list.Add (parameter);
			return parameter;
		}

		public OleDbParameter Add (string name, OleDbType type)
	        {
			OleDbParameter parameter = new OleDbParameter (name, type);
			list.Add (parameter);
			return parameter;
		}

		public OleDbParameter Add (string name, OleDbType type, int width)
		{
			OleDbParameter parameter = new OleDbParameter (name, type, width);
			list.Add (parameter);
			return parameter;
		}

		public OleDbParameter Add (string name, OleDbType type,
					   int width, string src_col)
		{
			OleDbParameter parameter = new OleDbParameter (name, type, width, src_col);
			list.Add (parameter);
			return parameter;
		}

		int IList.Add (object value)
		{
			if (!(value is IDataParameter))
				throw new InvalidCastException ();

			list.Add (value);
			return list.IndexOf (value);
		}

		void IList.Clear ()
		{
			list.Clear ();
		}

		bool IList.Contains (object value)
		{
			return list.Contains (value);
		}

		bool IDataParameterCollection.Contains (string value)
		{
			for (int i = 0; i < list.Count; i++) {
				IDataParameter parameter;

				parameter = (IDataParameter) list[i];
				if (parameter.ParameterName == value)
					return true;
			}

			return false;
		}

		void ICollection.CopyTo (Array array, int index)
		{
			((OleDbParameter[])(list.ToArray ())).CopyTo (array, index);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return list.GetEnumerator ();
		}
		
		int IList.IndexOf (object value)
		{
			return list.IndexOf (value);
		}

		int IDataParameterCollection.IndexOf (string name)
		{
			return list.IndexOf (((IDataParameterCollection) this)[name]);
		}

		void IList.Insert (int index, object value)
	        {
			list.Insert (index, value);
		}

		void IList.Remove (object value)
		{
			list.Remove (value);
		}

		void IList.RemoveAt (int index)
		{
			list.Remove ((object) list[index]);
		}

		void IDataParameterCollection.RemoveAt (string name)
		{
			list.Remove (((IDataParameterCollection) this)[name]);
		}

		#endregion // Methods
	}
}
