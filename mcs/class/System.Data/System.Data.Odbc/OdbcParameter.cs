//
// System.Data.Odbc.OdbcParameter
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
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
using System.ComponentModel;

namespace System.Data.Odbc
{
	[TypeConverterAttribute (typeof (OdbcParameterConverter))]	
	public sealed class OdbcParameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
	{
		#region Fields

		string name;
		object ParamValue;
		int size;
		bool isNullable;
		byte precision;
		byte scale;
		DataRowVersion sourceVersion;
		string sourceColumn;
		ParameterDirection direction;
		OdbcType odbcType;
		DbType dbType;
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
			name = String.Empty;
			ParamValue = null;
			size = 0;
			isNullable = true;
			precision = 0;
			scale = 0;
			sourceColumn = String.Empty;
		}

		public OdbcParameter (string name, object value) 
			: this ()
		{
			this.name = name;
			this.ParamValue = value;
		}

		public OdbcParameter (string name, OdbcType dataType) 
			: this ()
		{
			this.name = name;
			OdbcType = dataType;
		}

		public OdbcParameter (string name, OdbcType dataType, int size)
			: this (name, dataType)
		{
			this.size = size;
		}

		public OdbcParameter (string name, OdbcType dataType, int size, string srcColumn)
			: this (name, dataType, size)
		{
			this.sourceColumn = srcColumn;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public OdbcParameter(string name, OdbcType dataType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string srcColumn, DataRowVersion srcVersion, object value)
			: this (name, dataType, size, srcColumn)
		{
			this.direction = direction;
			this.isNullable = isNullable;
			this.precision = precision;
			this.scale = scale;
			this.sourceVersion = srcVersion;
			this.ParamValue = value;
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
		public DbType DbType {
			get { return dbType; }
			set { 
				dbType = value;
			}
		}
		
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
				return ParamValue;
			}
			set { 
				this.ParamValue = value;
				bufferIsSet = false;
			}
		}

		#endregion // Properties

		#region Methods

		internal void Bind(IntPtr hstmt, int ParamNum) {
			OdbcReturn ret;
			// Set up the buffer if we haven't done so yet
			if (!bufferIsSet)
				setBuffer();

			// Convert System.Data.ParameterDirection into odbc enum
			OdbcInputOutputDirection paramdir = libodbc.ConvertParameterDirection(this.direction);
			// Bind parameter based on type
			if (odbcType == OdbcType.Int)
				ret = libodbc.SQLBindParameter(hstmt, (ushort)ParamNum, (short)paramdir,
					(short)odbcType, (short)odbcType, Convert.ToUInt32(size),
					0, ref intbuf, 0, 0);
			else
				ret = libodbc.SQLBindParameter(hstmt, (ushort)ParamNum, (short)paramdir,
					(short)OdbcType.Char, (short)odbcType, Convert.ToUInt32(size),
					0, buffer, 0, 0);
			// Check for error condition
			if ((ret != OdbcReturn.Success) && (ret != OdbcReturn.SuccessWithInfo))
				throw new OdbcException(new OdbcError("SQLBindParam", OdbcHandleType.Stmt, hstmt));
		}

		private void setBuffer() {
			// Load buffer with new value
			if (odbcType == OdbcType.Int)
				intbuf = (int)ParamValue;
			else {
				string paramValueString = ParamValue.ToString();
				// Treat everything else as a string
				// Init string buffer
				if (ParamValue is String)
                                        paramValueString = "\'"+paramValueString+"\'";
                                
				if (buffer == null || buffer.Length < ((size > 20) ? size : 20))
					buffer = new byte[(size > 20) ? size : 20];
				else
					buffer.Initialize();
				// Convert value into string and store into buffer
				System.Text.Encoding.ASCII.GetBytes(paramValueString, 0, paramValueString.Length, buffer, 0);
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
		#endregion
	}
}
