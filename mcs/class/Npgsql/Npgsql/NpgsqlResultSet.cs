
// Npgsql.NpgsqlResultSet.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Data;
using System.Collections;

namespace Npgsql
{
    internal sealed class NpgsqlResultSet
    {
        private NpgsqlRowDescription 	row_desc;
        private ArrayList							data;


        public NpgsqlResultSet(NpgsqlRowDescription rowDesc, ArrayList data)
        {
            this.row_desc = rowDesc;
            this.data = data;
        }

        public NpgsqlRowDescription RowDescription
        {
            get
            {
                return row_desc;
            }
        }

        public Object this[Int32 index]
        {
            get
            {
                return data[index];
            }
        }

        public Int32 Count
        {
            get
            {
                return data.Count;
            }
        }
    }
}
