//
// System.Data.SqlClient.SqlException.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
// Copyright (C) Tim Coleman, 2002
//

using Mono.Data.Tds.Protocol;
using System;
using System.ComponentModel;
using System.Data;
using System.Runtime.Serialization;
using System.Text;

namespace System.Data.SqlClient {
	[Serializable]
	public sealed class SqlException : SystemException
	{
		#region Fields

		SqlErrorCollection errors; 

		#endregion // Fields

		#region Constructors

		internal SqlException () 
			: base ("a SQL Exception has occurred.") 
		{
			errors = new SqlErrorCollection();
		}

		internal SqlException (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state) 
			: base (message) 
		{
			errors = new SqlErrorCollection (theClass, lineNumber, message, number, procedure, server, source, state);
		}

		#endregion // Constructors

		#region Properties

		public byte Class {
			get { return Errors [0].Class; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public SqlErrorCollection Errors {
			get { return errors; }
		}

		public int LineNumber {
			get { return Errors [0].LineNumber; }
		}
		
		public override string Message 	{
			get { 
				StringBuilder result = new StringBuilder ();
				foreach (SqlError error in Errors) {
					if (result.Length > 0)
						result.Append ('\n');
					result.Append (error.Message);
				}
				return result.ToString ();
			}
		}
		
		public int Number {
			get { return Errors [0].Number; }
		}
		
		public string Procedure {
			get { return Errors [0].Procedure; }
		}

		public string Server {
			get { return Errors [0].Server; }
		}
		
		public override string Source {
			get { return Errors [0].Source; }
		}

		public byte State {
			get { return Errors [0].State; }
		}

		#endregion // Properties

		#region Methods

		internal static SqlException FromTdsInternalException (TdsInternalException e)
		{
			return new SqlException (e.Class, e.LineNumber, e.Message, e.Number, e.Procedure, e.Server, "Mono SqlClient Data Provider", e.State);
		}

		[MonoTODO ("Determine how to serialize this class.")]
		public override void GetObjectData (SerializationInfo si, StreamingContext context) 
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
