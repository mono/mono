// Npgsql.NpgsqlClosedState.cs
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
using System.Collections;

using Mono.Security.Protocol.Tls;

namespace Npgsql
{

    internal sealed class NpgsqlClosedState : NpgsqlState
    {

        private static NpgsqlClosedState _instance = null;
        private static readonly String CLASSNAME = "NpgsqlClosedState";

        private NpgsqlClosedState() : base()
        { }

        public static NpgsqlClosedState Instance {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Instance");
                if (_instance == null)
                {
                    _instance = new NpgsqlClosedState();
                }
                return _instance;
            }
        }



        /// <summary>
        /// Resolve a host name or IP address.
        /// This is needed because if you call Dns.Resolve() with an IP address, it will attempt
        /// to resolve it as a host name, when it should just convert it to an IP address.
        /// </summary>
        /// <param name="HostName"></param>
        private static IPAddress ResolveIPHost(String HostName)
        {

            try
            {
                // Is it a raw IP address?
                return IPAddress.Parse(HostName);
            }
            catch (FormatException)
            {
                // Not an IP, must be a host name...
                return Dns.Resolve(HostName).AddressList[0];
            }
        }

        public override void Open(NpgsqlConnector context)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Open");

            TcpClient tcpc = new TcpClient();
            tcpc.Connect(new IPEndPoint(ResolveIPHost(context.Host), context.Port));
            Stream stream = tcpc.GetStream();

            // If the PostgreSQL server has SSL connectors enabled Open SslClientStream if (response == 'S') {
            if (context.SSL)
            {
                PGUtil.WriteInt32(stream, 8);
                PGUtil.WriteInt32(stream,80877103);
                // Receive response
                Char response = (Char)stream.ReadByte();
                if (response == 'S')
                {
                    stream = new SslClientStream(
                                 tcpc.GetStream(),
                                 context.Host,
                                 true,
                                 Mono.Security.Protocol.Tls.SecurityProtocolType.Default
                             );

                    ((SslClientStream)stream).ClientCertSelectionDelegate = new CertificateSelectionCallback(context.DefaultCertificateSelectionCallback);
                    ((SslClientStream)stream).ServerCertValidationDelegate = new CertificateValidationCallback(context.DefaultCertificateValidationCallback);
                    ((SslClientStream)stream).PrivateKeyCertSelectionDelegate = new PrivateKeySelectionCallback(context.DefaultPrivateKeySelectionCallback);
                }
            }

            context.Stream = stream;

            NpgsqlEventLog.LogMsg(resman, "Log_ConnectedTo", LogLevel.Normal, context.Host, context.Port);
            ChangeState(context, NpgsqlConnectedState.Instance);
        }

    }

}
