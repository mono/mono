//---------------------------------------------------------------------
// <copyright file="EntityStoreSchemaGeneratorDatabaseSchemaLoader.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System.Collections.Generic;
using System.Globalization;
using System.Data.Common;
using System.Data.EntityClient;
using System.Data.Entity.Design.Common;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Entity.Design.SsdlGenerator
{
    /// <summary>
    /// Responsible for Loading Database Schema Information
    /// </summary>
    internal class EntityStoreSchemaGeneratorDatabaseSchemaLoader 
    {
        private readonly EntityConnection _connection;
        private readonly Version _storeSchemaModelVersion;
        private readonly string _providerInvariantName;

        public EntityStoreSchemaGeneratorDatabaseSchemaLoader(string providerInvariantName, string connectionString)
        {
            Debug.Assert(providerInvariantName != null, "providerInvariantName parameter is null");
            Debug.Assert(connectionString != null, "connectionString parameter is null");
            _providerInvariantName = providerInvariantName;
            _connection = CreateStoreSchemaConnection(providerInvariantName, connectionString, out _storeSchemaModelVersion);
        }

        private static EntityConnection CreateStoreSchemaConnection(string providerInvariantName, string connectionString, out Version storeSchemaModelVersion)
        {
            // We are going to try loading all versions of the store schema model starting from the newest.
            // The first version of the model that was shipped with EntityFrameworkVersions.Version1 and EntityFrameworkVersions.Version2 is the last one
            // we try, if it fails to load let the exception to propagate up to the caller.
            foreach (var version in EntityFrameworkVersions.ValidVersions.Where(v => v > EntityFrameworkVersions.Version2).OrderByDescending(v => v))
            {
                try
                {
                    storeSchemaModelVersion = version;
                    return EntityStoreSchemaGenerator.CreateStoreSchemaConnection(providerInvariantName, connectionString, storeSchemaModelVersion);
                }
                catch (Exception e)
                {
                    // Ignore the exception with the current version and try the next one.
                    if (!MetadataUtil.IsCatchableExceptionType(e))
                    {
                        throw;
                    }
                }
            }
            storeSchemaModelVersion = EntityFrameworkVersions.Version2;
            return EntityStoreSchemaGenerator.CreateStoreSchemaConnection(providerInvariantName, connectionString, storeSchemaModelVersion);
        }

        public void Open()
        {
            _connection.Open();
        }

        public void Close()
        {
            _connection.Close();
        }

        public string ProviderInvariantName
        {
            get { return _providerInvariantName; }
        }

        public Version StoreSchemaModelVersion
        {
            get { return _storeSchemaModelVersion; }
        }
        
        public FunctionDetailsReader LoadFunctionDetails(IEnumerable<EntityStoreSchemaFilterEntry> filters)
        {
            return FunctionDetailsReader.Create(_connection, filters, _storeSchemaModelVersion);
        }

        public IEnumerable<DataRow> LoadViewDetails(IEnumerable<EntityStoreSchemaFilterEntry> filters)
        {
            TableDetailsCollection views = new TableDetailsCollection();
            return LoadDataTable(
                ViewDetailSql, 
                rows => 
                    rows
                    .OrderBy(r => r.Field<string>("SchemaName"))
                    .ThenBy(r=>r.Field<string>("TableName"))
                    .ThenBy(r=>r.Field<int>("Ordinal")), 
                views, 
                EntityStoreSchemaFilterObjectTypes.View, 
                filters, 
                ViewDetailAlias);
        }

        public IEnumerable<DataRow> LoadTableDetails(IEnumerable<EntityStoreSchemaFilterEntry> filters)
        {
            TableDetailsCollection table = new TableDetailsCollection();
            return LoadDataTable(
                TableDetailSql,
                rows =>
                    rows
                    .OrderBy(r => r.Field<string>("SchemaName"))
                    .ThenBy(r => r.Field<string>("TableName"))
                    .ThenBy(r => r.Field<int>("Ordinal")),
                table,
                EntityStoreSchemaFilterObjectTypes.Table,
                filters,
                TableDetailAlias);
        }

        public IEnumerable<DataRow> LoadFunctionReturnTableDetails(IEnumerable<EntityStoreSchemaFilterEntry> filters)
        {
            Debug.Assert(_storeSchemaModelVersion >= EntityFrameworkVersions.Version3, "_storeSchemaModelVersion >= EntityFrameworkVersions.Version3");
            TableDetailsCollection table = new TableDetailsCollection();
            return LoadDataTable(
                FunctionReturnTableDetailSql,
                rows =>
                    rows
                    .OrderBy(r => r.Field<string>("SchemaName"))
                    .ThenBy(r => r.Field<string>("TableName"))
                    .ThenBy(r => r.Field<int>("Ordinal")),
                table,
                EntityStoreSchemaFilterObjectTypes.Function,
                filters,
                FunctionReturnTableDetailAlias);
        }

        public IEnumerable<DataRow> LoadRelationships(IEnumerable<EntityStoreSchemaFilterEntry> filters)
        {
            RelationshipDetailsCollection table = new RelationshipDetailsCollection();
            return LoadDataTable(
                RelationshipDetailSql,
                rows =>
                    rows
                    .OrderBy(r => r.Field<string>("RelationshipName"))
                    .ThenBy(r => r.Field<string>("RelationshipId"))
                    .ThenBy(r => r.Field<int>("Ordinal")), 
                table, 
                EntityStoreSchemaFilterObjectTypes.Table, 
                filters, 
                RelationshipDetailFromTableAlias, 
                RelationshipDetailToTableAlias);
        }

        /// <summary>
        /// The underlying connection that this DbSchemaLoader class is using
        /// This is used to get the provider manifest information only.
        /// </summary>
        public System.Data.Common.DbConnection InnerConnection
        {
            get { return _connection.StoreConnection; }
        }

        internal EntityConnection EntityConnection
        {
            get { return _connection; }
        }

        private IEnumerable<DataRow> LoadDataTable(string sql, Func<IEnumerable<DataRow>, IEnumerable<DataRow>> orderByFunc, DataTable table, EntityStoreSchemaFilterObjectTypes queryTypes, IEnumerable<EntityStoreSchemaFilterEntry> filters, params string[] filterAliases)
        {
            using (EntityCommand command = CreateFilteredCommand(_connection, sql, null, queryTypes, new List<EntityStoreSchemaFilterEntry>(filters), filterAliases))
            {
                using (DbDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess))
                {
                    object[] values = new object[table.Columns.Count];
                    while (reader.Read())
                    {
                        reader.GetValues(values);
                        table.Rows.Add(values);
                    }

                    return orderByFunc(table.AsEnumerable());
                }
            }
        }

        internal static EntityCommand CreateFilteredCommand(EntityConnection connection, string sql, string orderByClause, EntityStoreSchemaFilterObjectTypes queryTypes, List<EntityStoreSchemaFilterEntry> filters, string[] filterAliases)
        {
            EntityCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 0;

            if (filters.Count == 0)
            {
                if (!string.IsNullOrEmpty(orderByClause))
                {
                    command.CommandText = sql + Environment.NewLine + orderByClause;
                }
                else
                {
                    command.CommandText = sql;
                }
                return command;
            }

            StringBuilder whereClause = new StringBuilder();
            foreach (string alias in filterAliases)
            {
                StringBuilder allows = new StringBuilder();
                StringBuilder excludes = new StringBuilder();
                foreach (EntityStoreSchemaFilterEntry entry in filters)
                {
                    // only apply filters of the correct type
                    if ((queryTypes & entry.Types) == 0)
                    {
                        continue;
                    }


                    if (entry.Effect == EntityStoreSchemaFilterEffect.Allow)
                    {
                        AddFilterEntry(command, allows, alias, entry);
                    }
                    else
                    {
                        Debug.Assert(entry.Effect == EntityStoreSchemaFilterEffect.Exclude, "did you add new value?");
                        AddFilterEntry(command, excludes, alias, entry);
                    }
                }

                if (allows.Length != 0)
                {
                    if (whereClause.Length != 0)
                    {
                        whereClause.Append(Environment.NewLine);
                        whereClause.Append("AND");
                        whereClause.Append(Environment.NewLine);
                    }
                    whereClause.Append("(");
                    whereClause.Append(allows);
                    whereClause.Append(")");
                }

                if (excludes.Length != 0)
                {
                    if (whereClause.Length != 0)
                    {
                        whereClause.Append(Environment.NewLine);
                        whereClause.Append("AND");
                        whereClause.Append(Environment.NewLine);
                    }
                    whereClause.Append("NOT (");
                    whereClause.Append(excludes);
                    whereClause.Append(")");
                }
            }


            // did we end up with a where clause?
            StringBuilder sqlStatement = new StringBuilder(sql);
            if (whereClause.Length != 0)
            {
                sqlStatement.Append(Environment.NewLine);
                sqlStatement.Append("WHERE");
                sqlStatement.Append(Environment.NewLine);
                sqlStatement.Append(whereClause);
            }

            if (!string.IsNullOrEmpty(orderByClause))
            {
                sqlStatement.Append(Environment.NewLine);
                sqlStatement.Append(orderByClause);
            }

            command.CommandText = sqlStatement.ToString();

            return command;
        }

        private static void AddFilterEntry(EntityCommand command, StringBuilder segment, string alias, EntityStoreSchemaFilterEntry entry)
        {
            
            StringBuilder filterText = new StringBuilder();
            AddComparison(command, filterText, alias, "CatalogName", entry.Catalog);
            AddComparison(command, filterText, alias, "SchemaName", entry.Schema);

            string name = entry.Name;
            bool allNull = (entry.Catalog == null && entry.Schema == null && entry.Name == null);
            if (allNull)
            {
                name = "%";
            }
            AddComparison(command, filterText, alias, "Name", name);

            if (segment.Length != 0)
            {
                segment.Append(" OR ");
            }
            segment.Append("(");
            segment.Append(filterText);
            segment.Append(")");
        }

        private static void AddComparison(EntityCommand command, StringBuilder segment, string alias, string propertyName, string value)
        {
            if (value != null)
            {
                if (segment.Length != 0)
                {
                    segment.Append(" AND ");
                }

                segment.Append(alias);
                segment.Append(".");
                segment.Append(propertyName);
                segment.Append(" LIKE @");
                string parameterName = "p" + command.Parameters.Count.ToString(CultureInfo.InvariantCulture);
                segment.Append(parameterName);
                EntityParameter parameter = new EntityParameter();
                parameter.ParameterName = parameterName;
                parameter.Value = value;
                command.Parameters.Add(parameter);
            }
        }

        private static readonly string ViewDetailAlias = "v";
        private static readonly string ViewDetailSql = @"
              SELECT 
                  v.CatalogName
              ,   v.SchemaName                           
              ,   v.Name
              ,   v.ColumnName
              ,   v.Ordinal
              ,   v.IsNullable
              ,   v.TypeName
              ,   v.MaxLength
              ,   v.Precision
              ,   v.DateTimePrecision
              ,   v.Scale
              ,   v.IsIdentity
              ,   v.IsStoreGenerated
              ,   CASE WHEN pk.IsPrimaryKey IS NULL THEN false ELSE pk.IsPrimaryKey END as IsPrimaryKey
            FROM (
              SELECT
                  v.CatalogName
              ,   v.SchemaName                           
              ,   v.Name
              ,   c.Id as ColumnId
              ,   c.Name as ColumnName
              ,   c.Ordinal
              ,   c.IsNullable
              ,   c.ColumnType.TypeName as TypeName
              ,   c.ColumnType.MaxLength as MaxLength
              ,   c.ColumnType.Precision as Precision
              ,   c.ColumnType.DateTimePrecision as DateTimePrecision
              ,   c.ColumnType.Scale as Scale
              ,   c.IsIdentity
              ,   c.IsStoreGenerated
              FROM
                  SchemaInformation.Views as v 
                  cross apply 
                  v.Columns as c ) as v 
            LEFT OUTER JOIN (
              SELECT 
                  true as IsPrimaryKey
                , pkc.Id
              FROM
                  OfType(SchemaInformation.ViewConstraints, Store.PrimaryKeyConstraint) as pk
                  CROSS APPLY pk.Columns as pkc) as pk
            ON v.ColumnId = pk.Id                   
             ";
        
        private static readonly string TableDetailAlias = "t";
        private static readonly string TableDetailSql = @"
              SELECT 
                  t.CatalogName
              ,   t.SchemaName                           
              ,   t.Name
              ,   t.ColumnName
              ,   t.Ordinal
              ,   t.IsNullable
              ,   t.TypeName
              ,   t.MaxLength
              ,   t.Precision
              ,   t.DateTimePrecision
              ,   t.Scale
              ,   t.IsIdentity
              ,   t.IsStoreGenerated
              ,   CASE WHEN pk.IsPrimaryKey IS NULL THEN false ELSE pk.IsPrimaryKey END as IsPrimaryKey
            FROM (
              SELECT
                  t.CatalogName
              ,   t.SchemaName                           
              ,   t.Name
              ,   c.Id as ColumnId
              ,   c.Name as ColumnName
              ,   c.Ordinal
              ,   c.IsNullable
              ,   c.ColumnType.TypeName as TypeName
              ,   c.ColumnType.MaxLength as MaxLength
              ,   c.ColumnType.Precision as Precision
              ,   c.ColumnType.DateTimePrecision as DateTimePrecision
              ,   c.ColumnType.Scale as Scale
              ,   c.IsIdentity
              ,   c.IsStoreGenerated
              FROM
                  SchemaInformation.Tables as t 
                  cross apply 
                  t.Columns as c ) as t 
            LEFT OUTER JOIN (
              SELECT 
                  true as IsPrimaryKey
                , pkc.Id
              FROM
                  OfType(SchemaInformation.TableConstraints, Store.PrimaryKeyConstraint) as pk
                  CROSS APPLY pk.Columns as pkc) as pk
            ON t.ColumnId = pk.Id                   
            ";

        private static readonly string FunctionReturnTableDetailAlias = "tvf";
        private static readonly string FunctionReturnTableDetailSql = @"
              SELECT 
                  tvf.CatalogName
              ,   tvf.SchemaName                           
              ,   tvf.Name
              ,   tvf.ColumnName
              ,   tvf.Ordinal
              ,   tvf.IsNullable
              ,   tvf.TypeName
              ,   tvf.MaxLength
              ,   tvf.Precision
              ,   tvf.DateTimePrecision
              ,   tvf.Scale
              ,   false as IsIdentity
              ,   false as IsStoreGenerated
              ,   false as IsPrimaryKey
            FROM (
              SELECT
                  t.CatalogName
              ,   t.SchemaName                           
              ,   t.Name
              ,   c.Id as ColumnId
              ,   c.Name as ColumnName
              ,   c.Ordinal
              ,   c.IsNullable
              ,   c.ColumnType.TypeName as TypeName
              ,   c.ColumnType.MaxLength as MaxLength
              ,   c.ColumnType.Precision as Precision
              ,   c.ColumnType.DateTimePrecision as DateTimePrecision
              ,   c.ColumnType.Scale as Scale
              FROM
                  OfType(SchemaInformation.Functions, Store.TableValuedFunction) as t 
                  cross apply 
                  t.Columns as c ) as tvf
            ";

        private static readonly string RelationshipDetailFromTableAlias = "r.FromTable";
        private static readonly string RelationshipDetailToTableAlias = "r.ToTable";
        private static readonly string RelationshipDetailSql = @"
              SELECT
                 r.ToTable.CatalogName as ToTableCatalog
               , r.ToTable.SchemaName as ToTableSchema
               , r.ToTable.Name as ToTableName
               , r.ToColumnName
               , r.FromTable.CatalogName as FromTableCatalog
               , r.FromTable.SchemaName as FromTableSchema
               , r.FromTable.Name as FromTableName
               , r.FromColumnName
               , r.Ordinal
               , r.RelationshipName
               , r.RelationshipId
               , r.IsCascadeDelete
              FROM (
               SELECT 
                    fks.ToColumn.Parent as ToTable
               ,    fks.ToColumn.Name as ToColumnName
               ,    c.Parent as FromTable
               ,    fks.FromColumn.Name as FromColumnName
               ,    fks.Ordinal as Ordinal
               ,    c.Name as RelationshipName
               ,    c.Id as RelationshipId
               ,    c.DeleteRule = 'CASCADE' as IsCascadeDelete
            FROM 
                OfType(SchemaInformation.TableConstraints, Store.ForeignKeyConstraint) as c,
                ( SELECT 
                   Ref(fk.Constraint) as cRef
                 ,  fk.ToColumn
                 , fk.FromColumn
                 , fk.Ordinal
                FROM
                   c.ForeignKeys as fk) as fks
                WHERE fks.cRef = Ref(c)) as r
                ";
    }
}
