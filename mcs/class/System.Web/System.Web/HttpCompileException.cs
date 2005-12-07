// 
// System.Web.HttpCompileException.cs
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) Tim Coleman, 2002
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.CodeDom.Compiler;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Web {

	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#if NET_2_0
	[Serializable]
#endif
	public sealed class HttpCompileException : HttpException {

		CompilerResults results;
		string sourceCode;

#if NET_2_0
		public HttpCompileException ()
		{
		}

		public HttpCompileException (string message)
			: base (message)
		{
		}

		public HttpCompileException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		public HttpCompileException (CompilerResults results, string sourceCode)
#else
		internal HttpCompileException (CompilerResults results, string sourceCode)
#endif
		{
			this.results = results;
			this.sourceCode = sourceCode;
		}


		public CompilerResults Results {
			[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.High)]
			get { return results; }
		}

		public string SourceCode {
			[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.High)]
			get { return sourceCode; }
		}
#if NET_2_0
		public override string Message {
			get { return base.Message; }
		}

		[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			sourceCode = info.GetString ("sourcecode");
			results = (CompilerResults) info.GetValue ("results", typeof (CompilerResults));
		}
#endif
	}
}
