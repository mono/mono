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

using Mono.Data.TdsClient.Internal;
using System;
using System.Data;
using System.Runtime.Serialization;

namespace System.Data.SqlClient {
	[Serializable]
	public sealed class SqlException : SystemException
	{
		private SqlErrorCollection errors; 

		internal SqlException() 
			: base ("a SQL Exception has occurred.") 
		{
			errors = new SqlErrorCollection();
		}

		internal SqlException (byte theClass, int lineNumber, string message, int number, string procedure, string server, string source, byte state) 
			: base (message) 
		{
			errors = new SqlErrorCollection (theClass, lineNumber, message, number, procedure, server, source, state);
		}

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
		public SqlErrorCollection Errors 
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
		public override void GetObjectData(SerializationInfo si, StreamingContext context) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal static SqlException FromTdsError (TdsPacketErrorResultCollection errors)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
