//
// Mono.Data.SybaseClient.SybaseException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using Mono.Data.TdsClient.Internal;
using System;
using System.Data;
using System.Runtime.Serialization;

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

		[MonoTODO]
		public byte Class {
			get { 
				if(errors.Count == 0)
					return 0; // FIXME: throw exception here?
				else
					return errors[0].Class;
			}
			set { errors[0].SetClass(value); }
		}

		[MonoTODO]
		public SybaseErrorCollection Errors 
		{
			get { return errors; }
			set { errors = value; }
		}

		[MonoTODO]
		public int LineNumber {
			get { if(errors.Count == 0)
					return 0; // FIXME: throw exception here?
				return errors[0].LineNumber;
			}
			set { errors[0].SetLineNumber(value); }
		}
		
		[MonoTODO]
		public override string Message 	{
			get { 
				if(errors.Count == 0)
					return ""; // FIXME: throw exception?
				else {
					String msg = "";
					int i = 0;
					
					for(i = 0; i < errors.Count - 1; i++) {
						msg = msg + errors[i].Message + "\n";
                                        }
					msg = msg + errors[i].Message;

					return msg;
				}
			}
		}
		
		[MonoTODO]
		public int Number {
			get { 
				if(errors.Count == 0)
					return 0; // FIXME: throw exception?
				else
					return errors[0].Number;
			}
			set { errors[0].SetNumber(value); }
		}
		
		[MonoTODO]
		public string Procedure {
			get { 
				if(errors.Count == 0)
					return ""; // FIXME: throw exception?
				else
					return errors[0].Procedure;
			}
			set { errors[0].SetProcedure(value); }
		}

		[MonoTODO]
		public string Server {
			get { 
				if(errors.Count == 0)
					return ""; // FIXME: throw exception?
				else
					return errors[0].Server;
			}
			set { errors[0].SetServer(value); }
		}
		
		[MonoTODO]
		public override string Source {
			get { 
				if(errors.Count == 0)
					return ""; // FIXME: throw exception?
				else
					return errors[0].Source;
			}
			set { errors[0].SetSource(value); }
		}

		[MonoTODO]
		public byte State {
			get { 
				if(errors.Count == 0)
					return 0; // FIXME: throw exception?
				else
					return errors[0].State;
			}
			set { errors[0].SetState(value); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void GetObjectData (SerializationInfo si, StreamingContext context) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal static SybaseException FromTdsError (TdsPacketErrorResultCollection errors)
		{
			TdsMessage message = errors[0].Message;
			return new SybaseException (message.Severity, message.Line, message.Message, message.Number, message.ProcName, message.Server, "Mono SybaseClient Data Provider", message.State);
		}

		#endregion // Methods
	}
}
