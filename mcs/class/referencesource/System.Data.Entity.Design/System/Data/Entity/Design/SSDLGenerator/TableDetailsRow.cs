//---------------------------------------------------------------------
// <copyright file="TableDetailsRow.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System.Collections.Generic;
using System.Xml;
using System.Data.Entity.Design.Common;
using System.Globalization;
using System.Data;

namespace System.Data.Entity.Design.SsdlGenerator
{
    /// <summary>
    /// Strongly typed DataTable for TableDetails
    /// </summary>
    internal sealed class TableDetailsRow : System.Data.DataRow
    {

        private TableDetailsCollection _tableTableDetails;

        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        internal TableDetailsRow(System.Data.DataRowBuilder rb)
            :
                base(rb)
        {
            this._tableTableDetails = ((TableDetailsCollection)(base.Table));
        }

        /// <summary>
        /// Gets a strongly typed table
        /// </summary>
        public new TableDetailsCollection Table
        {
            get
            {
                return _tableTableDetails;
            }
        }

        /// <summary>
        /// Gets the Catalog column value
        /// </summary>
        public string Catalog
        {
            get
            {
                try
                {
                    return ((string)(this[this._tableTableDetails.CatalogColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableTableDetails.CatalogColumn.ColumnName, _tableTableDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableTableDetails.CatalogColumn] = value;
            }
        }

        /// <summary>
        /// Gets the Schema column value
        /// </summary>
        public string Schema
        {
            get
            {
                try
                {
                    return ((string)(this[this._tableTableDetails.SchemaColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableTableDetails.SchemaColumn.ColumnName, _tableTableDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableTableDetails.SchemaColumn] = value;
            }
        }

        /// <summary>
        /// Gets the TableName column value
        /// </summary>
        public string TableName
        {
            get
            {
                try
                {
                    return ((string)(this[this._tableTableDetails.TableNameColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableTableDetails.TableNameColumn.ColumnName, _tableTableDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableTableDetails.TableNameColumn] = value;
            }
        }

        /// <summary>
        /// Gets the ColumnName column value
        /// </summary>
        public string ColumnName
        {
            get
            {
                try
                {
                    return ((string)(this[this._tableTableDetails.ColumnNameColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableTableDetails.ColumnNameColumn.ColumnName, _tableTableDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableTableDetails.ColumnNameColumn] = value;
            }
        } 

        /// <summary>
        /// Gets the IsNullable column value
        /// </summary>
        public bool IsNullable
        {
            get
            {
                try
                {
                    return ((bool)(this[this._tableTableDetails.IsNullableColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableTableDetails.IsNullableColumn.ColumnName, _tableTableDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableTableDetails.IsNullableColumn] = value;
            }
        }

        /// <summary>
        /// Gets the DataType column value
        /// </summary>
        public string DataType
        {
            get
            {
                try
                {
                    return ((string)(this[this._tableTableDetails.DataTypeColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableTableDetails.DataTypeColumn.ColumnName, _tableTableDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableTableDetails.DataTypeColumn] = value;
            }
        }

        /// <summary>
        /// Gets the MaximumLength column value
        /// </summary>
        public int MaximumLength
        {
            get
            {
                try
                {
                    return ((int)(this[this._tableTableDetails.MaximumLengthColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableTableDetails.MaximumLengthColumn.ColumnName, _tableTableDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableTableDetails.MaximumLengthColumn] = value;
            }
        }

        /// <summary>
        /// Gets the DateTime Precision column value
        /// </summary>
        public int DateTimePrecision
        {
            get
            {
                try
                {
                    return ((int)(this[this._tableTableDetails.DateTimePrecisionColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableTableDetails.DateTimePrecisionColumn.ColumnName, _tableTableDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableTableDetails.DateTimePrecisionColumn] = value;
            }
        }


        /// <summary>
        /// Gets the Precision column value
        /// </summary>
        public int Precision
        {
            get
            {
                try
                {
                    return ((int)(this[this._tableTableDetails.PrecisionColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableTableDetails.PrecisionColumn.ColumnName, _tableTableDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableTableDetails.PrecisionColumn] = value;
            }
        }

        /// <summary>
        /// Gets the Scale column value
        /// </summary>
        public int Scale
        {
            get
            {
                try
                {
                    return ((int)(this[this._tableTableDetails.ScaleColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableTableDetails.ScaleColumn.ColumnName, _tableTableDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableTableDetails.ScaleColumn] = value;
            }
        }

        /// <summary>
        /// Gets the IsServerGenerated column value
        /// </summary>
        public bool IsIdentity
        {
            get
            {
                try
                {
                    return ((bool)(this[this._tableTableDetails.IsIdentityColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableTableDetails.IsIdentityColumn.ColumnName, _tableTableDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableTableDetails.IsIdentityColumn] = value;
            }
        }

        /// <summary>
        /// Gets the IsServerGenerated column value
        /// </summary>
        public bool IsServerGenerated
        {
            get
            {
                try
                {
                    return ((bool)(this[this._tableTableDetails.IsServerGeneratedColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableTableDetails.IsServerGeneratedColumn.ColumnName, _tableTableDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableTableDetails.IsServerGeneratedColumn] = value;
            }
        }

        /// <summary>
        /// Gets the IsPrimaryKey column value
        /// </summary>
        public bool IsPrimaryKey
        {
            get
            {
                try
                {
                    return ((bool)(this[this._tableTableDetails.IsPrimaryKeyColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableTableDetails.IsPrimaryKeyColumn.ColumnName, _tableTableDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableTableDetails.IsPrimaryKeyColumn] = value;
            }
        }

        /// <summary>
        /// Determines if the Catalog column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsCatalogNull()
        {
            return this.IsNull(this._tableTableDetails.CatalogColumn);
        }

        /// <summary>
        /// Determines if the Schema column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsSchemaNull()
        {
            return this.IsNull(this._tableTableDetails.SchemaColumn);
        }

        /// <summary>
        /// Determines if the DataType column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsDataTypeNull()
        {
            return this.IsNull(this._tableTableDetails.DataTypeColumn);
        }

        /// <summary>
        /// Determines if the MaximumLength column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsMaximumLengthNull()
        {
            return this.IsNull(this._tableTableDetails.MaximumLengthColumn);
        }

        /// <summary>
        /// Determines if the Precision column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsPrecisionNull()
        {
            return this.IsNull(this._tableTableDetails.PrecisionColumn);
        }

        /// <summary>
        /// Determines if the DateTime Precision column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsDateTimePrecisionNull()
        {
            return this.IsNull(this._tableTableDetails.DateTimePrecisionColumn);
        }


        /// <summary>
        /// Determines if the Scale column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsScaleNull()
        {
            return this.IsNull(this._tableTableDetails.ScaleColumn);
        }

        /// <summary>
        /// Determines if the IsIdentity column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsIsIdentityNull()
        {
            return this.IsNull(this._tableTableDetails.IsIdentityColumn);
        }

        /// <summary>
        /// Determines if the IsIdentity column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsIsServerGeneratedNull()
        {
            return this.IsNull(this._tableTableDetails.IsServerGeneratedColumn);
        }

        public string GetMostQualifiedTableName()
        {
            string name = string.Empty;
            if (!IsCatalogNull())
            {
                name = Catalog;
            }

            if (!IsSchemaNull())
            {
                if (name != string.Empty)
                {
                    name += ".";
                }
                name += Schema;
            }

            if (name != string.Empty)
            {
                name += ".";
            }

            // TableName is not allowed to be null
            name += TableName;

            return name;
        }

        public EntityStoreSchemaGenerator.DbObjectKey CreateDbObjectKey(EntityStoreSchemaGenerator.DbObjectType objectType)
        {
            return new EntityStoreSchemaGenerator.DbObjectKey(
                this[this._tableTableDetails.CatalogColumn], 
                this[this._tableTableDetails.SchemaColumn],
                this[this._tableTableDetails.TableNameColumn],
                objectType);
        }
    }
}
