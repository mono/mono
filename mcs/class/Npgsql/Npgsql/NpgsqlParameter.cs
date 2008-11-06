// created on 18/5/2002 at 01:25

// Npgsql.NpgsqlParameter.cs
//
// Author:
//    Francisco Jr. (fxjrlists@yahoo.com.br)
//
//    Copyright (C) 2002 The Npgsql Development Team
//    npgsql-general@gborg.postgresql.org
//    http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.


using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Resources;
using NpgsqlTypes;

#if WITHDESIGN
using Npgsql.Design;
#endif

namespace Npgsql
{
    ///<summary>
    /// This class represents a parameter to a command that will be sent to server
    ///</summary>
#if WITHDESIGN
    [TypeConverter(typeof(NpgsqlParameterConverter))]
#endif

    public sealed class NpgsqlParameter : DbParameter, ICloneable
    {
        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlParameter";

        // Fields to implement IDbDataParameter interface.
        private byte precision = 0;
        private byte scale = 0;
        private Int32 size = 0;

        // Fields to implement IDataParameter
        //private NpgsqlDbType                    npgsqldb_type = NpgsqlDbType.Text;
        //private DbType                    db_type = DbType.String;
        private NpgsqlNativeTypeInfo type_info;
        private ParameterDirection direction = ParameterDirection.Input;
        private Boolean is_nullable = false;
        private String m_Name = String.Empty;
        private String source_column = String.Empty;
        private DataRowVersion source_version = DataRowVersion.Current;
        private Object value = DBNull.Value;
        private Boolean sourceColumnNullMapping;
        private readonly ResourceManager resman;

        private Boolean useCast = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> class.
        /// </summary>
        public NpgsqlParameter()
        {
            resman = new ResourceManager(this.GetType());
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);
            //type_info = NpgsqlTypesHelper.GetNativeTypeInfo(typeof(String));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>
        /// class with the parameter m_Name and a value of the new <b>NpgsqlParameter</b>.
        /// </summary>
        /// <param m_Name="parameterName">The m_Name of the parameter to map.</param>
        /// <param m_Name="value">An <see cref="System.Object">Object</see> that is the value of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>.</param>
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
            resman = new ResourceManager(this.GetType());
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME, parameterName, value);

            this.ParameterName = parameterName;
            this.value = value;

