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

		#region Properties

		public string FileName {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public int Line {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
	}
}
