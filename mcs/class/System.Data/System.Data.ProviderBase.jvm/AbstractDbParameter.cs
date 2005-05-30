//
// System.Data.ProviderBase.AbstractDbParameter
//
// Author:
//   Boris Kirzner (borisk@mainsoft.com)
//

using System;
using System.Data;
using System.Data.Common;

using java.sql;

namespace System.Data.ProviderBase
{
    public abstract class AbstractDbParameter : DbParameterBase
    {
		#region Fields

		protected byte _precision;
		protected byte _scale;
		protected DataRowVersion _sourceVersion;
		private DbTypes.JavaSqlTypes _jdbcType;
		protected bool _isDbTypeSet = false;
		protected bool _isJdbcTypeSet = false;
		object _convertedValue;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		protected AbstractDbParameter ()
		{
		}

		#endregion // Constructors

		#region Properties
		
		public override byte Precision 
		{ 
			get { return _precision; }
			set { _precision = value; } 
		}

		public override byte Scale 
		{ 
			get { return _scale; }
			set { _scale = value; } 
		}

		public override DataRowVersion SourceVersion
		{
			get { return _sourceVersion; }
			set { _sourceVersion = value; }
		}

		internal DbTypes.JavaSqlTypes JdbcType
		{
			get { 
				if (!_isJdbcTypeSet) {
					return JdbcTypeFromProviderType();
				}
				return _jdbcType; 
			}
			set { 
				_jdbcType = value; 
				_isJdbcTypeSet = true;
			}
		}
		
		internal bool IsJdbcTypeSet
		{
			get { 
				return _isJdbcTypeSet; 
			}

			set {
				_isJdbcTypeSet = value;
			}
		}

		internal bool IsDbTypeSet
		{
			get { return _isDbTypeSet; }
		}

		internal virtual bool IsSpecial {
			get {
				return false;
			}
		}

		private bool IsFixedLength
		{
			get {
				return ((DbType != DbType.AnsiString) && (DbType != DbType.Binary) && 
						(DbType != DbType.String) && (DbType != DbType.VarNumeric));
			}
		}

		protected internal virtual string Placeholder {
			get {
				return "?";
			}
		}

		internal object ConvertedValue
		{
			get { 
				if (_convertedValue == null) {
					object value = Value;
					_convertedValue = ((value != null) && (value != DBNull.Value)) ? ConvertValue(value) : value;
				}
				return _convertedValue;
			}
		}

		public override object Value {
			get { return base.Value; }
			set { 
				_convertedValue = null;
				base.Value = value;
			}
		}

		#endregion // Properties

		#region Methods

		internal abstract void SetParameterName(ResultSet res);

		internal abstract void SetParameterDbType(ResultSet res);

		internal abstract void SetSpecialFeatures(ResultSet res);
		
		public abstract object Clone();

		internal abstract DbTypes.JavaSqlTypes JdbcTypeFromProviderType();

		internal abstract object ConvertValue(object value);

		internal void SetParameterPrecisionAndScale(ResultSet res)
		{
			int jdbcType = res.getInt("DATA_TYPE");
			if(jdbcType == java.sql.Types.DECIMAL || jdbcType == java.sql.Types.NUMERIC) {
				Precision = (byte)res.getInt("PRECISION");
				Scale = (byte)res.getInt("SCALE");
			}
		}

		internal void SetParameterSize(ResultSet res)
		{
			Size = res.getInt("LENGTH");
		}

		internal void SetParameterIsNullable(ResultSet res)
		{
			IsNullable = (res.getInt("NULLABLE") == 1);
		}

		internal void Validate()
		{
			if (!IsFixedLength && ((Direction & ParameterDirection.Output) != 0) && (Size == 0)) {
				throw ExceptionHelper.ParameterSizeNotInitialized(Offset,ParameterName,DbType.ToString(),Size);	
			}
		}

		public override void CopyTo (DbParameter target)
		{
			base.CopyTo(target);
			AbstractDbParameter t = (AbstractDbParameter) target;
			t._precision = _precision;
			t._scale = _scale;
			t._sourceVersion = _sourceVersion;
			t._jdbcType = _jdbcType;
		}

		#endregion // Methods
    }
}

