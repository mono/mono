using System;
using System.Web;
using System.Web.Hosting;

namespace NunitWeb
{
	public class MyWorkerRequest: SimpleWorkerRequest
	{
		Delegate _method;
		object _param;
		Exception _exception;
		bool _delegateInvoked;

		public Delegate Method
		{ get { return _method; } }

		public object Param
		{ get { return _param; } }

		public Exception Exception
		{
			get { return _exception; }
			set { _exception = value; }
		}

		public bool DelegateInvoked
		{
			get { return _delegateInvoked; }
			set { _delegateInvoked = value; }
		}

		public MyWorkerRequest (Delegate method, object param, string page, string query, System.IO.TextWriter output)
			: base (page, query, output)
		{
			_method = method;
			_param = param;
			_delegateInvoked = false;
			_exception = null;
		}
	}
}
