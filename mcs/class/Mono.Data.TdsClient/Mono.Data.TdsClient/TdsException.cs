//
// Mono.Data.TdsClient.TdsException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using Mono.Data.TdsClient.Internal;
using System;
using System.Runtime.Serialization;

namespace Mono.Data.TdsClient {
        public class TdsException : SystemException
	{
		#region Fields

		byte theClass;
		TdsErrorCollection errors;
		int lineNumber;
		string message;
		int number;
		string procedure;
		string server;
		string source;
		byte state;

		#endregion // Fields

		#region Constructors

		internal TdsException ()
                       : base ("a TDS Exception has occurred.")
		{
			errors = new TdsErrorCollection();
		}

		internal TdsException (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state)
			: base (message)
		{
			errors = new TdsErrorCollection (theClass, lineNumber, message, number, procedure, server, source, state);
		}

		#endregion // Constructors

		#region Properties

		public byte Class {
			get { return theClass; }
		}

		public TdsErrorCollection Errors {
			get { return errors; }
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
