//
// System.Web.HtmlizedException
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

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

using System;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Web
{
	[Serializable]
	internal abstract class HtmlizedException : HttpException
	{
		protected HtmlizedException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
		
		protected HtmlizedException ()
		{
		}

		protected HtmlizedException (string message)
			: base (message)
		{
		}

		protected HtmlizedException (string message, Exception inner)
			: base (message, inner)
		{
		}

		public abstract string Title { get; }
		public new abstract string Description { get; }
		public abstract string ErrorMessage { get; }
		public abstract string FileName { get; }
		public abstract string SourceFile { get; }
		public abstract string FileText { get; }
		public abstract int [] ErrorLines { get; }
		public abstract bool ErrorLinesPaired { get; }
	}
}

