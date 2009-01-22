using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using System.IO;
using System.Collections;
using System.Xml;
using System.Globalization;

namespace MonoTests.SystemWeb.Framework
{
	/// <summary>
	/// This class is used for HTML form postback request.
	/// </summary>
	[Serializable]
	public class FormRequest : PostableRequest
	{
		/// <summary>
		/// Create <see cref="FormRequest"/> instance from the given
		/// <paramref name="response">response</paramref> extracting
		/// form attributes and hidden controls from the form element
		/// with given id.
		/// </summary>
		/// <param name="response">The response to extract values from.</param>
		/// <param name="formId">The id of the form to use.</param>
		/// <remarks>Currently, the <paramref name="formId"/> is ignored, and the
		/// first form is used.</remarks>
		public FormRequest (Response response, string formId)
		{
			_controls = new BaseControlCollection ();
			ExtractFormAndHiddenControls (response, formId);
		}

		private BaseControlCollection _controls;
		/// <summary>
		/// Get or set the collection of controls, posted back to the server.
		/// </summary>
		public BaseControlCollection Controls
		{
			get { return _controls; }
			set { _controls = value; }
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
			if (method != null && "POST" == method.Value.ToUpper(CultureInfo.InvariantCulture))
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

		/// <summary>
		/// Get the URL extracted from the form. Unlike the base class, here this
		/// property should not be changed, otherwise an <see cref="Exception"/>
		/// is thrown.
		/// </summary>
		
		/// Get returns true if the form method was POST, otherwise returns false.
		/// Unlike the base class, here this property should not be
		/// changed, otherwise an <see cref="Exception"/> is thrown.
		/// </summary>
		/// <exception cref="Exception">Thrown when trying to change this property.</exception>
		public override bool IsPost
		{
			get { return base.IsPost; }
			set { throw new Exception ("Must not change IsPost of FormPostback"); }
		}

		/// <summary>
		/// Returns the HTTP content-type header value. Currently hard-coded to return
		/// <c>application/x-www-form-urlencoded</c>.
		/// </summary>
		public override string ContentType
		{
			get { return "application/x-www-form-urlencoded"; }
			set { throw new Exception ("Must not change PostContentType of FormPostback"); }
		}

		/// <summary>
		/// Returns the HTTP <c>entity-body</c> header value.
		/// </summary>
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

		/// <summary>
		/// Get the URL encoded query string in format <c><![CDATA[name1=value1&name2=value2]]></c>, etc.
		/// </summary>
		protected override string QueryString
		{
			get
			{
				if (IsPost)
					return "";
				else
					return GetUrlencodedDataset ();
			}
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
