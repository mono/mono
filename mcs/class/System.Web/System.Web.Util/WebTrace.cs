//
// System.Web.Util.WebTrace
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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

