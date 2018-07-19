using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace System.Data.Linq.SqlClient {
    using System.Data.Linq.Mapping;
    using System.Data.Linq.Provider;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Class for building up SQL DDL commands.
    /// </summary>
    internal static class SqlBuilder {

        internal static string GetCreateDatabaseCommand(string catalog, string dataFilename, string logFilename) {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("CREATE DATABASE {0}", SqlIdentifier.QuoteIdentifier(catalog));
            if (dataFilename != null) {
                sb.AppendFormat(" ON PRIMARY (NAME='{0}', FILENAME='{1}')", Path.GetFileName(dataFilename), dataFilename);
                sb.AppendFormat(" LOG ON (NAME='{0}', FILENAME='{1}')", Path.GetFileName(logFilename), logFilename);
            }
            return sb.ToString();
        }

        internal static string GetDropDatabaseCommand(string catalog) {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("DROP DATABASE {0}", SqlIdentifier.QuoteIdentifier(catalog));
            return sb.ToString();
        }

        internal static string GetCreateSchemaForTableCommand(MetaTable table) {
            StringBuilder sb = new StringBuilder();
            List<string> parts = new List<string>(SqlIdentifier.GetCompoundIdentifierParts(table.TableName));

            // table names look like this in Yukon (according to MSDN):
            //     [ database_name . [ schema_name ] . | schema_name . ] table_name 
            // ... which means that either way, the schema name is the second to last part.

            if ((parts.Count) < 2) {
                return null;
            }
            
            string schema = parts[parts.Count - 2];
            if (String.Compare(schema, "DBO", StringComparison.OrdinalIgnoreCase) != 0 &&
                String.Compare(schema, "[DBO]", StringComparison.OrdinalIgnoreCase) != 0) {
                sb.AppendFormat("CREATE SCHEMA {0}", SqlIdentifier.QuoteIdentifier(schema));
            }
            return sb.ToString();
        }

        internal static string GetCreateTableCommand(MetaTable table) {
            StringBuilder sb = new StringBuilder();
            StringBuilder decl = new StringBuilder();
            BuildFieldDeclarations(table, decl);
            sb.AppendFormat("CREATE TABLE {0}", SqlIdentifier.QuoteCompoundIdentifier(table.TableName));
            sb.Append("(");
            sb.Append(decl.ToString());
            decl = new StringBuilder();
            BuildPrimaryKey(table, decl);
            if (decl.Length > 0) {
                string name = String.Format(Globalization.CultureInfo.InvariantCulture, "PK_{0}", table.TableName);
                sb.Append(", ");
                sb.AppendLine();
                sb.AppendFormat("  CONSTRAINT {0} PRIMARY KEY ({1})", SqlIdentifier.QuoteIdentifier(name), decl.ToString());
            }
            sb.AppendLine();
            sb.Append("  )");
            return sb.ToString();
        }

        internal static void BuildFieldDeclarations(MetaTable table, StringBuilder sb) {
            int n = 0;
            Dictionary<object, string> memberNameToMappedName = new Dictionary<object, string>();
            foreach (MetaType type in table.RowType.InheritanceTypes) {
                n += BuildFieldDeclarations(type, memberNameToMappedName, sb);
            }
            if (n == 0) {
                throw Error.CreateDatabaseFailedBecauseOfClassWithNoMembers(table.RowType.Type);
            }
        }

        private static int BuildFieldDeclarations(MetaType type, Dictionary<object, string> memberNameToMappedName, StringBuilder sb) {
            int n = 0;
            foreach (MetaDataMember mm in type.DataMembers) {
                // Only generate declarations for the current type.
                if (mm.IsDeclaredBy(type)) {
                    if (!mm.IsAssociation) {
                        if (mm.IsPersistent) {
                            object dn = InheritanceRules.DistinguishedMemberName(mm.Member);
                            string mappedName;
                            if (memberNameToMappedName.TryGetValue(dn, out mappedName)) {
                                if (mappedName == mm.MappedName) {
                                    continue;
                                }
                            }
                            else {
                                memberNameToMappedName.Add(dn, mm.MappedName);
                            }
                            if (sb.Length > 0) {
                                sb.Append(", ");
                            }
                            sb.AppendLine();
                            sb.Append(string.Format(Globalization.CultureInfo.InvariantCulture, "  {0} ", SqlIdentifier.QuoteCompoundIdentifier(mm.MappedName)));
                            if (!string.IsNullOrEmpty(mm.Expression)) {
                                // Generate "AS <expression>" for computed columns
                                sb.Append("AS " + mm.Expression);
                            }
                            else {
                                sb.Append(GetDbType(mm));
                            }
                            n++;
                        }
                    }
                }
            }
            return n;
        }

        private static void BuildPrimaryKey(MetaTable table, StringBuilder sb) {
            foreach (MetaDataMember mm in table.RowType.IdentityMembers) {
                if (sb.Length > 0) {
                    sb.Append(", ");
                }
                sb.Append(SqlIdentifier.QuoteCompoundIdentifier(mm.MappedName));
            }
        }

        private static string BuildKey(IEnumerable<MetaDataMember> members) {
            StringBuilder sb = new StringBuilder();
            foreach (MetaDataMember mm in members) {
                if (sb.Length > 0) {
                    sb.Append(", ");
                }
                sb.Append(SqlIdentifier.QuoteCompoundIdentifier(mm.MappedName));
            }
            return sb.ToString();
        }

        internal static IEnumerable<String> GetCreateForeignKeyCommands(MetaTable table) {
            foreach (MetaType type in table.RowType.InheritanceTypes) {
                foreach (string command in GetCreateForeignKeyCommands(type)) {
                    yield return command;
                }
            }
        }

        private static IEnumerable<String> GetCreateForeignKeyCommands(MetaType type) {
            string tableName = type.Table.TableName;
            foreach (MetaDataMember mm in type.DataMembers) {
                if (mm.IsDeclaredBy(type) && mm.IsAssociation) {
                    MetaAssociation assoc = mm.Association;
                    if (assoc.IsForeignKey) {
                        StringBuilder sb = new StringBuilder();
                        string thisKey = BuildKey(assoc.ThisKey);
                        string otherKey = BuildKey(assoc.OtherKey);
                        string otherTable = assoc.OtherType.Table.TableName;
                        string name;
                        name = mm.MappedName;
                        if (name == mm.Name) {
                            name = String.Format(Globalization.CultureInfo.InvariantCulture, "FK_{0}_{1}", tableName, mm.Name);
                        }
                        string cmd = "ALTER TABLE {0}" + Environment.NewLine + "  ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3}({4})";
                        //In DLinq we put the constraint on the child object (which triggers the behavior when deleted),
                        //but in SQL it is part of the parent constraint (the parent row gets changed / deleted to satisfy the constraint)
                        MetaDataMember otherMember = mm.Association.OtherMember;
                        if (otherMember != null) {
                            string delConstr = otherMember.Association.DeleteRule;
                            if (delConstr != null) {
                                cmd += Environment.NewLine + "  ON DELETE " + delConstr;
                            }
                        }
                        sb.AppendFormat(cmd,
                            SqlIdentifier.QuoteCompoundIdentifier(tableName),
                            SqlIdentifier.QuoteIdentifier(name),
                            SqlIdentifier.QuoteCompoundIdentifier(thisKey),
                            SqlIdentifier.QuoteCompoundIdentifier(otherTable),
                            SqlIdentifier.QuoteCompoundIdentifier(otherKey));
                        yield return sb.ToString();
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        private static string GetDbType(MetaDataMember mm) {
            string dbType = mm.DbType;

            if (dbType != null) return dbType;
            StringBuilder sb = new StringBuilder();

            Type type = mm.Type;
            bool isNullable = mm.CanBeNull;
            if (type.IsValueType && IsNullable(type)) {
                type = type.GetGenericArguments()[0];
            }

            if (mm.IsVersion) {
                sb.Append("Timestamp");
            }
            else {
                if (mm.IsPrimaryKey && mm.IsDbGenerated) {
                    switch (Type.GetTypeCode(type)) {
                        case TypeCode.Byte:
                            sb.Append("TinyInt");
                            break;
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                            sb.Append("SmallInt");
                            break;
                        case TypeCode.Int32:
                        case TypeCode.UInt16:
                            sb.Append("Int");
                            break;
                        case TypeCode.Int64:
                        case TypeCode.UInt32:
                            sb.Append("BigInt");
                            break;
                        case TypeCode.UInt64:
                        case TypeCode.Decimal:
                            sb.Append("Decimal(20)");
                            break;
                        case TypeCode.Object:
                            if (type == typeof(Guid)) {
                                sb.Append("UniqueIdentifier");
                            } else {
                                throw Error.CouldNotDetermineDbGeneratedSqlType(type);
                            }
                            break;
                    }
                }
                else {
                    switch (Type.GetTypeCode(type)) {
                        case TypeCode.Boolean:
                           sb.Append("Bit");
                            break;
                        case TypeCode.Byte:
                            sb.Append("TinyInt");
                            break;
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                            sb.Append("SmallInt");
                            break;
                        case TypeCode.Int32:
                        case TypeCode.UInt16:
                            sb.Append("Int");
                            break;
                        case TypeCode.Int64:
                        case TypeCode.UInt32:
                            sb.Append("BigInt");
                            break;
                        case TypeCode.UInt64:
                            sb.Append("Decimal(20)");
                            break;
                        case TypeCode.Decimal:
                            sb.Append("Decimal(29, 4)");
                            break;
                        case TypeCode.Double:
                            sb.Append("Float");
                            break;
                        case TypeCode.Single:
                            sb.Append("Real");
                            break;
                        case TypeCode.Char:
                            sb.Append("NChar(1)");
                            break;
                        case TypeCode.String:
                            sb.Append("NVarChar(4000)");
                            break;
                        case TypeCode.DateTime:
                            sb.Append("DateTime");
                            break;
                            case TypeCode.Object:
                                if (type == typeof(Guid)) {
                                    sb.Append("UniqueIdentifier");
                                } else if (type == typeof(byte[])) {
                                    sb.Append("VarBinary(8000)");
                                } else if (type == typeof(char[])) {
                                    sb.Append("NVarChar(4000)");
                                } else if (type == typeof(DateTimeOffset)) {
                                    sb.Append("DateTimeOffset");
                                } else if (type == typeof(TimeSpan)) {
                                    sb.Append("Time");
                                } else {
                                    throw Error.CouldNotDetermineSqlType(type);
                                }
                                break;
                     }
                }
            }

            if (!isNullable) {
                sb.Append(" NOT NULL");
            }

            if (mm.IsPrimaryKey && mm.IsDbGenerated) {
                if (type == typeof(Guid)) {
                    sb.Append(" DEFAULT NEWID()");
                }
                else {
                    sb.Append(" IDENTITY");
                }
            }

            return sb.ToString();
        }

        internal static bool IsNullable(Type type) {
            return type.IsGenericType && typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition());
        }
    }
}
