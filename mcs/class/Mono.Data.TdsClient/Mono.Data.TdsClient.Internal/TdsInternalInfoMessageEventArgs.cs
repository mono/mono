//
// Mono.Data.TdsClient.Internal.TdsInternalInfoMessageEventArgs.cs
//
// Authors:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace Mono.Data.TdsClient.Internal {
	internal class TdsInternalInfoMessageEventArgs : EventArgs
	{
		#region Fields

		TdsInternalErrorCollection errors;

		#endregion // Fields

		#region Constructors
		
		public TdsInternalInfoMessageEventArgs (TdsInternalErrorCollection errors)
		{
			this.errors = errors;
		}

		public TdsInternalInfoMessageEventArgs (TdsInternalError error)
		{
			this.errors = new TdsInternalErrorCollection ();
			errors.Add (error);
		}

		#endregion // Constructors

		#region Properties

		public TdsInternalErrorCollection Errors {
			get { return errors; }
		}

		public byte Class {
			get { return errors[0].Class; }
		}

		public int LineNumber {
			get { return errors[0].LineNumber; }
		}

		public string Message {
			get { return errors[0].Message; }
		}

		public int Number {
			get { return errors[0].Number; }
		}

		public string Procedure {
			get { return errors[0].Procedure; }
		}

		public string Server {
			get { return errors[0].Server; }
		}
		
		public string Source {
			get { return errors[0].Source; }
		}

		public byte State {
			get { return errors[0].State; }
		}

		#endregion // Properties

		#region Methods

		public int Add (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state)
		{
			return errors.Add (new TdsInternalError (theClass, lineNumber, message, number, procedure, server, source, state));
		}

		#endregion // Methods
	}
}
