//
// System.Data.SqlClient.SqlError.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Data;
using System.Runtime.InteropServices;

namespace System.Data.SqlClient {
	/// <summary>
	/// Describes an error from a SQL database.
	/// </summary>
	[Serializable]
	public sealed class SqlError
	{
		#region Fields

		byte theClass = 0;
		int lineNumber = 0;
		string message = "";
		int number = 0;
		string procedure = "";
		string server = "";
		string source = "";
		byte state = 0;

		#endregion // Fields

		#region Constructors

		internal SqlError (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state) 
		{
			this.theClass = theClass;
			this.lineNumber = lineNumber;
			this.message = message;
			this.number = number;
			this.procedure = procedure;
			this.server = server;
			this.source = source;
			this.state = state;
		}

		#endregion // Constructors
		
		#region Properties

		public byte Class {
			get { return theClass; }
		}

		public int LineNumber {
			get { return lineNumber; }
		}

		public string Message {
			get { return message; }
		}
		
		public int Number {
			get { return number; }
		}

		public string Procedure {
			get { return procedure; }
		}

		public string Server {
			get { return server; }
		}

		public string Source {
			get { return source; }
		}

		public byte State {
			get { return state; }
		}

		#endregion

		#region Methods

		public override string ToString ()
		{
			return Message;
		}

		#endregion
		
	}
}
