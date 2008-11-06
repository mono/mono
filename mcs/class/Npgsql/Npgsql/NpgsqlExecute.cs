// created on 1/7/2003 at 20:03
// Npgsql.NpgsqlExecute.cs
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
using System.IO;
using System.Text;

namespace Npgsql
{

    /// <summary>
    /// This class represents the Parse message sent to PostgreSQL
    /// server.
    /// </summary>
    ///
    internal sealed class NpgsqlExecute
    {
        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlExecute";

        private String _portalName;
        private Int32 _maxRows;


        public NpgsqlExecute(String portalName, Int32 maxRows)
        {
            _portalName = portalName;
            _maxRows = maxRows;
        }

        public String PortalName
        {
            get
            {
                return _portalName;
            }
        }

        public void WriteToStream(Stream outputStream, Encoding encoding)
        {
            outputStream.WriteByte((Byte)'E');

            PGUtil.WriteInt32(outputStream, 4 +
                              encoding.GetByteCount(_portalName) + 1 +
                              4);

            PGUtil.WriteString(_portalName, outputStream, encoding);
            PGUtil.WriteInt32(outputStream, _maxRows);

        }

    }
}

