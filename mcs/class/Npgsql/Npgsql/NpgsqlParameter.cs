// created on 18/5/2002 at 01:25

// Npgsql.NpgsqlParameter.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA


using System;
using System.Data;
using System.ComponentModel;
using NpgsqlTypes;
using Npgsql.Design;


namespace Npgsql
{
    ///<summary>
    /// This class represents a parameter to a command that will be sent to server
    ///</summary>
    [TypeConverter(typeof(NpgsqlParameterConverter))]
    public sealed class NpgsqlParameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
    {

        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlParameter";

        // Fields to implement IDbDataParameter interface.
        private byte 				    precision = 0;
        private byte 				    scale = 0;
        private Int32				    size = 0;

        // Fields to implement IDataParameter
        //private NpgsqlDbType				    npgsqldb_type = NpgsqlDbType.Text;
        //private DbType                    db_type = DbType.String;
        private NpgsqlNativeTypeInfo	type_info;
        private ParameterDirection	    direction = ParameterDirection.Input;
        private Boolean				    is_nullable = false;
        private String				    name;
        private String				    source_column = String.Empty;
        private DataRowVersion		    source_version = DataRowVersion.Current;
        private Object				    value = DBNull.Value;
        private System.Resources.ResourceManager resman;

        /// <summary>

        /// Initializes a new instance of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> class.
        /// </summary>
        public NpgsqlParameter()
        {
            resman = new System.Resources.ResourceManager(this.GetType());
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>
        /// class with the parameter name and a value of the new <b>NpgsqlParameter</b>.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="value">An <see cref="System.Object">Object</see> that is the value of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>.</param>
        /// <remarks>
        /// <p>When you specify an <see cref="System.Object">Object</see>
        /// in the value parameter, the <see cref="System.Data.DbType">DbType</see> is
        /// inferred from the .NET Framework type of the <b>Object</b>.</p>
        /// <p>When using this constructor, you must be aware of a possible misuse of the constructor which takes a DbType parameter.
        /// This happens when calling this constructor passing an int 0 and the compiler thinks you are passing a value of DbType.
        /// Use <code> Convert.ToInt32(value) </code> for example to have compiler calling the correct constructor.</p>
        /// </remarks>
        public NpgsqlParameter(String parameterName, object value)
        {
            resman = new System.Resources.ResourceManager(this.GetType());
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME, parameterName, value);

            this.ParameterName = parameterName;
            this.value = value;

            if ((this.value == null) || (this.value == DBNull.Value) )
            {
                // don't really know what to do - leave default and do further exploration
                // Default type for null values is String.
                this.value = DBNull.Value;
                type_info = NpgsqlTypesHelper.GetNativeTypeInfo(typeof(String));
                return;
            }
            else
            {
                type_info = NpgsqlTypesHelper.GetNativeTypeInfo(value.GetType());
                if (type_info == null)
                {
                    throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), value.GetType()));
                }

            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>
        /// class with the parameter name and the data type.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        public NpgsqlParameter(String parameterName, NpgsqlDbType parameterType) : this(parameterName, parameterType, 0, String.Empty)
        {}


        public NpgsqlParameter(String parameterName, DbType parameterType) : this(parameterName, NpgsqlTypesHelper.GetNativeTypeInfo(parameterType).NpgsqlDbType, 0, String.Empty)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>
        /// class with the parameter name, the <see cref="System.Data.DbType">DbType</see>, and the size.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        /// <param name="size">The length of the parameter.</param>
        public NpgsqlParameter(String parameterName, NpgsqlDbType parameterType, Int32 size) : this(parameterName, parameterType, size, String.Empty)
        {}

        public NpgsqlParameter(String parameterName, DbType parameterType, Int32 size) : this(parameterName, NpgsqlTypesHelper.GetNativeTypeInfo(parameterType).NpgsqlDbType, size, String.Empty)
        {}


        /// <summary>
        /// Initializes a new instance of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>
        /// class with the parameter name, the <see cref="System.Data.DbType">DbType</see>, the size,
        /// and the source column name.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        /// <param name="size">The length of the parameter.</param>
        /// <param name="sourceColumn">The name of the source column.</param>
        public NpgsqlParameter(String parameterName, NpgsqlDbType parameterType, Int32 size, String sourceColumn)
        {

            resman = new System.Resources.ResourceManager(this.GetType());

            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME, parameterName, parameterType, size, source_column);

            this.ParameterName = parameterName;

