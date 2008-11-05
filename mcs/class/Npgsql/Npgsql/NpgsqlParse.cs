// created on 22/6/2003 at 18:33

// Npgsql.NpgsqlParse.cs
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
	internal sealed class NpgsqlParse : ClientMessage
	{
		// Logging related values
		//private static readonly String CLASSNAME = "NpgsqlParse";

		private readonly String _prepareName;
		private readonly String _queryString;
		private readonly Int32[] _parameterIDs;


		public NpgsqlParse(String prepareName, String queryString, Int32[] parameterIDs)
		{
			_prepareName = prepareName;
			_queryString = queryString;
			_parameterIDs = parameterIDs;
		}

		public override void WriteToStream(Stream outputStream)
		{
			outputStream.WriteByte((byte) FrontEndMessageCode.Parse);

			// message length =
			// Int32 self
			// name of prepared statement + 1 null string terminator +
			// query string + 1 null string terminator
			// + Int16
			// + Int32 * number of parameters.
			Int32 messageLength = 4 + UTF8Encoding.GetByteCount(_prepareName) + 1 + UTF8Encoding.GetByteCount(_queryString) + 1 +
			                      2 + (_parameterIDs.Length*4);
			//Int32 messageLength = 4 + _prepareName.Length + 1 + _queryString.Length + 1 + 2 + (_parameterIDs.Length * 4);

			PGUtil.WriteInt32(outputStream, messageLength);
			PGUtil.WriteString(_prepareName, outputStream);
			PGUtil.WriteString(_queryString, outputStream);
			PGUtil.WriteInt16(outputStream, (Int16) _parameterIDs.Length);


			for (Int32 i = 0; i < _parameterIDs.Length; i++)
			{
				PGUtil.WriteInt32(outputStream, _parameterIDs[i]);
			}
		}
	}
}