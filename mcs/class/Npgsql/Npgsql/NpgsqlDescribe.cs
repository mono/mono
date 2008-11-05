// created on 1/7/2003 at 20:48

// Npgsql.NpgsqlDescribe.cs
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

namespace Npgsql
{
	/// <summary>
	/// This class represents the Parse message sent to PostgreSQL
	/// server.
	/// </summary>
	///
	internal sealed class NpgsqlDescribe : ClientMessage
	{
		private readonly Char _whatToDescribe;
		private readonly String _portalName;

		public NpgsqlDescribe(Char whatToDescribe, String portalName)
		{
			_whatToDescribe = whatToDescribe;
			_portalName = portalName;
		}

		public override void WriteToStream(Stream outputStream)
		{
			outputStream.WriteByte((byte) FrontEndMessageCode.Describe);

			PGUtil.WriteInt32(outputStream, 4 + 1 + UTF8Encoding.GetByteCount(_portalName) + 1);

			outputStream.WriteByte((Byte) _whatToDescribe);
			PGUtil.WriteString(_portalName, outputStream);
		}
	}
}