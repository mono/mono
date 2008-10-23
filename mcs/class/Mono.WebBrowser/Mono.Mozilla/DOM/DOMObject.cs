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
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections;
using Mono.WebBrowser;

namespace Mono.Mozilla.DOM
{
	internal class DOMObject : IDisposable
	{
		private EventHandlerList event_handlers;		
		protected WebBrowser control;
		internal HandleRef storage;
		protected bool disposed = false;
		protected Hashtable resources;

		internal DOMObject (WebBrowser control)
		{
			this.control = control;
			IntPtr p = Base.StringInit ();
			storage = new HandleRef (this, p);
			resources = new Hashtable ();
			event_handlers = null;
		}

		~DOMObject ()
		{
			Dispose (false);
		}

		#region IDisposable Members

		protected virtual void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					Base.StringFinish (storage);
				}
				disposed = true;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		#endregion

		protected EventHandlerList Events {
			get {
				// Note: space vs. time tradeoff
				// We create the object here if it's never be accessed before.  This potentially 
				// saves space. However, we must check each time the propery is accessed to
				// determine whether we need to create the object, which increases overhead.
				// We could put the creation in the contructor, but that would waste space
				// if it were never used.  However, accessing this property would be faster.
				if (null == event_handlers)
					event_handlers = new EventHandlerList();

				return event_handlers;
			}
		}
		
#region Private
		internal Mono.WebBrowser.DOM.INode GetTypedNode (nsIDOMNode obj) 
		{
			if (obj == null)
				return null;
			obj.getLocalName (storage);
			ushort type;
			obj.getNodeType (out type);
			switch (type) {
				case (ushort)NodeType.Element:
#if DEBUG					
					Console.Write (Base.StringGet (storage) + ":Getting typed object from NodeType.Element:");
#endif
					if (obj is Mono.Mozilla.nsIDOMHTMLBodyElement) {
#if DEBUG					
						Console.WriteLine ("HTMLElement-nsIDOMHTMLBodyElement");
#endif
						return new HTMLElement (control, obj as nsIDOMHTMLBodyElement);
					}
					else if (obj is Mono.Mozilla.nsIDOMHTMLStyleElement) {
#if DEBUG					
						Console.WriteLine ("HTMLElement-nsIDOMHTMLStyleElement");
#endif
						return new HTMLElement (control, obj as nsIDOMHTMLStyleElement);
					}
					else if (obj is nsIDOMHTMLElement) {
#if DEBUG					
						Console.WriteLine ("HTMLElement-nsIDOMHTMLElement");
#endif
						return new HTMLElement (control, obj as nsIDOMHTMLElement);
					}
#if DEBUG					
					Console.WriteLine ("HTMLElement-nsIDOMHTMLElement");
#endif
					return new Element (control, obj as nsIDOMElement);
					break;
				case (ushort)NodeType.Attribute:
					return new Attribute (control, obj as nsIDOMAttr);
					break;
				case (ushort)NodeType.Document:
					if (obj is nsIDOMHTMLDocument)
						return new Document (control, obj as nsIDOMHTMLDocument);
					return new Document (control, obj as nsIDOMDocument);
					break;
				case (ushort)NodeType.Text:
				case (ushort)NodeType.CDataSection:
				case (ushort)NodeType.EntityReference:
				case (ushort)NodeType.Entity:
				case (ushort)NodeType.ProcessingInstruction:
				case (ushort)NodeType.Comment:
				case (ushort)NodeType.DocumentType:
				case (ushort)NodeType.DocumentFragment:
				case (ushort)NodeType.Notation:				
				default:
					return new Node (control, obj);
					break;
			}
		}
#endregion
		

	}
}
