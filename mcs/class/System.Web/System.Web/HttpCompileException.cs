// 
// System.Web.HttpCompileException.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.CodeDom.Compiler;

namespace System.Web {
	public sealed class HttpCompileException : HttpException {

		#region Fields

		CompilerResults results;
		string sourceCode;

		#endregion // Fields

		#region Constructors

		internal HttpCompileException (CompilerResults results, string sourceCode)
			: base ()
		{
			this.results = results;
			this.sourceCode = sourceCode;
		}

		#endregion // Constructors

		#region Properties

		public CompilerResults Results {
			get { return results; }
		}

		public string SourceCode {
			get { return sourceCode; }
		}

		#endregion // Properties
	}
}
