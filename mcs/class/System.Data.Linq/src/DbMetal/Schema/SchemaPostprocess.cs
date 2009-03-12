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
using System.Linq;
using System.Text;
using DbLinq.Schema;
using DbLinq.Util;

namespace DbMetal.Schema
{
    /// <summary>
    /// this class contains functionality common to all vendors -
    /// a) rename field Alltypes.Alltypes to Alltypes.Contents
    /// b) rename field Employees.Employees to Employees.RefersToEmployees
    /// c) rename field Alltypes.int to Alltypes.int_
    /// </summary>
    public class SchemaPostprocess
    {
        public static void PostProcess_DB(DbLinq.Schema.Dbml.Database schema)
        {
            if (schema == null)
                return;

            //sort tables, parent tables first
            // picrap: how useful was this?
            //TableSorter sorter = new TableSorter(schema.Tables);
            //schema.Tables.Sort(sorter);

            foreach (var tbl in schema.Tables)
            {
                PostProcess_Table(tbl);
            }
        }

        public static void PostProcess_Table(DbLinq.Schema.Dbml.Table table)
        {
            // picrap: this is processed earlier
            //table.Member = Util.FormatTableName(table.Type.Name, util.PluralEnum.Pluralize);
            //table.Type.Name = Util.FormatTableName(table.Type.Name, util.PluralEnum.Singularize);

            //if (mmConfig.renamesFile != null)
            //{
            //    table.Member = Util.Rename(table.Member);
            //}

            foreach (DbLinq.Schema.Dbml.Column col in table.Type.Columns)
            {
                if (col.Member == table.Type.Name)
                    col.Member = "Contents"; //rename field Alltypes.Alltypes to Alltypes.Contents

                // picrap processed earlier
                //col.Storage = "_" + col.Name;

                // picrap moved to CSCodeWriter
                //if (CSharp.IsCsharpKeyword(col.Storage))
                //    col.Storage += "_"; //rename column 'int' -> 'int_'

                //if (CSharp.IsCsharpKeyword(col.Member))
                //    col.Member += "_"; //rename column 'int' -> 'int_'
            }

            Dictionary<string, bool> knownAssocs = new Dictionary<string, bool>();
            foreach (DbLinq.Schema.Dbml.Association assoc in table.Type.Associations)
            {
                // picrap: processed earlier
                //assoc.Type = Util.FormatTableName(assoc.Type, util.PluralEnum.Singularize);

                //util.PluralEnum pluralEnum = assoc.IsForeignKey
                //    ? util.PluralEnum.Singularize
                //    : util.PluralEnum.Pluralize;

                //referring to parent: "public Employee Employee" 
                //referring to child:  "public EntityMSet<Product> Products"
                //assoc.Member = Util.FormatTableName(assoc.Member, pluralEnum);

                if (assoc.Member == table.Type.Name)
                {
                    string thisKey = assoc.ThisKey ?? "_TODO_L35";
                    //self-join: rename field Employees.Employees to Employees.RefersToEmployees
                    assoc.Member = thisKey + assoc.Member;
                }

                if (knownAssocs.ContainsKey(assoc.Member))
                {
                    //this is the Andrus test case in Pgsql:
                    //  create table t1 ( private int primary key);
                    //  create table t2 ( f1 int references t1, f2 int references t1 );

                    assoc.Member += "_" + assoc.Name;

                }

                // picrap: handled previously
                //if (mmConfig.renamesFile != null)
                //{
                //    assoc.Member = Util.Rename(assoc.Member);
                //}

                //if (assoc.Member == "employeeterritories" || assoc.Member == "Employeeterritories")
                //    assoc.Member = "EmployeeTerritories"; //hack to help with Northwind

                knownAssocs[assoc.Member] = true;
            }

        }
    }
}