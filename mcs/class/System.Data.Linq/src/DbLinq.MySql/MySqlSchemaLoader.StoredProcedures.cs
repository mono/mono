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

namespace DbLinq.MySql
{
    partial class MySqlSchemaLoader
    {
        /// <summary>
        /// represents one row from MySQL's MYSQL.PROC table
        /// </summary>
        public class DataStoredProcedure
        {
            public string db;
            public string name;
            public string type;
            public string specific_name;
            public string param_list;
            public string returns;
            public string body;

            public override string ToString()
            {
                return "ProcRow " + name;
            }
        }

        DataStoredProcedure ReadProcedure(IDataReader rdr)
        {
            DataStoredProcedure procedure = new DataStoredProcedure();
            int field = 0;
            procedure.db = rdr.GetAsString(field++);
            procedure.name = rdr.GetAsString(field++);
            procedure.type = rdr.GetAsString(field++);
            procedure.specific_name = rdr.GetAsString(field++);

            procedure.param_list = rdr.GetString(field++); // for some obscure reasons, GetAsString() doesn't work
            procedure.returns = rdr.GetAsString(field++);
            procedure.body = rdr.GetString(field++);
            return procedure;
        }

        // We use mysql.PROC instead of information_schema.ROUTINES, because it saves us parsing of parameters.
        // Note: higher permissions are required to access mysql.PROC.
        public List<DataStoredProcedure> ReadProcedures(IDbConnection conn, string db)
        {
            string sql = @"
SELECT db, name, type, specific_name, param_list, returns, body
FROM mysql.proc
WHERE db=?db AND type IN ('FUNCTION','PROCEDURE')";

            return DataCommand.Find<DataStoredProcedure>(conn, sql, "?db", db.ToLower(), ReadProcedure);
        }
    }
}
