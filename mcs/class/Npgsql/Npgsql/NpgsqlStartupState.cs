// Npgsql.NpgsqlStartupState.cs
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
using System.Net;
using System.Net.Sockets;

namespace Npgsql
{


    internal sealed class NpgsqlStartupState : NpgsqlState
    {
        private static NpgsqlStartupState _instance = null;

        private readonly String CLASSNAME = "NpgsqlStartupState";

        private NpgsqlStartupState() : base()
        { }

        public static NpgsqlStartupState Instance
        {
            get
            {
                if ( _instance == null )
                {
                    _instance = new NpgsqlStartupState();
                }
                return _instance;
            }
        }
        public override void Authenticate( NpgsqlConnector context, string password)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Authenticate");
            NpgsqlPasswordPacket pwpck = new NpgsqlPasswordPacket(password, context.BackendProtocolVersion);
            BufferedStream stream = new BufferedStream(context.Stream);
            pwpck.WriteToStream(stream, context.Encoding);
            stream.Flush();

        }
    }
}
