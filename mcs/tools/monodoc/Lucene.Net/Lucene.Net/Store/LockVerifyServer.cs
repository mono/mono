/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace Mono.Lucene.Net.Store
{
	
	/// <summary> Simple standalone server that must be running when you
	/// use {@link VerifyingLockFactory}.  This server simply
	/// verifies at most one process holds the lock at a time.
	/// Run without any args to see usage.
	/// 
	/// </summary>
	/// <seealso cref="VerifyingLockFactory">
	/// </seealso>
	/// <seealso cref="LockStressTest">
	/// </seealso>
	
	public class LockVerifyServer
	{
		
		private static System.String GetTime(long startTime)
		{
			return "[" + (((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTime) / 1000) + "s] ";
		}
		
		[STAThread]
		public static void  Main(System.String[] args)
		{
			
			if (args.Length != 1)
			{
				System.Console.Out.WriteLine("\nUsage: java Mono.Lucene.Net.Store.LockVerifyServer port\n");
				System.Environment.Exit(1);
			}
			
			int port = System.Int32.Parse(args[0]);
			
			System.Net.Sockets.TcpListener temp_tcpListener;
			temp_tcpListener = new System.Net.Sockets.TcpListener(System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList[0], port);
			temp_tcpListener.Server.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.ReuseAddress, 1);
			temp_tcpListener.Start();
			System.Net.Sockets.TcpListener s = temp_tcpListener;
			System.Console.Out.WriteLine("\nReady on port " + port + "...");
			
			int lockedID = 0;
			long startTime = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
			
			while (true)
			{
				System.Net.Sockets.TcpClient cs = s.AcceptTcpClient();
				System.IO.Stream out_Renamed = cs.GetStream();
				System.IO.Stream in_Renamed = cs.GetStream();
				
				int id = in_Renamed.ReadByte();
				int command = in_Renamed.ReadByte();
				
				bool err = false;
				
				if (command == 1)
				{
					// Locked
					if (lockedID != 0)
					{
						err = true;
						System.Console.Out.WriteLine(GetTime(startTime) + " ERROR: id " + id + " got lock, but " + lockedID + " already holds the lock");
					}
					lockedID = id;
				}
				else if (command == 0)
				{
					if (lockedID != id)
					{
						err = true;
						System.Console.Out.WriteLine(GetTime(startTime) + " ERROR: id " + id + " released the lock, but " + lockedID + " is the one holding the lock");
					}
					lockedID = 0;
				}
				else
					throw new System.SystemException("unrecognized command " + command);
				
				System.Console.Out.Write(".");
				
				if (err)
					out_Renamed.WriteByte((System.Byte) 1);
				else
					out_Renamed.WriteByte((System.Byte) 0);
				
				out_Renamed.Close();
				in_Renamed.Close();
				cs.Close();
			}
		}
	}
}
