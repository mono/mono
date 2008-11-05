// Npgsql.NpgsqlCopyFormat.cs
//
// Author:
// 	Kalle Hallivuori <kato@iki.fi>
//
//	Copyright (C) 2007 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
//  Copyright (c) 2002-2007, The Npgsql Development Team
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

namespace Npgsql
{
	/// <summary>
	/// Represents information about COPY operation data transfer format as returned by server.
	/// </summary>
	public sealed class NpgsqlCopyFormat
	{
		private readonly byte _copyFormat;
		private readonly Int16[] _copyFieldFormats;

		/// <summary>
		/// Only created when a CopyInResponse or CopyOutResponse is received by NpgsqlState.ProcessBackendResponses()
		/// </summary>
		internal NpgsqlCopyFormat(byte copyFormat, Int16[] fieldFormats)
		{
			_copyFormat = copyFormat;
			_copyFieldFormats = fieldFormats;
		}

		/// <summary>
		/// Returns true if this operation is currently active and in binary format.
		/// </summary>
		public bool IsBinary
		{
			get { return _copyFormat != 0; }
		}

		/// <summary>
		/// Returns true if this operation is currently active and field at given location is in binary format.
		/// </summary>
		public bool FieldIsBinary(int fieldNumber)
		{
			return _copyFieldFormats[fieldNumber] != 0;
		}

		/// <summary>
		/// Returns number of fields if this operation is currently active, otherwise -1
		/// </summary>
		public int FieldCount
		{
			get { return _copyFieldFormats.Length; }
		}
	}
}