// 
// System.Web.HttpParseException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web {
	public sealed class HttpParseException : HttpException {

		#region Fields
		
		string fileName;
		int line;

		#endregion // Fields

		#region Constructors

		[MonoTODO ("Figure out what to do with this.")]
		internal HttpParseException (string message, Exception innerException, string sourceCode, string fileName, int line)
			: base (message, innerException)
		{
			this.fileName = fileName;
			this.line = line;
		}

		#endregion // Constructors

		#region Properties

		public string FileName {
			get { return fileName; }
		}

		public int Line {
			get { return line; }
		}

		#endregion // Properties
	}
}
