//
// Mono.Data.TdsClient.TdsError.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using Mono.Data.TdsClient.Internal;
using System;

namespace Mono.Data.TdsClient {
        public sealed class TdsError
	{
		#region Fields
		
		byte theClass = 0x0;
		int lineNumber;
		string message;
		int number;
		string procedure;
		string server;
		string source;
		byte state;

		#endregion // Fields

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

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
