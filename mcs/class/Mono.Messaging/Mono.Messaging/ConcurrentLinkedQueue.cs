//
// Mono.Messaging
//
// Authors:
//		Michael Barker (mike@middlesoft.co.uk)
//
// (C) 2008 Michael Barker
//

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
using System.Threading;

namespace Mono.Messaging {
	
	public class ConcurrentLinkedQueue<T>
	{
		private Node<T> head;
		private Node<T> tail;
		
		public ConcurrentLinkedQueue ()
		{
			Node<T> node = new Node<T> (default (T));
			head = node;
			tail = node;
		}
		
		public void Enqueue (T context)
		{
			Console.WriteLine ("Insert: " + context);
			Node<T> newNode = new Node<T>(context);
			
			while (true) {
				Node<T> tail = this.tail;
				Node<T> next = tail.Next;

				if (tail == this.tail) {
					if (null == next) {
						if (tail.CAS (newNode, next))
							break;
						
					} else {
						Interlocked.CompareExchange<Node<T>> (ref this.tail, next, tail);
					}
				}
			}
		}
		
		public T Dequeue ()
		{
			while (true) {
				Node<T> head = this.head;
				Node<T> tail = this.tail;
				Node<T> next = head.Next;
				
				if (head == this.head) {
					if (head == tail) {
						if (null == next)
							return default(T);
						
						Interlocked.CompareExchange<Node<T>> (ref this.tail, next, tail);
						
					} else {
						T t = next.Value;
						
						if (Interlocked.CompareExchange(ref this.head, next, head) == head)
							return t;
					}
				}
			}
		}
		
		public override String ToString ()
		{
			return "Head: " + head;
		}
		
		internal class Node<N>
		{
			private readonly N context;
			private Node<N> next = null;
			
			public Node (N context)
			{
				this.context = context;
			}
			
			public Node<N> Next {
				get { return next; }
			}
			
			public N Value {
				get { return context; }
			}
			
			public bool CAS (Node<N> newNode, Node<N> oldNode)
			{
				return Interlocked.CompareExchange (ref next, newNode, oldNode) == oldNode;
			}
			
			public override String ToString ()
			{
				return "context: " + context + ", Next: " + next;
			}
		}
	}
}
