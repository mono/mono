//
// Mono.Data.Tds.Protocol.TdsMessage.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Text;

namespace Mono.Data.Tds.Protocol {
	public class TdsMessage 
	{
		#region Fields

		int number = 0;
		byte state = 0;
		byte severity = 0;
		string message = String.Empty;
		string server = String.Empty;
		string procName = String.Empty;
		int line = 0;

		#endregion // Fields

		#region Properties

		public int Line {
			get { return line; }
			set { line = value; }
		}

		public string Message {
			get { return message; }
			set { message = value; }
		}

		public int Number {
			get { return number; }
			set { number = value; }
		}

		public string ProcName {
			get { return procName; }
			set { procName = value; }
		}

		public byte State {
			get { return state; }
			set { state = value; }
		}

		public string Server {
			get { return server; }
			set { server = value; }
		}

		public byte Severity {
			get { return severity; }
			set { severity = value; }
		}

		#endregion // Properties

		#region Methods

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("Server: ");
			if (server != String.Empty)
				sb.Append (String.Format ("{0}, ", server));
			if (number != 0)
				sb.Append (String.Format ("Msg {0}, ", number));
			if (severity != 0)
				sb.Append (String.Format ("Level {0}, ", severity));
			if (state != 0)
				sb.Append (String.Format ("State {0}, ", state));
			if (procName != String.Empty)
				sb.Append (String.Format ("Procedure {0}, ", procName));
			if (line != 0)
				sb.Append (String.Format ("Line {0} ", line));
			sb.Append (message);
			return sb.ToString ();
		}

		#endregion // Methods
	}
}
