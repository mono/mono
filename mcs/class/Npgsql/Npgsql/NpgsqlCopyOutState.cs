// Npgsql.NpgsqlCopyOutState.cs
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
	/// Represents an ongoing COPY TO STDOUT operation.
	/// Provides methods to read data from server or end the operation.
	/// </summary>
	internal sealed class NpgsqlCopyOutState : NpgsqlState
	{
		public static readonly NpgsqlCopyOutState Instance = new NpgsqlCopyOutState();

		//private readonly String CLASSNAME = "NpgsqlCopyOutState";

		private NpgsqlCopyFormat _copyFormat = null;

		private NpgsqlCopyOutState()
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
		/// Called from NpgsqlState.ProcessBackendResponses upon CopyOutResponse.
		/// If CopyStream is already set, it is used to write data received from server, after which the copy ends.
		/// Otherwise CopyStream is set to a readable NpgsqlCopyOutStream that receives data from server.
		/// </summary>
		protected override void StartCopy(NpgsqlConnector context, NpgsqlCopyFormat copyFormat)
		{
			_copyFormat = copyFormat;
			Stream userFeed = context.Mediator.CopyStream;
			if (userFeed == null)
			{
				context.Mediator.CopyStream = new NpgsqlCopyOutStream(context);
			}
			else
			{
				byte[] buf;
				while ((buf = GetCopyData(context)) != null)
				{
					userFeed.Write(buf, 0, buf.Length);
				}
				userFeed.Close();
			}
		}

		/// <summary>
		/// Called from NpgsqlOutStream.Read to read copy data from server.
		/// </summary>
		public override byte[] GetCopyData(NpgsqlConnector context)
		{
			// polling in COPY would take seconds on Windows
			foreach (IServerResponseObject obj in ProcessBackendResponses_Ver_3(context))
			{
				if (obj is IDisposable)
				{
					(obj as IDisposable).Dispose();
				}
			}
			return context.Mediator.ReceivedCopyData;
		}
	}
}