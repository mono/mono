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

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/overview/*'/>
#if	(NET)
	[ParenthesizePropertyName(true)]
	[TypeConverter(typeof(Design.FbParameterConverter))]
#endif
	public sealed class FbParameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
	{
		#region Fields

		private FbParameterCollection parent;
		private FbDbType			fbType;
		private ParameterDirection	direction;
		private DataRowVersion		sourceVersion;
		private FbCharset			charset;
		private bool				isNullable;
		private string				parameterName;
		private string				sourceColumn;
		private object				value;
		private byte				precision;
		private byte				scale;
		private int					size;
		private bool				inferType;

		#endregion

		#region Properties

		string IDataParameter.ParameterName
		{
			get { return this.ParameterName; }
			set { this.ParameterName = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="ParameterName"]/*'/>
#if	(!NETCF)
		[DefaultValue("")]
#endif
		public string ParameterName
		{
			get { return this.parameterName; }
			set { this.parameterName = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="Precision"]/*'/>
#if	(!NETCF)
		[Category("Data"), DefaultValue((byte)0)]
#endif
		public byte Precision
		{
			get { return this.precision; }
			set { this.precision = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="Scale"]/*'/>
#if	(!NETCF)
		[Category("Data"), DefaultValue((byte)0)]
#endif
		public byte Scale
		{
			get { return this.scale; }
			set { this.scale = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="Size"]/*'/>
#if	(!NETCF)
		[Category("Data"), DefaultValue(0)]
#endif
		public int Size
		{
			get { return this.size; }
			set { this.size = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="DbType"]/*'/>
#if	(!NETCF)
		[Browsable(false), Category("Data"), RefreshProperties(RefreshProperties.All),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
		public DbType DbType
		{
			get { return TypeHelper.GetDbType((DbDataType)this.fbType); }
			set { this.fbType = (FbDbType)TypeHelper.GetDbDataType(value); }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="FbDbType"]/*'/>
#if	(!NETCF)
		[RefreshProperties(RefreshProperties.All), Category("Data"), DefaultValue(FbDbType.VarChar)]
#endif
		public FbDbType FbDbType
		{
			get { return this.fbType; }
			set
			{
				this.fbType		= value;
				this.inferType	= false;
			}
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="Direction"]/*'/>
#if	(!NETCF)
		[Category("Data"), DefaultValue(ParameterDirection.Input)]
#endif
		public ParameterDirection Direction
		{
			get { return this.direction; }
			set { this.direction = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="IsNullable"]/*'/>
#if	(!NETCF)
		[Browsable(false), DesignOnly(true), DefaultValue(false), EditorBrowsable(EditorBrowsableState.Advanced)]
#endif
		public Boolean IsNullable
		{
			get { return this.isNullable; }
			set { this.isNullable = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="SourceColumn"]/*'/>
#if	(!NETCF)
		[Category("Data"), DefaultValue("")]
#endif
		public string SourceColumn
		{
			get { return this.sourceColumn; }
			set { this.sourceColumn = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="SourceVersion"]/*'/>
#if	(!NETCF)
		[Category("Data"), DefaultValue(DataRowVersion.Current)]
#endif
		public DataRowVersion SourceVersion
		{
			get { return this.sourceVersion; }
			set { this.sourceVersion = value; }
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="Value"]/*'/>
#if	(!NETCF)
		[Category("Data"), TypeConverter(typeof(StringConverter)), DefaultValue(null)]
#endif
		public object Value
		{
			get { return this.value; }
			set
			{
				if (value == null)
				{
					value = System.DBNull.Value;
				}

				if (this.FbDbType == FbDbType.Guid && value != null 
					&& value != DBNull.Value &&
					!(value is Guid) && !(value is byte[]))
				{
					throw new InvalidOperationException("Incorrect Guid value.");
				}

				this.value = value;

				if (this.inferType)
				{
					this.SetFbDbType(value);
				}
			}
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/property[@name="Charset"]/*'/>
#if	(!NETCF)
		[Category("Data"), DefaultValue(FbCharset.Default)]
#endif
		public FbCharset Charset
		{
			get { return this.charset; }
			set { this.charset = value; }
		}

		#endregion

		#region Internal Properties

		internal FbParameterCollection Parent
		{
			get { return this.parent; }
			set { this.parent = value; }
		}

		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/constrctor[@name="ctor"]/*'/>
		public FbParameter()
		{
			this.fbType			= FbDbType.VarChar;
			this.direction		= ParameterDirection.Input;
			this.sourceVersion	= DataRowVersion.Current;
			this.sourceColumn	= String.Empty;
			this.parameterName	= String.Empty;
			this.inferType		= true;
			this.charset		= FbCharset.Default;
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/constrctor[@name="ctor(System.String,System.Object)"]/*'/>
		public FbParameter(string parameterName, object value) : this()
		{
			this.parameterName	= parameterName;
			this.Value			= value;
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/constrctor[@name="ctor(System.String,FbDbType)"]/*'/>
		public FbParameter(string parameterName, FbDbType fbType) : this()
		{
			this.inferType		= false;
			this.parameterName	= parameterName;
			this.fbType			= fbType;
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/constrctor[@name="ctor(System.String,FbDbType,System.Int32)"]/*'/>
		public FbParameter(string parameterName, FbDbType fbType, int size) : this()
		{
			this.inferType		= false;
			this.parameterName	= parameterName;
			this.fbType			= fbType;
			this.size			= size;
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/constrctor[@name="ctor(System.String,FbDbType,System.Int32,System.String)"]/*'/>
		public FbParameter(string parameterName, FbDbType fbType, int size, string sourceColumn)
			: this()
		{
			this.inferType		= false;
			this.parameterName	= parameterName;
			this.fbType			= fbType;
			this.size			= size;
			this.sourceColumn	= sourceColumn;
		}

		/// <include file='Doc/en_EN/FbParameter.xml' path='doc/class[@name="FbParameter"]/constrctor[@name="ctor(System.String,FbDbType,System.Int32,System.Data.ParameterDirection,System.Boolean,System.Byte,System.Byte,System.String,System.Data.DataRowVersion,System.Object)"]/*'/>
#if	(!NETCF)
		[EditorBrowsable(EditorBrowsableState.Advanced)]
#endif
		public FbParameter(
			string				parameterName, 
			FbDbType			dbType, 
			int					size,
			ParameterDirection	direction,
			bool				isNullable,
			byte				precision,
			byte				scale,
			string				sourceColumn,
			DataRowVersion		sourceVersion,
			object				value)
		{
			this.inferType		= false;
			this.parameterName	= parameterName;
			this.fbType			= dbType;
			this.size			= size;
			this.direction		= direction;
			this.isNullable		= isNullable;
			this.precision		= precision;
			this.scale			= scale;
			this.sourceColumn	= sourceColumn;
			this.sourceVersion	= sourceVersion;
			this.value			= value;
		}

		#endregion

		#region ICloneable Methods

		object ICloneable.Clone()
		{
			FbParameter p = new FbParameter(
				this.parameterName,
				this.fbType,
				this.size,
				this.direction,
				this.isNullable,
				this.precision,
				this.scale,
				this.sourceColumn,
				this.sourceVersion,
				this.value);

			// Set extra properties
			p.Charset = this.charset;

			return p;
		}

		#endregion

		#region Methods

		/// <include file='Doc/en_EN/FbCommand.xml'	path='doc/class[@name="FbParameter"]/method[@name="ToString"]/*'/>
		public override string ToString()
		{
			return this.parameterName;
		}

		#endregion

		#region Private	Methods

		private void SetFbDbType(object value)
		{
			if (value == null)
			{
				value = System.DBNull.Value;
			}

			TypeCode code = Type.GetTypeCode(value.GetType());

			switch (code)
			{
				case TypeCode.Object:
					this.fbType = FbDbType.Binary;
					break;

				case TypeCode.Char:
					this.fbType = FbDbType.Char;
					break;

				case TypeCode.DBNull:
				case TypeCode.String:
					this.fbType = FbDbType.VarChar;
					break;

				case TypeCode.Boolean:
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
					this.fbType = FbDbType.SmallInt;
					break;

				case TypeCode.Int32:
				case TypeCode.UInt32:
					this.fbType = FbDbType.Integer;
					break;

				case TypeCode.Int64:
				case TypeCode.UInt64:
					this.fbType = FbDbType.BigInt;
					break;

				case TypeCode.Single:
					this.fbType = FbDbType.Float;
					break;

				case TypeCode.Double:
					this.fbType = FbDbType.Double;
					break;

				case TypeCode.Decimal:
					this.fbType = FbDbType.Decimal;
					break;

				case TypeCode.DateTime:
					this.fbType = FbDbType.TimeStamp;
					break;

				case TypeCode.Empty:
				default:
					if (value is Guid)
					{
						this.fbType = FbDbType.Guid;
					}
					else
					{
						throw new SystemException("Value is of unknown data type");
					}
					break;
			}
		}

		#endregion
	}
}