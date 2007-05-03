//
// System.Runtime.Remoting.Services.TrackingServices.cs
//
// Author:
// 	Jaime Anguiano Olarra (jaime@gnome.org)
//	Patrik Torstensson
//
// (C) 2002, Jaime Anguiano Olarra
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Remoting;

#if NET_2_0
using System.Runtime.InteropServices;
#endif

namespace System.Runtime.Remoting.Services {
#if NET_2_0
	[ComVisible (true)]
#endif
	public class TrackingServices {
		static ArrayList _handlers = new ArrayList();

		public TrackingServices () {
		}

		public static void RegisterTrackingHandler (ITrackingHandler handler) {
			if (null == handler)
				throw new ArgumentNullException("handler");

			lock (_handlers.SyncRoot) {
				if (-1 != _handlers.IndexOf(handler))
					throw new RemotingException("handler already registered");

				_handlers.Add(handler);
			}
		}

		public static void UnregisterTrackingHandler (ITrackingHandler handler) {
			if (null == handler)
				throw new ArgumentNullException("handler");

			lock (_handlers.SyncRoot) {
				int idx = _handlers.IndexOf(handler);
				if (idx == -1)
					throw new RemotingException("handler is not registered");

				_handlers.RemoveAt(idx);
			}
		}
    
		public static ITrackingHandler[] RegisteredHandlers {
			get {
				lock (_handlers.SyncRoot) {
					if (_handlers.Count == 0)
						return new ITrackingHandler[0];


					return (ITrackingHandler[]) _handlers.ToArray (typeof(ITrackingHandler));
				}
			}
		}

		internal static void NotifyMarshaledObject(Object obj, ObjRef or)
		{
			ITrackingHandler[] handlers;
			
			lock (_handlers.SyncRoot) {
				if (_handlers.Count == 0) return;
				handlers = (ITrackingHandler[]) _handlers.ToArray (typeof(ITrackingHandler));
			}
			
			for(int i = 0; i < handlers.Length; i++) {
				handlers[i].MarshaledObject (obj, or);
			}
		}
    
		internal static void NotifyUnmarshaledObject(Object obj, ObjRef or)
		{
			ITrackingHandler[] handlers;
			
			lock (_handlers.SyncRoot) {
				if (_handlers.Count == 0) return;
				handlers = (ITrackingHandler[]) _handlers.ToArray (typeof(ITrackingHandler));
			}
			
			for(int i = 0; i < handlers.Length; i++) {
				handlers[i].UnmarshaledObject (obj, or);
			}
		}

		internal static void NotifyDisconnectedObject(Object obj)
		{
			ITrackingHandler[] handlers;
			
			lock (_handlers.SyncRoot) {
				if (_handlers.Count == 0) return;
				handlers = (ITrackingHandler[]) _handlers.ToArray (typeof(ITrackingHandler));
			}
			
			for(int i = 0; i < handlers.Length; i++) {
				handlers[i].DisconnectedObject (obj);
			}
		}
	}
}

