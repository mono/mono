// Npgsql.NpgsqlQuery.cs
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


using System;
using System.IO;
using System.Text;
using System.Net.Sockets;

namespace Npgsql
{
	/// <summary>
	/// Summary description for NpgsqlQuery
	/// </summary>
	internal sealed class NpgsqlQuery
	{
		private String _commandText = String.Empty;
		public NpgsqlQuery(String commandText)
		{
			_commandText = commandText;
		}
		public void WriteToStream( Stream outputStream, Encoding encoding )
		{
			NpgsqlEventLog.LogMsg( this.ToString() + _commandText, LogLevel.Debug  );
			// Send the query to server.
			// Write the byte 'Q' to identify a query message.
			outputStream.WriteByte((Byte)'Q');
			
			// Write the query. In this case it is the CommandText text.
			// It is a string terminated by a C NULL character.
			outputStream.Write(encoding.GetBytes(_commandText + '\x00') , 0, encoding.GetByteCount(_commandText) + 1);
			
			
			
		}
	}
}
