// Npgsql.NpgsqlCancelRequest.cs
//
// Author:
//  Francisco Jr. (fxjrlists@yahoo.com.br)
//
//  Copyright (C) 2002-2006 The Npgsql Development Team
//  http://pgfoundry.org/projects/npgsql
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
	/// This class represents the CancelRequest message sent to PostgreSQL
	/// server.
	/// </summary>
	///
	internal sealed class NpgsqlCancelRequest : ClientMessage
	{
		// Logging related values
		//private static readonly String CLASSNAME = "NpgsqlCancelRequest";


		private static readonly Int32 CancelRequestMessageSize = 16;
		private static readonly Int32 CancelRequestCode = 1234 << 16 | 5678;

		private readonly NpgsqlBackEndKeyData BackendKeydata;


		public NpgsqlCancelRequest(NpgsqlBackEndKeyData BackendKeydata)
		{
			this.BackendKeydata = BackendKeydata;
		}

		public override void WriteToStream(Stream outputStream)
		{
			PGUtil.WriteInt32(outputStream, CancelRequestMessageSize);
			PGUtil.WriteInt32(outputStream, CancelRequestCode);
			PGUtil.WriteInt32(outputStream, BackendKeydata.ProcessID);
			PGUtil.WriteInt32(outputStream, BackendKeydata.SecretKey);

			outputStream.Flush();
		}
	}
}