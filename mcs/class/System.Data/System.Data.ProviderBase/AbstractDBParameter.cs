//namespace System.Data.Common
//{
//
//    //using clr.System;
//    //using clr.compiler.BitConstants;
//
//    using System.Data;
//	using System.Data.ProviderBase;
//	using System.Text;
//
//    /**
//     * @author erand
//     */
//    public abstract class AbstractDBParameter : DbParameterBase, System.ICloneable
//    {
//        /* Properties from IDataParameter */
////        protected ParameterDirection _direction = ParameterDirection.Input;
////        protected bool _isNullable = false;
////        protected String _name = null;
////        protected String _sourceColumn = null;
//        protected DataRowVersion _version = DataRowVersion.Current;
////        protected Object _value = null;
//
//        /* Properties from IDbDataParameter */
////        protected byte _precision;
////        protected byte _scale;
////        protected int _size = -1;
//
//        private int _place;
//		private int _jdbcType;
//    
////        /**
////         * @see System.Data.IDbDataParameter#Precision
////         */
////        public virtual byte Precision
////        {
////            get
////            {
////                return _precision;
////            }
////            set
////            {
////                _precision = value;
////            }
////        }
//
//        
////        /**
////         * @see System.Data.IDbDataParameter#Scale
////         */
////        public virtual byte Scale
////        {
////            get
////            {
////                return _scale;
////            }
////            set
////            {
////                _scale = value;
////            }
////        }
//
//        
//
////        /**
////         * @see System.Data.IDbDataParameter#Size
////         */
////        public virtual int Size
////        {
////            get
////            {
////                return _size;
////            }
////            set
////            {
////                _size = value;
////            }
////        }
//
//        
////        /**
////         * @see System.Data.IDataParameter#Direction
////         */
////        public virtual ParameterDirection Direction
////        {
////            get
////            {
////                return _direction;
////            }
////            set
////            {
////                _direction = value;
////            }
////        }
//
//        
////        /**
////         * @see System.Data.IDataParameter#IsNullable
////         */
////        public virtual bool IsNullable
////        {
////            get
////            {
////                return _isNullable;
////            }
////            set
////            {
////                _isNullable = value;
////            }
////        }
//
//        
////        /**
////         * @see System.Data.IDataParameter#ParameterName
////         */
////        public virtual String ParameterName
////        {
////            get
////            {
////                /**@todo What's the value of the Empty string ?*/
////                /*Where to define our Empty String ?*/
////                if (_name == null)
////                    return String.Empty;
////                else
////                    return _name;
////            }
////            set
////            {
////                if ((value != null) && value.Equals(_name))
////                    return;
////                //if ((value != null) && (value.length() > Constants.MAXIMAL_PARAMETER_LENGTH))
////                /**@todo Implement Exception::-->*/
////                //    throw InvalidParameterLength(value);
////
////                _name = value;
////            }
////        }
//
//        
////        /**
////         * @see System.Data.IDataParameter#SourceColumn
////         */
////        public virtual String SourceColumn
////        {
////            get
////            {
////                if (_sourceColumn != null)
////                    return _sourceColumn;
////                else
////                    return String.Empty;
////            }
////            set
////            {
////                _sourceColumn = value;
////            }
////        }
//
//        
//
////        /**
////         * @see System.Data.IDataParameter#SourceVersion
////         */
////        public virtual DataRowVersion SourceVersion
////        {
////            get
////            {
////                return _version;
////            }
////            set
////            {
////                _version = value;
////            }
////        }
//
//        
////        /**
////         * @see System.Data.IDataParameter#Value
////         */
////        public virtual Object Value
////        {
////            get
////            {
////                return _value;
////            }
////            set
////            {
////                _value = value;
////            }
////        }
//
//        
//
//        public virtual void setParameterPlace(int place)
//        {
//            _place = place;
//        }
//
//        public virtual int getParameterPlace()
//        {
//            return _place;
//        }
//
//        abstract internal int getJDBCType(DbType dbType);
//
//		internal int JdbcType
//		{
//			get {
//				return _jdbcType;
//			}
//
//			set {
//				_jdbcType = value;
//			}
//		}
//
//        public abstract DbType DbType
//        {
//            get;
//            set;
//        }
//
//        public abstract Object Clone();
//
//		internal virtual bool IsOracleRefCursor
//		{
//			get
//			{
//				return false;
//			}
//		}
//        
//        internal virtual String formatParameter()
//        {
//			if (Value == null || Value == DBNull.Value)
//				return "NULL";
//	            
//			switch(DbType) {
//				case DbType.Byte:
//				case DbType.Currency:
//				case DbType.Decimal:
//				case DbType.Double:
//				case DbType.Int16:
//				case DbType.Int32:
//				case DbType.Int64:
//				case DbType.SByte:
//				case DbType.Single:
//				case DbType.UInt16:
//				case DbType.UInt32:
//				case DbType.UInt64:
//					return Value.ToString();
//				case DbType.Boolean:
//					return (bool)Value ? "0x1" : "0x0";
//				case DbType.Binary:
//				default:
//					return String.Concat("\'", Value.ToString().Replace("\'", "\'\'"),"\'");
//			}
//        }
//    }
//}