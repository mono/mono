// Npgsql.NpgsqlMessageTypes.cs
// 
// Author:
// 	Dave Joyner <d4ljoyn@yahoo.com>
//
//	Copyright (C) 2002 The Npgsql Development Team
//

// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA


using System;

namespace Npgsql
{
	/// <summary>
	/// Summary description for NpgsqlMessageTypes.
	/// </summary>
	internal sealed class NpgsqlMessageTypes
	{
		private NpgsqlMessageTypes()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		public const Char AsciiRow = 'D';
		public const Char AuthenticationRequest = 'R';

		// specific Authentication request types
		public const Int32 AuthenticationOk = 0;
		public const Int32 AuthenticationKerberosV4 = 1;
		public const Int32 AuthenticationKerberosV5 = 2;
		public const Int32 AuthenticationClearTextPassword = 3;
		public const Int32 AuthenticationCryptPassword = 4;
		public const Int32 AuthenticationMD5Password = 5;
		public const Int32 AuthenticationSCMCredential = 6;

		public const Char BackendKeyData = 'K';
		public const Char BinaryRow = 'B';
		public const Char CancelRequest = 'F';
		public const Char CompletedResponse = 'C';
		public const Char CopyDataRows = ' ';
		public const Char CopyInResponse = 'G';
		public const Char CopyOutResponse = 'H';
		public const Char CursorResponse = 'B';
		public const Char EmptyQueryResponse = 'I';
		public const Char ErrorResponse = 'E';
		public const Char FunctionCall = 'F';
		
		public const Char FunctionResultResponse = 'V';
		// specific function result responses
		public const Char FunctionResultNonEmptyResponse = 'G';
		public const Char FunctionResultVoidResponse = '0';
		
		public const Char NoticeResponse = 'N';
		public const Char NotificationResponse = 'A';
		public const Char PasswordPacket = ' ';
		public const Char Query = 'Q';
		public const Char ReadyForQuery = 'Z';
		public const Char RowDescription = 'T';
		public const Char SSLRequest = ' ';
		public const Char StartupPacket = ' ';



	}
}
