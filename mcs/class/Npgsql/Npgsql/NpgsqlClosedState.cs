// Npgsql.NpgsqlClosedState.cs
// 
// Author:
// 	Dave Joyner <d4ljoyn@yahoo.com>
//
//	Copyright (C) 2002 The Npgsql Development Team
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


namespace Npgsql
{
	using System;
	using System.IO;
	using System.Net;
	using System.Net.Sockets;
	
	internal sealed class NpgsqlClosedState : NpgsqlState
	{
		private static NpgsqlClosedState _instance = null;
		
	
		private NpgsqlClosedState()
		{
		}
		public static NpgsqlClosedState Instance
		{
			get
			{
				if ( _instance == null )
				{
					_instance = new NpgsqlClosedState();
				}
				return _instance;	
			}
		}
		public override void Open(NpgsqlConnection context)
		{
			IPEndPoint serverEndPoint;
			// Open the connection to the backend.
			context.TcpClient = new TcpClient();		    			    	
			// If it was specified an IP address in doted notation 
			// (i.e.:192.168.0.1), there may be a long delay trying
			// resolve it when it is not necessary.
			// So, try first connect as if it was a dotted ip address.	    
			try
			{
				IPAddress ipserver = IPAddress.Parse(context.ServerName);
				serverEndPoint = new IPEndPoint(ipserver, Int32.Parse(context.ServerPort));
			}
			catch(FormatException)	// The exception isn't used.
			{
				// Server isn't in dotted decimal format. Just connect using DNS resolves.
				IPHostEntry serverHostEntry = Dns.GetHostByName(context.ServerName);
				serverEndPoint = new IPEndPoint(serverHostEntry.AddressList[0], Int32.Parse(context.ServerPort));	
			}
			
			// Connect to the server.

   		context.TcpClient.Connect(serverEndPoint);	
			NpgsqlEventLog.LogMsg("Connected to: " + serverEndPoint.Address + ":" + serverEndPoint.Port, LogLevel.Normal);
		
			ChangeState( context, NpgsqlConnectedState.Instance );
			context.Startup();
		}
	}
}
