//---------------------------------------------------------------------
// <copyright file="SqlProviderUtilities.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Common;
using System.Data.Common.Utils;
using System.Data.Entity;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace System.Data.SqlClient 
{
    class SqlProviderUtilities
    {
        /// <summary>
        /// Requires that the given connection is of type  T. 
        /// Returns the connection or throws.
        /// </summary>
        internal static SqlConnection GetRequiredSqlConnection(DbConnection connection)
        {
            var result = connection as SqlConnection;
            if (null == result)
            {
                throw EntityUtil.Argument(Strings.Mapping_Provider_WrongConnectionType(typeof(SqlConnection)));
            }
            return result;
        }
    }

    sealed class SqlDdlBuilder
    {
        private readonly StringBuilder unencodedStringBuilder = new StringBuilder();
        private readonly HashSet<EntitySet> ignoredEntitySets = new HashSet<EntitySet>();

        #region Pulblic Surface
        internal static string CreateObjectsScript(StoreItemCollection itemCollection, bool createSchemas)
        {
            SqlDdlBuilder builder = new SqlDdlBuilder();

            foreach (EntityContainer container in itemCollection.GetItems<EntityContainer>())
            {
                var entitySets = container.BaseEntitySets.OfType<EntitySet>().OrderBy(s => s.Name);

                if (createSchemas)
                {
                    var schemas = new HashSet<string>(entitySets.Select(s => GetSchemaName(s)));
                    foreach (string schema in schemas.OrderBy(s => s))
                    {
                        // don't bother creating default schema
                        if (schema != "dbo")
                        {
                            builder.AppendCreateSchema(schema);
                        }
                    }
                }

                foreach (EntitySet entitySet in container.BaseEntitySets.OfType<EntitySet>().OrderBy(s => s.Name))
                {
                    builder.AppendCreateTable(entitySet);
                }

                foreach (AssociationSet associationSet in container.BaseEntitySets.OfType<AssociationSet>().OrderBy(s => s.Name))
                {
                    builder.AppendCreateForeignKeys(associationSet);
                }
            }
            return builder.GetCommandText();
        }

        internal static string CreateDatabaseScript(string databaseName, string dataFileName, string logFileName)
        {
            var builder = new SqlDdlBuilder();
            builder.AppendSql("create database ");
            builder.AppendIdentifier(databaseName);
            if (null != dataFileName)
            {
                Debug.Assert(logFileName != null, "must specify log file with data file");
                builder.AppendSql(" on primary ");
                builder.AppendFileName(dataFileName);
                builder.AppendSql(" log on ");
                builder.AppendFileName(logFileName);
            }
            return builder.unencodedStringBuilder.ToString();
        }

        internal static string CreateDatabaseExistsScript(string databaseName, bool useDeprecatedSystemTable)
        {
            var builder = new SqlDdlBuilder();
            builder.AppendSql("SELECT Count(*) FROM ");
            AppendSysDatabases(builder, useDeprecatedSystemTable);
            builder.AppendSql(" WHERE [name]=");
            builder.AppendStringLiteral(databaseName);
            return builder.unencodedStringBuilder.ToString();
        }

        private static void AppendSysDatabases(SqlDdlBuilder builder, bool useDeprecatedSystemTable)
        {
            if (useDeprecatedSystemTable)
            {
                builder.AppendSql("sysdatabases");
            }
            else
            {
                builder.AppendSql("sys.databases");
            }
        }

        internal static string CreateGetDatabaseNamesBasedOnFileNameScript(string databaseFileName, bool useDeprecatedSystemTable)
        {
            var builder = new SqlDdlBuilder();
            builder.AppendSql("SELECT [d].[name] FROM ");
            AppendSysDatabases(builder, useDeprecatedSystemTable);
            builder.AppendSql(" AS [d] ");
            if (!useDeprecatedSystemTable)
            {
                builder.AppendSql("INNER JOIN sys.master_files AS [f] ON [f].[database_id] = [d].[database_id]");
            }
            builder.AppendSql(" WHERE [");
            if (useDeprecatedSystemTable)
            {
                builder.AppendSql("filename");
            }
            else
            {
                builder.AppendSql("f].[physical_name");
            }
            builder.AppendSql("]=");
            builder.AppendStringLiteral(databaseFileName);
            return builder.unencodedStringBuilder.ToString();
        }

        internal static string CreateCountDatabasesBasedOnFileNameScript(string databaseFileName, bool useDeprecatedSystemTable)
        {
            var builder = new SqlDdlBuilder();
            builder.AppendSql("SELECT Count(*) FROM ");

            if (useDeprecatedSystemTable)
            {
                builder.AppendSql("sysdatabases");
            }
            if (!useDeprecatedSystemTable)
            {
                builder.AppendSql("sys.master_files");
            }
            builder.AppendSql(" WHERE [");
            if (useDeprecatedSystemTable)
            {
                builder.AppendSql("filename");
            }
            else
            {
                builder.AppendSql("physical_name");
            }
            builder.AppendSql("]=");
            builder.AppendStringLiteral(databaseFileName);
            return builder.unencodedStringBuilder.ToString();
        }

        internal static string DropDatabaseScript(string databaseName)
        {
            var builder = new SqlDdlBuilder();
            builder.AppendSql("drop database ");
            builder.AppendIdentifier(databaseName);
            return builder.unencodedStringBuilder.ToString();
        }

        internal string GetCommandText()
        {
            return this.unencodedStringBuilder.ToString();
        }
        #endregion

        #region Private Methods
        private static string GetSchemaName(EntitySet entitySet)
        {
            return entitySet.Schema ?? entitySet.EntityContainer.Name;
        }

        private static string GetTableName(EntitySet entitySet)
        {
            return entitySet.Table ?? entitySet.Name;
        }

        private void AppendCreateForeignKeys(AssociationSet associationSet)
        {
            var constraint = associationSet.ElementType.ReferentialConstraints.Single();
            var principalEnd = associationSet.AssociationSetEnds[constraint.FromRole.Name];
            var dependentEnd = associationSet.AssociationSetEnds[constraint.ToRole.Name];

            // If any of the participating entity sets was skipped, skip the association too
            if (ignoredEntitySets.Contains(principalEnd.EntitySet) || ignoredEntitySets.Contains(dependentEnd.EntitySet))
            {
                AppendSql("-- Ignoring association set with participating entity set with defining query: ");
                AppendIdentifierEscapeNewLine(associationSet.Name);
            }
            else
            {
                AppendSql("alter table ");
                AppendIdentifier(dependentEnd.EntitySet);
                AppendSql(" add constraint ");
                AppendIdentifier(associationSet.Name);
                AppendSql(" foreign key (");
                AppendIdentifiers(constraint.ToProperties);
                AppendSql(") references ");
                AppendIdentifier(principalEnd.EntitySet);
                AppendSql("(");
                AppendIdentifiers(constraint.FromProperties);
                AppendSql(")");
                if (principalEnd.CorrespondingAssociationEndMember.DeleteBehavior == OperationAction.Cascade)
                {
                    AppendSql(" on delete cascade");
                }
                AppendSql(";");
            }
            AppendNewLine();
        }

        private void AppendCreateTable(EntitySet entitySet)
        {
            // If the entity set has defining query, skip it
            if (entitySet.DefiningQuery != null)
            {
                AppendSql("-- Ignoring entity set with defining query: ");
                AppendIdentifier(entitySet, AppendIdentifierEscapeNewLine);
                ignoredEntitySets.Add(entitySet);
            }
            else
            {
                AppendSql("create table ");
                AppendIdentifier(entitySet);
                AppendSql(" (");
                AppendNewLine();

                foreach (EdmProperty column in entitySet.ElementType.Properties)
                {
                    AppendSql("    ");
                    AppendIdentifier(column.Name);
                    AppendSql(" ");
                    AppendType(column);
                    AppendSql(",");
                    AppendNewLine();
                }

                AppendSql("    primary key (");
                AppendJoin(entitySet.ElementType.KeyMembers, k => AppendIdentifier(k.Name), ", ");
                AppendSql(")");
                AppendNewLine();

                AppendSql(");");
            }
            AppendNewLine();
        }

        private void AppendCreateSchema(string schema)
        {
            AppendSql("if (schema_id(");
            AppendStringLiteral(schema);
            AppendSql(") is null) exec(");

            // need to create a sub-command and escape it as a string literal as well...
            SqlDdlBuilder schemaBuilder = new SqlDdlBuilder();
            schemaBuilder.AppendSql("create schema ");
            schemaBuilder.AppendIdentifier(schema);

            AppendStringLiteral(schemaBuilder.unencodedStringBuilder.ToString());
            AppendSql(");");
            AppendNewLine();
        }

        private void AppendIdentifier(EntitySet table)
        {
            AppendIdentifier(table, AppendIdentifier);
        }

        private void AppendIdentifier(EntitySet table, Action<string> AppendIdentifierEscape)
        {
            string schemaName = GetSchemaName(table);
            string tableName = GetTableName(table);
            if (schemaName != null)
            {
                AppendIdentifierEscape(schemaName);
                AppendSql(".");
            }
            AppendIdentifierEscape(tableName);
        }

        private void AppendStringLiteral(string literalValue)
        {
            AppendSql("N'" + literalValue.Replace("'", "''") + "'");
        }

        private void AppendIdentifiers(IEnumerable<EdmProperty> properties)
        {
            AppendJoin(properties, p => AppendIdentifier(p.Name), ", ");
        }

        private void AppendIdentifier(string identifier)
        {
            AppendSql("[" + identifier.Replace("]", "]]") + "]");
        }

        private void AppendIdentifierEscapeNewLine(string identifier)
        {
            AppendIdentifier(identifier.Replace("\r", "\r--").Replace("\n", "\n--"));
        }

        private void AppendFileName(string path)
        {
            AppendSql("(name=");
            AppendStringLiteral(Path.GetFileName(path));
            AppendSql(", filename=");
            AppendStringLiteral(path);
            AppendSql(")");
        }

        private void AppendJoin<T>(IEnumerable<T> elements, Action<T> appendElement, string unencodedSeparator)
        {
            bool first = true;
            foreach (T element in elements)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    AppendSql(unencodedSeparator);
                }
                appendElement(element);
            }
        }

        private void AppendType(EdmProperty column)
        {
            TypeUsage type = column.TypeUsage;

            // check for rowversion-like configurations
            Facet storeGenFacet;
            bool isTimestamp = false;
            if (type.EdmType.Name == "binary" &&
                8 == type.GetMaxLength() &&
                column.TypeUsage.Facets.TryGetValue("StoreGeneratedPattern", false, out storeGenFacet) &&
                storeGenFacet.Value != null &&
                StoreGeneratedPattern.Computed == (StoreGeneratedPattern)storeGenFacet.Value)
            {
                isTimestamp = true;
                AppendIdentifier("rowversion");
            }
            else
            {
                string typeName = type.EdmType.Name;
                // Special case: the EDM treats 'nvarchar(max)' as a type name, but SQL Server treats
                // it as a type 'nvarchar' and a type qualifier. As such, we can't escape the entire
                // type name as the EDM sees it.
                const string maxSuffix = "(max)";
                if (type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType && typeName.EndsWith(maxSuffix, StringComparison.Ordinal))
                {
                    Debug.Assert(new[] { "nvarchar(max)", "varchar(max)", "varbinary(max)" }.Contains(typeName),
                        "no other known SQL Server primitive types types accept (max)");
                    AppendIdentifier(typeName.Substring(0, typeName.Length - maxSuffix.Length));
                    AppendSql("(max)");
                }
                else
                {
                    AppendIdentifier(typeName);
                }
                switch (type.EdmType.Name)
                {
                    case "decimal":
                    case "numeric":
                        AppendSqlInvariantFormat("({0}, {1})", type.GetPrecision(), type.GetScale());
                        break;
                    case "datetime2":
                    case "datetimeoffset":
                    case "time":
                        AppendSqlInvariantFormat("({0})", type.GetPrecision());
                        break;
                    case "binary":
                    case "varbinary":
                    case "nvarchar":
                    case "varchar":
                    case "char":
                    case "nchar":
                        AppendSqlInvariantFormat("({0})", type.GetMaxLength());
                        break;
                    default:
                        break;
                }
            }
            AppendSql(column.Nullable ? " null" : " not null");

            if (!isTimestamp && column.TypeUsage.Facets.TryGetValue("StoreGeneratedPattern", false, out storeGenFacet) &&
                storeGenFacet.Value != null)
            {
                StoreGeneratedPattern storeGenPattern = (StoreGeneratedPattern)storeGenFacet.Value;
                if (storeGenPattern == StoreGeneratedPattern.Identity)
                {
                    if (type.EdmType.Name == "uniqueidentifier")
                    {
                        AppendSql(" default newid()");
                    }
                    else
                    {
                        AppendSql(" identity");
                    }
                }
            }
        }

        #region Access to underlying string builder
        /// <summary>
        /// Appends raw SQL into the string builder.
        /// </summary>
        /// <param name="text">Raw SQL string to append into the string builder.</param>
        private void AppendSql(string text)
        {
            unencodedStringBuilder.Append(text);
        }

        /// <summary>
        /// Appends new line for visual formatting or for ending a comment.
        /// </summary>
        private void AppendNewLine()
        {
            unencodedStringBuilder.Append("\r\n");
        }

        /// <summary>
        /// Append raw SQL into the string builder with formatting options and invariant culture formatting.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An array of objects to format.</param>
        private void AppendSqlInvariantFormat(string format, params object[] args)
        {
            unencodedStringBuilder.AppendFormat(CultureInfo.InvariantCulture, format, args);
        }
        #endregion
        #endregion
    }
}
