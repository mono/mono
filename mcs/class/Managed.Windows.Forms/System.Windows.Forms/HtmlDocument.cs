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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Andreia Gaita	<avidigal@novell.com>

#if NET_2_0

using System;
using System.Drawing;

namespace System.Windows.Forms
{
	[MonoTODO ("Needs Implementation")]
	public sealed class HtmlDocument
	{
		Mono.WebBrowser.IWebBrowser webHost;
		internal HtmlDocument (Mono.WebBrowser.IWebBrowser webHost)
		{
			this.webHost = webHost;
		}

		#region Methods


		public void AttachEventHandler (string eventName, EventHandler eventHandler)
		{ }

		public HtmlElement CreateElement (string elementTag) 
		{ 
			throw new NotImplementedException ();
		}

		public void DetachEventHandler (string eventName, EventHandler eventHandler) 
		{
			throw new NotImplementedException ();
		}


		public void ExecCommand (string command, bool showUI, Object value) 
		{
			throw new NotImplementedException ();
		}

		public void Focus () 
		{
			throw new NotImplementedException ();
		}

		public HtmlElement GetElementById (string id)
		{
			return new HtmlElement (webHost.Document.GetElementById (id));
		}

		public HtmlElement GetElementFromPoint (Point point) 
		{
			throw new NotImplementedException ();
		}

		public HtmlElementCollection GetElementsByTagName (string tagName) 
		{
			throw new NotImplementedException ();
		}

		public override int GetHashCode () 
		{ 
			return base.GetHashCode (); 
		}

		public Object InvokeScript (string scriptName)
		{ 
			throw new NotImplementedException (); 
		}

		public Object InvokeScript (string scriptName, Object[] args) 
		{
			throw new NotImplementedException ();
		}

		public HtmlDocument OpenNew (bool replaceInHistory) 
		{
			throw new NotImplementedException ();
		}

		public void Write (string text) 
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Properties
		[MonoTODO ("Needs Implementation")]

		public HtmlElement ActiveElement
		{
			get { throw new NotImplementedException (); }
		}
		public Color ActiveLinkColor {
			get { throw new NotImplementedException ();}
			set { throw new NotImplementedException (); } 
		}

		public HtmlElementCollection All
		{
			get { throw new NotImplementedException (); }
		}

		public Color BackColor
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		public HtmlElement Body {
			get {
				return new HtmlElement (webHost.Document.Body);
			}
			set { throw new NotImplementedException (); }
		}
		public string Cookie
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		public string DefaultEncoding { get { throw new NotImplementedException (); } }
		public string Domain
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		public Object DomDocument { get { throw new NotImplementedException (); } }
		public string Encoding
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		public bool Focused { get { throw new NotImplementedException (); } }
		public Color ForeColor
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		public HtmlElementCollection Forms { get { throw new NotImplementedException (); } }
		public HtmlElementCollection Images { get { throw new NotImplementedException (); } }
		public Color LinkColor
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		public HtmlElementCollection Links { get { throw new NotImplementedException (); } }
		public bool RightToLeft
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		public string Title
		{
			get { return webHost.Document.Title; }
			set { webHost.Document.Title = value; }
		}
		public Uri Url { get { throw new NotImplementedException (); } }
		public Color VisitedLinkColor
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		public HtmlWindow Window { get { throw new NotImplementedException (); } }


		#endregion
	}
}

#endif