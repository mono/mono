//
// System.Exception.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Patrik Torstensson
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Reflection;
using System.Diagnostics;

namespace System
{
	[Serializable]
	[ClassInterface (ClassInterfaceType.AutoDual)]
	public class Exception : ISerializable 
	{
		IntPtr [] trace_ips;
		Exception inner_exception;
		string message;
		string help_link;
		string class_name;
		string stack_trace = null;
		string remote_stack_trace = "";
		int remote_stack_index = 0;
		int hresult = unchecked ((int)0x80004005);
		string source;

		public Exception ()
		{
			inner_exception = null;
			message = null;
			class_name = GetType().FullName;
		}

		public Exception (string msg)
		{
			inner_exception = null;
			message = msg;
			class_name = GetType().FullName;
		}

		protected Exception (SerializationInfo info, StreamingContext sc)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			class_name          = info.GetString ("ClassName");
			message             = info.GetString ("Message");
			help_link           = info.GetString ("HelpURL");
			stack_trace         = info.GetString ("StackTraceString");
			remote_stack_trace  = info.GetString ("RemoteStackTraceString");
			remote_stack_index  = info.GetInt32  ("RemoteStackIndex");
			hresult             = info.GetInt32  ("HResult");
			source              = info.GetString ("Source");
			inner_exception     = (Exception) info.GetValue ("InnerException", typeof (Exception));
		}

		public Exception (string msg, Exception e)
		{
			inner_exception = e;
			message = msg;
			class_name = GetType().FullName;
		}

		public Exception InnerException {
			get { return inner_exception; }
		}

		public virtual string HelpLink {
			get { return help_link; }
			set { help_link = value; }
		}

		protected int HResult {
			get { return hresult; }
			set { hresult = value; }
		}

		internal void SetMessage (string s)
		{
			message = s;
		}

		internal void SetStackTrace (string s)
		{
			stack_trace = s;
		}

		public virtual string Message {
			get {
				if (message == null)
					message = string.Format (Locale.GetText ("Exception of type {0} was thrown."), GetType ().ToString());

				return message;
			}
		}

		public virtual string Source {
			get {
				if (source == null) {
					StackTrace st = new StackTrace (this, true);
					if (st.FrameCount > 0) {
						StackFrame sf = st.GetFrame (0);
						if (st != null) {
							MethodBase method = sf.GetMethod ();
							if (method != null) {
								source = method.DeclaringType.Assembly.GetName ().Name;
							}
						}
					}
				}

                                // source can be null
				return source;
			}

			set {
				source = value;
			}
		}

		public virtual string StackTrace {
			get {
				return stack_trace;
			}
		}

		public MethodBase TargetSite {
			get {
				StackTrace st = new StackTrace (this, true);
				if (st.FrameCount > 0)
					return st.GetFrame (0).GetMethod ();
				
				return null;
			}
		}

		public virtual Exception GetBaseException ()
		{
			Exception inner = inner_exception;
				
			while (inner != null)
			{
				if (inner.InnerException != null)
					inner = inner.InnerException;
				else
					return inner;
			}

			return this;
		}

		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("ClassName", class_name);
			info.AddValue ("Message", message);
			info.AddValue ("InnerException", inner_exception);
			info.AddValue ("HelpURL", help_link);
			info.AddValue ("StackTraceString", stack_trace);
			info.AddValue ("RemoteStackTraceString", remote_stack_trace);
			info.AddValue ("RemoteStackIndex", remote_stack_index);
			info.AddValue ("HResult", hresult);
			info.AddValue ("Source", Source);
			info.AddValue ("ExceptionMethod", null);
		}

		public override string ToString ()
		{
			System.Text.StringBuilder result = new System.Text.StringBuilder (this.GetType ().FullName);
			result.Append (": ").Append (Message);

			if (null != remote_stack_trace)
				result.Append (remote_stack_trace);
				
			if (inner_exception != null) 
			{
				result.Append (" ---> ").Append (inner_exception.ToString ());
				result.Append (Locale.GetText ("--- End of inner exception stack trace ---"));
				result.Append (Environment.NewLine);
			}

			if (stack_trace != null)
				result.Append (Environment.NewLine).Append (stack_trace);
			return result.ToString();
		}

		internal Exception FixRemotingException ()
		{
			string message = (0 == remote_stack_index) ?
				Locale.GetText ("{0}{0}Server stack trace: {0}{1}{0}{0}Exception rethrown at [{2}]: {0}") :
				Locale.GetText ("{1}{0}{0}Exception rethrown at [{2}]: {0}");
			string tmp = String.Format (message, Environment.NewLine, StackTrace, remote_stack_index);

			remote_stack_trace = tmp;
			remote_stack_index++;

			stack_trace = null;

			return this;
		}
	}
}
