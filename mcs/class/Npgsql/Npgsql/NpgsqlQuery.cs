// Npgsql.NpgsqlQuery.cs
//
// Author:
// 	Dave Joyner <d4ljoyn@yahoo.com>
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
using System.Net.Sockets;

namespace Npgsql
{
    /// <summary>
    /// Summary description for NpgsqlQuery
    /// </summary>
    internal sealed class NpgsqlQuery
    {
        private NpgsqlCommand _command;
        private ProtocolVersion _protocolVersion;

        public NpgsqlQuery(NpgsqlCommand command, ProtocolVersion protocolVersion)
        {
            _command = command;
            _protocolVersion = protocolVersion;

        }
        public void WriteToStream( Stream outputStream, Encoding encoding )
        {
            //NpgsqlEventLog.LogMsg( this.ToString() + _commandText, LogLevel.Debug  );


            String commandText = _command.GetCommandText();
            // Send the query to server.
            // Write the byte 'Q' to identify a query message.
            outputStream.WriteByte((Byte)'Q');

            if (_protocolVersion == ProtocolVersion.Version3)
            {
                // Write message length. Int32 + string length + null terminator.
                PGUtil.WriteInt32(outputStream, 4 + encoding.GetByteCount(commandText) + 1);
            }

            // Write the query. In this case it is the CommandText text.
            // It is a string terminated by a C NULL character.
            PGUtil.WriteString(commandText, outputStream, encoding);
        }


    }
}
