//
// System.Data.Odbc.OdbcException
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;

namespace System.Data.Odbc
{
	[MonoTODO("Override ToString, Message...")]
	public class OdbcException : Exception
	{
		OdbcError error;

		internal OdbcException (OdbcError error)
		{
			this.error = error;
		}
	}
}

