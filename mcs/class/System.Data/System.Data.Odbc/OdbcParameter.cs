//
// System.Data.Odbc.OdbcParameter
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//   Sureshkumar T <tsureshkumar@novell.com>  2004.
//
// Copyright (C) Brian Ritchie, 2002
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
using System.Data;
using System.Data.Common;
#if NET_2_0
using System.Data.ProviderBase;
#endif // NET_2_0
using System.ComponentModel;

namespace System.Data.Odbc
{
	[TypeConverterAttribute (typeof (OdbcParameterConverter))]
#if NET_2_0
        public sealed class OdbcParameter : DbParameterBase, ICloneable
#else
	public sealed class OdbcParameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
#endif // NET_2_0
	{
		#region Fields

#if ONLY_1_1
		string name;
		ParameterDirection direction;
		bool isNullable;
		int size;
		byte precision;
		byte scale;
		object paramValue;
		DataRowVersion sourceVersion;
		string sourceColumn;
#endif // ONLY_1_1
		OdbcType odbcType = OdbcType.NVarChar;
		DbType dbType = DbType.String;
		OdbcParameterCollection container = null;	
		
		// Buffers for parameter value based on type. Currently I've only optimized 
		// for int parameters and everything else is just converted to a string.
		private bool bufferIsSet;
		int intbuf;
		byte[] buffer;

		#endregion

		#region Constructors
		
		public OdbcParameter ()
		{
			ParameterName = String.Empty;
			Value = null;
			Size = 0;
			IsNullable = true;
			Precision = 0;
			Scale = 0;
			SourceColumn = String.Empty;
		}

		public OdbcParameter (string name, object value) 
			: this ()
		{
			this.ParameterName = name;
			this.Value = value;
                        
                        if (value != null && !value.GetType ().IsValueType) {
                                Type type = value.GetType ();
                                if (type.IsArray)
                                        Size = type.GetElementType () == typeof (byte) ? 
                                                ((Array) value).Length : 0;
                                else
                                        Size = value.ToString ().Length;
                        }


		}

		public OdbcParameter (string name, OdbcType dataType) 
			: this ()
		{
			this.ParameterName = name;
			OdbcType = dataType;
		}

		public OdbcParameter (string name, OdbcType dataType, int size)
			: this (name, dataType)
		{
			this.Size = size;
		}

		public OdbcParameter (string name, OdbcType dataType, int size, string srcColumn)
			: this (name, dataType, size)
		{
			this.SourceColumn = srcColumn;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public OdbcParameter(string name, OdbcType dataType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string srcColumn, DataRowVersion srcVersion, object value)
			: this (name, dataType, size, srcColumn)
		{
			this.Direction = direction;
			this.IsNullable = isNullable;
			this.Precision = precision;
			this.Scale = scale;
			this.SourceVersion = srcVersion;
			this.Value = value;
		}

		#endregion

		#region Properties

		// Used to ensure that only one collection can contain this
                // parameter
                internal OdbcParameterCollection Container {
                        get { return container; }
                        set { container = value; }
                }
		
		[BrowsableAttribute (false)]
                [OdbcDescriptionAttribute ("The parameter generic type")]
                [RefreshPropertiesAttribute (RefreshProperties.All)]
                [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		[OdbcCategory ("Data")]
		public 
#if NET_2_0
                override 
#endif // NET_2_0
                DbType DbType {
			get { return dbType; }
			set { 
				dbType = value;
			}
		}
		
#if ONLY_1_1
		[OdbcCategory ("Data")]
		[OdbcDescriptionAttribute ("Input, output, or bidirectional parameter")]  
		[DefaultValue (ParameterDirection.Input)]
		public ParameterDirection Direction {
			get { return direction; }
			set { direction = value; }
		}
		
		[BrowsableAttribute (false)]
                [OdbcDescriptionAttribute ("A design-time property used for strongly typed code generation")]
                [DesignOnlyAttribute (true)]
                [EditorBrowsableAttribute (EditorBrowsableState.Advanced)]
                [DefaultValue (false)]
		public bool IsNullable {
			get { return isNullable; }
			set { isNullable = value; }
		}
#endif // ONLY_1_1


		[DefaultValue (OdbcType.NChar)]
                [OdbcDescriptionAttribute ("The parameter native type")]
                [RefreshPropertiesAttribute (RefreshProperties.All)]
		[OdbcCategory ("Data")]
		public OdbcType OdbcType {
			get { return odbcType; }
			set {
				odbcType = value;
			}
		}
		
#if ONLY_1_1
 		[OdbcDescription ("DataParameter_ParameterName")]
                [DefaultValue ("")]	
		public string ParameterName {
			get { return name; }
			set { name = value; }
		}

		[OdbcDescription ("DbDataParameter_Precision")]
                [OdbcCategory ("DataCategory_Data")]
                [DefaultValue (0)]
		public byte Precision {
			get { return precision; }
			set { precision = value; }
		}
		
                [OdbcDescription ("DbDataParameter_Scale")]
                [OdbcCategory ("DataCategory_Data")]
                [DefaultValue (0)]
		public byte Scale {
			get { return scale; }
			set { scale = value; }
		}
		
		[OdbcDescription ("DbDataParameter_Size")]
                [OdbcCategory ("DataCategory_Data")]
                [DefaultValue (0)]
		public int Size {
			get { return size; }
			set { size = value; }
		}

		[OdbcDescription ("DataParameter_SourceColumn")]
                [OdbcCategory ("DataCategory_Data")]
                [DefaultValue ("")]
		public string SourceColumn {
			get { return sourceColumn; }
			set { sourceColumn = value; }
		}
		
                [OdbcDescription ("DataParameter_SourceVersion")]
                [OdbcCategory ("DataCategory_Data")]
                [DefaultValue (512)]			
		public DataRowVersion SourceVersion {
			get { return sourceVersion; }
			set { sourceVersion = value; }
		}

		[TypeConverter (typeof(StringConverter))]
                [OdbcDescription ("DataParameter_Value")]
                [OdbcCategory ("DataCategory_Data")]
                [DefaultValue (null)]		
		public object Value {
			get { 
				return paramValue;
			}
			set { 
				paramValue = value;
				bufferIsSet = false;
			}
		}
#endif // ONLY_1_1

#if NET_2_0
		[TypeConverter (typeof(StringConverter))]
                [OdbcDescription ("DataParameter_Value")]
                [OdbcCategory ("DataCategory_Data")]
                [DefaultValue (null)]		
		public override object Value {
			get { 
				return base.Value;
			}
			set { 
				base.Value = value;
				bufferIsSet = false;
			}
		}

#endif // NET_2_0


		#endregion // Properties

		#region Methods

		internal void Bind(IntPtr hstmt, int ParamNum) {
			OdbcReturn ret;
			// Set up the buffer if we haven't done so yet
			if (!bufferIsSet)
				setBuffer();

			// Convert System.Data.ParameterDirection into odbc enum
			OdbcInputOutputDirection paramdir = libodbc.ConvertParameterDirection(this.Direction);

                        SQL_C_TYPE ctype = OdbcTypeConverter.ConvertToSqlCType (odbcType);
                        SQL_TYPE   sqltype = OdbcTypeConverter.ConvertToSqlType (odbcType);
                        
			// Bind parameter based on type
			if (odbcType == OdbcType.Int)
                                ret = libodbc.SQLBindParameter(hstmt, (ushort)ParamNum, (short)paramdir,
                                                               ctype, sqltype, Convert.ToUInt32(Size),
                                                               0, ref intbuf, 0, 0);
			else
                                ret = libodbc.SQLBindParameter(hstmt, (ushort)ParamNum, (short)paramdir,
                                                               ctype, sqltype, Convert.ToUInt32(Size),
                                                               0, buffer, 0, 0);

                                
			// Check for error condition
			if ((ret != OdbcReturn.Success) && (ret != OdbcReturn.SuccessWithInfo))
				throw new OdbcException(new OdbcError("SQLBindParam", OdbcHandleType.Stmt, hstmt));
		}

		private void setBuffer() {
			// Load buffer with new value
			if (odbcType == OdbcType.Int)
                                intbuf = Value == null ? new int () : (int) Value;
                        else {
				string paramValueString = Value.ToString();
				// Treat everything else as a string
				// Init string buffer
				 if (Value is String)
                                        paramValueString = "\'"+paramValueString+"\'";

                                 int minSize = Size;
                                 minSize = Size > 20 ? Size : 20;
                                 if (Value is String)
                                         minSize += 2; // for enclosing apos
				 if (buffer == null || buffer.Length < minSize)
                                         buffer = new byte[minSize];
                                 else
                                         buffer.Initialize();
                                 
                                 // Convert value into string and store into buffer
                                 minSize = paramValueString.Length < minSize ? paramValueString.Length : minSize;
                                 System.Text.Encoding.ASCII.GetBytes(paramValueString, 0, minSize, buffer, 0);
			}
			bufferIsSet = true;
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			return ParameterName;
		}

#if NET_2_0
                [MonoTODO]
                public override void PropertyChanging () 
                {
                        throw new NotImplementedException ();
                }
                
                [MonoTODO]
		protected override byte ValuePrecision (object value) 
                {
                        throw new NotImplementedException ();
                }

                [MonoTODO]
                protected override byte ValueScale (object value)
                {
                        throw new NotImplementedException ();
                }

                [MonoTODO]
		protected override int ValueSize (object value)
                {
                        throw new NotImplementedException ();
                }

                [MonoTODO]
                public override void ResetDbType () 
                {
                        throw new NotImplementedException ();
                }

#endif // NET_2_0
		#endregion
	}
}
