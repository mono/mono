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
using System.ComponentModel;
using Mono.WebBrowser.DOM;

namespace System.Windows.Forms
{
	[MonoTODO ("Needs Implementation")]
	public sealed class HtmlElement
	{
		private EventHandlerList events;		
		private Mono.WebBrowser.IWebBrowser webHost;
		internal IElement element;
		
		internal HtmlElement (Mono.WebBrowser.IWebBrowser webHost, IElement element)
		{
			this.webHost = webHost;
			this.element = element;
		}

		internal EventHandlerList Events {
			get {
				if (events == null)
					events = new EventHandlerList();

				return events;
			}
		}

		#region Properties
		public HtmlElementCollection All {
			get {
				return new HtmlElementCollection (webHost, this.element.All);
			}
		}

		// from http://www.w3.org/TR/html401/index/elements.html
		public bool CanHaveChildren {
			get {
				string tag = this.TagName;
				switch (tag.ToLower ()) {
					case "area":
					case "base":
					case "basefont":
					case "br":
					case "col":
					case "frame":
					case "hr":
					case "img":
					case "input":
					case "isindex":
					case "link":
					case "meta":
					case "param":
						return false;
					break;
					default:
						return true;
						break;
				}
			}
		}

		public HtmlElementCollection Children {
			get {
				return new HtmlElementCollection (webHost, this.element.Children);
			}
		}
		
		public HtmlDocument Document {
			get {
				return new HtmlDocument (webHost, element.Owner);
			}
		}

		public string InnerHtml
		{
			get { return this.element.InnerHTML; }
			set { throw new NotImplementedException (); }
		}

		public string InnerText
		{
			get { return this.element.InnerText; }
			set { this.element.InnerText = value; }
		}

		public string Id
		{
			get { return GetAttribute("id"); }
			set { SetAttribute ("id", value); }
		}

		public string Name
		{
			get { return GetAttribute ("name"); }
			set { SetAttribute ("name", value); }
		}

		public HtmlElement FirstChild
		{
			get { return new HtmlElement (webHost, (IElement)element.FirstChild); }
		}

		public HtmlElement NextSibling {
			get { return new HtmlElement (webHost, (IElement)element.Next); }
		}
		
		public HtmlElement Parent {
			get { return new HtmlElement (webHost, (IElement)element.Parent); }
		}

		public string TagName {
			get { return element.TagName; }
		}
		#endregion
		
		#region Methods
		public HtmlElement AppendChild (HtmlElement child)
		{
			IElement newChild = this.element.AppendChild (child.element);
			child.element = newChild;
			return child;
		}
		
		public void AttachEventHandler (string eventName, EventHandler eventHandler)
		{ 
			throw new NotImplementedException ();
		}

		public void DetachEventHandler (string eventName, EventHandler eventHandler) 
		{
			throw new NotImplementedException ();
		}
		
		public void Focus () 
		{
			throw new NotImplementedException ();
		}

		public string GetAttribute (string name) {
			return element.GetAttribute (name);
		}

		public HtmlElementCollection GetElementsByTagName (string tagName) 
		{
			Mono.WebBrowser.DOM.IElementCollection col = element.GetElementsByTagName (tagName);
			return new HtmlElementCollection (webHost, col);
		}		
		
		public override int GetHashCode () 
		{ 
			return element.GetHashCode (); 
		}

		internal bool HasAttribute (string name) {
			return element.HasAttribute (name);
		}

		public HtmlElement InsertAdjacentElement (HtmlElementInsertionOrientation orientation, HtmlElement newElement) {
			
			switch (orientation) {
				case HtmlElementInsertionOrientation.BeforeBegin:
					IElement newChild1 = this.element.Parent.InsertBefore (newElement.element, this.element);
					newElement.element = newChild1;
					return newElement;					
				break;
				case HtmlElementInsertionOrientation.AfterBegin:
					IElement newChild2 = this.element.InsertBefore (newElement.element, this.element.FirstChild);
					newElement.element = newChild2;
					return newElement;					
				break;
				case HtmlElementInsertionOrientation.BeforeEnd:
					return this.AppendChild (newElement);					
				break;
				case HtmlElementInsertionOrientation.AfterEnd:
					return this.AppendChild (newElement);
				break;
			}
			return null;
		}
		
