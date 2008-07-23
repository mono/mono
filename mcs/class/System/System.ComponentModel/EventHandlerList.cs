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

#if NET_2_0
using System.Collections.Generic;
#endif

namespace System.ComponentModel {

	// <summary>
	//   List of Event delegates.
	// </summary>
	//
	// <remarks>
	//   Longer description
	// </remarks>
	public sealed class EventHandlerList : IDisposable
	{
#if NET_2_0
		Dictionary <object, HandlerEntry> handlers;
#else
		Hashtable handlers;
#endif
		HandlerEntry nullEntry;
		
		public EventHandlerList ()
		{
		}

		public Delegate this [object key] {
			get {
				HandlerEntry entry = FindEntry (key);
				return entry == null ? null : entry.value;
			}

			set {
				AddHandler (key, value);
			}
		}

		public void AddHandler (object key, Delegate value)
		{
			HandlerEntry entry = FindEntry (key);
			if (entry == null) {
				if (handlers == null) {
#if NET_2_0
					handlers = new Dictionary <object, HandlerEntry> ();
#else
					handlers = new Hashtable ();
#endif
				}

				if (key != null)
					handlers.Add (key, new HandlerEntry (value));
				else
					nullEntry = new HandlerEntry (value);

				return;
			}
			entry.value = Delegate.Combine (entry.value, value);
		}

#if NET_2_0
		public void AddHandlers (EventHandlerList listToAddFrom)
		{
			if (listToAddFrom == null)
				return;

			foreach (KeyValuePair <object, HandlerEntry> kvp in listToAddFrom.handlers)
				AddHandler (kvp.Key, kvp.Value.value);
		}
#endif

		public void RemoveHandler (object key, Delegate value)
		{
			HandlerEntry entry = FindEntry (key);
			if (entry == null)
				return;

			entry.value = Delegate.Remove (entry.value, value);
		}

		public void Dispose ()
		{
			handlers = null;
		}
		
		private HandlerEntry FindEntry (object key)
		{
			if (key == null)
				return nullEntry;
			
			if (handlers == null)
				return null;

#if NET_2_0
			HandlerEntry entry;
			if (handlers.TryGetValue (key, out entry))
				return entry;

			return null;
#else
			return handlers [key] as HandlerEntry;
#endif
		}

		[Serializable]
		sealed class HandlerEntry
		{
			public Delegate value;
			
			public HandlerEntry (Delegate value)
			{
				this.value = value;
			}
		}
	}
}
