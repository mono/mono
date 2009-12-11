using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

using DbLinq.Schema.Dbml;
using DbLinq.Vendor.Implementation;

namespace DbLinq.Vendor
{
#if !MONO_STRICT
    public
#endif
    class DbSchemaLoader : SchemaLoader
    {
        public DbSchemaLoader()
        {
        }

        public override IVendor Vendor { get; set; }

        protected virtual string UnquoteSqlName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;
            var quotes = new[]{
                new { Start = "[",  End = "]" },
                new { Start = "`",  End = "`" },
                new { Start = "\"", End = "\"" },
            };
            foreach (var q in quotes)
            {
                if (name.StartsWith(q.Start) && name.EndsWith(q.End))
                    return name.Substring(q.Start.Length, name.Length - q.Start.Length - q.End.Length);
            }
            return name;
        }

        protected override void LoadConstraints(Database schema, DbLinq.Schema.SchemaName schemaName, IDbConnection conn, DbLinq.Schema.NameFormat nameFormat, Names names)
        {
            DbConnection c = (DbConnection) conn;
            var foreignKeys = GetForeignKeys(c);

            int iConstName  = foreignKeys.Columns.IndexOf("CONSTRAINT_NAME");
            int iConstType  = foreignKeys.Columns.IndexOf("CONSTRAINT_TYPE");
            int iFromColumn = foreignKeys.Columns.IndexOf("FKEY_FROM_COLUMN");
            int iFromSchema = foreignKeys.Columns.IndexOf("TABLE_SCHEMA");
            int iFromTable  = foreignKeys.Columns.IndexOf("TABLE_NAME");
            int iToColumn   = foreignKeys.Columns.IndexOf("FKEY_TO_COLUMN");
            int iToSchema   = foreignKeys.Columns.IndexOf("FKEY_TO_SCHEMA");
            int iToTable    = foreignKeys.Columns.IndexOf("FKEY_TO_TABLE");

            if (iConstName < 0 || iConstType < 0 ||
                    iFromColumn < 0 || iFromSchema < 0 || iFromTable < 0 ||
                    iToColumn < 0 || iToSchema < 0 || iToTable < 0)
            {
                WriteErrorLine("Database connection '{0}' doesn't support querying foreign key information.",
                    c.GetType().Name);
                return;
            }

            foreach (DataRow r in foreignKeys.Rows)
            {
                var fromTable  = UnquoteSqlName(GetValue<string>(r, iFromTable, null));
                var fromSchema = UnquoteSqlName(GetValue<string>(r, iFromSchema, null));
                var fromColumn = UnquoteSqlName(GetValue<string>(r, iFromColumn, null));

                string fullFromTable = GetFullDbName(fromTable, fromSchema);
                DbLinq.Schema.Dbml.Table table = schema.Tables.FirstOrDefault(t => fullFromTable == t.Name);
                if (table == null)
                {
                    WriteErrorLine("ERROR L46: Table '" + fromTable + "' not found for column " + fromColumn);
                    continue;
                }

                var constraintType  = GetValue<string>(r, iConstType, null);
                var toTable         = UnquoteSqlName(GetValue<string>(r, iToTable, null));

                if (constraintType == "FOREIGN KEY" && toTable != null)
                {
                    var constraintName  = GetValue(r, iConstName, (string) null);
                    var toColumn        = UnquoteSqlName(GetValue<string>(r, iToColumn, null));
                    var toSchema        = UnquoteSqlName(GetValue<string>(r, iToSchema, null));
                    LoadForeignKey(schema, table, 
                            fromColumn, fromTable, fromSchema,
                            toColumn, toTable, toSchema,
                            constraintName, nameFormat, names);
                }
            }
        }

        protected virtual DataTable GetForeignKeys(DbConnection connection)
        {
            return connection.GetSchema("ForeignKeys");
        }

        private DataTable GetSchema(DbConnection connection, string schema)
        {
            var schemas = connection.GetSchema();
            var iCollectionName = schemas.Columns.IndexOf("CollectionName");
            if (!schemas.Rows.Cast<DataRow>().Any(r => r[iCollectionName].ToString() == schema))
                return null;
            return connection.GetSchema(schema);
        }

#if false
        protected override void LoadForeignKey(Database schema, Table table, string columnName, string tableName, string tableSchema, string referencedColumnName, string referencedTableName, string referencedTableSchema, string constraintName, DbLinq.Schema.NameFormat nameFormat, SchemaLoader.Names names)
        {
            var foriegnKeys = GetSchema("ForeignKeys");
            if (foriegnKeys == null)
                return;
            // throw new NotImplementedException();
        }
#endif

#if false
        protected override void LoadStoredProcedures(Database schema, DbLinq.Schema.SchemaName schemaName, IDbConnection conn, DbLinq.Schema.NameFormat nameFormat)
        {
            var foriegnKeys = GetSchema("Procedures");
            if (foriegnKeys == null)
                return;
           //  throw new NotImplementedException();
        }
#endif

