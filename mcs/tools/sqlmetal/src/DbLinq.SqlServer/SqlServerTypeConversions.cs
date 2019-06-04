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
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace DbLinq.SqlServer
{
    /// <summary>
    /// helper class which help to convert Microsoft Sql's types to SqlClient .NET types,
    /// eg. 'smalldatetime' to SqlDbType.Date.
    /// </summary>
#if MONO_STRICT
    internal
#else
    public
#endif
    static class SqlServerTypeConversions
    {
        static Dictionary<string, SqlDbType> s_typeMap = new Dictionary<string, SqlDbType>();

        static SqlServerTypeConversions()
        {
            foreach (SqlDbType dbType in Enum.GetValues(typeof(SqlDbType)))
            {
                s_typeMap[dbType.ToString().ToLower()] = dbType;
            }
        }

        /// <summary>
        /// given name of MySqlType, return it's MySqlDbType enum.
        /// </summary>
        public static SqlDbType ParseType(string typeStr)
        {
            string typeStrL = typeStr.ToLower();

            //convert "DateTime NOT NULL" to "DateTime"
            if (typeStrL.EndsWith(" not null"))
                typeStrL = typeStrL.Substring(0, typeStrL.Length - " NOT NULL".Length);
            
            //shorten "VarChar(50)" to "VarChar"
            int bracket = typeStrL.IndexOf("(");
            if (bracket > 0)
                typeStrL = typeStrL.Substring(0, bracket);


            if(!s_typeMap.ContainsKey(typeStrL))
            {
                switch(typeStrL){
                    case "tinyint":
                        return SqlDbType.Int;
                    case "int":
                        return SqlDbType.Int;
                }
                string msg = "TODO L24: add parsing of type "+typeStr;
                Console.WriteLine(msg);
                throw new ApplicationException(msg);
            }
            return s_typeMap[typeStrL];
        }
    }
}
