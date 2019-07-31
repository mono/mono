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
using System.Text;
using DbLinq.Util;

namespace DbLinq.PostgreSql
{
    partial class PgsqlSchemaLoader
    {
        /// <summary>
        /// represents one row from pg_proc table
        /// </summary>
        protected class DataStoredProcedure
        {
            public long proowner;
            public string proname;
            public bool proretset;
            public long prorettype;
            public string formatted_prorettype;

            /// <summary>
            /// species types of in-args, eg. '23 1043'
            /// </summary>
            public string proargtypes;

            /// <summary>
            /// species types of in,out args, eg. '{23,1043,1043}'
            /// </summary>
            public string proallargtypes;

            /// <summary>
            /// param names, eg {i1,i2,o2}
            /// </summary>
            public string proargnames;

            /// <summary>
            /// specifies in/out modes - eg. '{i,i,o}'
            /// </summary>
            public string proargmodes;

            public override string ToString() { return "Pg_Proc " + proname; }
        }

        protected virtual DataStoredProcedure ReadProcedure(IDataReader rdr)
        {
            DataStoredProcedure procedure = new DataStoredProcedure();
            int field = 0;
            procedure.proowner = rdr.GetAsNumeric<long>(field++);
            procedure.proname = rdr.GetAsString(field++);
            procedure.proretset = rdr.GetAsBool(field++);
            procedure.prorettype = rdr.GetAsNumeric<long>(field++);
            procedure.formatted_prorettype = rdr.GetAsString(field++);
            procedure.proargtypes = rdr.GetAsString(field++);
            procedure.proallargtypes = rdr.GetAsString( field++);
            procedure.proargnames = rdr.GetAsString(field++);
            procedure.proargmodes = rdr.GetAsString(field++);
            return procedure;
        }

        protected virtual List<DataStoredProcedure> ReadProcedures(IDbConnection conn, string db)
        {
            string sql = @"
SELECT pr.proowner, pr.proname, pr.proretset, pr.prorettype, pg_catalog.format_type(pr.prorettype, NULL) 
  ,pr.proargtypes, pr.proallargtypes, pr.proargnames, pr.proargmodes
FROM pg_proc pr, pg_type tp 
WHERE tp.oid = pr.prorettype AND pr.proisagg = FALSE 
AND tp.typname <> 'trigger' 
AND pr.pronamespace IN ( SELECT oid FROM pg_namespace 
WHERE nspname NOT LIKE 'pg_%' AND nspname != 'information_schema' ); 

";

            return DataCommand.Find<DataStoredProcedure>(conn, sql, ":db", db, ReadProcedure);
        }

        protected virtual int GetTypeNames(IDbConnection conn, string db, Dictionary<long, string> oid_to_name_map)
        {
            string sql = @"
SELECT pg_catalog.format_type(:typeOid, NULL)
";
            int numDone = 0;

            //clone to prevent 'collection was modified' exception
            Dictionary<long, string> oid_to_name_map2 = new Dictionary<long, string>(oid_to_name_map);

            foreach (var kv in oid_to_name_map2)
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    if (kv.Value != null)
                        continue; //value already known

                    long typeOid = kv.Key;
                    IDbDataParameter parameter = cmd.CreateParameter();
                    parameter.ParameterName = ":typeOid";
                    parameter.Value = typeOid;
                    cmd.Parameters.Add(parameter);
                    //cmd.CommandText = sql.Replace(":typeOid", typeOid.ToString());
                    numDone++;
                    object typeName1 = cmd.ExecuteScalar();
                    string typeName2 = typeName1 as string;
                    oid_to_name_map[typeOid] = typeName2; //eg. dic[23] = "integer"
                }
            }
            return numDone;
        }

    }
}
