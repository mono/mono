// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 1.1.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (C) 2007-2009 LShift Ltd., Cohesive Financial
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
//   The Initial Developers of the Original Code are LShift Ltd,
//   Cohesive Financial Technologies LLC, and Rabbit Technologies Ltd.
//
//   Portions created before 22-Nov-2008 00:00:00 GMT by LShift Ltd,
//   Cohesive Financial Technologies LLC, or Rabbit Technologies Ltd
//   are Copyright (C) 2007-2008 LShift Ltd, Cohesive Financial
//   Technologies LLC, and Rabbit Technologies Ltd.
//
//   Portions created by LShift Ltd are Copyright (C) 2007-2009 LShift
//   Ltd. Portions created by Cohesive Financial Technologies LLC are
//   Copyright (C) 2007-2009 Cohesive Financial Technologies
//   LLC. Portions created by Rabbit Technologies Ltd are Copyright
//   (C) 2007-2009 Rabbit Technologies Ltd.
//
//   All Rights Reserved.
//
//   Contributor(s): ______________________________________.
//
//---------------------------------------------------------------------------
using System;
using System.Collections;
using System.IO;
using System.Text;

using RabbitMQ.Client;
using RabbitMQ.Client.Content;
using RabbitMQ.Client.Events;
using RabbitMQ.Util;

namespace RabbitMQ.Client.Examples {
    public class DeclareQueue {
        public static int Main(string[] args) {
            try {
                int optionIndex = 0;
                bool durable = false;
                bool delete = false;
                IDictionary arguments = null;
                while (optionIndex < args.Length) {
                    if (args[optionIndex] == "/durable") { durable = true; }
                    else if (args[optionIndex] == "/delete") { delete = true; }
                    else if (args[optionIndex].StartsWith("/arg:")) {
                        if (arguments == null) { arguments = new Hashtable(); }
                        string[] pieces = args[optionIndex].Split(new Char[] { ':' });
                        if (pieces.Length >= 3) {
                            arguments[pieces[1]] = pieces[2];
                        }
                    }
                    else { break; }
                    optionIndex++;
                }

                if (((args.Length - optionIndex) < 2) ||
                    (((args.Length - optionIndex) % 2) != 0))
                {
                    Console.Error.WriteLine("Usage: DeclareQueue [<option> ...] <hostname>[:<portnumber>] <queue> [<exchange> <routingkey>] ...");
                    Console.Error.WriteLine("RabbitMQ .NET client version "+typeof(IModel).Assembly.GetName().Version.ToString());
                    Console.Error.WriteLine("Available options:");
                    Console.Error.WriteLine("  /durable      declare a durable queue");
                    Console.Error.WriteLine("  /delete       delete after declaring");
                    Console.Error.WriteLine("  /arg:KEY:VAL  add longstr entry to arguments table");
                    return 1;
                }

                string serverAddress = args[optionIndex++];
                string inputQueueName = args[optionIndex++];

                using (IConnection conn = new ConnectionFactory().CreateConnection(serverAddress))
                {
                    using (IModel ch = conn.CreateModel()) {

                        string finalName = ch.QueueDeclare(inputQueueName, false,
                                                           durable, false, false,
                                                           false, arguments);
                        Console.WriteLine("{0}\t{1}", finalName, durable);
                
                        while ((optionIndex + 1) < args.Length) {
                            string exchange = args[optionIndex++];
                            string routingKey = args[optionIndex++];
                            ch.QueueBind(finalName, exchange, routingKey, false, null);
                            Console.WriteLine("{0}\t{1}\t{2}", finalName, exchange, routingKey);
                        }

                        if (delete) {
                            ch.QueueDelete(finalName, false, false, false);
                        }

                        return 0;
                    }
                }
            } catch (Exception e) {
                Console.Error.WriteLine(e);
                return 2;
            }
        }
    }
}
