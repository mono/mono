// Npgsql.NpgsqlCopyInState.cs
//
// Author:
// 	Kalle Hallivuori <kato@iki.fi>
//
//	Copyright (C) 2007 The Npgsql Development Team
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
	/// Represents an ongoing COPY FROM STDIN operation.
	/// Provides methods to push data to server and end or cancel the operation.
	/// </summary>
	internal sealed class NpgsqlCopyInState : NpgsqlState
	{
		public static readonly NpgsqlCopyInState Instance = new NpgsqlCopyInState();

		private NpgsqlCopyFormat _copyFormat = null;

		private NpgsqlCopyInState()
			: base()
		{
		}

		/// <summary>
		/// Copy format information returned from server.
		/// </summary>
		public override NpgsqlCopyFormat CopyFormat
		{
			get { return _copyFormat; }
		}

		/// <summary>
		/// Called from NpgsqlState.ProcessBackendResponses upon CopyInResponse.
		/// If CopyStream is already set, it is used to read data to push to server, after which the copy is completed.
		/// Otherwise CopyStream is set to a writable NpgsqlCopyInStream that calls SendCopyData each time it is written to.
		/// </summary>
		protected override void StartCopy(NpgsqlConnector context, NpgsqlCopyFormat copyFormat)
		{
			_copyFormat = copyFormat;
			Stream userFeed = context.Mediator.CopyStream;
			if (userFeed == null)
			{
				context.Mediator.CopyStream = new NpgsqlCopyInStream(context);
			}
			else
			{
				// copy all of user feed to server at once
				int bufsiz = context.Mediator.CopyBufferSize;
				byte[] buf = new byte[bufsiz];
				int len;
				while ((len = userFeed.Read(buf, 0, bufsiz)) > 0)
				{
					SendCopyData(context, buf, 0, len);
				}
				SendCopyDone(context);
			}
		}

		/// <summary>
		/// Sends given packet to server as a CopyData message.
		/// Does not check for notifications! Use another thread for that.
		/// </summary>
		public override void SendCopyData(NpgsqlConnector context, byte[] buf, int off, int len)
		{
			Stream toServer = context.Stream;
			toServer.WriteByte((byte) FrontEndMessageCode.CopyData);
			PGUtil.WriteInt32(toServer, len + 4);
			toServer.Write(buf, off, len);
		}

		/// <summary>
		/// Sends CopyDone message to server. Handles responses, ie. may throw an exception.
		/// </summary>
		public override void SendCopyDone(NpgsqlConnector context)
		{
			Stream toServer = context.Stream;
			toServer.WriteByte((byte) FrontEndMessageCode.CopyDone);
			PGUtil.WriteInt32(toServer, 4); // message without data
			toServer.Flush();
			ProcessBackendResponses(context);
		}

		/// <summary>
		/// Sends CopyFail message to server. Handles responses, ie. should always throw an exception:
		/// in CopyIn state the server responds to CopyFail with an error response;
		/// outside of a CopyIn state the server responds to CopyFail with an error response;
		/// without network connection or whatever, there's going to eventually be a failure, timeout or user intervention.
		/// </summary>
		public override void SendCopyFail(NpgsqlConnector context, String message)
		{
			Stream toServer = context.Stream;
			toServer.WriteByte((byte) FrontEndMessageCode.CopyFail);
			byte[] buf = ENCODING_UTF8.GetBytes((message ?? string.Empty) + '\x00');
			PGUtil.WriteInt32(toServer, 4 + buf.Length);
			toServer.Write(buf, 0, buf.Length);
			toServer.Flush();
			ProcessBackendResponses(context);
		}
	}
}