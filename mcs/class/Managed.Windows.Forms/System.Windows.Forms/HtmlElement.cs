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
using Mono.WebBrowser.DOM;

namespace System.Windows.Forms
{
	public sealed class HtmlElement
	{
		private EventHandlerList events;
		private Mono.WebBrowser.IWebBrowser webHost;
		internal IElement element;
		private WebBrowser owner;
		
		internal HtmlElement (WebBrowser owner, Mono.WebBrowser.IWebBrowser webHost, IElement element)
		{
			this.webHost = webHost;
			this.element = element;
			this.owner = owner;
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
				return new HtmlElementCollection (owner, webHost, this.element.All);
			}
		}

		// from http://www.w3.org/TR/html401/index/elements.html
		public bool CanHaveChildren {
			get {
				string tag = this.TagName;
				switch (tag.ToLowerInvariant ()) {
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
					default:
						return true;
				}
			}
		}

		public HtmlElementCollection Children {
			get {
				return new HtmlElementCollection (owner, webHost, this.element.Children);
			}
		}

		public Rectangle ClientRectangle {
			get { return new Rectangle (0, 0, this.element.ClientWidth, this.element.ClientHeight); }
		}
		
		public Rectangle OffsetRectangle {
			get { return new Rectangle (this.element.OffsetLeft, this.element.OffsetTop, this.element.OffsetWidth, this.element.OffsetHeight); }
		}

		public Rectangle ScrollRectangle {
			get { return new Rectangle (this.element.ScrollLeft, this.element.ScrollTop, this.element.ScrollWidth, this.element.ScrollHeight); }
		}

		public int ScrollLeft {
			get { return this.element.ScrollLeft; }
			set { this.element.ScrollLeft = value; }
		}

		public int ScrollTop {
			get { return this.element.ScrollTop; }
			set { this.element.ScrollTop = value; }
		}

		public HtmlElement OffsetParent {
			get { return new HtmlElement (owner, this.webHost, this.element.OffsetParent); }
		}

		public HtmlDocument Document {
			get {
				return new HtmlDocument (owner, webHost, element.Owner);
			}
		}

		public bool Enabled	{
			get { return !this.element.Disabled; }
			set { this.element.Disabled = !value; }
		}

		public string InnerHtml	{
			get { return this.element.InnerHTML; }
			set { this.element.InnerHTML = value; }
		}

		public string InnerText {
			get { return this.element.InnerText; }
			set { this.element.InnerText = value; }
		}

		public string Id {
			get { return GetAttribute("id"); }
			set { SetAttribute ("id", value); }
		}

		public string Name {
			get { return GetAttribute ("name"); }
			set { SetAttribute ("name", value); }
		}

		public HtmlElement FirstChild {
			get {
				return new HtmlElement (owner, webHost, (IElement)element.FirstChild); 
			}
		}

		public HtmlElement NextSibling {
			get { return new HtmlElement (owner, webHost, (IElement)element.Next); }
		}
		
		public HtmlElement Parent {
			get { return new HtmlElement (owner, webHost, (IElement)element.Parent); }
		}

		public string TagName {
			get { return element.TagName; }
		}

		public short TabIndex {
			get { return (short)element.TabIndex; }
			set { element.TabIndex = value; }
		}

		public object DomElement {
			get { throw new NotSupportedException ("Retrieving a reference to an mshtml interface is not supported. Sorry."); } 
		}

		public string OuterHtml {
			get { return this.element.OuterHTML; }
			set { this.element.OuterHTML = value; }
		}

		public string OuterText {
			get { return this.element.OuterText; }
			set { this.element.OuterText = value; }
		}

		public string Style {
			get { return this.element.Style; }
			set { this.element.Style = value; }
		}

		#endregion
		
		#region Methods
		public HtmlElement AppendChild (HtmlElement newElement)
		{
			IElement newChild = this.element.AppendChild (newElement.element);
			newElement.element = newChild;
			return newElement;
		}
		
		public void AttachEventHandler (string eventName, EventHandler eventHandler)
		{
			element.AttachEventHandler (eventName, eventHandler);
		}

		public void DetachEventHandler (string eventName, EventHandler eventHandler)
		{
			element.DetachEventHandler (eventName, eventHandler);
		}
		
		public void Focus ()
		{
			throw new NotImplementedException ();
		}

		public string GetAttribute (string attributeName)
		{
			return element.GetAttribute (attributeName);
		}

		public HtmlElementCollection GetElementsByTagName (string tagName)
		{
			Mono.WebBrowser.DOM.IElementCollection col = element.GetElementsByTagName (tagName);
			return new HtmlElementCollection (owner, webHost, col);
		}
		
		public override int GetHashCode ()
		{
			if (element == null)
				return 0;
			return element.GetHashCode ();
		}

		internal bool HasAttribute (string name)
		{
			return element.HasAttribute (name);
		}

