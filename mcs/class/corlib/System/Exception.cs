//
// System.Exception.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Patrik Torstensson
//
// (C) Ximian, Inc.  http://www.ximian.com
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

		[MonoTODO ("Locale.GetText()")]
		public virtual string Message {
			get {
				if (message == null)
					message = "Exception of type " + GetType () + " was thrown.";

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
					if (source == null)
						source = "";
				}
				
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

		[MonoTODO ("Locale.GetText(), use Stringbuilder?")]
		public override string ToString ()
		{
			string result = this.GetType ().FullName + ": " + Message;

			if (null != remote_stack_trace)
				result = result + remote_stack_trace;
				
			if (inner_exception != null) 
			{
				result += " ---> " + inner_exception.ToString ();
				result += "--- End of inner exception stack trace ---";
				result += Environment.NewLine;
			}

			if (stack_trace != null)
				result += Environment.NewLine + stack_trace;
			return result;
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
