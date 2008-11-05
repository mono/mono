// created on 9/6/2002 at 16:56


// Npgsql.NpgsqlStartupPacket.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

using System;
using System.IO;
using System.Net;

namespace Npgsql
{
	/// <summary>
	/// This class represents a StartupPacket message of PostgreSQL
	/// protocol.
	/// </summary>
	///
	internal sealed class NpgsqlStartupPacket : ClientMessage
	{
		// Logging related values
		private static readonly String CLASSNAME = "NpgsqlStartupPacket";

		// Private fields.
		private readonly Int32 packet_size;
		private readonly ProtocolVersion protocol_version;
		private readonly String database_name;
		private readonly String user_name;
		private readonly String arguments;
		private readonly String unused;
		private readonly String optional_tty;

		public NpgsqlStartupPacket(Int32 packet_size, ProtocolVersion protocol_version, String database_name, String user_name,
		                           String arguments, String unused, String optional_tty)
		{
			NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);
			// Just copy the values.

			// [FIXME] Validate params? We are the only clients, so, hopefully, we
			// know what to send.

			this.packet_size = packet_size;
			this.protocol_version = protocol_version;

			this.database_name = database_name;
			this.user_name = user_name;
			this.arguments = arguments;
			this.unused = unused;
			this.optional_tty = optional_tty;
		}


		public override void WriteToStream(Stream output_stream)
		{
			NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "WriteToStream");

			switch (protocol_version)
			{
				case ProtocolVersion.Version2:
					WriteToStream_Ver_2(output_stream);
					break;

				case ProtocolVersion.Version3:
					WriteToStream_Ver_3(output_stream);
					break;
			}
		}


		private void WriteToStream_Ver_2(Stream output_stream)
		{
			NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "WriteToStream_Ver_2");

			// Packet length = 296
			output_stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(this.packet_size)), 0, 4);

			output_stream.Write(
				BitConverter.GetBytes(IPAddress.HostToNetworkOrder(PGUtil.ConvertProtocolVersion(this.protocol_version))), 0, 4);

			// Database name.
			PGUtil.WriteLimString(this.database_name, 64, output_stream);

			// User name.
			PGUtil.WriteLimString(this.user_name, 32, output_stream);

			// Arguments.
			PGUtil.WriteLimString(this.arguments, 64, output_stream);

			// Unused.
			PGUtil.WriteLimString(this.unused, 64, output_stream);

			// Optional tty.
			PGUtil.WriteLimString(this.optional_tty, 64, output_stream);
			output_stream.Flush();
		}


		private void WriteToStream_Ver_3(Stream output_stream)
		{
			NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "WriteToStream_Ver_3");

			PGUtil.WriteInt32(output_stream,
			                  4 + 4 + 5 + (UTF8Encoding.GetByteCount(user_name) + 1) + 9 +
			                  (UTF8Encoding.GetByteCount(database_name) + 1) + 10 + 4 + 1);

			PGUtil.WriteInt32(output_stream, PGUtil.ConvertProtocolVersion(this.protocol_version));

			// User name.
			PGUtil.WriteString("user", output_stream);

			// User name.
			PGUtil.WriteString(user_name, output_stream);

			// Database name.
			PGUtil.WriteString("database", output_stream);

			// Database name.
			PGUtil.WriteString(database_name, output_stream);

			// DateStyle.
			PGUtil.WriteString("DateStyle", output_stream);

			// DateStyle.
			PGUtil.WriteString("ISO", output_stream);

			output_stream.WriteByte(0);
			output_stream.Flush();
		}
	}
}