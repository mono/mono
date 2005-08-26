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

		[MonoTODO]
		public virtual StringCollection CacheVaryByHeaders 
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual StringCollection CacheVaryByParams 
		{
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		protected string ClientState 
		{
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public virtual NameValueCollection DeterminePostBackMode ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual ICollection GetRadioButtonsByGroup (string groupName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual PageStatePersister GetStatePersister ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RegisterRadioButton (RadioButton radioButton)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RenderBeginHyperlink (HtmlTextWriter w,
							  string targetUrl,
							  bool encodeUrl,
							  string softKeyLabel)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RenderBeginHyperlink (HtmlTextWriter w,
							  string targetUrl,
							  bool encodeUrl,
							  string softKeyLabel,
							  string accessKey)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual void RenderEndHyperlink (HtmlTextWriter w)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RenderPostBackEvent (HtmlTextWriter w,
							 string target,
							 string argument,
							 string softKeyLabel,
							 string text)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RenderPostBackEvent (HtmlTextWriter w,
							 string target,
							 string argument,
							 string softKeyLabel,
							 string text,
							 string postUrl,
							 string accessKey)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void RenderPostBackEvent (HtmlTextWriter w,
						    string target,
						    string argument,
						    string softKeyLabel,
						    string text,
						    string postUrl,
						    string accessKey,
						    bool encode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string TransformText (string text) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal virtual string GetPostBackFormReference (string formID)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
