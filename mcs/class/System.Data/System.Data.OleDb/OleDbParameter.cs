//
// System.Data.OleDb.OleDbParameter
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// Copyright (C) Rodrigo Moya, 2002
//

using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	public sealed class OleDbParameter : MarshalByRefObject,
		IDbDataParameter, IDataParameter, ICloneable
	{
		private string m_name;
		private object m_value;
		
		public OleDbParameter ()
		{
			m_name = null;
			m_value = null;
		}

		public OleDbParameter (string name, object value) : this ()
		{
			m_name = name;
			m_value = value;
		}

		public OleDbParameter (string name, OleDbType type)
		{
		}

		public OleDbParameter (string name, OleDbType type, int width)
		{
		}

		public OleDbParameter (string name, OleDbType type,
				       int width, string src_col)
		{
		}

		public OleDbParameter(string name, OleDbType type,
				      int with, ParameterDirection direction,
				      bool is_nullable, byte precision,
				      byte scale, string src_col,
				      DataRowVersion src_version, object value)
		{
		}

		/*
		 * Properties
		 */

		public string ParameterName
		{
			get {
				return m_name;
			}
			set {
				m_name = value;
			}
		}
	}
}
