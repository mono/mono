// created on 10/6/2002 at 21:33

// Npgsql.NpgsqlPasswordPacket.cs
// 
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
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


using System;
using System.IO;
using System.Text;
using System.Net;

namespace Npgsql
{
	/// <summary>
	/// This class represents a PasswordPacket message sent to backend
	/// PostgreSQL.
	/// </summary>
	internal sealed class NpgsqlPasswordPacket
	{
		// Logging related values
    private static readonly String CLASSNAME = "NpgsqlPasswordPacket";
		
		private String password;
		
		public NpgsqlPasswordPacket(String password)
		{
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".NpgsqlPasswordPacket()", LogLevel.Debug);
			
			this.password = password;	
		}
		
		public void WriteToStream(Stream output_stream, Encoding encoding)
		{
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".WriteToStream()", LogLevel.Debug);
			// Write the size of the packet.
			// 4 + (passwordlength + 1) -> Int32 + NULL terminated string.
			output_stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(4 + (password.Length + 1))), 0, 4);
			
			// Write String.
			PGUtil.WriteString(password, output_stream, encoding);
		}
	}
	
}