            if ((this.value == null) || (this.value == DBNull.Value))
            {
                // don't really know what to do - leave default and do further exploration
                // Default type for null values is String.
                this.value = DBNull.Value;
                type_info = NpgsqlTypesHelper.GetNativeTypeInfo(typeof(String));
            }
            else if (!NpgsqlTypesHelper.TryGetNativeTypeInfo(value.GetType(), out type_info))
            {
                throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), value.GetType()));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>
        /// class with the parameter m_Name and the data type.
        /// </summary>
        /// <param m_Name="parameterName">The m_Name of the parameter to map.</param>
        /// <param m_Name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        public NpgsqlParameter(String parameterName, NpgsqlDbType parameterType)
            : this(parameterName, parameterType, 0, String.Empty)
        {
        }


        public NpgsqlParameter(String parameterName, DbType parameterType)
            : this(parameterName, NpgsqlTypesHelper.GetNativeTypeInfo(parameterType).NpgsqlDbType, 0, String.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>
        /// class with the parameter m_Name, the <see cref="System.Data.DbType">DbType</see>, and the size.
        /// </summary>
        /// <param m_Name="parameterName">The m_Name of the parameter to map.</param>
        /// <param m_Name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        /// <param m_Name="size">The length of the parameter.</param>
        public NpgsqlParameter(String parameterName, NpgsqlDbType parameterType, Int32 size)
            : this(parameterName, parameterType, size, String.Empty)
        {
        }

        public NpgsqlParameter(String parameterName, DbType parameterType, Int32 size)
            : this(parameterName, NpgsqlTypesHelper.GetNativeTypeInfo(parameterType).NpgsqlDbType, size, String.Empty)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>
        /// class with the parameter m_Name, the <see cref="System.Data.DbType">DbType</see>, the size,
        /// and the source column m_Name.
        /// </summary>
        /// <param m_Name="parameterName">The m_Name of the parameter to map.</param>
        /// <param m_Name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        /// <param m_Name="size">The length of the parameter.</param>
        /// <param m_Name="sourceColumn">The m_Name of the source column.</param>
        public NpgsqlParameter(String parameterName, NpgsqlDbType parameterType, Int32 size, String sourceColumn)
        {
            resman = new ResourceManager(this.GetType());

            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME, parameterName, parameterType, size, source_column);

            this.ParameterName = parameterName;

            NpgsqlDbType = parameterType; //Allow the setter to catch any exceptions.

            this.size = size;
            source_column = sourceColumn;
        }

        public NpgsqlParameter(String parameterName, DbType parameterType, Int32 size, String sourceColumn)
            : this(parameterName, NpgsqlTypesHelper.GetNativeTypeInfo(parameterType).NpgsqlDbType, size, sourceColumn)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>
        /// class with the parameter m_Name, the <see cref="System.Data.DbType">DbType</see>, the size,
        /// the source column m_Name, a <see cref="System.Data.ParameterDirection">ParameterDirection</see>,
        /// the precision of the parameter, the scale of the parameter, a
        /// <see cref="System.Data.DataRowVersion">DataRowVersion</see> to use, and the
        /// value of the parameter.
        /// </summary>
        /// <param m_Name="parameterName">The m_Name of the parameter to map.</param>
        /// <param m_Name="parameterType">One of the <see cref="System.Data.DbType">DbType</see> values.</param>
        /// <param m_Name="size">The length of the parameter.</param>
        /// <param m_Name="sourceColumn">The m_Name of the source column.</param>
        /// <param m_Name="direction">One of the <see cref="System.Data.ParameterDirection">ParameterDirection</see> values.</param>
        /// <param m_Name="isNullable"><b>true</b> if the value of the field can be null, otherwise <b>false</b>.</param>
        /// <param m_Name="precision">The total number of digits to the left and right of the decimal point to which
        /// <see cref="Npgsql.NpgsqlParameter.Value">Value</see> is resolved.</param>
        /// <param m_Name="scale">The total number of decimal places to which
        /// <see cref="Npgsql.NpgsqlParameter.Value">Value</see> is resolved.</param>
        /// <param m_Name="sourceVersion">One of the <see cref="System.Data.DataRowVersion">DataRowVersion</see> values.</param>
        /// <param m_Name="value">An <see cref="System.Object">Object</see> that is the value
        /// of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>.</param>
        public NpgsqlParameter(String parameterName, NpgsqlDbType parameterType, Int32 size, String sourceColumn,
                               ParameterDirection direction, bool isNullable, byte precision, byte scale,
                               DataRowVersion sourceVersion, object value)
        {
            resman = new ResourceManager(this.GetType());

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
                NpgsqlDbType = parameterType; //allow the setter to catch exceptions if necessary.
            }
        }

        public NpgsqlParameter(String parameterName, DbType parameterType, Int32 size, String sourceColumn,
                               ParameterDirection direction, bool isNullable, byte precision, byte scale,
                               DataRowVersion sourceVersion, object value)
            : this(
                parameterName, NpgsqlTypesHelper.GetNativeTypeInfo(parameterType).NpgsqlDbType, size, sourceColumn, direction,
                isNullable, precision, scale, sourceVersion, value)
        {
        }

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
        
        
        public Boolean UseCast
        {
            get
            {
                //return useCast; //&& (value != DBNull.Value);
                // This check for Datetime.minvalue and maxvalue is needed in order to
                // workaround a problem when comparing date values with infinity.
                // This is a known issue with postgresql and it is reported here:
                // http://archives.postgresql.org/pgsql-general/2008-10/msg00535.php
                // Josh's solution to add cast is documented here:
                // http://pgfoundry.org/forum/message.php?msg_id=1004118
                
                return useCast || DateTime.MinValue.Equals(value) || DateTime.MinValue.Equals(value);
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
        public override Int32 Size
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
        public override DbType DbType
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "DbType");
                return TypeInfo.DbType;
            } // [TODO] Validate data type.
            set
            {
                
                NpgsqlEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "DbType", value);
                
                useCast = value != DbType.Object;
                
                if (!NpgsqlTypesHelper.TryGetNativeTypeInfo(value, out type_info))
                {
                    throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), value));
                }
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
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "NpgsqlDbType");

                return TypeInfo.NpgsqlDbType;
            } // [TODO] Validate data type.
            set
            {
                NpgsqlEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "NpgsqlDbType", value);
                useCast = true;
                if (value == NpgsqlDbType.Array)
                {
                    throw new ArgumentOutOfRangeException(resman.GetString("Exception_ParameterTypeIsOnlyArray"));
                }
                if (!NpgsqlTypesHelper.TryGetNativeTypeInfo(value, out type_info))
                {
                    throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), value));
                }
            }
        }


        internal NpgsqlNativeTypeInfo TypeInfo
        {
            get
            {
                if (type_info == null)
                {
                    type_info = NpgsqlTypesHelper.GetNativeTypeInfo(typeof(String));
                }
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
        public override ParameterDirection Direction
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

#if WITHDESIGN
        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false), DefaultValue(false), DesignOnly(true)]
