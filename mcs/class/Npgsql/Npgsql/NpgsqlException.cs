// created on 12/5/2002 at 23:10

// Npgsql.NpgsqlException.cs
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
using System.Resources;

namespace Npgsql
{
    [Serializable]
    public class NpgsqlException : Exception
    {

        // Logging related values
        private static readonly String CLASSNAME = "NpgsqlException";
        private static ResourceManager resman = new ResourceManager(typeof(NpgsqlException));

        public NpgsqlException()
        {
            NpgsqlEventLog.LogMsg(resman, "Log_ExceptionOccured", LogLevel.Normal, "<no message>");

        }

        public NpgsqlException(String message) : base(message)
        {
            NpgsqlEventLog.LogMsg(resman, "Log_ExceptionOccured", LogLevel.Normal, message);
        }

        public NpgsqlException(String message, Exception inner)
                : base(message, inner)
        {
            NpgsqlEventLog.LogMsg(resman, "Log_ExceptionOccured", LogLevel.Normal, message + " (" + inner.Message + ")");
        }
    }
}
