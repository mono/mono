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
using System.Threading;

using Mono.Security.Protocol.Tls;

namespace Npgsql
{

    internal sealed class NpgsqlClosedState : NpgsqlState
    {

        private static NpgsqlClosedState _instance = new NpgsqlClosedState();
        private static readonly String CLASSNAME = "NpgsqlClosedState";


        private NpgsqlClosedState() : base()
        { }

        public static NpgsqlClosedState Instance {
            get
            {
                NpgsqlEventLog.LogPropertyGet(LogLevel.Debug, CLASSNAME, "Instance");
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
            
            try
            {
                
                NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Open");
    
                /*TcpClient tcpc = new TcpClient();
                tcpc.Connect(new IPEndPoint(ResolveIPHost(context.Host), context.Port));
                Stream stream = tcpc.GetStream();*/
                
                Socket socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
                
                /*socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.SendTimeout, context.ConnectionTimeout*1000);*/

                //socket.Connect(new IPEndPoint(ResolveIPHost(context.Host), context.Port));
                
                IAsyncResult result = socket.BeginConnect(new IPEndPoint(ResolveIPHost(context.Host), context.Port), null, null);

                if (!result.AsyncWaitHandle.WaitOne(context.ConnectionTimeout*1000, true))
                {
                    socket.Close();
                    throw new Exception(resman.GetString("Exception_ConnectionTimeout"));
                }

                try
                {
                    socket.EndConnect(result);
                }
                catch (Exception ex)
                {
                    socket.Close();
                    throw;
                }

                Stream stream = new NetworkStream(socket, true);

    
                               
                // If the PostgreSQL server has SSL connectors enabled Open SslClientStream if (response == 'S') {
                if (context.SSL || (context.SslMode == SslMode.Require) || (context.SslMode == SslMode.Prefer))
                {
                    PGUtil.WriteInt32(stream, 8);
                    PGUtil.WriteInt32(stream,80877103);
                    // Receive response
                    
                    Char response = (Char)stream.ReadByte();
                    if (response == 'S')
                    {
                        stream = new SslClientStream(
                                    stream,
                                    context.Host,
                                    true,
                                    Mono.Security.Protocol.Tls.SecurityProtocolType.Default
                                );
    
                        ((SslClientStream)stream).ClientCertSelectionDelegate = new CertificateSelectionCallback(context.DefaultCertificateSelectionCallback);
                        ((SslClientStream)stream).ServerCertValidationDelegate = new CertificateValidationCallback(context.DefaultCertificateValidationCallback);
                        ((SslClientStream)stream).PrivateKeyCertSelectionDelegate = new PrivateKeySelectionCallback(context.DefaultPrivateKeySelectionCallback);
                    }
                    else if (context.SslMode == SslMode.Require)
                        throw new InvalidOperationException(resman.GetString("Exception_Ssl_RequestError"));
                    
                }
    
                context.Stream = new BufferedStream(stream);
                context.Socket = socket;
                
    
                NpgsqlEventLog.LogMsg(resman, "Log_ConnectedTo", LogLevel.Normal, context.Host, context.Port);
                ChangeState(context, NpgsqlConnectedState.Instance);
                
                
            }
            catch (Exception e)
            {
                throw new NpgsqlException(e.Message, e);
            }
        }

    }

}