        protected override IList<IDataTableColumn> ReadColumns(IDbConnection connection, string databaseName)
        {
            var db = (DbConnection) connection;

            var typeMap   = GetSqlToManagedTypeMapping(db);

            var dbColumns = GetColumns(db);
            var iColumn   = dbColumns.Columns.IndexOf("COLUMN_NAME");
            var iDefValue = dbColumns.Columns.IndexOf("COLUMN_DEFAULT");
            var iNullable = dbColumns.Columns.IndexOf("IS_NULLABLE");
            var iMaxLen   = dbColumns.Columns.IndexOf("CHARACTER_MAXIMUM_LENGTH");
            var iNumPrec  = dbColumns.Columns.IndexOf("NUMERIC_PRECISION");
            var iDatPrec  = dbColumns.Columns.IndexOf("DATETIME_PRECISION");
            var iTable    = dbColumns.Columns.IndexOf("TABLE_NAME");
            var iSchema   = dbColumns.Columns.IndexOf("TABLE_SCHEMA");
            var iSqlType  = dbColumns.Columns.IndexOf("DATA_TYPE");

            var iPK = dbColumns.Columns.IndexOf("PRIMARY_KEY");
            if (iPK < 0)
            {
                WriteErrorLine("Database connection '{0}' doesn't support querying primary key information.",
                        db.GetType().Name);
            }

            var columns = new List<IDataTableColumn>();
            foreach (DataRow c in dbColumns.Rows)
            {
                var sqlType     = c[iSqlType].ToString().Trim();
                var tableName   = UnquoteSqlName(GetValue<string>(c, iTable, null));
                var tableSchema = UnquoteSqlName(GetValue<string>(c, iSchema, null));
                var columnName  = UnquoteSqlName(GetValue<string>(c, iColumn, null));

                if (sqlType.Length == 0)
                {
                    // Column has no type; wtf are we supposed to do?
                    // This happens w/ SQLite while processing Northwind.db3 for the 
                    // 'Customer and Suppliers by City' view, Relationship column.
                    Console.Error.WriteLine("Warning: The column '{0}.{1}.{2}' could not be imported because the column's DATA_TYPE is empty.",
                            tableSchema, tableName, columnName);
                    continue;
                }

                var v = new DataTableColumn()
                {
                    ColumnName      = columnName,
                    DefaultValue    = GetValue<string>(c, iDefValue, null),
                    FullType        = sqlType,
                    Length          = (long) GetValue<int>(c, iMaxLen, 0),
                    ManagedType     = typeMap[sqlType],
                    Nullable        = GetValue(c, iNullable, false),
                    Precision       = GetValue<int?>(c, iNumPrec, null),
                    PrimaryKey      = iPK < 0 ? (bool?) null : (bool?) GetValue(c, iPK, false),
                    SqlType         = sqlType,
                    TableName       = tableName,
                    TableSchema     = tableSchema,
                };
                FillDataTableColumnInformation(c, v);
                columns.Add(v);
            }
            return columns;
        }

        protected virtual DataTable GetColumns(DbConnection connection)
        {
            return connection.GetSchema("Columns");
        }

        protected virtual void FillDataTableColumnInformation(DataRow row, DataTableColumn column)
        {
            if (column.PrimaryKey.HasValue && column.PrimaryKey.Value &&
                    (column.ManagedType == "System.Int32" || column.ManagedType == "System.Int64"))
                column.Generated = true;
        }

        private static bool GetValue(DataRow r, int index, bool defaultValue)
        {
            var v = r[index];
            if (v is DBNull)
                return defaultValue;
            if (v is string)
            {
                switch (v.ToString().ToUpperInvariant())
                {
                    case "YES": return true;
                    case "NO":  return false;
                    default:    return defaultValue;
                }
            }
            return (bool) v;
        }

        private static T GetValue<T>(DataRow r, int index, T defaultValue)
        {
            var v = r[index];
            if (v is DBNull)
                return defaultValue;
            return (T) v;
        }

        private Dictionary<string, string> GetSqlToManagedTypeMapping(DbConnection connection)
        {
            var dataTypes = connection.GetSchema("DataTypes");
            var iSqlType = dataTypes.Columns.IndexOf("TypeName");
            var iNetType = dataTypes.Columns.IndexOf("DataType");
            return dataTypes.Rows.Cast<DataRow>()
                .ToDictionary(r => r[iSqlType].ToString(), r => r[iNetType].ToString());
        }

        public override IList<IDataName> ReadTables(IDbConnection connection, string databaseName)
        {
            DbConnection db = (DbConnection) connection;
            var dbTables  = db.GetSchema("Tables");
            var iName     = dbTables.Columns.IndexOf("TABLE_NAME");
            var iSchema   = dbTables.Columns.IndexOf("TABLE_SCHEMA");
            List<IDataName> tables = new List<IDataName>();
            foreach (DataRow table in dbTables.Rows)
            {
                var schema = UnquoteSqlName(GetValue<string>(table, iSchema, null));
                tables.Add(new DataName()
                {
                    Name    = UnquoteSqlName(table[iName].ToString()),
                    Schema  = string.IsNullOrEmpty(schema) ? null : schema,
                });
            }
            return tables;
        }
    }
}
