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

namespace DbLinq.Ingres
{
    partial class IngresSchemaLoader
    {
        /// <summary>
        /// represents one row from pg_proc table
        /// </summary>
        protected class DataStoredProcedure
        {
            public string procedure_name;
            public string procedure_owner;
            public string text_segment;

            public override string ToString() { return "Ing_Proc " + procedure_name; }
        }

        protected virtual DataStoredProcedure ReadProcedure(IDataReader rdr)
        {
            DataStoredProcedure procedure = new DataStoredProcedure();
            int field = 0;
            procedure.procedure_name = rdr.GetAsString(field++);
            procedure.procedure_owner = rdr.GetAsString(field++);
            procedure.text_segment = rdr.GetAsString(field++);
            return procedure;
        }

        protected virtual List<DataStoredProcedure> ReadProcedures(IDbConnection conn, string db)
        {
            string sql = @"select procedure_name, procedure_owner, text_segment " +
                "from iiprocedures where system_use='U' and " +
                "procedure_owner!='$ingres' and text_sequence=1";

            return DataCommand.Find<DataStoredProcedure>(conn, sql, ":db", db, ReadProcedure);
        }
    }
}
