using System;
using System.Collections.Specialized;

namespace MonoTests.SystemWeb.Framework
{
	[Serializable]
	public class FormPostback:FormRequest
	{
		internal FormPostback (string url, NameValueCollection fields)
			:base (url)
		{
			this._fields = fields;
		}

		private NameValueCollection _fields;
		public NameValueCollection Fields
		{
			get { return Fields; }
		}

		public virtual string Url
		{
			get { return base.Url; }
			set { throw new Exception ("Must not change Url of FormPostback"); }
		}
	}
}
