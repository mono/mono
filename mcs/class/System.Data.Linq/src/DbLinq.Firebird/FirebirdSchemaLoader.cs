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
using System.IO;
using System.Linq;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;
using DbLinq.Util;
using DbLinq.Vendor;
using DbLinq.Vendor.Implementation;

namespace DbLinq.Firebird
{
    partial class FirebirdSchemaLoader : SchemaLoader
    {
        private readonly IVendor vendor = new FirebirdVendor();
        public override IVendor Vendor { get { return vendor; } }

        public override System.Type DataContextType { get { return typeof(FirebirdDataContext); } }

        protected override TableName CreateTableName(string dbTableName, string dbSchema, INameAliases nameAliases, NameFormat nameFormat)
        {
            return CreateTableName(dbTableName, dbSchema, nameAliases, nameFormat, WordsExtraction.FromDictionary);
        }

        /// <summary>
        /// Gets a usable name for the database.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns></returns>
        protected override string GetDatabaseName(string databaseName)
        {
            return Path.GetFileNameWithoutExtension(databaseName);
        }

        protected override void LoadStoredProcedures(Database schema, SchemaName schemaName, IDbConnection conn, NameFormat nameFormat)
        {
            // TODO: debug stored procedures support
            return;

            var procs = ReadProcedures(conn, schemaName.DbName);

            foreach (DataStoredProcedure proc in procs)
            {
                var procedureName = CreateProcedureName(proc.Name, proc.TableSchema, nameFormat);

                var func = new Function();

                func.Name = procedureName.DbName;
                func.Method = procedureName.MethodName;
                func.IsComposable = string.Compare(proc.Type, "FUNCTION") == 0;
                func.BodyContainsSelectStatement = proc.BodyContainsSelectStatement;
                ParseProcParams(proc, func);

                schema.Functions.Add(func);
            }
        }

        protected override void LoadConstraints(Database schema, SchemaName schemaName, IDbConnection conn, NameFormat nameFormat, Names names)
        {
            var constraints = ReadConstraints(conn, schemaName.DbName);

            //sort tables - parents first (this is moving to SchemaPostprocess)
            //TableSorter.Sort(tables, constraints); 

            foreach (DataConstraint keyColRow in constraints)
            {
                //find my table:
                string fullKeyDbName = GetFullDbName(keyColRow.TableName, keyColRow.TableSchema);
                DbLinq.Schema.Dbml.Table table = schema.Tables.FirstOrDefault(t => fullKeyDbName == t.Name);
                if (table == null)
                {
                    bool ignoreCase = true;
                    table = schema.Tables.FirstOrDefault(t => 0 == string.Compare(fullKeyDbName, t.Name, ignoreCase));
                    if (table == null)
                    {
                        WriteErrorLine("ERROR L46: Table '" + keyColRow.TableName + "' not found for column " + keyColRow.ColumnName);
                        continue;
                    }
                }

                bool isForeignKey = keyColRow.ConstraintName != "PRIMARY"
                                    && keyColRow.ReferencedTableName != null;

                if (isForeignKey)
                {
                    LoadForeignKey(schema, table, keyColRow.ColumnName, keyColRow.TableName, keyColRow.TableSchema,
                                   keyColRow.ReferencedColumnName, keyColRow.ReferencedTableName, keyColRow.ReferencedTableSchema,
                                   keyColRow.ConstraintName, nameFormat, names);
                }

            }
        }

        protected void ParseProcParams(DataStoredProcedure inputData, Function outputFunc)
        {
            //string paramString = inputData.ParamList;
            //if (string.IsNullOrEmpty(paramString))
            //{
            //    //nothing to parse
            //}
            //else
            //{
            //    string[] parts = paramString.Split(',');

            //    foreach (string part in parts) //part='OUT param1 int'
            //    {
            //        DbLinq.Schema.Dbml.Parameter paramObj = ParseParameterString(part);
            //        if (paramObj != null)
            //            outputFunc.Parameters.Add(paramObj);
            //    }
            //}

            //if (!string.IsNullOrEmpty(inputData.Returns))
            //{
            //    var paramRet = new Return();
            //    paramRet.DbType = inputData.Returns;
            //    paramRet.Type = ParseDbType(null, inputData.Returns);
            //    outputFunc.Return = paramRet;
            //}
        }

        /// <summary>
        /// parse strings such as 'INOUT param2 INT' or 'param4 varchar ( 32 )'
        /// </summary>
        /// <param name="paramStr"></param>
        /// <returns></returns>
        protected DbLinq.Schema.Dbml.Parameter ParseParameterString(string param)
        {
            param = param.Trim();
            var inOut = DbLinq.Schema.Dbml.ParameterDirection.In;

            if (param.StartsWith("IN", StringComparison.CurrentCultureIgnoreCase))
            {
                inOut = DbLinq.Schema.Dbml.ParameterDirection.In;
                param = param.Substring(2).Trim();
            }
            if (param.StartsWith("INOUT", StringComparison.CurrentCultureIgnoreCase))
            {
                inOut = DbLinq.Schema.Dbml.ParameterDirection.InOut;
                param = param.Substring(5).Trim();
            }
            if (param.StartsWith("OUT", StringComparison.CurrentCultureIgnoreCase))
            {
                inOut = DbLinq.Schema.Dbml.ParameterDirection.Out;
                param = param.Substring(3).Trim();
            }

            int indxSpace = param.IndexOfAny(new char[] { ' ', '\t' });
            if (indxSpace == -1)
                return null; //cannot find space between varName and varType

            string varName = param.Substring(0, indxSpace);
            string varType = param.Substring(indxSpace + 1);

            var paramObj = new Parameter();
            paramObj.Direction = inOut;
            paramObj.Name = varName;
            paramObj.DbType = varType;
            paramObj.Type = ParseDbType(varName, varType);

            return paramObj;
        }

        static System.Text.RegularExpressions.Regex re_CHARSET = new System.Text.RegularExpressions.Regex(@" CHARSET \w+$");
        /// <summary>
        /// given 'CHAR(30)', return 'string'
        /// </summary>
        protected string ParseDbType(string columnName, string dbType1)
        {
            //strip 'CHARSET latin1' from the end
            string dbType2 = re_CHARSET.Replace(dbType1, "");
            var dataType = new DataType();
            dataType.UnpackRawDbType(dbType2);
            return MapDbType(columnName, dataType).ToString();
        }

        /// <summary>
        /// This is a hack while I figure out a way to produce Dialect 3 types.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        protected override System.Type MapDbType(string columnName, IDataType dataType)
        {
            switch (dataType.Type)
            {
            // string
            case "CSTRING":
            case "TEXT":
            case "VARYING":
                return typeof(String);

            // int16
            case "SHORT":
                if (dataType.Unsigned ?? false)
                    return typeof(UInt16);
                return typeof(Int16);

            // int32
            case "LONG":
                if (dataType.Unsigned ?? false)
                    return typeof(UInt32);
                return typeof(Int32);

            // int64
            case "INT64":
                return typeof(Int64);

            // single
            case "FLOAT":
                return typeof(Single);

            // double
            case "DOUBLE":
                return typeof(Double);

            // decimal
            case "QUAD":
                return typeof(Decimal);

            // time interval
            case "TIME":
                return typeof(TimeSpan);

            // date
            case "TIMESTAMP":
            case "DATE":
                return typeof(DateTime);

            // byte[]
            case "BLOB":
            case "BLOB_ID":
                return typeof(Byte[]);

            // if we fall to this case, we must handle the type
            default:
                return null;
            }

        }
    }
}
