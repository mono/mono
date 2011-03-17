//
// System.Collections.Generic.LinkedListNode
//
// Author:
//    David Waite
//
// (C) 2005 David Waite (mass@akuma.org)
//

//
// Copyright (C) 2005 David Waite
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

using System;
using System.Runtime.InteropServices;

namespace System.Collections.Generic
{
	[ComVisible (false)]
	public sealed class LinkedListNode <T>
	{
		T item;
		LinkedList <T> container;

		internal LinkedListNode <T> forward, back;
		
		public LinkedListNode (T value)
		{
			item = value;
		}

		internal LinkedListNode (LinkedList <T> list, T value)
		{
			container = list;
			item = value;
			this.back = this.forward = this;
		}

		internal LinkedListNode (LinkedList <T> list, T value, LinkedListNode <T> previousNode, LinkedListNode <T> nextNode)
		{
			container = list;
			item = value;
			this.back = previousNode;
			this.forward = nextNode;
			previousNode.forward = this;
			nextNode.back = this;
		}
		
		internal void Detach ()
		{
			back.forward = forward;
			forward.back = back;

			forward = back = null;
			container = null;
		}
		
		internal void SelfReference (LinkedList <T> list)
		{
			forward = this;
			back = this;
			container = list;
		}
		
		internal void InsertBetween (LinkedListNode <T> previousNode, LinkedListNode <T> nextNode, LinkedList <T> list)
		{
			previousNode.forward = this;
			nextNode.back = this;
			this.forward = nextNode;
			this.back = previousNode;
			this.container = list;
		} 
				
		public LinkedList <T> List {
			get { return container; }
		}
		
		public LinkedListNode <T> Next {
			get { return (container != null && forward != container.first) ? forward : null; }
		}

		public LinkedListNode <T> Previous {
			get { return (container != null && this != container.first) ? back : null ; }
		}

		public T Value { 
			get { return item; }
			set { item = value; }
		}
	}
}
