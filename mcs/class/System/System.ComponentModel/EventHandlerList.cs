//
// System.ComponentModel.EventHandlerList.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
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
using System.Collections;

namespace System.ComponentModel {

	// <summary>
	//   List of Event delegates.
	// </summary>
	//
	// <remarks>
	//   Longer description
	// </remarks>
	public sealed class EventHandlerList : IDisposable {
		public EventHandlerList ()
		{
			head = null;
		}

		public Delegate this [object key] {
			get {
				ListNode entry = FindEntry (key);
				return entry == null ? null : entry.value;
			}

			set {
				AddHandler (key, value);
			}
		}

		public void AddHandler (object key, Delegate value)
		{
			ListNode entry = FindEntry (key);
			if (entry == null) {
				head = new ListNode (key, value, head);
				return;
			}
			entry.value = Delegate.Combine (entry.value, value);
		}

#if NET_2_0
		public void AddHandlers (EventHandlerList listToAddFrom)
		{
			if (listToAddFrom == null) {
				return;
			}

			for (ListNode entry = listToAddFrom.head; entry != null; entry = entry.next) {
				AddHandler (entry.key, entry.value);
			}
		}
#endif

		public void RemoveHandler (object key, Delegate value)
		{
			ListNode entry = FindEntry (key);
			if (entry == null)
				return;

			entry.value = Delegate.Remove (entry.value, value);
		}

		public void Dispose ()
		{
			head = null;
		}
		private ListNode FindEntry (object key)
		{
			for (ListNode entry = head; entry != null; entry = entry.next)
				if (key == entry.key)
					return entry;
			return null;
		}

		[Serializable]
		private class ListNode
		{
			public object key;
			public Delegate value;
			public ListNode next;
			public ListNode (object key, Delegate value, ListNode next)
			{
				this.key = key;
				this.value = value;
				this.next = next;
			}
		}

		private ListNode head;

	}
	
}
