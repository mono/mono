//
// System.Exception.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Patrik Torstensson
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System
{
	[Serializable]
	[ComVisible(true)]
	[ComDefaultInterface (typeof (_Exception))]
	[ClassInterface (ClassInterfaceType.None)]
	[StructLayout (LayoutKind.Sequential)]
#if MOBILE
	public class Exception : ISerializable
#else
	public class Exception : ISerializable, _Exception
#endif
	{
#pragma warning disable 169, 649
		#region Sync with object-internals.h
		/* Stores the IPs and the generic sharing infos
		   (vtable/MRGCTX) of the frames. */
		IntPtr [] trace_ips;
		Exception inner_exception;
		internal string _message;
		string help_link;
		string class_name;
		string stack_trace;
		// formerly known as remote_stack_trace (see #425512):
		string _remoteStackTraceString;
		int remote_stack_index;
		internal int hresult = -2146233088;
		string source;
		IDictionary _data;
		internal StackTrace[] captured_traces;
		IntPtr[] native_trace_ips;
		#endregion
#pragma warning restore 169, 649

		/* Don't add fields here, the runtime depends on the layout of subclasses */

		public Exception ()
		{
		}

		public Exception (string message)
		{
			this._message = message;
		}

		protected Exception (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			class_name          = info.GetString ("ClassName");
			_message             = info.GetString ("Message");
			help_link           = info.GetString ("HelpURL");
			stack_trace         = info.GetString ("StackTraceString");
			_remoteStackTraceString  = info.GetString ("RemoteStackTraceString");
			remote_stack_index  = info.GetInt32  ("RemoteStackIndex");
			hresult             = info.GetInt32  ("HResult");
			source              = info.GetString ("Source");
			inner_exception     = (Exception) info.GetValue ("InnerException", typeof (Exception));

			try {
				_data = (IDictionary) info.GetValue ("Data", typeof (IDictionary));
			} catch (SerializationException) {
				// member did not exist in .NET 1.x
			}
		}

		public Exception (string message, Exception innerException)
		{
			inner_exception = innerException;
			this._message = message;
		}

		public Exception InnerException {
			get { return inner_exception; }
		}

		public virtual string HelpLink {
			get { return help_link; }
			set { help_link = value; }
		}

		public int HResult {
			get { return hresult; }
			protected set { hresult = value; }
		}
		
		internal void SetErrorCode(int hr)
		{
			HResult = hr;
		}

		internal void SetMessage (string s)
		{
			_message = s;
		}

		internal void SetStackTrace (string s)
		{
			stack_trace = s;
		}

		string ClassName {
			get {
				if (class_name == null)
					class_name = GetType ().ToString ();
				return class_name;
			}
		}

		public virtual string Message {
			get {
				if (_message == null)
					_message = string.Format (Locale.GetText ("Exception of type '{0}' was thrown."),
						ClassName);

				return _message;
			}
		}
		
		[MonoTODO]
		protected event EventHandler<SafeSerializationEventArgs> SerializeObjectState {
			add {
			}
			remove {
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
								source = method.DeclaringType.Assembly.UnprotectedGetName ().Name;
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
				if (stack_trace != null)
					return stack_trace;

				if (trace_ips == null)
					/* Not thrown yet */
					return null;

				StringBuilder sb = new StringBuilder ();

				// Add traces captured using ExceptionDispatchInfo
				if (captured_traces != null) {
					foreach (var t in captured_traces) {
						if (!t.AddFrames (sb, true))
							continue;

						sb.Append (Environment.NewLine);
						sb.Append ("--- End of stack trace from previous location where exception was thrown ---");
						sb.Append (Environment.NewLine);
					}
				}

				StackTrace st = new StackTrace (this, 0, true, true);
				st.AddFrames (sb, true);

				stack_trace = sb.ToString ();

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

		public virtual IDictionary Data {
			get {
				if (_data == null) {
					// default to empty dictionary
					_data = new Dictionary<object, object> ();
				}
				return _data;
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

		[SecurityPermission (SecurityAction.LinkDemand, SerializationFormatter = true)]
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			info.AddValue ("ClassName", ClassName);
			info.AddValue ("Message", _message);
			info.AddValue ("InnerException", inner_exception);
			info.AddValue ("HelpURL", help_link);
			info.AddValue ("StackTraceString", StackTrace);
			info.AddValue ("RemoteStackTraceString", _remoteStackTraceString);
			info.AddValue ("RemoteStackIndex", remote_stack_index);
			info.AddValue ("HResult", hresult);
			info.AddValue ("Source", Source);
			info.AddValue ("ExceptionMethod", null);
			info.AddValue ("Data", _data, typeof (IDictionary));
		}

		public override string ToString ()
		{
			System.Text.StringBuilder result = new System.Text.StringBuilder (ClassName);
			result.Append (": ").Append (Message);

			if (null != _remoteStackTraceString)
				result.Append (_remoteStackTraceString);
				
			if (inner_exception != null) 
			{
				result.Append (" ---> ").Append (inner_exception.ToString ());
				result.Append (Environment.NewLine);
				result.Append (Locale.GetText ("  --- End of inner exception stack trace ---"));
			}

			if (StackTrace != null)
				result.Append (Environment.NewLine).Append (StackTrace);
			return result.ToString();
		}

		internal Exception FixRemotingException ()
		{
			string message = (0 == remote_stack_index) ?
				Locale.GetText ("{0}{0}Server stack trace: {0}{1}{0}{0}Exception rethrown at [{2}]: {0}") :
				Locale.GetText ("{1}{0}{0}Exception rethrown at [{2}]: {0}");
			string tmp = String.Format (message, Environment.NewLine, StackTrace, remote_stack_index);

			_remoteStackTraceString = tmp;
			remote_stack_index++;

			stack_trace = null;

			return this;
		}

		// For ExceptionDispatchInfo
		internal void RestoreExceptionDispatchInfo (System.Runtime.ExceptionServices.ExceptionDispatchInfo exceptionDispatchInfo)
		{
			captured_traces = (StackTrace[]) exceptionDispatchInfo.BinaryStackTraceArray;
			trace_ips = null;
		}

		//
		// The documentation states that this is available in 1.x,
		// but it was not available (MemberRefing this would fail)
		// and it states the signature is `override sealed', but the
		// correct value is `newslot' 
		//
		public new Type GetType ()
		{
			return base.GetType ();
		}

		internal enum ExceptionMessageKind
		{
			ThreadAbort = 1,
			ThreadInterrupted = 2,
			OutOfMemory = 3
		}

		internal static String GetMessageFromNativeResources (ExceptionMessageKind kind)
		{
			switch (kind) {
			case ExceptionMessageKind.ThreadAbort:
				return "";
			case ExceptionMessageKind.ThreadInterrupted:
				return "";
			case ExceptionMessageKind.OutOfMemory:
				return "Out of memory";
			}
			return "";
		}
	}
}
