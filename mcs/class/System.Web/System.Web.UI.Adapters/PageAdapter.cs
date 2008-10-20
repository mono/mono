//
// System.Web.UI.Adapters.PageAdapter
//
// Author:
//	Dick Porter  <dick@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Collections.Specialized;

namespace System.Web.UI.Adapters
{
	public abstract class PageAdapter : ControlAdapter
	{
		protected PageAdapter ()
		{
		}

		internal PageAdapter (Page p) : base (p)
		{
		}

		public virtual StringCollection CacheVaryByHeaders {
			get { return null; }
		}

		public virtual StringCollection CacheVaryByParams {
			get { return null; }
		}
		
		protected string ClientState 
		{
			get {
				return Page.GetSavedViewState ();
			}
		}
		
		public virtual NameValueCollection DeterminePostBackMode ()
		{
			return Page.DeterminePostBackMode ();
		}

		public virtual ICollection GetRadioButtonsByGroup (string groupName)
		{
			if (radio_button_group == null)
				return new ArrayList();
			ArrayList radioButtons = (ArrayList) radio_button_group [groupName];
			if (radioButtons == null)
				return new ArrayList();
			return radioButtons;
		}

		public virtual PageStatePersister GetStatePersister ()
		{
			return new HiddenFieldPageStatePersister ((Page)Control);
		}

		public virtual void RegisterRadioButton (RadioButton radioButton)
		{
			if (radio_button_group == null)
				radio_button_group = new ListDictionary();
			ArrayList radioButtons = (ArrayList) radio_button_group [radioButton.GroupName];
			if (radioButtons == null)
				radio_button_group [radioButton.GroupName] = radioButtons = new ArrayList();
			if (!radioButtons.Contains(radioButton))
				radioButtons.Add(radioButton);
		}

		public virtual void RenderBeginHyperlink (HtmlTextWriter w,
							  string targetUrl,
							  bool encodeUrl,
							  string softKeyLabel)
		{
			InternalRenderBeginHyperlink (w, targetUrl, encodeUrl, softKeyLabel, null);
		}

		public virtual void RenderBeginHyperlink (HtmlTextWriter w,
							  string targetUrl,
							  bool encodeUrl,
							  string softKeyLabel,
							  string accessKey)
		{
			if (accessKey != null && accessKey.Length > 1)
				throw new ArgumentOutOfRangeException("accessKey");
			InternalRenderBeginHyperlink (w, targetUrl, encodeUrl, softKeyLabel, accessKey);
		}
		
		void InternalRenderBeginHyperlink (HtmlTextWriter w,
						   string targetUrl,
						   bool encodeUrl,
						   string softKeyLabel,
						   string accessKey)
		{
			w.AddAttribute (HtmlTextWriterAttribute.Href, targetUrl, encodeUrl);
			if (accessKey != null)
				w.AddAttribute (HtmlTextWriterAttribute.Accesskey, accessKey);
			w.RenderBeginTag (HtmlTextWriterTag.A);
		}
		
		
		public virtual void RenderEndHyperlink (HtmlTextWriter w)
		{
			w.RenderEndTag();
		}

		public virtual void RenderPostBackEvent (HtmlTextWriter w,
							 string target,
							 string argument,
							 string softKeyLabel,
							 string text)
		{
			RenderPostBackEvent (w, target, argument, softKeyLabel, text, Page.Request.FilePath, null, true);
		}

		public virtual void RenderPostBackEvent (HtmlTextWriter w,
							 string target,
							 string argument,
							 string softKeyLabel,
							 string text,
							 string postUrl,
							 string accessKey)
		{
			RenderPostBackEvent (w, target, argument, softKeyLabel, text, postUrl, accessKey, true);
		}

		protected void RenderPostBackEvent (HtmlTextWriter w,
						    string target,
						    string argument,
						    string softKeyLabel,
						    string text,
						    string postUrl,
						    string accessKey,
						    bool encode)
		{
			string url = String.Format ("{0}?__VIEWSTATE={1}&__EVENTTARGET={2}&__EVENTARGUMENT={3}&__PREVIOUSPAGE={4}",
				postUrl, HttpUtility.UrlEncode (Page.GetSavedViewState ()), target, argument, Page.Request.FilePath);
			RenderBeginHyperlink (w, url, encode, softKeyLabel, accessKey);
			w.Write(text);
			RenderEndHyperlink(w);
		}

		public virtual string TransformText (string text) 
		{
			return text;
		}
		
		protected internal virtual string GetPostBackFormReference (string formID)
		{
			return String.Format("document.forms['{0}']", formID);
		}
		
		ListDictionary radio_button_group;
	}
}
#endif
