//
// Microsoft.Web.Services.Routing.RoutingFault.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using System.Web.Services.Protocols;

namespace Microsoft.Web.Services.Routing
{
	public class RoutingFault : Exception
	{
		private string _code;
		private Exception _detailedException = null;
		private string _endpoint;
		private Found _found = null;
		private int _maxsize = 0;
		private int _maxtime = 0;
		private string _reason;
		private int _retryAfter = 0;

		public RoutingFault () : base ()
		{
		}

		public RoutingFault (XmlElement element) : base ()
		{
			LoadXml (element);
		}

		public RoutingFault (string code, string reason, int maxtime, Exception ex) : base (reason)
		{
			_code = code;
			_reason = reason;
			_maxtime = maxtime;
			_detailedException = ex;
		}

		public RoutingFault (string code, string reason, string endpoint, Exception ex) : base (reason)
		{
			_code = code;
			_reason = reason;
			_endpoint = endpoint;
			_detailedException = ex;
		}

		public RoutingFault (string code, string reason, Exception ex) : base (reason)
		{
			_code = code;
			_reason = reason;
			_detailedException = ex;
		}

		public SoapHeaderException GetSoapHeaderException ()
		{
			return new SoapHeaderException (Reason, SoapException.ClientFaultCode, DetailException);
		}

		[MonoTODO]
		public XmlElement GetXml (XmlDocument doc)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void LoadXml (XmlElement element)
		{
			throw new NotImplementedException ();
		}

		public string Code {
			get { return _code; }
			set { _code = value; }
		}

		public Exception DetailException {
			get { return _detailedException; }
			set { _detailedException = value; }
		}

		public string Endpoint {
			get { return _endpoint; }
			set { _endpoint = value; }
		}

		public Found Found {
			get { return _found; }
			set { _found = value; }
		}

		public int MaxSize {
			get { return _maxsize; }
			set { _maxsize = value; }
		}

		public int MaxTime {
			get { return _maxtime; }
			set { _maxtime = value; }
		}

		public string Reason {
			get { return _reason; }
			set { _reason = value; }
		}

		public int RetryAfter {
			get { return _retryAfter; }
			set { _retryAfter = value; }
		}
	}
}