		public HtmlElement InsertAdjacentElement (HtmlElementInsertionOrientation orient, HtmlElement newElement)
		{
			switch (orient) {
				case HtmlElementInsertionOrientation.BeforeBegin:
					element.Parent.InsertBefore (newElement.element, element);
					return newElement;
				case HtmlElementInsertionOrientation.AfterBegin:
					element.InsertBefore (newElement.element, element.FirstChild);
					return newElement;
				case HtmlElementInsertionOrientation.BeforeEnd:
					return this.AppendChild (newElement);
				case HtmlElementInsertionOrientation.AfterEnd:
					return this.AppendChild (newElement);
			}
			return null;
		}
		
		public object InvokeMember (string methodName)
		{
			return this.element.Owner.InvokeScript ("eval ('" + methodName + "()');");
		}

		public object InvokeMember (string methodName, params object[] parameter)
		{
			string[] strArgs = new string[parameter.Length];
			for (int i = 0; i < parameter.Length; i++) {
				strArgs[i] = parameter.ToString();
			}
			return this.element.Owner.InvokeScript ("eval ('" + methodName + "(" + String.Join (",", strArgs) + ")');");
		}
		
		public void RaiseEvent (string eventName) 
		{
			this.element.FireEvent (eventName);
		}

		public void RemoveFocus () 
		{
			this.element.Blur ();
		}

		public void ScrollIntoView (bool alignWithTop) 
		{
			this.element.ScrollIntoView (alignWithTop);
		}
		
		public void SetAttribute (string attributeName, string value)
		{
			this.element.SetAttribute (attributeName, value);
		}

		public override bool Equals (object obj)
		{
			return this == (HtmlElement) obj;
		}
		
		public static bool operator == (HtmlElement left, HtmlElement right)
		{
			if ((object)left == (object)right)
				return true;
			if ((object)left == null || (object)right == null)
				return false;
			return left.element.Equals (right.element); 
		}

