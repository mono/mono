//
// Mono.Data.SybaseClient.SybaseException.cs
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

namespace Mono.Data.SybaseClient {
	[Serializable]
	public sealed class SybaseException : SystemException
	{
		#region Fields

		SybaseErrorCollection errors; 

		#endregion Fields

		#region Constructors

		internal SybaseException () 
			: base ("a SQL Exception has occurred.") 
		{
			errors = new SybaseErrorCollection();
		}

		internal SybaseException (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state) 
			: base (message) 
		{
			errors = new SybaseErrorCollection (theClass, lineNumber, message, number, procedure, server, source, state);
		}

		#endregion // Constructors

		#region Properties

		public byte Class {
			get { return errors [0].Class; }
		}

		public SybaseErrorCollection Errors {
			get { return errors; }
		}

		public int LineNumber {
			get { return errors [0].LineNumber; }
		}
		
		public override string Message 	{
			get {
				StringBuilder result = new StringBuilder ();
				foreach (SybaseError error in Errors) {
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

		internal static SybaseException FromTdsInternalException (TdsInternalException e)
		{
			return new SybaseException (e.Class, e.LineNumber, e.Message, e.Number, e.Procedure, e.Server, "Mono SybaseClient Data Provider", e.State);
		}

		#endregion // Methods
	}
}
