// 
// System.Web.HttpCompileException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.CodeDom;

namespace System.Web {
	public sealed class HttpCompileException : HttpException {

		#region Properties

		public CompilerResults Results {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public string SourceCode {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties
	}
}
