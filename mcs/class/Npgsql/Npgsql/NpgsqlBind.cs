// created on 29/6/2003 at 13:28

// Npgsql.NpgsqlBind.cs
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
using System.Data;


namespace Npgsql
{

    /// <summary>
    /// This class represents the Bind message sent to PostgreSQL
    /// server.
    /// </summary>
    ///
    internal sealed class NpgsqlBind
    {

        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlBind";

        private String _portalName;
        private String _preparedStatementName;
        private Int16[] _parameterFormatCodes;
        private Object[] _parameterValues;
        private Int16[] _resultFormatCodes;


        public NpgsqlBind(String portalName,
                          String preparedStatementName,
                          Int16[] parameterFormatCodes,
                          Object[] parameterValues,
                          Int16[] resultFormatCodes)
        {

            _portalName = portalName;
            _preparedStatementName = preparedStatementName;
            _parameterFormatCodes = parameterFormatCodes;
            _parameterValues = parameterValues;
            _resultFormatCodes = resultFormatCodes;


        }

        public String PortalName
        {
            get
            {
                return _portalName;
            }
        }

        public Int16[] ParameterFormatCodes
        {
            get
            {
                return _parameterFormatCodes;
            }

            set
            {
                _parameterFormatCodes = value;
            }
        }

        public Object[] ParameterValues
        {
            get
            {
                return _parameterValues;
            }

            set
            {
                _parameterValues = value;
            }
        }


        public void WriteToStream(Stream outputStream, Encoding encoding)
        {



            Int32 messageLength = 4 +
                                  encoding.GetByteCount(_portalName) + 1 +
                                  encoding.GetByteCount(_preparedStatementName) + 1 +
                                  2 +
                                  (_parameterFormatCodes.Length * 2) +
                                  2;


            // Get size of parameter values.
            Int32 i;

            if (_parameterValues != null)
                for (i = 0; i < _parameterValues.Length; i++)
                {
                    messageLength += 4;
                    if ( _parameterValues[i] != null)
                        if ( ((_parameterFormatCodes.Length == 1) && (_parameterFormatCodes[0] == (Int16) FormatCode.Binary)) ||
                                ((_parameterFormatCodes.Length != 1) && (_parameterFormatCodes[i] == (Int16) FormatCode.Binary)) )
                            messageLength += ((Byte[])_parameterValues[i]).Length;
                        else
                            messageLength += encoding.GetByteCount((String)_parameterValues[i]);

                }

            messageLength += 2 + (_resultFormatCodes.Length * 2);


            outputStream.WriteByte((Byte)'B');

            PGUtil.WriteInt32(outputStream, messageLength);
            PGUtil.WriteString(_portalName, outputStream, encoding);
            PGUtil.WriteString(_preparedStatementName, outputStream, encoding);

            PGUtil.WriteInt16(outputStream, (Int16)_parameterFormatCodes.Length);

            for (i = 0; i < _parameterFormatCodes.Length; i++)
                PGUtil.WriteInt16(outputStream, _parameterFormatCodes[i]);

            if (_parameterValues != null)
            {
                PGUtil.WriteInt16(outputStream, (Int16)_parameterValues.Length);

                for (i = 0; i < _parameterValues.Length; i++)
                {
                    if ( ((_parameterFormatCodes.Length == 1) && (_parameterFormatCodes[0] == (Int16) FormatCode.Binary)) ||
                            ((_parameterFormatCodes.Length != 1) && (_parameterFormatCodes[i] == (Int16) FormatCode.Binary)) )
                    {

                        Byte[] parameterValue = (Byte[])_parameterValues[i];
                        if (parameterValue == null)
                            PGUtil.WriteInt32(outputStream, -1);
                        else
                        {
                            PGUtil.WriteInt32(outputStream, parameterValue.Length);
                            outputStream.Write(parameterValue, 0, parameterValue.Length);
                        }
                    }
                    else
                    {
                        if ((_parameterValues[i] == null))
                            PGUtil.WriteInt32(outputStream, -1);
                        else
                        {
                            String parameterValue = (String)_parameterValues[i];
                            PGUtil.WriteInt32(outputStream, encoding.GetByteCount(parameterValue));
                            outputStream.Write(encoding.GetBytes(parameterValue), 0, encoding.GetByteCount(parameterValue));
                        }
                    }

                }
            }
            else
                PGUtil.WriteInt16(outputStream, 0);

            PGUtil.WriteInt16(outputStream, (Int16)_resultFormatCodes.Length);
            for (i = 0; i < _resultFormatCodes.Length; i++)
                PGUtil.WriteInt16(outputStream, _resultFormatCodes[i]);



        }

    }
}

