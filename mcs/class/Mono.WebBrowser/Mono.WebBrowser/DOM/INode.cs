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
		IElement	InsertBefore (INode child, INode refChild);
		INode 		LastChild { get; }
		string 		LocalName { get; }
		INode 		Next { get; }
		IDocument 	Owner { get; }
		INode		Parent {get;}
		INode 		Previous { get; }
		NodeType 	Type { get;}
		string 		Value {get;} 
		
		void FireEvent	(string eventName);
		int 		GetHashCode ();
		
		void AttachEventHandler (string eventName, EventHandler handler);
		void DetachEventHandler (string eventName, EventHandler handler);
		
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
