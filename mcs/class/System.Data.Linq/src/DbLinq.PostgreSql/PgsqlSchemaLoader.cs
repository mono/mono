#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;
using DbLinq.Util;
using DbLinq.Vendor.Implementation;
using DbLinq.Logging;

namespace DbLinq.PostgreSql
{
    partial class PgsqlSchemaLoader : SchemaLoader
    {
        private readonly Vendor.IVendor vendor = new PgsqlVendor();
        public override Vendor.IVendor Vendor { get { return vendor; } }

        public override System.Type DataContextType { get { return typeof(PgsqlDataContext); } }

        protected override void LoadStoredProcedures(Database schema, SchemaName schemaName, IDbConnection conn, NameFormat nameFormat)
        {
            var procs = ReadProcedures(conn, schemaName.DbName);

            //4a. determine unknown types
            Dictionary<long, string> typeOidToName = new Dictionary<long, string>();

            foreach (DataStoredProcedure proc in procs)
            {
                if (proc.proallargtypes == null && !string.IsNullOrEmpty(proc.proargtypes))
                    proc.proallargtypes = "{" + proc.proargtypes.Replace(' ', ',') + "}"; //work around pgsql weirdness?
            }

            foreach (DataStoredProcedure proc in procs)
            {
                typeOidToName[proc.prorettype] = proc.formatted_prorettype;
                if (proc.proallargtypes == null)
                    continue; //no args, no Oids to resolve, skip

                string[] argTypes1 = parseCsvString(proc.proallargtypes); //eg. {23,24,1043}
                var argTypes2 = from t in argTypes1 select long.Parse(t);

                foreach (long argType in argTypes2)
                {
                    if (!typeOidToName.ContainsKey(argType))
                        typeOidToName[argType] = null;
                }
            }

            //4b. get names for unknown types
            GetTypeNames(conn, schemaName.DbName, typeOidToName);

            //4c. generate dbml objects
            foreach (DataStoredProcedure proc in procs)
            {
                DbLinq.Schema.Dbml.Function dbml_fct = ParseFunction(proc, typeOidToName, nameFormat);
                if (!SkipProc(dbml_fct.Name))
                    schema.Functions.Add(dbml_fct);
            }
        }

        protected override void LoadConstraints(Database schema, SchemaName schemaName, IDbConnection conn, NameFormat nameFormat, Names names)
        {
            //TableSorter.Sort(tables, constraints); //sort tables - parents first

            var constraints = ReadConstraints(conn, schemaName.DbName);

            var allKeys2 = ReadForeignConstraints(conn, schemaName.DbName);
            var foreignKeys = allKeys2.Where(k => k.ConstraintType == "FOREIGN KEY").ToList();
            var primaryKeys = allKeys2.Where(k => k.ConstraintType == "PRIMARY KEY").ToList();


            foreach (DataConstraint keyColRow in constraints)
            {
                //find my table:
                string constraintFullDbName = GetFullDbName(keyColRow.TableName, keyColRow.TableSchema);
                DbLinq.Schema.Dbml.Table table = schema.Tables.FirstOrDefault(t => constraintFullDbName == t.Name);
                if (table == null)
                {
                    Logger.Write(Level.Error, "ERROR L138: Table '" + keyColRow.TableName + "' not found for column " + keyColRow.ColumnName);
                    continue;
                }

                //todo: must understand better how PKEYs are encoded.
                //In Sasha's DB, they don't end with "_pkey", you need to rely on ReadForeignConstraints().
                //In Northwind, they do end with "_pkey".
                bool isPrimaryKey = keyColRow.ConstraintName.EndsWith("_pkey")
                    || primaryKeys.Count(k => k.ConstraintName == keyColRow.ConstraintName) == 1;

                if (isPrimaryKey)
                {
                    //A) add primary key
                    DbLinq.Schema.Dbml.Column primaryKeyCol = table.Type.Columns.First(c => c.Name == keyColRow.ColumnName);
                    primaryKeyCol.IsPrimaryKey = true;
                }
                else
                {
                    DataForeignConstraint dataForeignConstraint = foreignKeys.FirstOrDefault(f => f.ConstraintName == keyColRow.ConstraintName);

                    if (dataForeignConstraint == null)
                    {
                        string msg = "Missing data from 'constraint_column_usage' for foreign key " + keyColRow.ConstraintName;
                        Logger.Write(Level.Error, msg);
                        //throw new ApplicationException(msg);
                        continue; //as per Andrus, do not throw. //putting together an Adnrus_DB test case.
                    }

                    LoadForeignKey(schema, table, keyColRow.ColumnName, keyColRow.TableName, keyColRow.TableSchema,
                                   dataForeignConstraint.ColumnName, dataForeignConstraint.ReferencedTableName,
                                   dataForeignConstraint.ReferencedTableSchema,
                                   keyColRow.ConstraintName, nameFormat, names);

                }

            }
        }

