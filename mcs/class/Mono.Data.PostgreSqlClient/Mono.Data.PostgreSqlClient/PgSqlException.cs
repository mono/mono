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

namespace System.Data.SqlClient
{
	/// <summary>
	/// Exceptions, as returned by SQL databases.
	/// </summary>
	public sealed class SqlException : SystemException
	{
		#region Properties
		
		[MonoTODO]
		public byte Class {
			get { 
				throw new NotImplementedException (); 
			}
			set { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public SqlErrorCollection Errors {
			get { 
				throw new NotImplementedException (); 
			}
			set { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public int LineNumber {
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}
		
		[MonoTODO]
		public override string Message 	{
			get { 
				throw new NotImplementedException (); 
			}
		}
		
		[MonoTODO]
		public int Number {
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}
		
		[MonoTODO]
		public string Procedure {
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public string Server {
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}
		
		[MonoTODO]
		public override string Source {
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public byte State {
			get { 
				throw new NotImplementedException (); 
			}

			set { 
				throw new NotImplementedException (); 
			}
		}

		#endregion // Properties
	}
}
