//
// Mono.Data.Tds.Protocol.TdsInternalException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using System;
using System.Runtime.Serialization;

namespace Mono.Data.Tds.Protocol {
        public class TdsInternalException : SystemException
	{
		#region Fields

		byte theClass;
		int lineNumber;
		string message;
		int number;
		string procedure;
		string server;
		string source;
		byte state;

		#endregion // Fields

		#region Constructors

		internal TdsInternalException ()
                       : base ("a TDS Exception has occurred.")
		{
		}

		internal TdsInternalException (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state)
			: base (message)
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

		public override string Message {
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

		public override string Source {
			get { return source; }
		}

		public byte State {
			get { return state; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void GetObjectData (SerializationInfo si, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
