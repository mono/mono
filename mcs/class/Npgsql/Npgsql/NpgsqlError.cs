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
    /// EventArgs class to send Notice parameters, which are just NpgsqlError's in a lighter context.
    /// </summary>
    public class NpgsqlNoticeEventArgs : EventArgs
    {
        /// <summary>
        /// Notice information.
        /// </summary>
        public NpgsqlError Notice = null;

        internal NpgsqlNoticeEventArgs(NpgsqlError eNotice)
        {
            Notice = eNotice;
        }
    }

    /// <summary>
    /// This class represents the ErrorResponse and NoticeResponse
    /// message sent from PostgreSQL server.
    /// </summary>
    public sealed class NpgsqlError
    {
        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlError";

        private ProtocolVersion protocol_version;
        private String _severity = "";
        private String _code = "";
        private String _message = "";
        private String _detail = "";
        private String _hint = "";
        private String _position = "";
        private String _where = "";
        private String _file = "";
        private String _line = "";
        private String _routine = "";

        /// <summary>
        /// Severity code.  All versions.
        /// </summary>
        public String Severity
        {
            get
            {
                return _severity;
            }
        }

        /// <summary>
        /// Error code.  PostgreSQL 7.4 and up.
        /// </summary>
        public String Code
        {
            get
            {
                return _code;
            }
        }

        /// <summary>
        /// Terse error message.  All versions.
        /// </summary>
        public String Message
        {
            get
            {
                return _message;
            }
        }

        /// <summary>
        /// Detailed error message.  PostgreSQL 7.4 and up.
        /// </summary>
        public String Detail
        {
            get
            {
                return _detail;
            }
        }

        /// <summary>
        /// Suggestion to help resolve the error.  PostgreSQL 7.4 and up.
        /// </summary>
        public String Hint
        {
            get
            {
                return _hint;
            }
        }

        /// <summary>
        /// Position (one based) within the query string where the error was encounterd.  PostgreSQL 7.4 and up.
        /// </summary>
        public String Position
        {
            get
            {
                return _position;
            }
        }

        /// <summary>
        /// Trace back information.  PostgreSQL 7.4 and up.
        /// </summary>
        public String Where
        {
            get
            {
                return _where;
            }
        }

        /// <summary>
        /// Source file (in backend) reporting the error.  PostgreSQL 7.4 and up.
        /// </summary>
        public String File
        {
            get
            {
                return _file;
            }
        }

        /// <summary>
        /// Source file line number (in backend) reporting the error.  PostgreSQL 7.4 and up.
        /// </summary>
        public String Line
        {
            get
            {
                return _line;
            }
        }

        /// <summary>
        /// Source routine (in backend) reporting the error.  PostgreSQL 7.4 and up.
        /// </summary>
        public String Routine
        {
            get
            {
                return _routine;
            }
        }

        /// <summary>
        /// Return a string representation of this error object.
        /// </summary>
        public override String ToString()
        {
            StringBuilder     B = new StringBuilder();

            if (Severity.Length > 0)
            {
                B.AppendFormat("{0}: ", Severity);
            }
            if (Code.Length > 0)
            {
                B.AppendFormat("{0}: ", Code);
            }
            B.AppendFormat("{0}", Message);
            // CHECKME - possibly multi-line, that is yucky
            //            if (Hint.Length > 0) {
            //                B.AppendFormat(" ({0})", Hint);
            //            }

            return B.ToString();
        }

        internal NpgsqlError(ProtocolVersion protocolVersion)
        {
            protocol_version = protocolVersion;
        }

        /// <summary>
        /// Backend protocol version in use.
        /// </summary>
        internal ProtocolVersion BackendProtocolVersion
        {
            get
            {
                return protocol_version;
            }
        }

        internal void ReadFromStream(Stream inputStream, Encoding encoding)
        {
            switch (protocol_version) {
            case ProtocolVersion.Version2 :
                ReadFromStream_Ver_2(inputStream, encoding);
                break;

            case ProtocolVersion.Version3 :
                ReadFromStream_Ver_3(inputStream, encoding);
                break;

            }
        }

        private void ReadFromStream_Ver_2(Stream inputStream, Encoding encoding)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ReadFromStream_Ver_2");

            String Raw;
            String[] Parts;

            Raw = PGUtil.ReadString(inputStream, encoding);

            Parts = Raw.Split(new char[] {':'}, 2);

            if (Parts.Length == 2)
            {
                _severity = Parts[0].Trim();
                _message = Parts[1].Trim();
            }
            else
            {
                _message = Parts[0].Trim();
            }
        }

        private void ReadFromStream_Ver_3(Stream inputStream, Encoding encoding)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ReadFromStream_Ver_3");

            Int32 messageLength = PGUtil.ReadInt32(inputStream, new Byte[4]);

            // [TODO] Would this be the right way to do?
            // Check the messageLength value. If it is 1178686529, this would be the
            // "FATA" string, which would mean a protocol 2.0 error string.
            if (messageLength == 1178686529)
            {
								String Raw;
                String[] Parts;

                Raw = "FATA" + PGUtil.ReadString(inputStream, encoding);

                Parts = Raw.Split(new char[] {':'}, 2);

                if (Parts.Length == 2)
                {
                    _severity = Parts[0].Trim();
                    _message = Parts[1].Trim();
                }
                else
                {
                    _message = Parts[0].Trim();
                }

                protocol_version = ProtocolVersion.Version2;

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
