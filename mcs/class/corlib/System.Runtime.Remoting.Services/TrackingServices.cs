//
// System.Runtime.Remoting.Services.TrackingServices.cs
//
// Author:
// 	Jaime Anguiano Olarra (jaime@gnome.org)
//	Patrik Torstensson
//
// (C) 2002, Jaime Anguiano Olarra
//

using System;
using System.Collections;
using System.Runtime.Remoting;

namespace System.Runtime.Remoting.Services {
	public class TrackingServices {
		static ArrayList _handlers = new ArrayList();

		public TrackingServices () {
		}

		public static void RegisterTrackingHandler (ITrackingHandler handler) {
			if (null == handler)
				throw new ArgumentNullException("handler");

			lock (typeof(TrackingServices)) {
				if (-1 != _handlers.IndexOf(handler))
					throw new RemotingException("handler already registered");

				_handlers.Add(handler);
			}
		}

		public static void UnregisterTrackingHandler (ITrackingHandler handler) {
			if (null == handler)
				throw new ArgumentNullException("handler");

			lock (typeof(TrackingServices)) {
				int idx = _handlers.IndexOf(handler);
				if (idx == -1)
					throw new RemotingException("handler is not registered");

				_handlers.RemoveAt(idx);
			}
		}
    
		public static ITrackingHandler[] RegisteredHandlers {
			get {
				lock (typeof(TrackingServices)) {
					if (_handlers.Count == 0)
						return new ITrackingHandler[0];


					return (ITrackingHandler[]) _handlers.ToArray();
				}
			}
		}

		internal static void NotifyMarshaledObject(Object obj, ObjRef or) {
			ITrackingHandler[] handlers = RegisteredHandlers;
			for(int i = 0; i < handlers.Length; i++) {
				handlers[i].MarshaledObject (obj, or);
			}
		}
    
		internal static void NotifyUnmarshaledObject(Object obj, ObjRef or) {
			ITrackingHandler[] handlers = RegisteredHandlers;
			for(int i = 0; i < handlers.Length; i++) {
				handlers[i].UnmarshaledObject (obj, or);
			}
		}

		internal static void NotifyDisconnectedObject(Object obj) {
			ITrackingHandler[] handlers = RegisteredHandlers;
			for(int i = 0; i < handlers.Length; i++) {
				handlers[i].DisconnectedObject (obj);
			}
		}
	}
}

