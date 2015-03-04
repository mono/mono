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


using System;
using System.Drawing;
using System.ComponentModel;
using System.Globalization;
using Mono.WebBrowser.DOM;

namespace System.Windows.Forms
{
	public sealed class HtmlDocument
	{
		private EventHandlerList events;
		private Mono.WebBrowser.IWebBrowser webHost;
		private IDocument document;
		private WebBrowser owner;

		internal HtmlDocument (WebBrowser owner, Mono.WebBrowser.IWebBrowser webHost) : 
			this (owner, webHost, webHost.Document)
		{
		}

		internal HtmlDocument (WebBrowser owner, Mono.WebBrowser.IWebBrowser webHost, IDocument doc)
		{
			this.webHost = webHost;
			this.document = doc;
			this.owner = owner;
		}


		internal EventHandlerList Events {
			get {
				if (events == null)
					events = new EventHandlerList ();

				return events;
			}
		}

		#region Methods


		public void AttachEventHandler (string eventName, EventHandler eventHandler)
		{ 
			document.AttachEventHandler (eventName, eventHandler);
		}

		public HtmlElement CreateElement (string elementTag) 
		{ 
			Mono.WebBrowser.DOM.IElement element = document.CreateElement (elementTag);
			return new HtmlElement (owner, webHost, element);
		}

		public void DetachEventHandler (string eventName, EventHandler eventHandler) 
		{
			document.DetachEventHandler (eventName, eventHandler);
		}

		public override bool Equals (object obj) {
			return this == (HtmlDocument) obj;
		}

		public void ExecCommand (string command, bool showUI, Object value) 
		{
			throw new NotImplementedException ("Not Supported");
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public void Focus () 
		{
			webHost.FocusIn (Mono.WebBrowser.FocusOption.None);
		}

		public HtmlElement GetElementById (string id)
		{
			IElement elem = document.GetElementById (id);
			if (elem != null)
				return new HtmlElement (owner, webHost, elem);
			return null;
		}

		public HtmlElement GetElementFromPoint (Point point) 
		{
			Mono.WebBrowser.DOM.IElement elem = document.GetElement (point.X, point.Y);
			if (elem != null)
				return new HtmlElement(owner, webHost, elem);
			return null;
		}

		public HtmlElementCollection GetElementsByTagName (string tagName) 
		{
			Mono.WebBrowser.DOM.IElementCollection col = document.GetElementsByTagName (tagName);
			if (col != null)
				return new HtmlElementCollection (owner, webHost, col);
			return null;
		}

		public override int GetHashCode () 
		{
			if (document == null)
				return 0;			
			return document.GetHashCode (); 
		}

		public Object InvokeScript (string scriptName)
		{ 
			return document.InvokeScript ("eval ('" + scriptName + "()');");
		}

		public Object InvokeScript (string scriptName, object[] args) 
		{
			string[] strArgs = new string[args.Length];
			for (int i = 0; i < args.Length; i++) {
				if (args[i] is string)
					strArgs[i] = "\"" + args[i].ToString() + "\"";
				else
					strArgs[i] = args[i].ToString();
			}
			return document.InvokeScript ("eval ('" + scriptName + "(" + String.Join (",", strArgs) + ")');");
		}

		public static bool operator ==(HtmlDocument left, HtmlDocument right) {
			if ((object)left == (object)right) {
				return true;
			}

			if ((object)left == null || (object)right == null) {
				return false;
			}

			return left.document.Equals (right.document); 
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
				return new HtmlElement (owner, webHost, element);
			
			}
		}
		
		public Color ActiveLinkColor {
			get { return ParseColor(document.ActiveLinkColor); }
			set { document.ActiveLinkColor = value.ToArgb().ToString(); }
		}

		public HtmlElementCollection All {
			get {
				return new HtmlElementCollection (owner, webHost, document.DocumentElement.All);
			}
		}

		public Color BackColor {
			get { return ParseColor(document.BackColor); }
			set { document.BackColor = value.ToArgb().ToString(); }
		}
		
