//
// Mono.Data.TdsClient.TdsException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using Mono.Data.Tds.Protocol;
using System;
using System.Data;
using System.Runtime.Serialization;
using System.Text;

namespace Mono.Data.TdsClient {
	[Serializable]
	public sealed class TdsException : SystemException
	{
		#region Fields

		TdsErrorCollection errors; 

		#endregion Fields

		#region Constructors

		internal TdsException () 
			: base ("a SQL Exception has occurred.") 
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
			get { return errors [0].Class; }
		}

		public TdsErrorCollection Errors {
			get { return errors; }
		}

		public int LineNumber {
			get { return errors [0].LineNumber; }
		}
		
		public override string Message 	{
			get {
				StringBuilder result = new StringBuilder ();
				foreach (TdsError error in Errors) {
					if (result.Length > 0)
						result.Append ('\n');
					result.Append (error.Message);
				}
				return result.ToString ();
			}                                                                
		}
		
		public int Number {
			get { return errors [0].Number; }
		}
		
		public string Procedure {
			get { return errors [0].Procedure; }
		}

		public string Server {
			get { return errors [0].Server; }
		}
		
		public override string Source {
			get { return errors [0].Source; }
		}

		public byte State {
			get { return errors [0].State; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void GetObjectData (SerializationInfo si, StreamingContext context) 
		{
			throw new NotImplementedException ();
		}

		internal static TdsException FromTdsInternalException (TdsInternalException e)
		{
			return new TdsException (e.Class, e.LineNumber, e.Message, e.Number, e.Procedure, e.Server, "Mono TdsClient Data Provider", e.State);
		}

		#endregion // Methods
	}
}
