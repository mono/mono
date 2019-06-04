// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Runtime.Serialization;

namespace System.Data.SqlClient
{
	public class SqlException : DbException 
	{
		const string EXCEPTION_MESSAGE = "System.Data.SqlClient.SqlException is not supported on the current platform.";

		internal bool _doNotReconnect;

		static internal SqlException CreateException(SqlErrorCollection errorCollection, string serverVersion)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		static internal SqlException CreateException(SqlErrorCollection errorCollection, string serverVersion, SqlInternalConnectionTds internalConnection, Exception innerException = null)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		static internal SqlException CreateException(SqlErrorCollection errorCollection, string serverVersion, Guid conId, Exception innerException = null)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		SqlException () {}

		public override void GetObjectData (SerializationInfo si, StreamingContext context)
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public byte Class
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public Guid ClientConnectionId
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public SqlErrorCollection Errors
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public int LineNumber
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override string Message
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public int Number
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public string Procedure
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public string Server
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public override string Source
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public byte State
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		
		internal SqlException InternalClone()
			=> throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
	}
}
