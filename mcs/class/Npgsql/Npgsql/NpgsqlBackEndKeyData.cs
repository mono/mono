// created on 11/6/2002 at 11:53

// Npgsql.NpgsqlBackEndKeyData.cs
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
	/// This class represents a BackEndKeyData message received
	/// from PostgreSQL
	/// </summary>
	internal sealed class NpgsqlBackEndKeyData
	{
		// Logging related values
    private static readonly String CLASSNAME = "NpgsqlBackEndKeyData";
		
		private Int32 process_id;
		private Int32 secret_key;
		
				
		public void ReadFromStream(Stream input_stream)
		{
			NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".ReadFromStream()", LogLevel.Debug);
			
			Byte[] input_buffer = new Byte[8];
			
			// Read the BackendKeyData message contents. Two Int32 integers = 8 Bytes.
			input_stream.Read(input_buffer, 0, 8);
			process_id = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 0));
			secret_key = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 4));
			
		}
		
		public Int32 ProcessID
		{
			get
			{
				NpgsqlEventLog.LogMsg("Got ProcessID. Value: " + process_id, LogLevel.Debug);
				return process_id;
			}
		}
		
		public Int32 SecretKey
		{
			get
			{
				NpgsqlEventLog.LogMsg("Got SecretKey. Value: " + secret_key, LogLevel.Debug);
				return secret_key;
			}
		}
	}
}
