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
		Dictionary <object, Delegate> handlers;
#else
		Hashtable handlers;
#endif
		Delegate null_entry;

		public EventHandlerList ()
		{
		}

		public Delegate this [object key] {
			get {
				if (key == null)
					return null_entry;
				return FindEntry (key);
			}

			set {
				AddHandler (key, value);
			}
		}

		public void AddHandler (object key, Delegate value)
		{
			if (key == null) {
				null_entry = Delegate.Combine (null_entry, value);
				return;
			}

			Delegate prev = FindEntry (key);
			if (prev == null) {
				if (handlers == null) {
#if NET_2_0
					handlers = new Dictionary <object, Delegate> ();
#else
					handlers = new Hashtable ();
#endif
				}
			}
			handlers [key] = Delegate.Combine (prev, value);
		}

#if NET_2_0
		public void AddHandlers (EventHandlerList listToAddFrom)
		{
			if (listToAddFrom == null)
				return;

			foreach (KeyValuePair <object, Delegate> kvp in listToAddFrom.handlers)
				AddHandler (kvp.Key, kvp.Value);
		}
#endif

		public void RemoveHandler (object key, Delegate value)
		{
			if (key == null) {
				null_entry = Delegate.Remove (null_entry, value);
				return;
			}

			Delegate entry = FindEntry (key);
			if (entry == null)
				return;

			handlers [key] = Delegate.Remove (entry, value);
		}

		public void Dispose ()
		{
			handlers = null;
		}
		
		private Delegate FindEntry (object key)
		{
			if (handlers == null)
				return null;
#if NET_2_0
			Delegate entry;
			if (handlers.TryGetValue (key, out entry))
				return entry;

			return null;
#else
			return handlers [key] as Delegate;
#endif
		}
	}
}