		public static bool operator != (HtmlElement left, HtmlElement right)
		{
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

		private void OnDoubleClick (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[DoubleClickEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}

		private static object DoubleClickEvent = new object ();
		public event HtmlElementEventHandler DoubleClick {
			add {
				Events.AddHandler (DoubleClickEvent, value);
				element.DoubleClick += new NodeEventHandler (OnDoubleClick);
			}
			remove {
				Events.RemoveHandler (DoubleClickEvent, value);
				element.DoubleClick -= new NodeEventHandler (OnDoubleClick);
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

		private static object MouseDownEvent = new object ();
		public event HtmlElementEventHandler MouseDown {
			add {
				Events.AddHandler (MouseDownEvent, value);
				element.MouseDown += new NodeEventHandler (OnMouseDown);
			}
			remove {
				Events.RemoveHandler (MouseDownEvent, value);
				element.MouseDown -= new NodeEventHandler (OnMouseDown);
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

		private static object MouseUpEvent = new object ();
		public event HtmlElementEventHandler MouseUp {
			add {
				Events.AddHandler (MouseUpEvent, value);
				element.MouseUp += new NodeEventHandler (OnMouseUp);
			}
			remove {
				Events.RemoveHandler (MouseUpEvent, value);
				element.MouseUp -= new NodeEventHandler (OnMouseUp);
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

		private static object MouseMoveEvent = new object ();
		public event HtmlElementEventHandler MouseMove {
			add {
				Events.AddHandler (MouseMoveEvent, value);
				element.MouseMove += new NodeEventHandler (OnMouseMove);
			}
			remove {
				Events.RemoveHandler (MouseMoveEvent, value);
				element.MouseMove -= new NodeEventHandler (OnMouseMove);
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

		private static object MouseOverEvent = new object ();
		public event HtmlElementEventHandler MouseOver {
			add { 
				Events.AddHandler (MouseOverEvent, value);
				element.MouseOver += new NodeEventHandler (OnMouseOver);
			}
			remove { 
				Events.RemoveHandler (MouseOverEvent, value);
				element.MouseOver -= new NodeEventHandler (OnMouseOver);
			}
		}
		
		private void OnMouseEnter (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[MouseEnterEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}
		
		private static object MouseEnterEvent = new object ();
		public event HtmlElementEventHandler MouseEnter {
			add {
				Events.AddHandler (MouseEnterEvent, value);
				element.MouseEnter += new NodeEventHandler (OnMouseEnter);
			}
			remove {
				Events.RemoveHandler (MouseEnterEvent, value);
				element.MouseEnter -= new NodeEventHandler (OnMouseEnter);
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
		
		private static object MouseLeaveEvent = new object ();
		public event HtmlElementEventHandler MouseLeave {
			add {
				Events.AddHandler (MouseLeaveEvent, value);
				element.MouseLeave += new NodeEventHandler (OnMouseLeave);
			}
			remove {
				Events.RemoveHandler (MouseLeaveEvent, value);
				element.MouseLeave -= new NodeEventHandler (OnMouseLeave);
			}
		}

		private void OnKeyDown (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[KeyDownEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}
		
		private static object KeyDownEvent = new object ();
		public event HtmlElementEventHandler KeyDown {
			add {
				Events.AddHandler (KeyDownEvent, value);
				element.KeyDown += new NodeEventHandler (OnKeyDown);
			}
			remove {
				Events.RemoveHandler (KeyDownEvent, value);
				element.KeyDown -= new NodeEventHandler (OnKeyDown);
			}
		}

		private void OnKeyPress (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[KeyPressEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}
		
		private static object KeyPressEvent = new object ();
		public event HtmlElementEventHandler KeyPress {
			add {
				Events.AddHandler (KeyPressEvent, value);
				element.KeyPress += new NodeEventHandler (OnKeyPress);
			}
			remove {
				Events.RemoveHandler (KeyPressEvent, value);
				element.KeyPress -= new NodeEventHandler (OnKeyPress);
			}
		}

		private void OnKeyUp (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[KeyUpEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}
		
		private static object KeyUpEvent = new object ();
		public event HtmlElementEventHandler KeyUp {
			add {
				Events.AddHandler (KeyUpEvent, value);
				element.KeyUp += new NodeEventHandler (OnKeyUp);
			}
			remove {
				Events.RemoveHandler (KeyUpEvent, value);
				element.KeyUp -= new NodeEventHandler (OnKeyUp);
			}
		}

		private static object DragEvent = new object ();
		public event HtmlElementEventHandler Drag {
			add {
				Events.AddHandler (DragEvent, value);
			}
			remove {
				Events.RemoveHandler (DragEvent, value);
			}
		}

		private void OnDrag (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[DragEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}


		private static object DragEndEvent = new object ();
		public event HtmlElementEventHandler DragEnd {
			add	{
				Events.AddHandler (DragEndEvent, value);
			}
			remove {
				Events.RemoveHandler (DragEndEvent, value);
			}
		}

		private void OnDragEnd (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[DragEndEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}
		private static object DragLeaveEvent = new object ();
		public event HtmlElementEventHandler DragLeave {
			add {
				Events.AddHandler (DragLeaveEvent, value);
			}
			remove {
				Events.RemoveHandler (DragLeaveEvent, value);
			}
		}

		private void OnDragLeave (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[DragLeaveEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}
		private static object DragOverEvent = new object ();
		public event HtmlElementEventHandler DragOver {
			add {
				Events.AddHandler (DragOverEvent, value);
			}
			remove {
				Events.RemoveHandler (DragOverEvent, value);
			}
		}

		private void OnDragOver (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[DragOverEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}
		private static object FocusingEvent = new object ();
		public event HtmlElementEventHandler Focusing {
			add {
				Events.AddHandler (FocusingEvent, value);
				((INode)element).OnFocus += new NodeEventHandler (OnFocusing);
			}
			remove {
				Events.RemoveHandler (FocusingEvent, value);
				((INode)element).OnFocus -= new NodeEventHandler (OnFocusing);
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

		private static object GotFocusEvent = new object ();
		public event HtmlElementEventHandler GotFocus {
			add {
				Events.AddHandler (GotFocusEvent, value);
				((INode)element).OnFocus += new NodeEventHandler (OnGotFocus);
			}
			remove {
				Events.RemoveHandler (GotFocusEvent, value);
				((INode)element).OnFocus -= new NodeEventHandler (OnGotFocus);
			}
		}

		private void OnGotFocus (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[GotFocusEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}
		private static object LosingFocusEvent = new object ();
		public event HtmlElementEventHandler LosingFocus {
			add {
				Events.AddHandler (LosingFocusEvent, value);
				((INode)element).OnBlur += new NodeEventHandler (OnLosingFocus);
			}
			remove {
				Events.RemoveHandler (LosingFocusEvent, value);
				((INode)element).OnBlur -= new NodeEventHandler (OnLosingFocus);
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
		private static object LostFocusEvent = new object ();
		public event HtmlElementEventHandler LostFocus {
			add {
				Events.AddHandler (LostFocusEvent, value);
				((INode)element).OnBlur += new NodeEventHandler (OnLostFocus);
			}
			remove {
				Events.RemoveHandler (LostFocusEvent, value);
				((INode)element).OnBlur -= new NodeEventHandler (OnLostFocus);
			}
		}

		private void OnLostFocus (object sender, EventArgs e)
		{
			HtmlElementEventHandler eh = (HtmlElementEventHandler) Events[LostFocusEvent];
			if (eh != null) {
				HtmlElementEventArgs ev = new HtmlElementEventArgs ();
				eh (this, ev);
			}
		}

		#endregion
	}
}
