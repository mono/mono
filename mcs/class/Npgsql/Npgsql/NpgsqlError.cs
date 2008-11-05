// created on 12/7/2003 at 18:36

// Npgsql.NpgsqlError.cs
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
using System.Text;

namespace Npgsql
{
	/// <summary>
	/// EventArgs class to send Notice parameters, which are just NpgsqlError's in a lighter context.
	/// </summary>
	public class NpgsqlNoticeEventArgs : EventArgs
	{
		/// <summary>
		/// Notice information.
		/// </summary>
		public NpgsqlError Notice = null;

		internal NpgsqlNoticeEventArgs(NpgsqlError eNotice)
		{
			Notice = eNotice;
		}
	}

	/// <summary>
	/// This class represents the ErrorResponse and NoticeResponse
	/// message sent from PostgreSQL server.
	/// </summary>
	[Serializable]
	public sealed class NpgsqlError
	{
		private readonly ProtocolVersion protocol_version;
		private readonly String _severity = String.Empty;
		private readonly String _code = String.Empty;
		private readonly String _message = String.Empty;
		private readonly String _detail = String.Empty;
		private readonly String _hint = String.Empty;
		private readonly String _position = String.Empty;
		private readonly String _internalPosition = String.Empty;
		private readonly String _internalQuery = String.Empty;
		private readonly String _where = String.Empty;
		private readonly String _file = String.Empty;
		private readonly String _line = String.Empty;
		private readonly String _routine = String.Empty;
		private String _errorSql = String.Empty;

		/// <summary>
		/// Severity code.  All versions.
		/// </summary>
		public String Severity
		{
			get { return _severity; }
		}

		/// <summary>
		/// Error code.  PostgreSQL 7.4 and up.
		/// </summary>
		public String Code
		{
			get { return _code; }
		}

		/// <summary>
		/// Terse error message.  All versions.
		/// </summary>
		public String Message
		{
			get { return _message; }
		}

		/// <summary>
		/// Detailed error message.  PostgreSQL 7.4 and up.
		/// </summary>
		public String Detail
		{
			get { return _detail; }
		}

		/// <summary>
		/// Suggestion to help resolve the error.  PostgreSQL 7.4 and up.
		/// </summary>
		public String Hint
		{
			get { return _hint; }
		}

		/// <summary>
		/// Position (one based) within the query string where the error was encounterd.  PostgreSQL 7.4 and up.
		/// </summary>
		public String Position
		{
			get { return _position; }
		}

		/// <summary>
		/// Position (one based) within the query string where the error was encounterd.  This position refers to an internal command executed for example inside a PL/pgSQL function. PostgreSQL 7.4 and up.
		/// </summary>
		public String InternalPosition
		{
			get { return _internalPosition; }
		}

		/// <summary>
		/// Internal query string where the error was encounterd.  This position refers to an internal command executed for example inside a PL/pgSQL function. PostgreSQL 7.4 and up.
		/// </summary>
		public String InternalQuery
		{
			get { return _internalQuery; }
		}
		/// <summary>
		/// Trace back information.  PostgreSQL 7.4 and up.
		/// </summary>
		public String Where
		{
			get { return _where; }
		}

		/// <summary>
		/// Source file (in backend) reporting the error.  PostgreSQL 7.4 and up.
		/// </summary>
		public String File
		{
			get { return _file; }
		}

		/// <summary>
		/// Source file line number (in backend) reporting the error.  PostgreSQL 7.4 and up.
		/// </summary>
		public String Line
		{
			get { return _line; }
		}

		/// <summary>
		/// Source routine (in backend) reporting the error.  PostgreSQL 7.4 and up.
		/// </summary>
		public String Routine
		{
			get { return _routine; }
		}

		/// <summary>
		/// String containing the sql sent which produced this error.
		/// </summary>
		public String ErrorSql
		{
			set { _errorSql = value; }
			get { return _errorSql; }
		}

		/// <summary>
		/// Return a string representation of this error object.
		/// </summary>
		public override String ToString()
		{
			StringBuilder B = new StringBuilder();

			if (Severity.Length > 0)
			{
				B.AppendFormat("{0}: ", Severity);
			}
			if (Code.Length > 0)
			{
				B.AppendFormat("{0}: ", Code);
			}
			B.AppendFormat("{0}", Message);
			// CHECKME - possibly multi-line, that is yucky
			//            if (Hint.Length > 0) {
			//                B.AppendFormat(" ({0})", Hint);
			//            }

			return B.ToString();
		}

		internal NpgsqlError(ProtocolVersion protocolVersion, Stream stream)
		{
			switch (protocol_version = protocolVersion)
			{
				case ProtocolVersion.Version2:
					string[] parts = PGUtil.ReadString(stream).Split(new char[] {':'}, 2);
					if (parts.Length == 2)
					{
						_severity = parts[0].Trim();
						_message = parts[1].Trim();
					}
					else
					{
						_severity = string.Empty;
						_message = parts[0].Trim();
					}
					break;
				case ProtocolVersion.Version3:
					// Check the messageLength value. If it is 1178686529, this would be the
					// "FATA" string, which would mean a protocol 2.0 error string.
					if (PGUtil.ReadInt32(stream) == 1178686529)
					{
						string[] v2Parts = ("FATA" + PGUtil.ReadString(stream)).Split(new char[] {':'}, 2);
						if (v2Parts.Length == 2)
						{
							_severity = v2Parts[0].Trim();
							_message = v2Parts[1].Trim();
						}
						else
						{
							_severity = string.Empty;
							_message = v2Parts[0].Trim();
						}
						protocol_version = ProtocolVersion.Version2;
					}
					else
					{
						for (char field = (char) stream.ReadByte(); field != 0; field = (char) stream.ReadByte())
						{
							switch (field)
							{
								case 'S':
									_severity = PGUtil.ReadString(stream);
									;
									break;
								case 'C':
									_code = PGUtil.ReadString(stream);
									;
									break;
								case 'M':
									_message = PGUtil.ReadString(stream);
									;
									break;
								case 'D':
									_detail = PGUtil.ReadString(stream);
									;
									break;
								case 'H':
									_hint = PGUtil.ReadString(stream);
									;
									break;
								case 'P':
									_position = PGUtil.ReadString(stream);
									;
									break;
								case 'p':
									_internalPosition = PGUtil.ReadString(stream);
									;
									break;
								case 'q':
									_internalQuery = PGUtil.ReadString(stream);
									;
									break;
								case 'W':
									_where = PGUtil.ReadString(stream);
									;
									break;
								case 'F':
									_file = PGUtil.ReadString(stream);
									;
									break;
								case 'L':
									_line = PGUtil.ReadString(stream);
									;
									break;
								case 'R':
									_routine = PGUtil.ReadString(stream);
									;
									break;
								
							}
						}
					}
					break;
			}
		}

		internal NpgsqlError(ProtocolVersion protocolVersion, String errorMessage)
		{
			protocol_version = protocolVersion;
			_message = errorMessage;
		}

		/// <summary>
		/// Backend protocol version in use.
		/// </summary>
		internal ProtocolVersion BackendProtocolVersion
		{
			get { return protocol_version; }
		}
	}
}
