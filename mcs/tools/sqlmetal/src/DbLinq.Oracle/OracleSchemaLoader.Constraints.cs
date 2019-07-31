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
using System.Text.RegularExpressions;
using DbLinq.Util;

namespace DbLinq.Oracle
{
    partial class OracleSchemaLoader
    {
        protected class DataConstraint
        {
            public string TableSchema;
            public string ConstraintName;
            public string TableName;
            public List<string> ColumnNames = new List<string>();
            public string ColumnNameList { get { return string.Join(",", ColumnNames.ToArray()); } }
            public string ConstraintType;
            public string ReverseConstraintName;
            public string Expression;

            public override string ToString()
            {
                return "User_Constraint  " + TableName + "." + ColumnNameList;
            }
        }

        private static Regex TriggerMatch1 = new Regex(@".*SELECT\s+(?<exp>\S+.*)\s+INTO\s+\:new.(?<col>\S+)\s+FROM\s+DUAL.*",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected bool MatchTrigger(Regex regex, string fullText, out string expression, out string column)
        {
            var match = regex.Match(fullText);
            if (match.Success)
            {
                expression = match.Groups["exp"].Value;
                column = match.Groups["col"].Value;
                return true;
            }
            expression = null;
            column = null;
            return false;
        }

        protected virtual DataConstraint ReadTrigger(IDataReader rdr)
        {
            var constraint = new DataConstraint();
            int field = 0;
            constraint.ConstraintName = rdr.GetAsString(field++);
            constraint.TableSchema = rdr.GetAsString(field++);
            constraint.TableName = rdr.GetAsString(field++);
            constraint.ConstraintType = "T";
            string body = rdr.GetAsString(field++);
            //BEGIN
            //   IF (:new."EmployeeID" IS NULL) THEN
            //        SELECT Employees_seq.NEXTVAL INTO :new."EmployeeID" FROM DUAL;
            //   END IF;
            //END;
            string expression, column;
            if (MatchTrigger(TriggerMatch1, body, out expression, out column))
            {
                constraint.ColumnNames.Add(column.Trim('"'));
                constraint.Expression = expression;
            }
            return constraint;
        }

        protected virtual List<DataConstraint> ReadConstraints(IDbConnection conn, string db)
        {
            var constraints = new List<DataConstraint>();

            string sql = @"
SELECT UCC.owner, UCC.constraint_name, UCC.table_name, UC.constraint_type, UC.R_constraint_name, UCC.column_name, UCC.position
FROM all_cons_columns UCC, all_constraints UC
WHERE UCC.constraint_name=UC.constraint_name
AND UCC.table_name=UC.table_name
AND UCC.owner=UC.owner
AND UCC.TABLE_NAME NOT LIKE '%$%' AND UCC.TABLE_NAME NOT LIKE 'LOGMNR%' AND UCC.TABLE_NAME NOT IN ('HELP','SQLPLUS_PRODUCT_PROFILE')
AND UC.CONSTRAINT_TYPE!='C'
and lower(UCC.owner) = :owner";

            constraints.AddRange(DataCommand.Find(conn, sql, ":owner", db.ToLower(),
                    r => new
                    {
                        Key = new
                        {
                            Owner = r.GetString(0),
                            ConName = r.GetString(1),
                            TableName = r.GetString(2),
                            ConType = r.GetString(3),
                            RevCconName = r.GetAsString(4)
                        },
                        Value = new
                        {
                            ColName = r.GetString(5),
                            ColPos = r.GetInt32(6)
                        }
                    })
                .GroupBy(r => r.Key, r => r.Value, (r, rs) => new DataConstraint
                {
                    TableSchema = r.Owner,
                    ConstraintName = r.ConName,
                    TableName = r.TableName,
                    ConstraintType = r.ConType,
                    ReverseConstraintName = r.RevCconName,
                    ColumnNames = rs.OrderBy(t => t.ColPos).Select(t => t.ColName).ToList()
                }));

            string sql2 =
                @"
select t.TRIGGER_NAME, t.TABLE_OWNER, t.TABLE_NAME, t.TRIGGER_BODY from ALL_TRIGGERS t
where t.status = 'ENABLED'
 and t.TRIGGERING_EVENT = 'INSERT'
 and t.TRIGGER_TYPE='BEFORE EACH ROW'
 and lower(t.owner) = :owner";

            constraints.AddRange(DataCommand.Find<DataConstraint>(conn, sql2, ":owner", db.ToLower(), ReadTrigger));
            return constraints;
        }
    }
}
