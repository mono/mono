// 
// System.Web.HttpUnhandledException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web {
	public sealed class HttpUnhandledException : HttpException {

		#region Constructors

		internal HttpUnhandledException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		[MonoTODO ("What does this do?")]
		internal HttpUnhandledException (string message, string x, Exception innerException)
			: base (message, innerException)
		{
		}

		#endregion
	}
}
