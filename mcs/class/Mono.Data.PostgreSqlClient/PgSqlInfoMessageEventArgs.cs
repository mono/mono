//
// Mono.Data.PostgreSqlClient.PgSqlInfoMessageEventArgs.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;
using System.Data;

namespace Mono.Data.PostgreSqlClient
{
	public sealed class PgSqlInfoMessageEventArgs : EventArgs
	{
		[MonoTODO]
		public PgSqlErrorCollection Errors {
			get { 
				throw new NotImplementedException (); 
			}
		}	

		[MonoTODO]
		public string Message 
		{
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
		public override string ToString() {
			// representation of InfoMessage event
			return "'ToString() for SqlInfoMessageEventArgs Not Implemented'";
		}

		//[MonoTODO]
		//~PgSqlInfoMessageEventArgs() {
			// FIXME: destructor needs to release resources
		//}
	}
}
