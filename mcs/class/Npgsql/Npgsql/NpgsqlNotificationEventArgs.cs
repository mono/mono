// Npgsql.NpgsqlNotificationEventArgs.cs
//
// Author:
//  Wojtek Wierzbicki
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
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
	/// EventArgs class to send Notification parameters.
	/// </summary>
	public class NpgsqlNotificationEventArgs : EventArgs
	{
		/// <summary>
		/// Process ID of the PostgreSQL backend that sent this notification.
		/// </summary>
		public readonly int PID;

		/// <summary>
		/// Condition that triggered that notification.
		/// </summary>
		public readonly string Condition;

		/// <summary>
		/// Additional Information From Notifiying Process (for future use, currently postgres always sets this to an empty string)
		/// </summary>
		public readonly string AdditionalInformation;

		internal NpgsqlNotificationEventArgs(Stream stream, bool readAdditional)
		{
			PID = PGUtil.ReadInt32(stream);
			Condition = PGUtil.ReadString(stream);
			AdditionalInformation = readAdditional ? PGUtil.ReadString(stream) : string.Empty;
		}
	}
}