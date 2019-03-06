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
using DbLinq.Util;

namespace DbLinq.Firebird
{
    partial class FirebirdSchemaLoader
    {
        public class DataStoredProcedure
        {
            public string TableSchema;
            public string Name;
            public string Type;
            public string ParameterName;
            public bool IsOutputParameter;
            public string ParameterType;
            public long? Length;
            public int? Precision;
            public int? Scale;
            public string DefaultValue;
            public bool BodyContainsSelectStatement;

            public override string ToString()
            {
                return "ProcRow " + Name;
            }
        }

        DataStoredProcedure ReadProcedure(IDataReader rdr)
        {
            DataStoredProcedure procedure = new DataStoredProcedure();
            int field = 0;
            procedure.TableSchema = rdr.GetAsString(field++);
            procedure.Name = rdr.GetAsString(field++).Trim();
            procedure.Type = rdr.GetAsString(field++);
            procedure.ParameterName = rdr.GetAsString(field++).Trim();
            procedure.IsOutputParameter = rdr.GetAsBool(field++);
            procedure.ParameterType = rdr.GetAsString(field++).Trim();
            procedure.Length = rdr.GetAsNullableNumeric<long>(field++);
            procedure.Precision = rdr.GetAsNullableNumeric<int>(field++);
            procedure.Scale = rdr.GetAsNullableNumeric<int>(field++);
            procedure.DefaultValue = rdr.GetAsString(field++);
            procedure.BodyContainsSelectStatement = rdr.GetAsBool(field++);
            return procedure;
        }

        public List<DataStoredProcedure> ReadProcedures(IDbConnection conn, string db)
        {
            string sql = @"
select 'Foo' ""TableSchema""
        , p.RDB$PROCEDURE_NAME ""Name""
        , 'PROCEDURE' ""Type""
        , pp.RDB$PARAMETER_NAME ""ParameterName""
        , pp.RDB$PARAMETER_TYPE ""IsOutputParameter""
        , t.RDB$TYPE_NAME ""ParameterType""
        , f.RDB$FIELD_LENGTH ""Length""
        , f.RDB$FIELD_PRECISION ""Precision""
        , f.RDB$FIELD_SCALE ""Scale""
        , pp.RDB$DEFAULT_VALUE ""DefaultValue""
        , case when p.RDB$PROCEDURE_OUTPUTS is null then 0 else 1 end ""BodyContainsSelectStatement""
    from RDB$PROCEDURES p
        inner join RDB$PROCEDURE_PARAMETERS pp on pp.RDB$PROCEDURE_NAME = p.RDB$PROCEDURE_NAME
        inner join RDB$FIELDS f on f.RDB$FIELD_NAME = pp.RDB$FIELD_SOURCE
        inner join RDB$TYPES t on t.RDB$TYPE = f.RDB$FIELD_TYPE and t.RDB$FIELD_NAME = 'RDB$FIELD_TYPE'
    where p.RDB$SYSTEM_FLAG = 0
union
select @db ""TableSchema""
        , p.RDB$FUNCTION_NAME ""Name""
        , 'FUNCTION' ""Type""
        , pp.RDB$FUNCTION_NAME ""ParameterName""
        , case when pp.rdb$mechanism = 5 then 1 else 0 end ""IsOutputParameter""
        , t.RDB$TYPE_NAME ""ParameterType""
        , pp.RDB$FIELD_LENGTH ""Length""
        , pp.RDB$FIELD_PRECISION ""Precision""
        , pp.RDB$FIELD_SCALE ""Scale""
        , null ""DefaultValue""
        , 0 ""BodyContainsSelectStatement""
    from RDB$FUNCTIONS p
        inner join RDB$FUNCTION_ARGUMENTS pp on pp.RDB$FUNCTION_NAME = p.RDB$FUNCTION_NAME
        inner join RDB$TYPES t on t.RDB$TYPE = pp.RDB$FIELD_TYPE and t.RDB$FIELD_NAME = 'RDB$FIELD_TYPE'
    where p.RDB$SYSTEM_FLAG = 0
";

            return DataCommand.Find<DataStoredProcedure>(conn, sql, "@db", db.ToLower(), ReadProcedure);
        }
    }
}
