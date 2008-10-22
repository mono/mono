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

namespace Mono.WebBrowser.DOM
{
	public interface INode
	{
		IAttributeCollection Attributes { get; }
		INodeList 	ChildNodes { get; }
		INode 		FirstChild { get; }
		INode		InsertBefore (INode newChild, INode refChild);
		INode		ReplaceChild (INode newChild, INode oldChild);
		INode		RemoveChild (INode child);
		INode		AppendChild (INode child);
		INode 		LastChild { get; }
		string 		LocalName { get; }
		INode 		Next { get; }
		IDocument 	Owner { get; }
		INode		Parent {get;}
		INode 		Previous { get; }
		NodeType 	Type { get;}
		string 		Value {get; set;}
		IntPtr		AccessibleObject {get;}
		
		void FireEvent	(string eventName);
		int 		GetHashCode ();
		bool 				Equals (object obj);
		
		void AttachEventHandler (string eventName, EventHandler handler);
		void DetachEventHandler (string eventName, EventHandler handler);
		
		/// <summary>
		/// Attach a generic handler for events. The delegate needs to conform to the EventHandler signature, 
		/// i.e., be in the form (object sender, EventArgs e)
		/// </summary>
		/// <param name="eventName">
		/// A <see cref="System.String"/> with the name of the event to listen for.
		/// </param>
		/// <param name="handler">
		/// A <see cref="System.Delegate"/> conforming to the EventHandler signature. 
		/// It will throw an exception if the delegate is not of the format (object sender, EventArgs e).
		/// </param>
		void AttachEventHandler (string eventName, System.Delegate handler);
		void DetachEventHandler (string eventName, System.Delegate handler);

		event NodeEventHandler Click;
		event NodeEventHandler DoubleClick;
		event NodeEventHandler KeyDown;
		event NodeEventHandler KeyPress;
		event NodeEventHandler KeyUp;
		event NodeEventHandler MouseDown;
		event NodeEventHandler MouseEnter;
		event NodeEventHandler MouseLeave;
		event NodeEventHandler MouseMove;
		event NodeEventHandler MouseOver;
		event NodeEventHandler MouseUp;
		event NodeEventHandler OnFocus;
		event NodeEventHandler OnBlur;
	}
	
	public enum NodeType
	{
		Element       = 1,
		Attribute     = 2,
		Text          = 3,
		CDataSection  = 4,
		EntityReference = 5,
		Entity       = 6,
		ProcessingInstruction = 7,
		Comment       = 8,
		Document      = 9,
		DocumentType = 10,
		DocumentFragment = 11,
		Notation      = 12
	}
}
