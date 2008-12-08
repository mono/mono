// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 1.1.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (C) 2007, 2008 LShift Ltd., Cohesive Financial
//   Technologies LLC., and Rabbit Technologies Ltd.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//---------------------------------------------------------------------------
//
// The MPL v1.1:
//
//---------------------------------------------------------------------------
//   The contents of this file are subject to the Mozilla Public License
//   Version 1.1 (the "License"); you may not use this file except in
//   compliance with the License. You may obtain a copy of the License at
//   http://www.rabbitmq.com/mpl.html
//
//   Software distributed under the License is distributed on an "AS IS"
//   basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//   License for the specific language governing rights and limitations
//   under the License.
//
//   The Original Code is The RabbitMQ .NET Client.
//
//   The Initial Developers of the Original Code are LShift Ltd.,
//   Cohesive Financial Technologies LLC., and Rabbit Technologies Ltd.
//
//   Portions created by LShift Ltd., Cohesive Financial Technologies
//   LLC., and Rabbit Technologies Ltd. are Copyright (C) 2007, 2008
//   LShift Ltd., Cohesive Financial Technologies LLC., and Rabbit
//   Technologies Ltd.;
//
//   All Rights Reserved.
//
//   Contributor(s): ______________________________________.
//
//---------------------------------------------------------------------------
using System;
using System.IO;
using System.Text;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Util;

namespace RabbitMQ.Client.Examples {
    public class SingleGet {
        public static int Main(string[] args) {
            try {
                if (args.Length < 2) {
                    Console.Error.WriteLine("Usage: SingleGet <hostname>[:<portnumber>] <queuename>");
                    Console.Error.WriteLine("RabbitMQ .NET client version "+typeof(IModel).Assembly.GetName().Version.ToString());
                    return 1;
                }

                string serverAddress = args[0];
                string queueName = args[1];
            
                IConnection conn = new ConnectionFactory().CreateConnection(serverAddress);
                conn.ConnectionShutdown += new ConnectionShutdownEventHandler(LogConnClose);

                using (IModel ch = conn.CreateModel()) {
                    conn.AutoClose = true;
                    ushort ticket = ch.AccessRequest("/data");

                    ch.QueueDeclare(ticket, queueName);
                    BasicGetResult result = ch.BasicGet(ticket, queueName, false);
                    if (result == null) {
                        Console.WriteLine("No message available.");
                    } else {
                        ch.BasicAck(result.DeliveryTag, false);
                        Console.WriteLine("Message:");
                        DebugUtil.DumpProperties(result, Console.Out, 0);
                    }

                    return 0;
                }

                // conn will have been closed here by AutoClose above.
            } catch (Exception e) {
                Console.Error.WriteLine(e);
                return 2;
            }
        }

        public static void LogConnClose(IConnection conn, ShutdownEventArgs reason) {
            Console.Error.WriteLine("Closing connection "+conn+" with reason "+reason);
        }
    }
}
