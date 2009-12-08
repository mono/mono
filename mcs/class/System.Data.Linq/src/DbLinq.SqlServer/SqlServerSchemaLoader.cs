#region MIT license
// 
// MIT license
//
// Copyright (c) 2009 Novell, Inc.
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
using System.Data;
using System.Data.Common;
using System.Linq;

using DbLinq.Vendor;

namespace DbLinq.SqlServer
{
#if !MONO_STRICT
    public
#endif
    class SqlServerSchemaLoader : DbSchemaLoader
    {
        readonly IVendor vendor = new SqlServerVendor();
        public override IVendor Vendor {
            get {return vendor;}
            set {}
        }

        protected override DataTable GetForeignKeys(DbConnection connection)
        {
            var t = new DataTable("ForeignKeys");
            using (var c = connection.CreateCommand())
            {
                c.CommandText = @"
SELECT
    rc.CONSTRAINT_NAME      AS 'CONSTRAINT_NAME', 
    'FOREIGN KEY'           AS 'CONSTRAINT_TYPE',
    rcu_from.TABLE_NAME     AS 'TABLE_NAME', 
    rcu_from.TABLE_SCHEMA   AS 'TABLE_SCHEMA',
    rcu_from.COLUMN_NAME    AS 'FKEY_FROM_COLUMN', 
    rcu_to.TABLE_SCHEMA     AS 'FKEY_TO_SCHEMA',
    rcu_to.TABLE_NAME       AS 'FKEY_TO_TABLE', 
    rcu_to.COLUMN_NAME      AS 'FKEY_TO_COLUMN'
FROM
    INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc
INNER JOIN
    INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE rcu_from ON
        rc.CONSTRAINT_CATALOG   = rcu_from.CONSTRAINT_CATALOG AND
        rc.CONSTRAINT_NAME      = rcu_from.CONSTRAINT_NAME
INNER JOIN
    INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE rcu_to ON
        rc.UNIQUE_CONSTRAINT_CATALOG  = rcu_to.CONSTRAINT_CATALOG AND
        rc.UNIQUE_CONSTRAINT_NAME     = rcu_to.CONSTRAINT_NAME
";
                using (var r = c.ExecuteReader())
                    t.Load(r);
            }
            return t;
        }

        protected override DataTable GetColumns(DbConnection connection)
        {
            var t = new DataTable("Columns");
            using (var c = connection.CreateCommand())
            {
                c.CommandText = @"
SELECT
    columns.TABLE_CATALOG,
    columns.TABLE_SCHEMA,
    columns.TABLE_NAME,
    columns.COLUMN_NAME,
    columns.ORDINAL_POSITION,
    columns.COLUMN_DEFAULT,
    (SELECT CAST (COUNT(*) AS BIT)
     FROM
        INFORMATION_SCHEMA.TABLE_CONSTRAINTS rc
     INNER JOIN
        INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE rcu ON
            rc.CONSTRAINT_CATALOG   = rcu.CONSTRAINT_CATALOG AND
            rc.CONSTRAINT_NAME      = rcu.CONSTRAINT_NAME
     WHERE
		rc.CONSTRAINT_TYPE  = 'PRIMARY KEY' AND
		rc.TABLE_CATALOG    = columns.TABLE_CATALOG AND
		rc.TABLE_SCHEMA     = columns.TABLE_SCHEMA AND
		rc.TABLE_NAME       = columns.TABLE_NAME AND
		rcu.COLUMN_NAME     = columns.COLUMN_NAME
    ) AS 'PRIMARY_KEY',
    columns.IS_NULLABLE,
    columns.DATA_TYPE,
    columns.CHARACTER_MAXIMUM_LENGTH,
    columns.CHARACTER_OCTET_LENGTH,
    CAST(columns.NUMERIC_PRECISION AS INT) AS 'NUMERIC_PRECISION',
    columns.NUMERIC_PRECISION_RADIX,
    columns.NUMERIC_SCALE,
    columns.DATETIME_PRECISION,
    columns.CHARACTER_SET_CATALOG,
    columns.CHARACTER_SET_SCHEMA,
    columns.CHARACTER_SET_NAME,
    columns.COLLATION_CATALOG
FROM
    INFORMATION_SCHEMA.COLUMNS columns
ORDER BY
    columns.TABLE_CATALOG, columns.TABLE_SCHEMA, 
    columns.TABLE_NAME, columns.COLUMN_NAME
";
                using (var r = c.ExecuteReader())
                    t.Load(r);
            }
            return t;
        }
    }
}
