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
		private string m_name = null;
		private object m_value = null;
		private int m_size = 0;
		private bool m_isNullable = true;
		private byte m_precision = 0;
		private byte m_scale = 0;
		private DataRowVersion m_sourceVersion;
		private string m_sourceColumn = null;
		private ParameterDirection m_direction;
		private DbType m_type;

		/*
		 * Constructors
		 */
		
		public OleDbParameter ()
		{
		}

		public OleDbParameter (string name, object value) : this ()
		{
			m_name = name;
			m_value = value;
		}

		public OleDbParameter (string name, OleDbType type) : this ()
		{
			m_name = name;
			m_type = (DbType) type;
		}

		public OleDbParameter (string name, OleDbType type, int width)
			: this (name, type)
		{
			m_size = width;
		}

		public OleDbParameter (string name, OleDbType type,
				       int width, string src_col)
			: this (name, type, width)
		{
			m_name = name;
			m_type = (DbType) type;
			m_size = width;
			m_sourceColumn = src_col;
		}

		public OleDbParameter(string name, OleDbType type,
				      int width, ParameterDirection direction,
				      bool is_nullable, byte precision,
				      byte scale, string src_col,
				      DataRowVersion src_version, object value)
			: this (name, type, width, src_col)
		{
			m_direction = direction;
			m_isNullable = is_nullable;
			m_precision = precision;
			m_scale = scale;
			m_sourceVersion = src_version;
			m_value = value;
		}

		/*
		 * Properties
		 */

		DbType IDataParameter.DbType
		{
			get {
				return m_type;
			}
			set {
				m_type = value;
			}
		}
		
		ParameterDirection IDataParameter.Direction
		{
			get {
				return m_direction;
			}
			set {
				m_direction = value;
			}
		}
		
		bool IDataParameter.IsNullable
	        {
			get {
				return m_isNullable;
			}
			set {
				m_isNullable = value;
			}
		}
		
		string IDataParameter.ParameterName
		{
			get {
				return m_name;
			}
			set {
				m_name = value;
			}
		}

		byte IDbDataParameter.Precision
		{
			get {
				return m_precision;
			}
			set {
				m_precision = value;
			}
		}
		
		byte IDbDataParameter.Scale
	        {
			get {
				return m_scale;
			}
			set {
				m_scale = value;
			}
		}
		
		int IDbDataParameter.Size
		{
			get {
				return m_size;
			}
			set {
				m_size = value;
			}
		}

		string IDataParameter.SourceColumn
		{
			get {
				return m_sourceColumn;
			}
			set {
				m_sourceColumn = value;
			}
		}
		
		DataRowVersion IDataParameter.SourceVersion
		{
			get {
				return m_sourceVersion;
			}
			set {
				m_sourceVersion = value;
			}
		}
		
		object IDataParameter.Value
		{
			get {
				return m_value;
			}
			set {
				m_value = value;
			}
		}

		/*
		 * Methods
		 */

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
		}
	}
}
