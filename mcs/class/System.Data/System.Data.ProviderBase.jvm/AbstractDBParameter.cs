//
// System.Data.ProviderBase.AbstractDbParameter
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//

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
using System.Data;
using System.Data.Common;

using java.sql;

namespace System.Data.ProviderBase
{
    public abstract class AbstractDbParameter : DbParameter, IDbDataParameter, ICloneable
    {
		#region Fields

		byte _precision;
		byte _scale;
		protected DataRowVersion _sourceVersion;
		private int _jdbcType;
		bool _isDbTypeSet;
		bool _isJdbcTypeSet;
		object _convertedValue;

		string _parameterName;
		ParameterDirection _direction = ParameterDirection.Input;
		int _size;
		object _value;
		bool _isNullable;
		int _offset;
		string _sourceColumn;
		DbParameterCollection _parent = null;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		protected AbstractDbParameter ()
		{
		}

		#endregion // Constructors

		#region Properties

		public override ParameterDirection Direction {
			get { return _direction; }
			set {
				if (_direction != value) {
					switch (value) {
							case ParameterDirection.Input:
							case ParameterDirection.Output:
							case ParameterDirection.InputOutput:
							case ParameterDirection.ReturnValue:
							{
								_direction = value;
								return;
							}
					}
					throw ExceptionHelper.InvalidParameterDirection (value);
				}
			}
		}

		public override bool IsNullable {
			get { return _isNullable; }
			set { _isNullable = value; }
		}

		
		public virtual int Offset {
			get { return _offset; }
			set { _offset = value; }			
		}

		public override string ParameterName {
			get {
				if (_parameterName == null)
						return String.Empty;

				return _parameterName;
			}
			set {
				if (_parameterName != value) {
					_parameterName = value;
				}
			}
		}

		public override int Size {
			get { return _size; }

			set {
				if (_size != value) {
					if (value < -1)
						throw ExceptionHelper.InvalidSizeValue (value);

					_size = value;
				}
			}
		}

		
		public override string SourceColumn {
			get { 
				if (_sourceColumn == null)
					return String.Empty;

				return _sourceColumn;
			}

			set	{ _sourceColumn = value; }
		}

		internal DbParameterCollection Parent
		{
			get { return _parent; }
			set { _parent = value; }
		}
		
		public byte Precision 
		{ 
			get { return _precision; }
			set { _precision = value; } 
		}

		public byte Scale 
		{ 
			get { return _scale; }
			set { _scale = value; } 
		}

		public override DataRowVersion SourceVersion
		{
			get { return _sourceVersion; }
			set { _sourceVersion = value; }
		}

		protected internal int JdbcType
		{
			get { 
				if (!IsJdbcTypeSet) {
					return JdbcTypeFromProviderType();
				}
				return _jdbcType; 
			}
			set { 
				_jdbcType = value; 
				IsJdbcTypeSet = true;
			}
		}
		
		protected internal bool IsJdbcTypeSet
		{
			get { 
				return _isJdbcTypeSet; 
			}

			set {
				_isJdbcTypeSet = value;
			}
		}

		protected internal bool IsDbTypeSet
		{
			get { return _isDbTypeSet; }
			set { _isDbTypeSet = value; }
		}

		protected internal virtual bool IsSpecial {
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
			get { return _value; }
			set { 
				_convertedValue = null;
				_value = value;
			}
		}

		//DbParameter overrides

		public override bool SourceColumnNullMapping {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}


		#endregion // Properties

		#region Methods

		public override String ToString()
        {
            return ParameterName;
        }

		protected internal abstract void SetParameterName(ResultSet res);

		protected internal abstract void SetParameterDbType(ResultSet res);

		protected internal abstract void SetSpecialFeatures(ResultSet res);
		
		public virtual object Clone()
		{
			AbstractDbParameter other = (AbstractDbParameter) MemberwiseClone ();
			other._parent = null;
			return other;
		}

		protected internal abstract int JdbcTypeFromProviderType();

		protected internal abstract object ConvertValue(object value);

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

		//DbParameter overrides

		public override void ResetDbType() {
			throw new NotImplementedException();
		}

		#endregion // Methods
	}
}