#endif

        public override Boolean IsNullable
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
        /// Gets or sets the m_Name of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>.
        /// </summary>
        /// <value>The m_Name of the <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>.
        /// The default is an empty string.</value>
        [DefaultValue("")]
        public override String ParameterName
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Normal, CLASSNAME, "ParameterName");
                return m_Name;
            }

            set
            {
                m_Name = value;
                if (value == null)
                {
                    m_Name = String.Empty;
                }
                // no longer prefix with : so that the m_Name returned is the m_Name set

                m_Name = m_Name.Trim();

                NpgsqlEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "ParameterName", m_Name);
            }
        }

        /// <summary>
        /// The m_Name scrubbed of any optional marker
        /// </summary>
        internal string CleanName
        {
            get
            {
                string name = ParameterName;
                if (name[0] == ':' || name[0] == '@')
                {
                    return name.Length > 1 ? name.Substring(1) : string.Empty;
                }
                return name;

            }
        }

        /// <summary>
        /// Gets or sets the m_Name of the source column that is mapped to the
        /// <see cref="System.Data.DataSet">DataSet</see> and used for loading or
        /// returning the <see cref="Npgsql.NpgsqlParameter.Value">Value</see>.
        /// </summary>
        /// <value>The m_Name of the source column that is mapped to the
        /// <see cref="System.Data.DataSet">DataSet</see>. The default is an empty string.</value>
        [Category("Data"), DefaultValue("")]
        public override String SourceColumn
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
        public override DataRowVersion SourceVersion
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
        public override Object Value
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Normal, CLASSNAME, "Value");
                return value;
            } // [TODO] Check and validate data type.
            set
            {
                NpgsqlEventLog.LogPropertySet(LogLevel.Normal, CLASSNAME, "Value", value);

                this.value = value;
                if ((this.value == null) || (this.value == DBNull.Value))
                {
                    // don't really know what to do - leave default and do further exploration
                    // Default type for null values is String.
                    this.value = DBNull.Value;
                    if (type_info == null)
                    {
                        type_info = NpgsqlTypesHelper.GetNativeTypeInfo(typeof(String));
                    }
                }
                else if (type_info == null && !NpgsqlTypesHelper.TryGetNativeTypeInfo(value.GetType(), out type_info))
                {
                    throw new InvalidCastException(String.Format(resman.GetString("Exception_ImpossibleToCast"), value.GetType()));
                }
            }
        }

        public override void ResetDbType()
        {
            type_info = NpgsqlTypesHelper.GetNativeTypeInfo(typeof(String));
        }

        public override bool SourceColumnNullMapping
        {
            get { return sourceColumnNullMapping; }
            set { sourceColumnNullMapping = value; }
        }

        /// <summary>
        /// Creates a new <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> that
        /// is a copy of the current instance.
        /// </summary>
        /// <returns>A new <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see> that is a copy of this instance.</returns>
        object ICloneable.Clone()
        {
            // use fields instead of properties
            // to avoid auto-initializing something like type_info
            NpgsqlParameter clone = new NpgsqlParameter();
            clone.precision = precision;
            clone.scale = scale;
            clone.size = size;
            clone.type_info = type_info;
            clone.direction = direction;
            clone.is_nullable = is_nullable;
            clone.m_Name = m_Name;
            clone.source_column = source_column;
            clone.source_version = source_version;
            clone.value = value;
            clone.sourceColumnNullMapping = sourceColumnNullMapping;

            return clone;
        }
    }
}
