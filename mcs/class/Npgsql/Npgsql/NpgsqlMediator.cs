// created on 30/7/2002 at 00:31

// Npgsql.NpgsqlMediator.cs
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
	///<summary>
	/// This class is responsible for serving as bridge between the backend
	/// protocol handling and the core classes. It is used as the mediator for
	/// exchanging data generated/sent from/to backend.
	/// </summary>
	///
	internal sealed class NpgsqlMediator
	{
		// Stream for user to exchange COPY data
		private Stream _copyStream;
		// Size of data chunks read from user stream and written to server in COPY IN
		private int _copyBufferSize = 8192;
		// Very temporary holder of data received during COPY OUT
		private byte[] _receivedCopyData;


		//
		// Responses collected from the backend.
		//
		private String _sqlSent;
		private Int32 _commandTimeout;


		public NpgsqlMediator()
		{
			_sqlSent = String.Empty;
			_commandTimeout = 20;
		}

		public void ResetResponses()
		{
			_sqlSent = String.Empty;
			_commandTimeout = 20;
		}

		public String SqlSent
		{
			set { _sqlSent = value; }

			get { return _sqlSent; }
		}

		public Int32 CommandTimeout
		{
			set { _commandTimeout = value; }

			get { return _commandTimeout; }
		}

		public Stream CopyStream
		{
			get { return _copyStream; }
			set { _copyStream = value; }
		}

		public int CopyBufferSize
		{
			get { return _copyBufferSize; }
			set { _copyBufferSize = value; }
		}

		public byte[] ReceivedCopyData
		{
			get
			{
				byte[] result = _receivedCopyData;
				_receivedCopyData = null;
				return result;
			}
			set { _receivedCopyData = value; }
		}
	}
}