            type_info = NpgsqlTypesHelper.GetNativeTypeInfo(parameterType);
            if (type_info == null)
                throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), parameterType));

            this.size = size;
            source_column = sourceColumn;


        }

        public NpgsqlParameter(String parameterName, DbType parameterType, Int32 size, String sourceColumn) : this(parameterName, NpgsqlTypesHelper.GetNativeTypeInfo(parameterType).NpgsqlDbType, size, sourceColumn)
    {}



        /// <summary>
        /// Initializes a new instance of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>
        /// class with the parameter name, the <see cref="System.Data.DbType">DbType</see>, the size,
        /// the source column name, a <see cref="System.Data.ParameterDirection">ParameterDirection</see>,
        /// the precision of the parameter, the scale of the parameter, a
        /// <see cref="System.Data.DataRowVersion">DataRowVersion</see> to use, and the
        /// value of the parameter.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <param name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        /// <param name="size">The length of the parameter.</param>
        /// <param name="sourceColumn">The name of the source column.</param>
        /// <param name="direction">One of the <see cref="System.Data.ParameterDirection">ParameterDirection</see> values.</param>
        /// <param name="isNullable"><b>true</b> if the value of the field can be null, otherwise <b>false</b>.</param>
        /// <param name="precision">The total number of digits to the left and right of the decimal point to which
        /// <see cref="Npgsql.NpgsqlParameter.Value">Value</see> is resolved.</param>
        /// <param name="scale">The total number of decimal places to which
        /// <see cref="Npgsql.NpgsqlParameter.Value">Value</see> is resolved.</param>
        /// <param name="sourceVersion">One of the <see cref="System.Data.DataRowVersion">DataRowVersion</see> values.</param>
        /// <param name="value">An <see cref="System.Object">Object</see> that is the value
        /// of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>.</param>
        public NpgsqlParameter (String parameterName, NpgsqlDbType parameterType, Int32 size, String sourceColumn, ParameterDirection direction, bool isNullable, byte precision, byte scale, DataRowVersion sourceVersion, object value)
        {

            resman = new System.Resources.ResourceManager(this.GetType());

            this.ParameterName = parameterName;
            this.Size = size;
            this.SourceColumn = sourceColumn;
            this.Direction = direction;
            this.IsNullable = isNullable;
            this.Precision = precision;
            this.Scale = scale;
            this.SourceVersion = sourceVersion;
            this.Value = value;

            if (this.value == null)
            {
                this.value = DBNull.Value;
                type_info = NpgsqlTypesHelper.GetNativeTypeInfo(typeof(String));
            }
            else
            {
                type_info = NpgsqlTypesHelper.GetNativeTypeInfo(parameterType);
                if (type_info == null)
                    throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), parameterType));
            }

        }

        public NpgsqlParameter (String parameterName, DbType parameterType, Int32 size, String sourceColumn, ParameterDirection direction, bool isNullable, byte precision, byte scale, DataRowVersion sourceVersion, object value) : this(parameterName, NpgsqlTypesHelper.GetNativeTypeInfo(parameterType).NpgsqlDbType, size, sourceColumn, direction, isNullable, precision, scale, sourceVersion, value)
    {}

        // Implementation of IDbDataParameter
        /// <summary>
        /// Gets or sets the maximum number of digits used to represent the
        /// <see cref="Npgsql.NpgsqlParameter.Value">Value</see> property.
        /// </summary>
        /// <value>The maximum number of digits used to represent the
        /// <see cref="Npgsql.NpgsqlParameter.Value">Value</see> property.
        /// The default value is 0, which indicates that the data provider
        /// sets the precision for <b>Value</b>.</value>
        [Category("Data"), DefaultValue((Byte)0)]
        public Byte Precision
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Precision");
                return precision;
            }

            set
            {
                NpgsqlEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "Precision", value);
                precision = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of decimal places to which
        /// <see cref="Npgsql.NpgsqlParameter.Value">Value</see> is resolved.
        /// </summary>
        /// <value>The number of decimal places to which
        /// <see cref="Npgsql.NpgsqlParameter.Value">Value</see> is resolved. The default is 0.</value>
        [Category("Data"), DefaultValue((Byte)0)]
        public Byte Scale
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Scale");
                return scale;
            }

            set
            {
                NpgsqlEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "Scale", value);
                scale = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum size, in bytes, of the data within the column.
        /// </summary>
        /// <value>The maximum size, in bytes, of the data within the column.
        /// The default value is inferred from the parameter value.</value>
        [Category("Data"), DefaultValue(0)]
        public Int32 Size
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Size");
                return size;
            }

            set
            {
                NpgsqlEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "Size", value);
                size = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Data.DbType">DbType</see> of the parameter.
        /// </summary>
        /// <value>One of the <see cref="System.Data.DbType">DbType</see> values. The default is <b>String</b>.</value>
        [Category("Data"), RefreshProperties(RefreshProperties.All), DefaultValue(DbType.String)]
        public DbType DbType
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "DbType");
                return TypeInfo.DbType;
            }

            // [TODO] Validate data type.
            set
            {
                NpgsqlEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "DbType", value);
                type_info = NpgsqlTypesHelper.GetNativeTypeInfo(value);
                if (type_info == null)
                    throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), value));

            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Data.DbType">DbType</see> of the parameter.
        /// </summary>
        /// <value>One of the <see cref="System.Data.DbType">DbType</see> values. The default is <b>String</b>.</value>
        [Category("Data"), RefreshProperties(RefreshProperties.All), DefaultValue(NpgsqlDbType.Text)]
        public NpgsqlDbType NpgsqlDbType
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "DbType");

                return TypeInfo.NpgsqlDbType;
            }

            // [TODO] Validate data type.
            set
            {
                NpgsqlEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "DbType", value);
                type_info = NpgsqlTypesHelper.GetNativeTypeInfo(value);
                if (type_info == null)
                    throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), value));

            }
        }



        internal NpgsqlNativeTypeInfo TypeInfo
        {
            get
            {
                return type_info;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is input-only,
        /// output-only, bidirectional, or a stored procedure return value parameter.
        /// </summary>
        /// <value>One of the <see cref="System.Data.ParameterDirection">ParameterDirection</see>
        /// values. The default is <b>Input</b>.</value>
        [Category("Data"), DefaultValue(ParameterDirection.Input)]
        public ParameterDirection Direction
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Normal, CLASSNAME, "Direction");
                return direction;
            }

            set
            {
                NpgsqlEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "Direction", value);
                direction = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter accepts null values.
        /// </summary>
        /// <value><b>true</b> if null values are accepted; otherwise, <b>false</b>. The default is <b>false</b>.</value>
        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false), DefaultValue(false), DesignOnly(true)]
        public Boolean IsNullable
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "IsNullable");
                return is_nullable;
            }

            set
            {
                NpgsqlEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "IsNullable", value);
                is_nullable = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>.
        /// </summary>
        /// <value>The name of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>.
        /// The default is an empty string.</value>
        [DefaultValue("")]
        public String ParameterName
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Normal, CLASSNAME, "ParameterName");
                return name;
            }

            set
            {
                name = value;
                if ( (name.Equals(String.Empty)) || ((name[0] != ':') && (name[0] != '@')) )
                    name = ':' + name;

                NpgsqlEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "ParameterName", value);
            }
        }

        /// <summary>
        /// Gets or sets the name of the source column that is mapped to the
        /// <see cref="System.Data.DataSet">DataSet</see> and used for loading or
        /// returning the <see cref="Npgsql.NpgsqlParameter.Value">Value</see>.
        /// </summary>
        /// <value>The name of the source column that is mapped to the
        /// <see cref="System.Data.DataSet">DataSet</see>. The default is an empty string.</value>
        [Category("Data"), DefaultValue("")]
        public String SourceColumn
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Normal, CLASSNAME, "SourceColumn");
                return source_column;
            }

            set
            {
                NpgsqlEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "SourceColumn", value);
                source_column = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Data.DataRowVersion">DataRowVersion</see>
        /// to use when loading <see cref="Npgsql.NpgsqlParameter.Value">Value</see>.
        /// </summary>
        /// <value>One of the <see cref="System.Data.DataRowVersion">DataRowVersion</see> values.
        /// The default is <b>Current</b>.</value>
        [Category("Data"), DefaultValue(DataRowVersion.Current)]
        public DataRowVersion SourceVersion
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Normal, CLASSNAME, "SourceVersion");
                return source_version;
            }

            set
            {
                NpgsqlEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "SourceVersion", value);
                source_version = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        /// <value>An <see cref="System.Object">Object</see> that is the value of the parameter.
        /// The default value is null.</value>
        [TypeConverter(typeof(StringConverter)), Category("Data")]
        public Object Value
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Normal, CLASSNAME, "Value");
                return value;
            }

            // [TODO] Check and validate data type.
            set
            {
                NpgsqlEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "Value", value);

                this.value = value;
                if ((this.value == null) || (this.value == DBNull.Value) )
                {
                    // don't really know what to do - leave default and do further exploration
                    // Default type for null values is String.
                    this.value = DBNull.Value;
                    if (type_info == null)
                        type_info = NpgsqlTypesHelper.GetNativeTypeInfo(typeof(String));

                }
                else
                {
                    if (type_info == null)
                    {
                        type_info = NpgsqlTypesHelper.GetNativeTypeInfo(value.GetType());
                        if (type_info == null)
                        	throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), value.GetType()));
                        
                    }

                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> that
        /// is a copy of the current instance.
        /// </summary>
        /// <returns>A new <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> that is a copy of this instance.</returns>
        object System.ICloneable.Clone()
        {
            return new NpgsqlParameter(this.ParameterName, this.NpgsqlDbType,	this.Size, this.SourceColumn, this.Direction, this.IsNullable, this.Precision, this.Scale, this.SourceVersion, this.Value);
        }


    }
}