        #region function parsing

        /// <summary>
        /// parse pg param modes string such as '{i,i,o}'
        /// </summary>
        static string[] parseCsvString(string csvString)
        {
            if (csvString == null || (!csvString.StartsWith("{")) || (!csvString.EndsWith("}")))
                return null;
            List<string> list = new List<string>();
            string middle = csvString.Substring(1, csvString.Length - 2);
            string[] parts = middle.Split(',');
            return parts;
        }

        Function ParseFunction(DataStoredProcedure pg_proc, Dictionary<long, string> typeOidToName, NameFormat nameFormat)
        {
            var procedureName = CreateProcedureName(pg_proc.proname, null, nameFormat);

            DbLinq.Schema.Dbml.Function dbml_func = new Function();
            dbml_func.Name = procedureName.DbName;
            dbml_func.Method = procedureName.MethodName;

            if (pg_proc.formatted_prorettype != null && string.Compare(pg_proc.formatted_prorettype, "void") != 0)
            {
                var dbml_param = new Return();
                dbml_param.DbType = pg_proc.formatted_prorettype;
                dbml_param.Type = MapDbType(null, new DataType { Type = pg_proc.formatted_prorettype }).ToString();
                dbml_func.Return = dbml_param;
                dbml_func.IsComposable = true;
            }

            if (pg_proc.proallargtypes != null)
            {
                string[] argModes = parseCsvString(pg_proc.proargmodes);
                string[] argNames = parseCsvString(pg_proc.proargnames);
                string[] argTypes1 = parseCsvString(pg_proc.proallargtypes); //eg. {23,24,1043}
                List<long> argTypes2 = (from t in argTypes1 select long.Parse(t)).ToList();

                if (argNames == null)
                {
                    //proc was specified as 'FUNCTION doverlaps(IN date)' - names not specified
                    argNames = new string[argTypes1.Length];
                    for (int i = 0; i < argNames.Length; i++) { argNames[i] = ((char)('a' + i)).ToString(); }
                }

                bool doLengthsMatch = (argTypes2.Count != argNames.Length
                    || (argModes != null && argModes.Length != argNames.Length));
                if (doLengthsMatch)
                {
                    Logger.Write(Level.Error, "L238 Mistmatch between modesArr, typeArr and nameArr for func " + pg_proc.proname);
                    return null;
                }

                List<DbLinq.Schema.Dbml.Parameter> paramList = new List<Parameter>();
                for (int i = 0; i < argNames.Length; i++)
                {
                    DbLinq.Schema.Dbml.Parameter dbml_param = new Parameter();
                    long argTypeOid = argTypes2[i];
                    dbml_param.DbType = typeOidToName[argTypeOid];
                    dbml_param.Name = argNames[i];
                    dbml_param.Type = MapDbType(argNames[i], new DataType { Type = dbml_param.DbType }).ToString();
                    string inOut = argModes == null ? "i" : argModes[i];
                    dbml_param.Direction = ParseInOut(inOut);
                    dbml_func.Parameters.Add(dbml_param);
                }
            }

            return dbml_func;
        }

        static DbLinq.Schema.Dbml.ParameterDirection ParseInOut(string inOut)
        {
            switch (inOut)
            {
                case "i": return DbLinq.Schema.Dbml.ParameterDirection.In;
                case "o": return DbLinq.Schema.Dbml.ParameterDirection.Out;
                case "b": return DbLinq.Schema.Dbml.ParameterDirection.InOut;
                default: return DbLinq.Schema.Dbml.ParameterDirection.InOut;
            }
        }

        #endregion

        private bool SkipProc(string name)
        {
            //string[] prefixes = System.Configuration.ConfigurationManager.AppSettings["postgresqlSkipProcPrefixes"].Split(',');
            string[] prefixes = { "pldbg", "gbtreekey", "gbt_", "pg_buffercache", "plpgsql_", "plpgsql_call_handler" };

            foreach (string s in prefixes)
            {
                if (name.StartsWith(s))
                    return true;
            }
            return false;
        }

    }
}
