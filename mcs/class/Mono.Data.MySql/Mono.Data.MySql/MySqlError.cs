//
// Mono.Data.MySql.MySqlError
//
// Author:
//    Daniel Morgan <danmorg@sc.rr.com>
//
// (c)copyright 2002 Daniel Morgan
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace Mono.Data.MySql
{
	public class MySqlError
	{
		private string message;
		private string source;
		private string sqlstate;
		private int nativeerror;

		#region Constructors

		internal MySqlError(string Source)
		{
			nativeerror = 1;
			source = Source;
			message = "Error in " + source;
			sqlstate = "";
		}

		#endregion // Constructors
		
		#region Properties

		public string Message
		{
			get
			{
				return message;
			}
		}

		public int NativeError
		{
			get
			{
				return nativeerror;
			}
		}

		public string Source
		{
			get
			{
				return source;
			}
		}

		public string SQLState
		{
			get
			{
				return sqlstate;
			}
		}

		#endregion // Properties

	}
}