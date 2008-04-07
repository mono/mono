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
// Copyright (c) 2007, 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita	<avidigal@novell.com>

#if NET_2_0

using System;
using System.Drawing;
using System.ComponentModel;
using System.Globalization;
using Mono.WebBrowser.DOM;

namespace System.Windows.Forms
{
	[MonoTODO ("Needs Implementation")]
	public sealed class HtmlDocument
	{
		Mono.WebBrowser.IWebBrowser webHost;
		IDocument document;

		internal HtmlDocument (Mono.WebBrowser.IWebBrowser webHost) : this (webHost, webHost.Document)
		{
		}

		internal HtmlDocument (Mono.WebBrowser.IWebBrowser webHost, IDocument doc)
		{
			this.webHost = webHost;
			this.document = doc;
		}

		#region Methods


		public void AttachEventHandler (string eventName, EventHandler eventHandler)
		{ 
			throw new NotImplementedException ();
		}

		public HtmlElement CreateElement (string elementTag) 
		{ 
			Mono.WebBrowser.DOM.IElement element = document.CreateElement (elementTag);
			return new HtmlElement (webHost, element);
		}

		public void DetachEventHandler (string eventName, EventHandler eventHandler) 
		{
			throw new NotImplementedException ();
		}

		public override bool Equals (object obj) {
			return this == (HtmlDocument) obj;
		}

		public void ExecCommand (string command, bool showUI, Object value) 
		{
			throw new NotImplementedException ();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public void Focus () 
		{
			webHost.FocusIn (Mono.WebBrowser.FocusOption.None);
		}

		public HtmlElement GetElementById (string id)
		{
			return new HtmlElement (webHost, document.GetElementById (id));
		}

		public HtmlElement GetElementFromPoint (Point point) 
		{
			Mono.WebBrowser.DOM.IElement elem = document.GetElement (point.X, point.Y);
			if (elem != null)
				return new HtmlElement(webHost, elem);
			return null;
		}

		public HtmlElementCollection GetElementsByTagName (string tagName) 
		{
			Mono.WebBrowser.DOM.IElementCollection col = document.GetElementsByTagName (tagName);
			return new HtmlElementCollection (webHost, col);
		}

		public override int GetHashCode () 
		{ 
			return document.GetHashCode (); 
		}

		public Object InvokeScript (string scriptName)
		{ 
			throw new NotImplementedException (); 
		}

		public Object InvokeScript (string scriptName, Object[] args) 
		{
			throw new NotImplementedException ();
		}

		public static bool operator ==(HtmlDocument left, HtmlDocument right) {
			if ((object)left == (object)right) {
				return true;
			}

			if ((object)left == null || (object)right == null) {
				return false;
			}

			return left.Equals (right); 
		}

		public static bool operator !=(HtmlDocument left, HtmlDocument right) {
			return !(left == right);
		}


		public HtmlDocument OpenNew (bool replaceInHistory) 
		{
			Mono.WebBrowser.DOM.LoadFlags flags = Mono.WebBrowser.DOM.LoadFlags.None;
			if (replaceInHistory)
				flags |= Mono.WebBrowser.DOM.LoadFlags.ReplaceHistory;
			webHost.Navigation.Go ("about:blank", flags);
			return this;
		}

		public void Write (string text) 
		{
			document.Write (text);
		}

		#endregion

		#region Properties
		public HtmlElement ActiveElement {
			get { 
				Mono.WebBrowser.DOM.IElement element = document.Active;
				if (element == null)
					return null;
				return new HtmlElement (webHost, element);
			
			}
		}
		public Color ActiveLinkColor {
			get { return ParseColor(document.ActiveLinkColor); }
			set { document.ActiveLinkColor = value.ToArgb().ToString(); }
		}

		public HtmlElementCollection All
		{
			get {
				return new HtmlElementCollection (webHost, document.DocumentElement.All);
			}
		}

		public Color BackColor
		{
			get { return ParseColor(document.BackColor); }
			set { document.BackColor = value.ToArgb().ToString(); }
		}
		
		public HtmlElement Body {
			get { return new HtmlElement (webHost, document.Body); }
		}
		public string Cookie
		{
			get { return document.Cookie; }
			set { document.Cookie = value; }
		}
		public string DefaultEncoding { get { throw new NotImplementedException (); } }
		public string Domain
		{
			get { return document.Domain; }
			set { throw new NotSupportedException ("Setting the domain is not supported per the DOM Level 2 HTML specification. Sorry."); }
		}
		public Object DomDocument { get { throw new NotImplementedException (); } }
		
		public string Encoding
		{
			get { return document.Charset; }
			set { document.Charset = value; }
		}

		public bool Focused { get { throw new NotImplementedException (); } }
		public Color ForeColor
		{
			get { return ParseColor(document.ForeColor); }
			set { document.ForeColor = value.ToArgb().ToString(); }
		}
		
		public HtmlElementCollection Forms { 
			get { return new HtmlElementCollection (webHost, document.Forms); } 
		}
		
		public HtmlElementCollection Images { 
			get { return new HtmlElementCollection (webHost, document.Images); } 	
		}
		public Color LinkColor
		{
			get { return ParseColor(document.LinkColor); }
			set { document.LinkColor = value.ToArgb().ToString(); }
		}
		
		public HtmlElementCollection Links {
			get { return new HtmlElementCollection (webHost, document.Links); } 
		}
		
		public bool RightToLeft
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public string Title
		{
			get { return document.Title; }
			set { document.Title = value; }
		}
		
		public Uri Url { 
			get { return new Uri (document.Url); } 
		}
		
		public Color VisitedLinkColor
		{
			get { return ParseColor(document.VisitedLinkColor); }
			set { document.VisitedLinkColor =  value.ToArgb().ToString(); }
		}
		
		public HtmlWindow Window { 
			get { return new HtmlWindow (webHost, webHost.Window); } 
		
		}


		#endregion
		
		private Color ParseColor (string color) {
			if (color.IndexOf ("#") >= 0) {
				return Color.FromArgb (int.Parse (color.Substring (color.IndexOf ("#") + 1), NumberStyles.HexNumber));
			}
			return Color.FromName (color);
		}
	}
}

#endif