//
// SqlException.cs
//
// Author:
//       Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright (c) 2016 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		static internal SqlException CreateException(SqlErrorCollection errorCollection, string serverVersion, SqlInternalConnectionTds internalConnection, Exception innerException = null)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		static internal SqlException CreateException(SqlErrorCollection errorCollection, string serverVersion, Guid conId, Exception innerException = null)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		SqlException () {}

		public override void GetObjectData (SerializationInfo si, StreamingContext context)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public byte Class {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public Guid ClientConnectionId {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public SqlErrorCollection Errors {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int LineNumber {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override string Message {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int Number {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string Procedure {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string Server {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override string Source {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public byte State {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}
	}
}
