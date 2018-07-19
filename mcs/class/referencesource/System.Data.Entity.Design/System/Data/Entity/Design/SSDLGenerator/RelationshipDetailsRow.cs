//---------------------------------------------------------------------
// <copyright file="RelationshipDetailsRow.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System.Collections.Generic;
using System.Xml;
using System.Data.Common;
using System.Globalization;
using System.Data;
using System.Data.Entity.Design.Common;

namespace System.Data.Entity.Design.SsdlGenerator
{
    /// <summary>
    /// Strongly typed RelationshipDetail row
    /// </summary>
    internal sealed class RelationshipDetailsRow : System.Data.DataRow
    {

        private RelationshipDetailsCollection _tableRelationshipDetails;

        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        internal RelationshipDetailsRow(System.Data.DataRowBuilder rb)
            : base(rb)
        {
            this._tableRelationshipDetails = ((RelationshipDetailsCollection)(base.Table));
        }

        /// <summary>
        /// Gets a strongly typed table
        /// </summary>
        public new RelationshipDetailsCollection Table
        {
            get
            {
                return _tableRelationshipDetails;
            }
        }

        /// <summary>
        /// Gets the PkCatalog column value
        /// </summary>
        public string PKCatalog
        {
            get
            {
                try
                {
                    return ((string)(this[this._tableRelationshipDetails.PKCatalogColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableRelationshipDetails.PKCatalogColumn.ColumnName, _tableRelationshipDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableRelationshipDetails.PKCatalogColumn] = value;
            }
        }

        /// <summary>
        /// Gets the PkSchema column value
        /// </summary>
        public string PKSchema
        {
            get
            {
                try
                {
                    return ((string)(this[this._tableRelationshipDetails.PKSchemaColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableRelationshipDetails.PKSchemaColumn.ColumnName, _tableRelationshipDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableRelationshipDetails.PKSchemaColumn] = value;
            }
        }

        /// <summary>
        /// Gets the PkTable column value
        /// </summary>
        public string PKTable
        {
            get
            {
                try
                {
                    return ((string)(this[this._tableRelationshipDetails.PKTableColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableRelationshipDetails.PKTableColumn.ColumnName, _tableRelationshipDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableRelationshipDetails.PKTableColumn] = value;
            }
        }

        /// <summary>
        /// Gets the PkColumn column value
        /// </summary>
        public string PKColumn
        {
            get
            {
                try
                {
                    return ((string)(this[this._tableRelationshipDetails.PKColumnColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableRelationshipDetails.PKColumnColumn.ColumnName, _tableRelationshipDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableRelationshipDetails.PKColumnColumn] = value;
            }
        }

        /// <summary>
        /// Gets the FkCatalog column value
        /// </summary>
        public string FKCatalog
        {
            get
            {
                try
                {
                    return ((string)(this[this._tableRelationshipDetails.FKCatalogColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableRelationshipDetails.FKCatalogColumn.ColumnName, _tableRelationshipDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableRelationshipDetails.FKCatalogColumn] = value;
            }
        }

        /// <summary>
        /// Gets the FkSchema column value
        /// </summary>
        public string FKSchema
        {
            get
            {
                try
                {
                    return ((string)(this[this._tableRelationshipDetails.FKSchemaColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableRelationshipDetails.FKSchemaColumn.ColumnName, _tableRelationshipDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableRelationshipDetails.FKSchemaColumn] = value;
            }
        }

        /// <summary>
        /// Gets the FkTable column value
        /// </summary>
        public string FKTable
        {
            get
            {
                try
                {
                    return ((string)(this[this._tableRelationshipDetails.FKTableColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableRelationshipDetails.FKTableColumn.ColumnName, _tableRelationshipDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableRelationshipDetails.FKTableColumn] = value;
            }
        }

        /// <summary>
        /// Gets the FkColumn column value
        /// </summary>
        public string FKColumn
        {
            get
            {
                try
                {
                    return ((string)(this[this._tableRelationshipDetails.FKColumnColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableRelationshipDetails.FKColumnColumn.ColumnName, _tableRelationshipDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableRelationshipDetails.FKColumnColumn] = value;
            }
        }

        /// <summary>
        /// Gets the Ordinal column value
        /// </summary>
        public int Ordinal
        {
            get
            {
                try
                {
                    return ((int)(this[this._tableRelationshipDetails.OrdinalColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableRelationshipDetails.OrdinalColumn.ColumnName, _tableRelationshipDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableRelationshipDetails.OrdinalColumn] = value;
            }
        }

        /// <summary>
        /// Gets the RelationshipName column value
        /// </summary>
        public string RelationshipName
        {
            get
            {
                try
                {
                    return ((string)(this[this._tableRelationshipDetails.RelationshipNameColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableRelationshipDetails.RelationshipNameColumn.ColumnName, _tableRelationshipDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableRelationshipDetails.RelationshipNameColumn] = value;
            }
        }

        /// <summary>
        /// Gets the RelationshipName column value
        /// </summary>
        public string RelationshipId
        {
            get
            {
                try
                {
                    return ((string)(this[this._tableRelationshipDetails.RelationshipIdColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableRelationshipDetails.RelationshipIdColumn.ColumnName, _tableRelationshipDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableRelationshipDetails.RelationshipIdColumn] = value;
            }
        }
        /// <summary>
        /// Gets the IsCascadeDelete column value
        /// </summary>
        public bool RelationshipIsCascadeDelete
        {
            get
            {
                try
                {
                    return ((bool)(this[this._tableRelationshipDetails.RelationshipIsCascadeDeleteColumn]));
                }
                catch (System.InvalidCastException e)
                {
                    throw EDesignUtil.StonglyTypedAccessToNullValue(_tableRelationshipDetails.RelationshipIsCascadeDeleteColumn.ColumnName, _tableRelationshipDetails.TableName, e);
                }
            }
            set
            {
                this[this._tableRelationshipDetails.RelationshipIsCascadeDeleteColumn] = value;
            }
        }


        /// <summary>
        /// Determines if the PkCatalog column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsPKCatalogNull()
        {
            return this.IsNull(this._tableRelationshipDetails.PKCatalogColumn);
        }

        /// <summary>
        /// Determines if the PkSchema column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsPKSchemaNull()
        {
            return this.IsNull(this._tableRelationshipDetails.PKSchemaColumn);
        }

        /// <summary>
        /// Determines if the PkTable column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsPKTableNull()
        {
            return this.IsNull(this._tableRelationshipDetails.PKTableColumn);
        }

        /// <summary>
        /// Determines if the PkColumn column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsPKColumnNull()
        {
            return this.IsNull(this._tableRelationshipDetails.PKColumnColumn);
        }

        /// <summary>
        /// Determines if the FkCatalog column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsFKCatalogNull()
        {
            return this.IsNull(this._tableRelationshipDetails.FKCatalogColumn);
        }

        /// <summary>
        /// Determines if the FkSchema column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsFKSchemaNull()
        {
            return this.IsNull(this._tableRelationshipDetails.FKSchemaColumn);
        }

        /// <summary>
        /// Determines if the FkTable column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsFKTableNull()
        {
            return this.IsNull(this._tableRelationshipDetails.FKTableColumn);
        }

        /// <summary>
        /// Determines if the FkColumn column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsFKColumnNull()
        {
            return this.IsNull(this._tableRelationshipDetails.FKColumnColumn);
        }

        /// <summary>
        /// Determines if the Ordinal column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsOrdinalNull()
        {
            return this.IsNull(this._tableRelationshipDetails.OrdinalColumn);
        }

        /// <summary>
        /// Determines if the RelationshipName column value is null
        /// </summary>
        /// <returns>true if the value is null, otherwise false.</returns>
        public bool IsRelationshipNameNull()
        {
            return this.IsNull(this._tableRelationshipDetails.RelationshipNameColumn);
        }
    }
}
