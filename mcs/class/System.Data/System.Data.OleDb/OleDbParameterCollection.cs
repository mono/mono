//
// System.Data.OleDb.OleDbParameterCollection
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// Copyright (C) Rodrigo Moya, 2002
//

using System.Collections;
using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	public sealed class OleDbParameterCollection : MarshalByRefObject,
		IDataParameterCollection, IList, ICollection, IEnumerable
	{
		private ArrayList m_list = new ArrayList ();

		/*
		 * Properties
		 */

		int ICollection.Count {
			get {
				return m_list.Count;
			}
		}

		public IDataParameter this[int index]
		{
			get {
				return (OleDbParameter) m_list[index];
			}
			set {
				m_list[index] = value;
			}
		}

		public object this[string name]
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

		/*
		 * Methods
		 */

		int IList.Add (object value)
		{
			if (!(value is IDataParameter))
				throw new InvalidCastException ();

			m_list.Add (value);
			return m_list.IndexOf (value);
		}

		public OleDbParameter Add (OleDbParameter parameter)
		{
			m_list.Add (parameter);
			return parameter;
		}

		public OleDbParameter Add (string name, object value)
		{
			OleDbParameter parameter = new OleDbParameter (name, value);
			m_list.Add (parameter);
			return parameter;
		}

		public OleDbParameter Add (string name, OleDbType type)
	        {
			OleDbParameter parameter = new OleDbParameter (name, type);
			m_list.Add (parameter);
			return parameter;
		}

		public OleDbParameter Add (string name, OleDbType type, int width)
		{
			OleDbParameter parameter = new OleDbParameter (name, type, width);
			m_list.Add (parameter);
			return parameter;
		}

		public OleDbParameter Add (string name, OleDbType type,
					   int width, string src_col)
		{
			OleDbParameter parameter = new OleDbParameter (name, type, width, src_col);
			m_list.Add (parameter);
			return parameter;
		}

		void IList.Clear ()
		{
			m_list.Clear ();
		}

		bool IList.Contains (object value)
		{
			return m_list.Contains (value);
		}

		bool IDataParameterCollection.Contains (string value)
		{
			for (int i = 0; i < m_list.Count; i++) {
				OleDbParameter parameter;

				parameter = (OleDbParameter) m_list[i];
				if (parameter.ParameterName == value)
					return true;
			}

			return false;
		}

		void ICollection.CopyTo (Array array, int index)
		{
			((OleDbParameter[])(m_list.ToArray ())).CopyTo (array, index);
		}

		int IList.IndexOf (object value)
		{
			return m_list.IndexOf (value);
		}

		int IDataParameterCollection.IndexOf (string name)
		{
			return m_list.IndexOf ((object) this[name]);
		}

		void IList.Insert (int index, object value)
	        {
			m_list.Insert (index, value);
		}

		void IList.Remove (object value)
		{
			m_list.Remove (value);
		}

		void IList.RemoveAt (int index)
		{
			m_list.Remove ((object) m_list[index]);
		}

		void IDataParameterCollection.RemoveAt (string name)
		{
			m_list.Remove ((object) this[name]);
		}
	}
}
