//
// System.Data.SqlClient.SqlError.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//
using System;
using System.Data;
using System.Runtime.InteropServices;

namespace System.Data.SqlClient
{
	/// <summary>
	/// Describes an error from a SQL database.
	/// </summary>
	[MonoTODO]
	public sealed class SqlError
	{
		#region Properties

		[MonoTODO]
		public byte Class {
			get { 
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public int LineNumber {
			get { 
			   throw new NotImplementedException ();
		   }
		}

		[MonoTODO]
		public string Message {
			get { 
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public int Number {
			get { 
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string Procedure {
			get { 
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string Server {
			get { 
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string Source {
			get { 
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public byte State {
			get { 
				throw new NotImplementedException ();
			}
		}

		#endregion

		#region Methods
		
		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
		#endregion

		#region Destructors

		// FIXME: do the destructor
/*
		[MonoTODO]
		[ClassInterface(ClassInterfaceType.AutoDual)]
		~SqlError()
		{

		}
*/

		#endregion
		
	}
}
