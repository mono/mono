// created on 1/6/2002 at 22:27

// Npgsql.PGUtil.cs
// 
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
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
using System.Text;
using System.Net.Sockets;
using System.Net;


namespace Npgsql
{
	///<summary>
	/// This class provides many util methods to handle 
	/// reading and writing of PostgreSQL protocol messages.
	/// </summary>
	/// [FIXME] Does this name fully represent the class responsability?
	/// Should it be abstract or with a private constructor to prevent
	/// creating instances?
	
	// 
	internal sealed class PGUtil
	{
		
    // Logging related values
    private static readonly String CLASSNAME = "PGUtil";
				
		///<summary>
		/// This method gets a C NULL terminated string from the network stream.
		/// It keeps reading a byte in each time until a NULL byte is returned.
		/// It returns the resultant string of bytes read.
		/// This string is sent from backend.
		/// </summary>
		
		public static String ReadString(Stream network_stream, Encoding encoding)
		{  
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".ReadString()", LogLevel.Debug);
		
			// [FIXME] Is 512 enough?
			Byte[] buffer = new Byte[512];
			Byte b;
			Int16 counter = 0;
			
			
			// [FIXME] Is this cast always safe?
			b = (Byte)network_stream.ReadByte();
			while(b != 0)
			{
				buffer[counter] = b;
				counter++;
				b = (Byte)network_stream.ReadByte();
			}
			String string_read = encoding.GetString(buffer, 0, counter);
			NpgsqlEventLog.LogMsg("String Read: " + string_read, LogLevel.Debug);
			return string_read;
		}
		
		///<summary>
		/// This method writes a C NULL terminated string to the network stream.
		/// It appends a NULL terminator to the end of the String.
		/// </summary>
		
		public static void WriteString(String the_string, Stream network_stream, Encoding encoding)
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".WriteString()", LogLevel.Debug);
		  
			network_stream.Write(encoding.GetBytes(the_string + '\x00') , 0, the_string.Length + 1);
		}
		
		///<summary>
		/// This method writes a C NULL terminated string limited in length to the 
		/// backend server.
		/// It pads the string with null bytes to the size specified.
		/// </summary>
		
		public static void WriteLimString(String the_string, Int32 n, Stream network_stream, Encoding encoding)
		{
		  NpgsqlEventLog.LogMsg("Entering " + CLASSNAME + ".WriteLimString()", LogLevel.Debug);
		  
			// [FIXME] Parameters should be validated. And what about strings
			// larger than or equal to n?
			
			// Pad the string to the specified value.
			String string_padded = the_string.PadRight(n, '\x00');
      
      network_stream.Write(encoding.GetBytes(string_padded), 0, n);
		}
		
		public static void CheckedStreamRead(Stream stream, Byte[] buffer, Int32 offset, Int32 size)
		{
			Int32 bytes_from_stream = 0;
			Int32 total_bytes_read = 0;
			do
			{
				bytes_from_stream = stream.Read(buffer, offset + total_bytes_read, size);
				total_bytes_read += bytes_from_stream;
				size -= bytes_from_stream;
			}
			while(size > 0);
			
		}
		
		public static void WriteQueryToStream( String query, Stream stream, Encoding encoding )
		{
			NpgsqlEventLog.LogMsg( CLASSNAME + query, LogLevel.Debug  );
			// Send the query to server.
			// Write the byte 'Q' to identify a query message.
			stream.WriteByte((Byte)'Q');
			
			// Write the query. In this case it is the CommandText text.
			// It is a string terminated by a C NULL character.
			stream.Write(encoding.GetBytes(query + '\x00') , 0, query.Length + 1);
			
			// Send bytes.
			stream.Flush();
			
		}
		
		
   
	}
}