		public object InvokeMember (string method) {
			throw new NotImplementedException ();
		}		
		public object InvokeMember (string method, object[] parameters) {
			throw new NotImplementedException ();
		}
		
		public void RaiseEvent (string name) 
		{
			throw new NotImplementedException ();
		}

		public void RemoveFocus () 
		{
			throw new NotImplementedException ();
		}

		public void ScrollIntoView (bool alignWithTop) 
		{
			throw new NotImplementedException ();
		}
		
		public void SetAttribute (string name, string value)
		{
			this.element.SetAttribute (name, value);
		}

		public override bool Equals (object obj) {
			return this == (HtmlElement) obj;
		}
		
		public static bool operator ==(HtmlElement left, HtmlElement right) {
			if ((object)left == (object)right) {
				return true;
			}

			if ((object)left == null || (object)right == null) {
				return false;
			}

			return left.Equals (right); 
		}

		public static bool operator !=(HtmlElement left, HtmlElement right) {
			return !(left == right);
		}
		#endregion

#region Events
		private void OnClick (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[ClickEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}

		private static object ClickEvent = new object ();
		public event HtmlElementEventHandler Click {
			add { 
				Events.AddHandler (ClickEvent, value);
				element.Click += new NodeEventHandler (OnClick);
			}
			remove { 
				Events.RemoveHandler (ClickEvent, value);
				element.Click -= new NodeEventHandler (OnClick);
			}
		}

		private static object DoubleClickEvent = new object ();
		public event HtmlElementEventHandler DoubleClick {
			add { Events.AddHandler (DoubleClickEvent, value); }
			remove { Events.RemoveHandler (DoubleClickEvent, value); }
		}

		private static object MouseDownEvent = new object ();
		public event HtmlElementEventHandler MouseDown {
			add { Events.AddHandler (MouseDownEvent, value); }
			remove { Events.RemoveHandler (MouseDownEvent, value); }
		}

		private static object MouseUpEvent = new object ();
		public event HtmlElementEventHandler MouseUp {
			add { Events.AddHandler (MouseUpEvent, value); }
			remove { Events.RemoveHandler (MouseUpEvent, value); }
		}

		private static object MouseMoveEvent = new object ();
		public event HtmlElementEventHandler MouseMove {
			add { Events.AddHandler (MouseMoveEvent, value); }
			remove { Events.RemoveHandler (MouseMoveEvent, value); }
		}

		private static object MouseOverEvent = new object ();
		public event HtmlElementEventHandler MouseOver {
			add { Events.AddHandler (MouseOverEvent, value); }
			remove { Events.RemoveHandler (MouseOverEvent, value); }
		}
		private static object MouseEnterEvent = new object ();
		public event HtmlElementEventHandler MouseEnter {
			add { Events.AddHandler (MouseEnterEvent, value); }
			remove { Events.RemoveHandler (MouseEnterEvent, value); }
		}
		private static object MouseLeaveEvent = new object ();
		public event HtmlElementEventHandler MouseLeave {
			add { Events.AddHandler (MouseLeaveEvent, value); }
			remove { Events.RemoveHandler (MouseLeaveEvent, value); }
		}
		private static object KeyDownEvent = new object ();
		public event HtmlElementEventHandler KeyDown {
			add { Events.AddHandler (KeyDownEvent, value); }
			remove { Events.RemoveHandler (KeyDownEvent, value); }
		}
		private static object KeyPressEvent = new object ();
		public event HtmlElementEventHandler KeyPress {
			add { Events.AddHandler (KeyPressEvent, value); }
			remove { Events.RemoveHandler (KeyPressEvent, value); }
		}
		private static object KeyUpEvent = new object ();
		public event HtmlElementEventHandler KeyUp {
			add { Events.AddHandler (KeyUpEvent, value); }
			remove { Events.RemoveHandler (KeyUpEvent, value); }
		}

#endregion

	}
}

#endif