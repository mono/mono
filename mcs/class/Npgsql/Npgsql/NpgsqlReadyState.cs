// Npgsql.NpgsqlReadyState.cs
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
using System.Resources;

namespace Npgsql
{


    internal sealed class NpgsqlReadyState : NpgsqlState
    {
        private static NpgsqlReadyState _instance = null;


        // Flush and Sync messages. It doesn't need to be created every time it is called.
        private static readonly NpgsqlFlush _flushMessage = new NpgsqlFlush();

        private static readonly NpgsqlSync _syncMessage = new NpgsqlSync();

        private readonly String CLASSNAME = "NpgsqlReadyState";

        private NpgsqlReadyState() : base()
        { }

        public static NpgsqlReadyState Instance
        {
            get
            {
                if ( _instance == null )
                {
                    _instance = new NpgsqlReadyState();
                }
                return _instance;
            }
        }



        public override void Query( NpgsqlConnector context, NpgsqlCommand command )
        {

            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Query");

            String commandText = command.GetCommandText();
            NpgsqlEventLog.LogMsg(resman, "Log_QuerySent", LogLevel.Debug, commandText);

            // Send the query request to backend.

            NpgsqlQuery query = new NpgsqlQuery(commandText, context.BackendProtocolVersion);
            BufferedStream stream = new BufferedStream(context.Stream);
            query.WriteToStream(stream, context.Encoding);
            stream.Flush();

            ProcessBackendResponses(context);

        }

        public override void Parse(NpgsqlConnector context, NpgsqlParse parse)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Parse");
            BufferedStream stream = new BufferedStream(context.Stream);
            parse.WriteToStream(stream, context.Encoding);
            stream.Flush();
        }


        public override void Sync(NpgsqlConnector context)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Sync");
            _syncMessage.WriteToStream(context.Stream, context.Encoding);
            context.Stream.Flush();
            ProcessBackendResponses(context);
        }

        public override void Flush(NpgsqlConnector context)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Flush");
            _flushMessage.WriteToStream(context.Stream, context.Encoding);
            ProcessBackendResponses(context);
        }

        public override void Bind(NpgsqlConnector context, NpgsqlBind bind)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Bind");
            BufferedStream stream = new BufferedStream(context.Stream);
            bind.WriteToStream(stream, context.Encoding);
            stream.Flush();

        }

        public override void Execute(NpgsqlConnector context, NpgsqlExecute execute)
        {

            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Execute");
            NpgsqlDescribe describe = new NpgsqlDescribe('P', execute.PortalName);
            BufferedStream stream = new BufferedStream(context.Stream);
            describe.WriteToStream(stream, context.Encoding);
            execute.WriteToStream(stream, context.Encoding);
            stream.Flush();
            Sync(context);
        }

        public override void Close( NpgsqlConnector context )
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Close");   
            Stream stream = context.Stream;
            stream.WriteByte((Byte)'X');
            if (context.BackendProtocolVersion >= ProtocolVersion.Version3)
                PGUtil.WriteInt32(stream, 4);
            stream.Flush();

            try {
                stream.Close();
            } catch {}

            context.Stream = null;
            ChangeState( context, NpgsqlClosedState.Instance );
        }
    }
}
