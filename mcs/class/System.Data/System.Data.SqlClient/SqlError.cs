//
// System.Data.SqlClient.SqlError.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
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
		byte theClass = 0;
		int lineNumber = 0;
		string message = "";
		int number = 0;
		string procedure = "";
		string server = "";
		string source = "";
		byte state = 0;

		internal SqlError(byte theClass, int lineNumber,
			string message,	int number, string procedure,
			string server, string source, byte state) {
			this.theClass = theClass;
			this.lineNumber = lineNumber;
			this.message = message;
			this.number = number;
			this.procedure = procedure;
			this.server = server;
			this.source = source;
			this.state = state;
		}
		
		#region Properties

		[MonoTODO]
		/// <summary>
		/// severity level of the error
		/// </summary>
		public byte Class {
			get { 
				return theClass;
			}
		}

		[MonoTODO]
		public int LineNumber {
			get { 
			   return lineNumber;
		   }
		}

		[MonoTODO]
		public string Message {
			get { 
				return message;
			}
		}
		
		[MonoTODO]
		public int Number {
			get { 
				return number;
			}
		}

		[MonoTODO]
		public string Procedure {
			get { 
				return procedure;
			}
		}

		[MonoTODO]
		public string Server {
			get { 
				return server;
			}
		}

		[MonoTODO]
		public string Source {
			get { 
				return source;
			}
		}

		[MonoTODO]
		public byte State {
			get { 
				return state;
			}
		}

		#endregion

		#region Methods

		[MonoTODO]
		public override string ToString ()
		{
			String toStr;
			String stackTrace;
			stackTrace = " <Stack Trace>";
			// FIXME: generate the correct SQL error string
			toStr = "SqlError:" + message + stackTrace;
			return toStr;
		}

		internal void SetClass(byte theClass) {
			this.theClass = theClass;
		}

		internal void SetLineNumber(int lineNumber) {
			this.lineNumber = lineNumber;
		}

		internal void SetMessage(string message) {
			this.message = message;
		}

		internal void SetNumber(int number) {
			this.number = number;
		}

		internal void SetProcedure(string procedure) {
			this.procedure = procedure;
		}

		internal void SetServer(string server) {
			this.server = server;
		}

		internal void SetSource(string source) {
			this.source = source;
		}

		internal void SetState(byte state) {
			this.state = state;
		}

		#endregion
		
	}
}
