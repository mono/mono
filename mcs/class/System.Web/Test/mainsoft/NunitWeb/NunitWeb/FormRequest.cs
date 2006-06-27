using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using System.IO;
using System.Collections;
using System.Xml;

namespace MonoTests.SystemWeb.Framework
{
	[Serializable]
	public class FormRequest : PostableRequest
	{
		private BaseControlCollection _controls;
		public BaseControlCollection Controls
		{
			get { return _controls; }
			set { _controls = value; }
		}

		public FormRequest (Response response, string formId)
		{
			_controls = new BaseControlCollection ();
			ExtractFormAndHiddenControls (response, formId);
		}

		private void ExtractFormAndHiddenControls (Response response, string formId)
		{
			HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument ();
			htmlDoc.LoadHtml (response.Body);

			StringBuilder tempxml = new StringBuilder ();
			StringWriter tsw = new StringWriter (tempxml);
			htmlDoc.OptionOutputAsXml = true;
			htmlDoc.Save (tsw);

			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (tempxml.ToString ());

			const string HTML_NAMESPACE = "http://www.w3.org/1999/xhtml";

			XmlNamespaceManager nsmgr = new XmlNamespaceManager (doc.NameTable);
			nsmgr.AddNamespace ("html", HTML_NAMESPACE);

#if USE_CORRECT_FORMID
			XmlNode formNode = doc.SelectSingleNode ("//html:form[@name='" + formId + "']", nsmgr);
#else
			XmlNode formNode = doc.SelectSingleNode ("//html:form", nsmgr);
#endif
			if (formNode == null)
				throw new ArgumentException ("Form with id='" + formId +
					"' was not found in document: " + response.Body);

			string actionUrl = formNode.Attributes["action"].Value;
			if (actionUrl != null && actionUrl != string.Empty)
				base.Url = actionUrl;
			XmlNode method = formNode.Attributes["method"];
			if (method != null && "POST" == method.Value)
				base.IsPost = true;
			else
				base.IsPost = false;
#if USE_CORRECT_FORMID

			foreach (XmlNode inputNode in formNode.SelectNodes ("//html:input", nsmgr))
#else
			foreach (XmlNode inputNode in doc.SelectNodes ("//html:input[@type='hidden']", nsmgr))
#endif
 {
				BaseControl bc = new BaseControl ();
				bc.Name = inputNode.Attributes["name"].Value;
				if (bc.Name == null || bc.Name == string.Empty)
					continue;
				if (inputNode.Attributes["value"] != null)
					bc.Value = inputNode.Attributes["value"].Value;
				else
					bc.Value = "";

				Controls[bc.Name] = bc;
			}
		}


		public override string Url
		{
			get { return base.Url; }
			set { throw new Exception ("Must not change Url of FormPostback"); }
		}

		public override bool IsPost
		{
			get { return base.IsPost; }
			set { throw new Exception ("Must not change IsPost of FormPostback"); }
		}

		public override string PostContentType
		{
			get { return "application/x-www-form-urlencoded"; }
			set { throw new Exception ("Must not change PostContentType of FormPostback"); }
		}

		public override byte[] EntityBody
		{
			get
			{
				if (IsPost)
					return Encoding.ASCII.GetBytes (GetUrlencodedDataset ());
				else
					return null;
			}
			set { throw new Exception ("Must not change EntityBody of FormPostback"); }
		}

		protected override string GetQueryString ()
		{
			if (IsPost)
				return "";
			else
				return GetUrlencodedDataset ();
		}

		private string GetUrlencodedDataset ()
		{
			StringBuilder query = new StringBuilder ();
			bool first = true;
 			foreach (string key in Controls.Keys) {
 				BaseControl ctrl = Controls[key];
				if (!ctrl.IsSuccessful ())
					continue;

				if (first)
					first = false;
				else
					query.Append ("&");

				query.Append (HttpUtility.UrlEncode (ctrl.Name));
				query.Append ("=");
				query.Append (HttpUtility.UrlEncode (ctrl.Value));
			}
			return query.ToString ();
		}
	}
}
