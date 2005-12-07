//
// System.Web.Util.WebTrace
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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

using System.Collections;
using System.Diagnostics;

namespace System.Web.Util
{
	internal class WebTrace
	{
		static Stack ctxStack;
		static bool trace;

		static WebTrace ()
		{
			ctxStack = new Stack ();
		}

		[Conditional("WEBTRACE")]
		static public void PushContext (string context)
		{
			ctxStack.Push (context);
			Trace.Indent ();
		}
		
		[Conditional("WEBTRACE")]
		static public void PopContext ()
		{
			if (ctxStack.Count == 0)
				return;

			Trace.Unindent ();
			ctxStack.Pop ();
		}

		static public string Context
		{
			get {
				if (ctxStack.Count == 0)
					return "No context";

				return (string) ctxStack.Peek ();
			}
		}

		static public bool StackTrace
		{
			get { return trace; }

			set { trace = value; }
		}
		
		[Conditional("WEBTRACE")]
		static public void WriteLine (string msg)
		{
			Trace.WriteLine (Format (msg));
		}

		[Conditional("WEBTRACE")]
		static public void WriteLine (string msg, object arg)
		{
			Trace.WriteLine (Format (String.Format (msg, arg)));
		}

		[Conditional("WEBTRACE")]
		static public void WriteLine (string msg, object arg1, object arg2)
		{
			Trace.WriteLine (Format (String.Format (msg, arg1, arg2)));
		}

		[Conditional("WEBTRACE")]
		static public void WriteLine (string msg, object arg1, object arg2, object arg3)
		{
			Trace.WriteLine (Format (String.Format (msg, arg1, arg2, arg3)));
		}

		[Conditional("WEBTRACE")]
		static public void WriteLine (string msg, params object [] args)
		{
			Trace.WriteLine (Format (String.Format (msg, args)));
		}

		static string Format (string msg)
		{
			if (trace)
				return String.Format ("{0}: {1}\n{2}", Context, msg, Environment.StackTrace);
			else
				return String.Format ("{0}: {1}", Context, msg);
		}
	}
}