		public HtmlElement Body {
			get { return new HtmlElement (owner, webHost, document.Body); }
		}
		
		public string Cookie {
			get { return document.Cookie; }
			set { document.Cookie = value; }
		}
		
		public string DefaultEncoding { 
			get { return document.Charset; }
		}
		
		public string Domain {
			get { return document.Domain; }
			set { throw new NotSupportedException ("Setting the domain is not supported per the DOM Level 2 HTML specification. Sorry."); }
		}
		
		public Object DomDocument { 
			get { throw new NotSupportedException ("Retrieving a reference to an mshtml interface is not supported. Sorry."); } 
		}
		
		public string Encoding {
			get { return document.Charset; }
			set { document.Charset = value; }
		}

		public bool Focused { 
			get { return webHost.Window.Document == document; }
		}
		
		public Color ForeColor {
			get { return ParseColor(document.ForeColor); }
			set { document.ForeColor = value.ToArgb().ToString(); }
		}
		
		public HtmlElementCollection Forms { 
			get { return new HtmlElementCollection (owner, webHost, document.Forms); } 
		}
		
		public HtmlElementCollection Images { 
			get { return new HtmlElementCollection (owner, webHost, document.Images); } 	
		}
		
		public Color LinkColor {
			get { return ParseColor(document.LinkColor); }
			set { document.LinkColor = value.ToArgb().ToString(); }
		}
		
		public HtmlElementCollection Links {
			get { return new HtmlElementCollection (owner, webHost, document.Links); } 
		}
		
		public bool RightToLeft {
			get { 
				IAttribute dir = document.Attributes["dir"];
				return (dir != null && dir.Value == "rtl");					
			}
			set {
				
				IAttribute dir = document.Attributes["dir"];
				if (dir == null && value) {
					IAttribute attr = document.CreateAttribute ("dir");
					attr.Value = "rtl";
					document.AppendChild (attr);
				} else if (dir != null && !value) {
					document.RemoveChild (dir);
				}
			}
		}
		
		public string Title {
			get {
				if (document == null)
					return String.Empty;
				return document.Title; 
			}
			set { document.Title = value; }
		}
		
		public Uri Url { 
			get { return new Uri (document.Url); } 
		}
		
		public Color VisitedLinkColor {
			get { return ParseColor(document.VisitedLinkColor); }
			set { document.VisitedLinkColor =  value.ToArgb().ToString(); }
		}
		
		public HtmlWindow Window { 
			get { return new HtmlWindow (owner, webHost, webHost.Window); } 
		
		}


		#endregion

		#region Events
		private static object ClickEvent = new object ();
		public event HtmlElementEventHandler Click {
			add { 
				Events.AddHandler (ClickEvent, value);
				document.Click += new NodeEventHandler (OnClick);
			}
			remove { 
				Events.RemoveHandler (ClickEvent, value);
				document.Click -= new NodeEventHandler (OnClick);
			}
		}
		
