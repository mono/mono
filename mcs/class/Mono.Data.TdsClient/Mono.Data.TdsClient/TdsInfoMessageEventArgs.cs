//
// Mono.Data.TdsClient.TdsInfoMessageEventArgs.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002
//

using Mono.Data.Tds.Protocol;
using System;
using System.Data;

namespace Mono.Data.TdsClient {
	public sealed class TdsInfoMessageEventArgs : EventArgs
	{
		#region Fields

		TdsErrorCollection errors = new TdsErrorCollection ();

		#endregion // Fields

		#region Constructors
	
		internal TdsInfoMessageEventArgs (TdsInternalErrorCollection tdsErrors)
		{
			foreach (TdsInternalError e in tdsErrors)
				errors.Add (e.Class, e.LineNumber, e.Message, e.Number, e.Procedure, e.Server, "Mono TdsClient Data Provider", e.State);
		}

		#endregion // Constructors

		#region Properties

		public TdsErrorCollection Errors {
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
