// created on 12/7/2003 at 18:36

// Npgsql.NpgsqlError.cs
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
    /// This class represents the ErrorResponse and NoticeResponse
    /// message sent from PostgreSQL server.
    /// </summary>
    ///
    public sealed class NpgsqlError
    {
        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlError";

        private Int32 protocol_version;
        private String _severity = "";
        private String _code = "";
        private String _message = "";
        private String _detail;
        private String _hint = "";
        private String _position;
        private String _where;
        private String _file;
        private String _line;
        private String _routine;

        public String Severity
        {
            get
            {
                return _severity;
            }
        }

        public String Code
        {
            get
            {
                return _code;
            }
        }

        public String Message
        {
            get
            {
                return _message;
            }
        }

        public String Hint
        {
            get
            {
                return _hint;
            }
        }


        private NpgsqlError()
        {}


        internal NpgsqlError(Int32 protocolVersion)
        {
            protocol_version = protocolVersion;

        }

        internal void ReadFromStream(Stream inputStream, Encoding encoding)
        {
            if (protocol_version == ProtocolVersion.Version2)
            {
                ReadFromStream_Ver_2(inputStream, encoding);
            }
            else
            {
                ReadFromStream_Ver_3(inputStream, encoding);
            }
        }

        private void ReadFromStream_Ver_2(Stream inputStream, Encoding encoding)
        {
            String Raw;
            String[] Parts;

            Raw = PGUtil.ReadString(inputStream, encoding);

            Parts = Raw.Split(new char[] {':'}, 2);

            if (Parts.Length == 2)
            {
                _severity = Parts[0];
                _message = Parts[1].Trim();
            }
            else
            {
                _message = Parts[0];
            }
        }

        private void ReadFromStream_Ver_3(Stream inputStream, Encoding encoding)
        {
            Int32 messageLength = PGUtil.ReadInt32(inputStream, new Byte[4]);

            //[TODO] Would this be the right way to do?
            // Check the messageLength value. If it is 1178686529, this would be the
            // "FATA" string, which would mean a protocol 2.0 error string.
            if (messageLength == 1178686529)
            {
                _severity = "FATAL";
                _message = "FATA" + PGUtil.ReadString(inputStream, encoding);
                return;
            }

            Char field;
            String fieldValue;

            field = (Char) inputStream.ReadByte();

            // Now start to read fields.
            while (field != 0)
            {
                fieldValue = PGUtil.ReadString(inputStream, encoding);

                switch (field)
                {
                case 'S':
                    _severity = fieldValue;
                    break;
                case 'C':
                    _code = fieldValue;
                    break;
                case 'M':
                    _message = fieldValue;
                    break;
                case 'D':
                    _detail = fieldValue;
                    break;
                case 'H':
                    _hint = fieldValue;
                    break;
                case 'P':
                    _position = fieldValue;
                    break;
                case 'W':
                    _where = fieldValue;
                    break;
                case 'F':
                    _file = fieldValue;
                    break;
                case 'L':
                    _line = fieldValue;
                    break;
                case 'R':
                    _routine = fieldValue;
                    break;

                }

                field = (Char) inputStream.ReadByte();

            }
        }
    }
}
