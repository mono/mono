// created on 22/6/2003 at 18:33

// Npgsql.NpgsqlParse.cs
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
using System.Collections;
using System.IO;
using System.Text;
using System.Net;
using NpgsqlTypes;


namespace Npgsql
{

    /// <summary>
    /// This class represents the Parse message sent to PostgreSQL
    /// server.
    /// </summary>
    ///
    internal sealed class NpgsqlParse
    {
        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlParse";

        private String    _prepareName;
        private String    _queryString;
        private Int32[]   _parameterIDs;


        public NpgsqlParse(String prepareName, String queryString, Int32[] parameterIDs)
        {
            _prepareName = prepareName;
            _queryString = queryString;
            _parameterIDs = parameterIDs;

        }

        public void WriteToStream(Stream outputStream, Encoding encoding)
        {
            outputStream.WriteByte((Byte)'P');

            // message length =
            // Int32 self
            // name of prepared statement + 1 null string terminator +
            // query string + 1 null string terminator
            // + Int16
            // + Int32 * number of parameters.
            Int32 messageLength = 4 + encoding.GetByteCount(_prepareName) + 1 + encoding.GetByteCount(_queryString) + 1 + 2 + (_parameterIDs.Length * 4);
            //Int32 messageLength = 4 + _prepareName.Length + 1 + _queryString.Length + 1 + 2 + (_parameterIDs.Length * 4);

            PGUtil.WriteInt32(outputStream, messageLength);
            PGUtil.WriteString(_prepareName, outputStream, encoding);
            PGUtil.WriteString(_queryString, outputStream, encoding);
            PGUtil.WriteInt16(outputStream, (Int16)_parameterIDs.Length);


            for(Int32 i = 0; i < _parameterIDs.Length; i++)
                PGUtil.WriteInt32(outputStream, _parameterIDs[i]);




        }
    }
}
