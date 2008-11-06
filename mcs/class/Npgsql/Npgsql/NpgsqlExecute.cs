// created on 1/7/2003 at 20:03
// Npgsql.NpgsqlExecute.cs
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
	internal sealed class NpgsqlExecute : ClientMessage
	{
		private readonly String _portalName;
		private readonly Int32 _maxRows;


		public NpgsqlExecute(String portalName, Int32 maxRows)
		{
			_portalName = portalName;
			_maxRows = maxRows;
		}

		public String PortalName
		{
			get { return _portalName; }
		}

		public override void WriteToStream(Stream outputStream)
		{
			outputStream.WriteByte((byte) FrontEndMessageCode.Execute);

			PGUtil.WriteInt32(outputStream, 4 + UTF8Encoding.GetByteCount(_portalName) + 1 + 4);

			PGUtil.WriteString(_portalName, outputStream);
			PGUtil.WriteInt32(outputStream, _maxRows);
		}
	}
}