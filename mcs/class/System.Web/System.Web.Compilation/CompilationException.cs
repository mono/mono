//
// System.Web.Compilation.CompilationException
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;

namespace System.Web.Compilation
{
	internal class CompilationException : Exception
	{
		CompilationResult result;

		public CompilationException (CompilationResult result)
		{
			this.result = result;
		}

		public CompilationException (string msg, CompilationResult result)
		{
			this.result = result;
		}

		public CompilationException (CompilationCacheItem item)
		{
			this.result = item.Result;
		}

		public override string Message {
			get {
				return result.ToString ();
			}
		}
	}
}

