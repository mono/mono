//
// Mono.Data.SybaseClient.SybaseInfoMessageEventArgs.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using Mono.Data.Tds.Protocol;
using System;
using System.Data;

namespace Mono.Data.SybaseClient {
	public sealed class SybaseInfoMessageEventArgs : EventArgs
	{
		#region Fields

		SybaseErrorCollection errors = new SybaseErrorCollection ();

		#endregion // Fields

		#region Constructors

		internal SybaseInfoMessageEventArgs (TdsInternalErrorCollection tdsErrors)
		{
			foreach (TdsInternalError e in tdsErrors)
				errors.Add (e.Class, e.LineNumber, e.Message, e.Number, e.Procedure, e.Server, "Mono SybaseClient Data Provider", e.State);
		}

		#endregion // Constructors

		#region Properties

		public SybaseErrorCollection Errors {
			get { return errors; }
		}	

		public string Message {
			get { return errors[0].Message; }
		}	

		public string Source {
			get { return errors[0].Source; }
		}

		#endregion // Properties

		#region Methods

		public override string ToString () 
		{
			return Message;
		}

		#endregion // Methods
	}
}
