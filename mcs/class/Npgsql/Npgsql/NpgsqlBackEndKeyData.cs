// created on 11/6/2002 at 11:53

// Npgsql.NpgsqlBackEndKeyData.cs
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
using System.Net;

namespace Npgsql
{
    /// <summary>
    /// This class represents a BackEndKeyData message received
    /// from PostgreSQL
    /// </summary>
    internal sealed class NpgsqlBackEndKeyData
    {
        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlBackEndKeyData";

        private Int32 _processId;
        private Int32 _secretKey;

        private ProtocolVersion _protocolVersion;

        public NpgsqlBackEndKeyData(ProtocolVersion protocolVersion)
        {
            _protocolVersion = protocolVersion;
            _processId = -1;
            _secretKey = -1;
        }


        public void ReadFromStream(Stream inputStream)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);

            Byte[] inputBuffer = new Byte[8];

            // Read the BackendKeyData message contents. Two Int32 integers = 8 Bytes.
            // For protocol version 3.0 they are three integers. The first one is just the size of message
            // so, just read it.
            if (_protocolVersion >= ProtocolVersion.Version3)
                inputStream.Read(inputBuffer, 0, 4);

            inputStream.Read(inputBuffer, 0, 8);
            _processId = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(inputBuffer, 0));
            _secretKey = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(inputBuffer, 4));

        }

        public Int32 ProcessID
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "ProcessID");
                return _processId;
            }
        }

        public Int32 SecretKey
        {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "SecretKey");
                return _secretKey;
            }
        }
    }
}
