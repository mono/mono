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
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla.DOM
{
	internal class Node: DOMObject, INode
	{
		private nsIDOMNode _node;
		internal nsIDOMNode node {
			get { return _node; }
			set {
				if (!(value is nsIDOMHTMLDocument) && control.platform != control.enginePlatform)
					_node = nsDOMNode.GetProxy (control, value);
				else
					_node = value;
			}
		}
		
		protected int hashcode;
		private EventListener eventListener;
		private WebBrowser control;
			
		public Node (WebBrowser control, nsIDOMNode domNode) : base (control)
		{
			hashcode = domNode.GetHashCode ();
			this.control = control;
			this.node = domNode;
		}

		#region IDisposable Members
		protected override  void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					this.resources.Clear ();
					this.node = null;
				}
			}
			base.Dispose(disposing);
		}		
		#endregion

		#region INode Members
		public virtual IAttributeCollection Attributes
		{
			get
			{
				if (!resources.Contains ("Attributes")) {
					nsIDOMNamedNodeMap attributes;
					this.node.getAttributes (out attributes);
					resources.Add ("Attributes", new AttributeCollection (control, attributes));
				}
				return resources["Attributes"] as IAttributeCollection;
			}
		}

		public virtual INodeList ChildNodes {
			get {
				if (!resources.Contains ("ChildNodes")) {
					nsIDOMNodeList children;
					this.node.getChildNodes (out children);
					resources.Add ("ChildNodes", new NodeList (control, children));
				}
				return resources["ChildNodes"] as INodeList;
			}
		}


		public virtual INode FirstChild {
			get {
				if (!resources.Contains ("FirstChild")) {
					nsIDOMNode child;
					this.node.getFirstChild (out child);
					resources.Add ("FirstChild", new Node (control, child));
				}
				return resources["FirstChild"] as INode;
			}
		}

		public virtual INode LastChild {
			get {
				if (!resources.Contains ("LastChild")) {
					nsIDOMNode child;
					this.node.getLastChild (out child);
					resources.Add ("LastChild", new Node (control, child));
				}
				return resources["LastChild"] as INode;
			}
		}
		
		public virtual INode Parent {
			get {
				if (!resources.Contains ("Parent")) {
					nsIDOMNode parent;
					this.node.getParentNode (out parent);
					resources.Add ("Parent", new Node (control, parent));
				}
				return resources["Parent"] as INode;
			}
		}

		public virtual INode Previous {
			get {
				if (!resources.Contains ("Previous")) {
					nsIDOMNode child;
					this.node.getPreviousSibling (out child);
					resources.Add ("Previous", new Node (control, child));
				}
				return resources["Previous"] as INode;
			}
		}

		public virtual INode Next {
			get {
				if (!resources.Contains ("Next")) {
					nsIDOMNode child;
					this.node.getNextSibling (out child);
					resources.Add ("Next", new Node (control, child));
				}
				return resources["Next"] as INode;
			}
		}


		public virtual string LocalName {
			get {
				this.node.getLocalName (storage);
				return Base.StringGet (storage);				
			}
		}
		
		public IDocument Owner {
			get {
				nsIDOMDocument doc;
				this.node.getOwnerDocument (out doc);
				return new Document (control, doc as Mono.Mozilla.nsIDOMHTMLDocument);
			}
		}

		public string Style {
			get {				
				nsIDOMDocument doc;
				this.node.getOwnerDocument (out doc);
				nsIDOMDocumentView docView = (nsIDOMDocumentView) doc;
				nsIDOMAbstractView abstractView;
				docView.getDefaultView (out abstractView);
				nsIDOMViewCSS viewCss = (nsIDOMViewCSS)abstractView;
				Base.StringSet (storage, String.Empty);
				nsIDOMCSSStyleDeclaration styleDecl;
				AsciiString s = new Mono.Mozilla.AsciiString(String.Empty);
				viewCss.getComputedStyle (this.node as Mono.Mozilla.nsIDOMElement, s.Handle, out styleDecl);
				styleDecl.getCssText (storage);
				return Base.StringGet (storage);
			}
			set {
				nsIDOMDocument doc;
				this.node.getOwnerDocument (out doc);
				nsIDOMDocumentView docView = (nsIDOMDocumentView) doc;
				nsIDOMAbstractView abstractView;
				docView.getDefaultView (out abstractView);
				nsIDOMViewCSS viewCss = (nsIDOMViewCSS)abstractView;
				Base.StringSet (storage, String.Empty);
				nsIDOMCSSStyleDeclaration styleDecl;
				viewCss.getComputedStyle (this.node as Mono.Mozilla.nsIDOMElement, storage, out styleDecl);
				Base.StringSet (storage, value);
				styleDecl.setCssText (storage);				
			}
		}
		
		public virtual Mono.WebBrowser.DOM.NodeType Type {
			get {
				ushort type;
				this.node.getNodeType (out type);
				return (Mono.WebBrowser.DOM.NodeType) Enum.ToObject (typeof (Mono.WebBrowser.DOM.NodeType), type);
			}
		}

		public virtual string Value {
			get
			{
				this.node.getNodeValue (storage);
				return Base.StringGet (storage);
			}
		}
		
		#endregion
		
		#region Methods
		public virtual void FireEvent (string eventName)
		{
			nsIDOMDocument doc;
			this.node.getOwnerDocument (out doc);

			nsIDOMDocumentEvent docEvent = (nsIDOMDocumentEvent) doc;
			nsIDOMDocumentView docView = (nsIDOMDocumentView) doc;
			nsIDOMAbstractView abstractView;
			docView.getDefaultView (out abstractView);
			nsIDOMEventTarget target = (nsIDOMEventTarget) this.node;
			bool ret = false;

			string eventType;
			switch (eventName) {
				case "mousedown":
				case "mouseup":
				case "click":
				case "dblclick":
				case "mouseover":
				case "mouseout":
				case "mousemove":
				case "contextmenu":
					eventType = "mouseevents";
					nsIDOMEvent evtMouse;
					Base.StringSet (storage, eventType);
					docEvent.createEvent (storage, out evtMouse);
					nsIDOMMouseEvent domEventMouse = evtMouse as nsIDOMMouseEvent;
					Base.StringSet (storage, eventName);
					domEventMouse.initMouseEvent (storage, true, true, abstractView, 1, 0, 0, 0, 0, false, false, false, false, 0, target);
					target.dispatchEvent (domEventMouse, out ret);
					break;
				case "keydown":
				case "keyup":
				case "keypress":
					eventType = "keyevents";
					nsIDOMEvent evtKey;
					Base.StringSet (storage, eventType);
					docEvent.createEvent (storage, out evtKey);
					Base.StringSet (storage, eventName);
					nsIDOMKeyEvent domEventKey = evtKey as nsIDOMKeyEvent;
					domEventKey.initKeyEvent (storage, true, true, abstractView, false, false, false, false, 0, 0);
					target.dispatchEvent (domEventKey, out ret);
					break;
				case "DOMActivate":
				case "DOMFocusIn":
				case "DOMFocusOut":
				case "input":
					eventType = "uievents";
					nsIDOMEvent evtUI;
					Base.StringSet (storage, eventType);
					docEvent.createEvent (storage, out evtUI);
					Base.StringSet (storage, eventName);
					nsIDOMUIEvent domEventUI = evtUI as nsIDOMUIEvent;
					domEventUI.initUIEvent (storage, true, true, abstractView, 1);
					target.dispatchEvent (domEventUI, out ret);
					break;
				case "focus":
				case "blur":
				case "submit":
				case "reset":
				case "change":
				case "select":
				case "load":
				case "beforeunload":
				case "unload":
				case "abort":
				case "error":
				default:
					eventType = "events";
					nsIDOMEvent domEvent;
					Base.StringSet (storage, eventType);
					docEvent.createEvent (storage, out domEvent);
					Base.StringSet (storage, eventName);
					domEvent.initEvent (storage, true, true);
					target.dispatchEvent (domEvent, out ret);
					break;
			}

		}

		public virtual IElement InsertBefore (INode child, INode refChild) {
			nsIDOMNode newChild;
			Node elem = (Node) child;
			Node reference = (Node) refChild;
			this.node.insertBefore (elem.node, reference.node, out newChild);
			return new Element (control, newChild as nsIDOMElement);
		}		
		#endregion

		
		
		public override int GetHashCode () 
		{
			return this.hashcode;
		}
		
		#region Events
		
		private EventListener EventListener {
			get {
				if (eventListener == null)
					eventListener = new EventListener (this.node as nsIDOMEventTarget, this);
				return eventListener;
			}
		}
		
		public void AttachEventHandler (string eventName, EventHandler handler) 
		{
			EventListener.AddHandler (handler, eventName);			
		}
		
		public void DetachEventHandler (string eventName, EventHandler handler) 
		{
			EventListener.RemoveHandler (handler, eventName);
		}
		
		public event NodeEventHandler Click
		{
			add { EventListener.AddHandler (value, "click"); }
			remove { EventListener.RemoveHandler (value, "click"); }
		}		

		public event NodeEventHandler DoubleClick
		{
			add { EventListener.AddHandler (value, "dblclick"); }
			remove { EventListener.RemoveHandler (value, "dblclick"); }
		}		
		public event NodeEventHandler KeyDown
		{
			add { EventListener.AddHandler (value, "keydown"); }
			remove { EventListener.RemoveHandler (value, "keydown"); }
		}		
		public event NodeEventHandler KeyPress
		{
			add { EventListener.AddHandler (value, "keypress"); }
			remove { EventListener.RemoveHandler (value, "keypress"); }
		}		
		public event NodeEventHandler KeyUp
		{
			add { EventListener.AddHandler (value, "keyup"); }
			remove { EventListener.RemoveHandler (value, "keyup"); }
		}		
		public event NodeEventHandler MouseDown
		{
			add { EventListener.AddHandler (value, "mousedown"); }
			remove { EventListener.RemoveHandler (value, "mousedown"); }
		}		
		public event NodeEventHandler MouseEnter
		{
			add { EventListener.AddHandler (value, "mouseenter"); }
			remove { EventListener.RemoveHandler (value, "mouseenter"); }
		}		
		public event NodeEventHandler MouseLeave
		{
			add { EventListener.AddHandler (value, "mouseout"); }
			remove { EventListener.RemoveHandler (value, "mouseout"); }
		}		
		public event NodeEventHandler MouseMove
		{
			add { EventListener.AddHandler (value, "mousemove"); }
			remove { EventListener.RemoveHandler (value, "mousemove"); }
		}		
		public event NodeEventHandler MouseOver
		{
			add { EventListener.AddHandler (value, "mouseover"); }
			remove { EventListener.RemoveHandler (value, "mouseover"); }
		}		
		public event NodeEventHandler MouseUp
		{
			add { EventListener.AddHandler (value, "mouseup"); }
			remove { EventListener.RemoveHandler (value, "mouseup"); }
		}		

		#endregion
	}
}
