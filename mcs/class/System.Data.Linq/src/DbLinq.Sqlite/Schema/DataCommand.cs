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

namespace DbLinq.Sqlite.Schema
{
#if !MONO_STRICT
    public
#endif
    static class DataCommand
    {
        public delegate T ReadDelegate<T>(IDataReader reader, string table);

        public static List<T> Find<T>(IDbConnection connection, string sql, string pragma, ReadDelegate<T> readDelegate)
        {
            using (IDbCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = sql;
                using (IDataReader rdr = cmd.ExecuteReader())
                {
                    List<T> list = new List<T>();

                    while (rdr.Read())
                    {
                        string table = rdr.GetString(0);
                        //string sqlPragma = @"PRAGMA foreign_key_list('" + table + "');";
                        string sqlPragma = string.Format(pragma, table);
                        using (IDbCommand cmdPragma = connection.CreateCommand())
                        {
                            cmdPragma.CommandText = sqlPragma;
                            using (IDataReader rdrPragma = cmdPragma.ExecuteReader())
                            {
                                while (rdrPragma.Read())
                                {
                                    list.Add(readDelegate(rdrPragma, table));
                                }

                            }
                        }
                    }
                    return list;
                }
            }
        }
    }
}
