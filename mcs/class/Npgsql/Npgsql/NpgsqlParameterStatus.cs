// created on 8/6/2003 at 13:57
// Npgsql.NpgsqlAsciiRow.cs
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
    /// This class represents the ParameterStatus message sent from PostgreSQL
    /// server.
    /// </summary>
    ///
    internal sealed class NpgsqlParameterStatus
    {

        private String _parameter;
        private String _parameterValue;


        public void ReadFromStream(Stream inputStream, Encoding encoding)
        {

            //Read message length
            Byte[] inputBuffer = new Byte[4];
            PGUtil.CheckedStreamRead(inputStream, inputBuffer, 0, 4 );

            Int32 messageLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(inputBuffer, 0));

            _parameter = PGUtil.ReadString(inputStream, encoding);
            _parameterValue = PGUtil.ReadString(inputStream, encoding);


        }

        public String Parameter
        {
            get
            {
                return _parameter;
            }
        }

        public String ParameterValue
        {
            get
            {
                return _parameterValue;
            }
        }


    }


}
