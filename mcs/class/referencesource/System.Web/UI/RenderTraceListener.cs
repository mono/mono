//------------------------------------------------------------------------------
// <copyright file="RenderTraceListener.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Abstract base class for an object that wants to be notified as controls
    /// are rendering during a page request.
    /// </summary>
    public abstract class RenderTraceListener {
        private static readonly RenderTraceListener _nullListener = new NullRenderTraceListener();

        private static List<Func<RenderTraceListener>> _factories;

        /// <summary>
        /// Functions added to this list are called to instantiate RenderTraceListeners
        /// for each request.
        /// </summary>
        public static IList<Func<RenderTraceListener>> ListenerFactories {
            get {
                if (_factories == null) {
                    _factories = new List<Func<RenderTraceListener>>();
                }
                return _factories;
            }
        }

        internal static RenderTraceListener CurrentListeners {
            get {
                if (_factories != null && HttpContext.Current != null) {
                    RenderTraceListener listener = HttpContext.Current.Items[typeof(RenderTraceListener)] as RenderTraceListener;
                    if (listener == null) {
                        listener = CreateListener(HttpContext.Current);
                        HttpContext.Current.Items[typeof(RenderTraceListener)] = listener;
                    }
                    return listener;
                }

                return _nullListener;
            }
        }

        private static RenderTraceListener CreateListener(HttpContext context) {
            List<RenderTraceListener> listeners = new List<RenderTraceListener>();

            foreach (Func<RenderTraceListener> factory in _factories) {
                RenderTraceListener listener = factory();

                if (listener != null) {
                    listeners.Add(listener);
                }
            }

            RenderTraceListenerList list = new RenderTraceListenerList(listeners);

            list.Initialize(context);

            return list;
        }

        /// <summary>
        /// Called to initialize the listener.
        /// </summary>
        public virtual void Initialize(HttpContext context) {
        }

        /// <summary>
        /// Called to associate a data object with a Control or other object
        /// that will be rendered.
        /// </summary>
        public virtual void SetTraceData(object tracedObject, object traceDataKey, object traceDataValue) {
        }

        /// <summary>
        /// Called to associate the trace data from one object with another object.
        /// </summary>
        public virtual void ShareTraceData(object source, object destination) {
        }

        /// <summary>
        /// Called when an object is about to be rendered.
        /// </summary>
        public virtual void BeginRendering(TextWriter writer, object renderedObject) {
        }

        /// <summary>
        /// Called when an object is finished rendering.
        /// </summary>
        public virtual void EndRendering(TextWriter writer, object renderedObject) {
        }

        private sealed class NullRenderTraceListener : RenderTraceListener {
        }

        private sealed class RenderTraceListenerList : RenderTraceListener {
            private readonly List<RenderTraceListener> _listeners;

            internal RenderTraceListenerList(List<RenderTraceListener> listeners) {
                _listeners = listeners;
            }

            public override void Initialize(HttpContext context) {
                foreach (RenderTraceListener listener in _listeners) {
                    listener.Initialize(context);
                }
            }

            public override void SetTraceData(object tracedObject, object traceDataKey, object traceDataValue) {
                foreach (RenderTraceListener listener in _listeners) {
                    listener.SetTraceData(tracedObject, traceDataKey, traceDataValue);
                }
            }

            public override void ShareTraceData(object source, object destination) {
                foreach (RenderTraceListener listener in _listeners) {
                    listener.ShareTraceData(source, destination);
                }
            }

            public override void BeginRendering(TextWriter writer, object renderedObject) {
                foreach (RenderTraceListener listener in _listeners) {
                    listener.BeginRendering(writer, renderedObject);
                }
            }

            public override void EndRendering(TextWriter writer, object renderedObject) {
                // Call EndRendering in the reverse order from BeginRendering,
                // so that listeners are called in a nested fashion
                for (int i = _listeners.Count - 1; i >= 0; i--) {
                    _listeners[i].EndRendering(writer, renderedObject);
                }
            }
        }
    }
}
