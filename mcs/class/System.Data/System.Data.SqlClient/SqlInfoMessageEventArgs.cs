//
// System.Data.SqlClient.SqlInfoMessageEventArgs.cs
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

namespace System.Data.SqlClient {
	public sealed class SqlInfoMessageEventArgs : EventArgs
	{
		#region Fields

		SqlErrorCollection errors = new SqlErrorCollection ();

		#endregion // Fields

		#region Constructors
	
		internal SqlInfoMessageEventArgs (TdsInternalErrorCollection tdsErrors)
		{
			foreach (TdsInternalError e in tdsErrors) 
				errors.Add (e.Class, e.LineNumber, e.Message, e.Number, e.Procedure, e.Server, "Mono SqlClient Data Provider", e.State);
		}

		#endregion // Constructors

		#region Properties

		public SqlErrorCollection Errors {
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

		public override string ToString() 
		{
			return Message;
		}

		#endregion // Methods
	}
}
