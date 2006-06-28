using System;
using System.Web;
using System.Web.Hosting;
using System.IO;
using System.Collections;
using System.Reflection;

namespace MonoTests.SystemWeb.Framework
{
	public class BaseWorkerRequest : SimpleWorkerRequest, IForeignData
	{
		string _userAgent;
		public BaseWorkerRequest (string page, string query, TextWriter writer, string userAgent)
			: base (page, query, writer)
		{
			_userAgent = userAgent;
		}

		public override string GetKnownRequestHeader(int index) {
			switch (index) {
			case HttpWorkerRequest.HeaderUserAgent:
				return _userAgent;
			}
			return base.GetKnownRequestHeader (index);
		}

		Hashtable foreignData = new Hashtable ();
		object IForeignData.this [Type type] {
			get {return foreignData[type];}
			set {
				if (value == null)
					foreignData.Remove (type);
				else
					foreignData[type] = value;
			}
		}
	}
}