		private void OnClick (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[ClickEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}

		private static object ContextMenuShowingEvent = new object ();
		public event HtmlElementEventHandler ContextMenuShowing {
			add { 
				Events.AddHandler (ContextMenuShowingEvent, value);
				owner.WebHost.ContextMenuShown += new Mono.WebBrowser.ContextMenuEventHandler (OnContextMenuShowing);
			}
			remove { 
				Events.RemoveHandler (ContextMenuShowingEvent, value);
				owner.WebHost.ContextMenuShown -= new Mono.WebBrowser.ContextMenuEventHandler (OnContextMenuShowing);
			}
		}
		private void OnContextMenuShowing (object sender, Mono.WebBrowser.ContextMenuEventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[ContextMenuShowingEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
				if (ev.ReturnValue) {
					owner.OnWebHostContextMenuShown (sender, e);
				}
			}
		}

		private static object FocusingEvent = new object ();
		public event HtmlElementEventHandler Focusing {
			add { 
				Events.AddHandler (FocusingEvent, value); 
				document.OnFocus += new NodeEventHandler (OnFocusing);
			}
			remove { 
				Events.RemoveHandler (FocusingEvent, value); 
				document.OnFocus -= new NodeEventHandler (OnFocusing);
			}
		}
		
		private void OnFocusing (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[FocusingEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}

		private static object LosingFocusEvent = new object ();
		public event HtmlElementEventHandler LosingFocus {
			add { 
				Events.AddHandler (LosingFocusEvent, value); 
				document.OnBlur += new NodeEventHandler (OnLosingFocus);
			}
			remove { 
				Events.RemoveHandler (LosingFocusEvent, value); 
				document.OnBlur -= new NodeEventHandler (OnLosingFocus);
			}
		}
		
		private void OnLosingFocus (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[LosingFocusEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}

		private static object MouseDownEvent = new object ();
		public event HtmlElementEventHandler MouseDown {
			add { 
				Events.AddHandler (MouseDownEvent, value); 
				document.MouseDown += new NodeEventHandler (OnMouseDown);
			}
			remove { 
				Events.RemoveHandler (MouseDownEvent, value); 
				document.MouseDown -= new NodeEventHandler (OnMouseDown);
			}
		}
		
		private void OnMouseDown (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[MouseDownEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}

		private static object MouseLeaveEvent = new object ();
		public event HtmlElementEventHandler MouseLeave {
			add { 
				Events.AddHandler (MouseLeaveEvent, value); 
				document.MouseLeave += new NodeEventHandler (OnMouseLeave);
			}
			remove { 
				Events.RemoveHandler (MouseLeaveEvent, value); 
				document.MouseLeave -= new NodeEventHandler (OnMouseLeave);
			}
		}
		
		private void OnMouseLeave (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[MouseLeaveEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}

		private static object MouseMoveEvent = new object ();
		public event HtmlElementEventHandler MouseMove {
			add { 
				Events.AddHandler (MouseMoveEvent, value); 
				document.MouseMove += new NodeEventHandler (OnMouseMove);
			}
			remove { 
				Events.RemoveHandler (MouseMoveEvent, value); 
				document.MouseMove -= new NodeEventHandler (OnMouseMove);
			}
		}
		
		private void OnMouseMove (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[MouseMoveEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}

		private static object MouseOverEvent = new object ();
		public event HtmlElementEventHandler MouseOver {
			add { 
				Events.AddHandler (MouseOverEvent, value); 
				document.MouseOver += new NodeEventHandler (OnMouseOver);
			}
			remove { 
				Events.RemoveHandler (MouseOverEvent, value); 
				document.MouseOver -= new NodeEventHandler (OnMouseOver);
			}
		}
		
		private void OnMouseOver (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[MouseOverEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}

		private static object MouseUpEvent = new object ();
		public event HtmlElementEventHandler MouseUp {
			add { 
				Events.AddHandler (MouseUpEvent, value); 
				document.MouseUp += new NodeEventHandler (OnMouseUp);
			}
			remove { 
				Events.RemoveHandler (MouseUpEvent, value); 
				document.MouseUp -= new NodeEventHandler (OnMouseUp);
			}
		}
		
		private void OnMouseUp (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[MouseUpEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}

		private static object StopEvent = new object ();
		public event HtmlElementEventHandler Stop {
			add { 
				Events.AddHandler (StopEvent, value); 
				document.LoadStopped += new EventHandler (OnStop);
			}			
			remove { 
				Events.RemoveHandler (StopEvent, value); 
				document.LoadStopped -= new EventHandler (OnStop);
			}
		}
		
		private void OnStop (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[StopEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}

		#endregion


		#region Internal and Private
		private Color ParseColor (string color) {
			if (color.IndexOf ("#") >= 0) {
				return Color.FromArgb (int.Parse (color.Substring (color.IndexOf ("#") + 1), NumberStyles.HexNumber));
			}
			return Color.FromName (color);
		}
		
		internal string DocType {
			get { 
				if (this.document == null)
					return String.Empty;
				if (this.document.DocType != null)
					return this.document.DocType.Name;
				return String.Empty;
			}
		}
		#endregion
	}
		
}
