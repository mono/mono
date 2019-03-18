using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace System
{
    [Serializable]
    [StructLayout (LayoutKind.Sequential)]
	public class Exception : ISerializable
	{
		# region Keep in sync with MonoException in object-internals.h
        string _className;
        internal string _message;
        IDictionary _data;
        Exception _innerException;
        String _helpURL;
        Object _stackTrace;
		// Unused
        String _stackTraceString;
		// Unused
        String _remoteStackTraceString;
        int _remoteStackIndex;
        Object _dynamicMethods;
        int _HResult;
        String _source;
		// Unused
        Object _safeSerializationManager;
        StackTrace[] captured_traces;
        IntPtr[] native_trace_ips;
        int caught_in_unmanaged;
		#endregion

		protected event EventHandler<SafeSerializationEventArgs> SerializeObjectState;

        public Exception () {
        }

        public Exception (string message) : base () {
            _message = message;
        }

        public Exception (string message, Exception innerException) : base () {
            _message = message;
            _innerException = innerException;
        }

        protected Exception (SerializationInfo info, StreamingContext context) {
			throw new NotImplementedException ();
		}

		public virtual String Message {
               get {
                if (_message == null) {
                    if (_className == null)
                        _className = GetClassName ();
                    return Environment.GetResourceString ("Exception_WasThrown", _className);
                } else {
                    return _message;
                }
            }
        }

        public virtual IDictionary Data {
            get {
                if (_data == null)
					_data = new ListDictionaryInternal ();
                return _data;
            }
        }

        string GetClassName () {
            if (_className == null)
                _className = GetType ().ToString ();

            return _className;
        }

        public virtual Exception GetBaseException () {
            Exception inner = InnerException;
            Exception back = this;

            while (inner != null) {
                back = inner;
                inner = inner.InnerException;
            }

            return back;
        }

        public Exception InnerException {
            get {
				return _innerException;
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

        public virtual String StackTrace
        {
            get {
                return GetStackTrace (true);
            }
        }

        string GetStackTrace (bool needFileInfo) {
            string stackTraceString = _stackTraceString;

            if (stackTraceString != null)
                return stackTraceString;
            if (_stackTrace == null)
                return null;

            return Environment.GetStackTrace (this, needFileInfo);
         }

        public virtual String HelpLink {
            get {
                return _helpURL;
            }
            set {
                _helpURL = value;
            }
        }

        public virtual String Source {
            get {
                if (_source == null) {
                    StackTrace st = new StackTrace (this, true);
                    if (st.FrameCount > 0) {
                        StackFrame sf = st.GetFrame (0);
                        MethodBase method = sf.GetMethod ();

                        if (method != null) // source can be null
                            _source = method.DeclaringType.Assembly.GetName ().Name;
                    }
                }

                return _source;
            }
            set {
				_source = value;
			}
        }

        public override string ToString () {
            return ToString (true, true);
        }

        string ToString (bool needFileLineInfo, bool needMessage) {
			string message = (needMessage ? Message : null);
            string s;

            if (String.IsNullOrEmpty (message))
                s = GetClassName ();
            else
                s = GetClassName () + ": " + message;

            if (_innerException != null) {
                s = s + " ---> " + _innerException.ToString (needFileLineInfo, needMessage) + Environment.NewLine +
                "   " + Environment.GetResourceString("Exception_EndOfInnerExceptionStack");
            }

            string stackTrace = GetStackTrace (needFileLineInfo);
            if (stackTrace != null)
                s += Environment.NewLine + stackTrace;
            return s;
        }

        public virtual void GetObjectData (SerializationInfo info, StreamingContext context) {
			throw new NotImplementedException ();
		}

        [OnDeserialized]
        void OnDeserialized (StreamingContext context) {
            _stackTrace = null;
		}

        public int HResult {
            get {
                return _HResult;
            }
			set {
                _HResult = value;
            }
        }

		internal void SetErrorCode (int hr) {
			HResult = hr;
		}

		internal readonly struct DispatchState
		{
		}

		internal DispatchState CaptureDispatchState ()
		{
			throw new NotImplementedException ();
		}

		internal void RestoreDispatchState (DispatchState state)
		{
		}
	}
}
