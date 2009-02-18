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

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DbLinq.Schema;
using DbLinq.Schema.Dbml;
using DbLinq.Util;

namespace DbLinq.Oracle
{
    partial class OracleSchemaLoader
    {
        protected class StoredProcedureParameter
        {
            public string ProcedureName { get; set; }
            public string Name { get; set; }
            public string Schema { get; set; }
            public DataType Type { get; set; }
            public bool In { get; set; }
            public bool Out { get; set; }
        }

        protected virtual StoredProcedureParameter ReadParameter(IDataRecord dataRecord)
        {
            var parameter = new StoredProcedureParameter();
            int field = 0;
            parameter.ProcedureName = dataRecord.GetAsString(field++);
            parameter.Name = dataRecord.GetAsString(field++);
            parameter.Schema = dataRecord.GetAsString(field++);
            parameter.Type = new DataType();
            parameter.Type.Type = dataRecord.GetAsString(field++);
            parameter.Type.Length = dataRecord.GetAsNullableNumeric<long>(field++);
            parameter.Type.Precision = dataRecord.GetAsNullableNumeric<int>(field++);
            parameter.Type.Scale = dataRecord.GetAsNullableNumeric<int>(field++);
            string inOut = dataRecord.GetAsString(field++).ToLower();
            parameter.In = inOut.Contains("in");
            parameter.Out = inOut.Contains("out");
            return parameter;
        }

        protected virtual IList<StoredProcedureParameter> ReadParameters(IDbConnection connection, string databaseName)
        {
            const string sql = @"select object_name, argument_name, owner, data_type, data_length, data_precision, data_scale, in_out
from all_arguments where lower(owner) = :db order by object_id, position";

            return DataCommand.Find<StoredProcedureParameter>(connection, sql, ":db", databaseName.ToLower(), ReadParameter);
        }

        protected override void LoadStoredProcedures(Database schema, SchemaName schemaName, IDbConnection conn, NameFormat nameFormat)
        {
            var parameters = ReadParameters(conn, schemaName.DbName);
            foreach (var parameter in parameters)
            {
                var procedureName = CreateProcedureName(parameter.ProcedureName, parameter.Schema, nameFormat);

                Function function = schema.Functions.SingleOrDefault(f => f.Method == procedureName.MethodName);
                if (function == null)
                {
                    function = new Function { Name = procedureName.DbName, Method = procedureName.MethodName };
                    schema.Functions.Add(function);
                }

                if (parameter.Name == null)
                {
                    var returnParameter = new Return();
                    returnParameter.DbType = parameter.Type.Type;
                    returnParameter.Type = MapDbType(parameter.Name, parameter.Type).ToString();

                    function.IsComposable = true;
                    function.Return = returnParameter;
                }
                else
                {
                    var functionParameter = new Parameter();
                    functionParameter.DbType = parameter.Type.Type;
                    functionParameter.Type = MapDbType(parameter.Name, parameter.Type).ToString();
                    if (parameter.In)
                    {
                        if (parameter.Out)
                            functionParameter.Direction = DbLinq.Schema.Dbml.ParameterDirection.InOut;
                        else
                            functionParameter.Direction = DbLinq.Schema.Dbml.ParameterDirection.In;
                    }
                    else
                        functionParameter.Direction = DbLinq.Schema.Dbml.ParameterDirection.Out;

                    var parameterName = CreateParameterName(parameter.Name, nameFormat);
                    functionParameter.Name = parameterName.CallName;

                    function.Parameters.Add(functionParameter);
                }
            }
        }
    }
}
