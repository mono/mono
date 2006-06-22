using System;
using System.Xml;
using System.Collections.Specialized;
using System.Web;
using System.IO;
using System.Text;

namespace MonoTests.SystemWeb.Framework
{
	[Serializable]
	public class FormRequest: BaseRequest
	{
		public FormRequest ()
		{
		}

		public FormRequest (string url)
			:base (url)
		{
		}

		[NonSerialized]
		string lastResult;

		public virtual string GetRequestResult (HttpWorkerRequest request)
		{
			lastResult = base.GetRequestResult (request);
			return lastResult;
		}

		public FormPostback CreateNext (string formId)
		{
			HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument ();
			htmlDoc.LoadHtml (lastResult);

			StringBuilder tempxml = new StringBuilder (); 
			StringWriter tsw = new StringWriter (tempxml);
			htmlDoc.OptionOutputAsXml = true;
			htmlDoc.Save (tsw);

			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (tempxml.ToString ());

			XmlNode formNode = doc.SelectSingleNode ("form[@id=" + formId + "]");
			if (formNode == null)
				throw new ArgumentException ("Form with id='" + formId +
					"' was not found in document: " + lastResult);

			string targetUrl = formNode.Attributes ["action"].Value;
			if (targetUrl == null)
				targetUrl = this.Url;

			NameValueCollection fields = new NameValueCollection ();
			foreach (XmlNode inputNode in formNode.SelectNodes ("input"))
				fields.Add (inputNode.Attributes["name"].Value,
					inputNode.Attributes["value"].Value);

			return new FormPostback (targetUrl, fields);
		}
	}
}
