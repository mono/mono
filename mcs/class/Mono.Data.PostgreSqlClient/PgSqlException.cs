//
// System.Data.SqlClient.SqlException.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc
//
using System;
using System.Data;
using System.Runtime.Serialization;

namespace System.Data.SqlClient
{
	/// <summary>
	/// Exceptions, as returned by SQL databases.
	/// </summary>
	public sealed class SqlException : SystemException
	{
		private SqlErrorCollection errors; 

		internal SqlException() {
			errors = new SqlErrorCollection();
		}

		internal SqlException(byte theClass, int lineNumber,
			string message,	int number, string procedure,
			string server, string source, byte state) {
						
			errors = new SqlErrorCollection (theClass, 
				lineNumber, message,
				number, procedure,
				server, source, state);
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

			set { 
				errors[0].SetClass(value);
			}
		}

		[MonoTODO]
		public SqlErrorCollection Errors {
			get { 
				return errors;
			}

			set { 
				errors = value;
			}
		}

		[MonoTODO]
		public int LineNumber {
			get { 
				if(errors.Count == 0)
					return 0; // FIXME: throw exception here?
				return errors[0].LineNumber;
			}

			set { 
				errors[0].SetLineNumber(value);
			}
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

			set { 
				errors[0].SetNumber(value);
			}
		}
		
		[MonoTODO]
		public string Procedure {
			get { 
				if(errors.Count == 0)
					return ""; // FIXME: throw exception?
				else
					return errors[0].Procedure;
			}

			set { 
				errors[0].SetProcedure(value);
			}
		}

		[MonoTODO]
		public string Server {
			get { 
				if(errors.Count == 0)
					return ""; // FIXME: throw exception?
				else
					return errors[0].Server;
			}

			set { 
				errors[0].SetServer(value);
			}
		}
		
		[MonoTODO]
		public override string Source {
			get { 
				if(errors.Count == 0)
					return ""; // FIXME: throw exception?
				else
					return errors[0].Source;
			}

			set { 
				errors[0].SetSource(value);
			}
		}

		[MonoTODO]
		public byte State {
			get { 
				if(errors.Count == 0)
					return 0; // FIXME: throw exception?
				else
					return errors[0].State;
			}

			set { 
				errors[0].SetState(value);
			}
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void GetObjectData(SerializationInfo si,
			StreamingContext context) {
			// FIXME: to do
		}

		// [Serializable]
		// [ClassInterface(ClassInterfaceType.AutoDual)]
		public override string ToString() {
			String toStr = "";
			for (int i = 0; i < errors.Count; i++) {
				toStr = toStr + errors[i].ToString() + "\n";
			}
			return toStr;
		}

		internal void Add(byte theClass, int lineNumber,
			string message,	int number, string procedure,
			string server, string source, byte state) {
			
			errors.Add (theClass, lineNumber, message,
				number, procedure,
				server, source, state);
		}

		[MonoTODO]
		~SqlException() {
			// FIXME: destructor to release resources
		}

		#endregion // Methods
	}
}
