// Npgsql.NpgsqlStartupState.cs
//
// Author:
// 	Dave Joyner <d4ljoyn@yahoo.com>
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

namespace Npgsql
{
	internal sealed class NpgsqlStartupState : NpgsqlState
	{
		public static readonly NpgsqlStartupState Instance = new NpgsqlStartupState();

		private readonly String CLASSNAME = "NpgsqlStartupState";

		private NpgsqlStartupState()
			: base()
		{
		}

		public override void Authenticate(NpgsqlConnector context, string password)
		{
			NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Authenticate");
			NpgsqlPasswordPacket pwpck = new NpgsqlPasswordPacket(password, context.BackendProtocolVersion);
			BufferedStream stream = new BufferedStream(context.Stream);
			pwpck.WriteToStream(stream);
			stream.Flush();
		}
	}
}