//
// System.Exception.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Serialization;
using System.Reflection;

namespace System {

	[Serializable]
//	[ClassInterface (ClassInterfaceType.AutoDual)] (no implementation yet)
	[MonoTODO]
	public class Exception : ISerializable {
		Exception inner_exception;
		string message;
		string help_link;
		string class_name;
		string stack_trace = "TODO: implement stack traces";
		int hresult;
		private string source;
		
		public Exception ()
		{
			inner_exception = null;
			message = "";
		}

		public Exception (string msg)
		{
			inner_exception = null;
			message = msg;
		}

		protected Exception (SerializationInfo info, StreamingContext sc)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			class_name      = info.GetString ("ClassName");
			message         = info.GetString ("Message");
			inner_exception = (Exception) info.GetValue  ("InnerException", typeof (Exception));
			help_link       = info.GetString ("HelpURL");
			stack_trace     = info.GetString ("StackTraceString");
			hresult         = info.GetInt32  ("HResult");
		}

		public Exception (string msg, Exception e)
		{
			inner_exception = e;
			message = msg;
		}

		public Exception InnerException {
			get {
				return inner_exception;
			}
		}

		public virtual string HelpLink {
			get {
				return help_link;
			}

			set {
				help_link = value;
			}
		}

		protected int HResult {
			get {
				return hresult;
			}

			set {
				hresult = value;
			}
		}

		public virtual string Message {
			get {
				return message;
			}
		}

		[MonoTODO]
		public virtual string Source {
			get {
				// TODO: if source is null, we must return
				// the name of the assembly where the error
				// originated.
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

		[MonoTODO]
		public MethodBase TargetSite {
			get {
				// TODO: Implement this.
				return null;
			}
		}

		public virtual Exception GetBaseException ()
		{
			Exception inner = inner_exception;
				
			while (inner != null){
				if (inner.InnerException != null)
					inner = inner.InnerException;
				else
					return inner;
			}

			return this;
		}

		[MonoTODO]
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			// TODO: implement me.
		}

		public override string ToString ()
		{
			return this.GetType ().FullName + "\n" +
				message +
				GetBaseException ().GetType ().FullName +
				stack_trace;
		}
	}
}

