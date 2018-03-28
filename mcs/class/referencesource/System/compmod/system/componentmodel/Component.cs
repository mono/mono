//------------------------------------------------------------------------------
// <copyright file="Component.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    using System;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>Provides the default implementation for the 
    ///    <see cref='System.ComponentModel.IComponent'/>
    ///    interface and enables object-sharing between applications.</para>
    /// </devdoc>
    [
        ComVisible(true),
        ClassInterface(ClassInterfaceType.AutoDispatch),
        DesignerCategory("Component")
    ]
    public class Component : MarshalByRefObject, IComponent {

        /// <devdoc>
        ///    <para>Static hask key for the Disposed event. This field is read-only.</para>
        /// </devdoc>
        private static readonly object EventDisposed = new object(); 

        private ISite site;
        private EventHandlerList events;

        ~Component() {
            Dispose(false);
        }

        /// <devdoc>
        ///     This property returns true if the component is in a mode that supports
        ///     raising events.  By default, components always support raising their events
        ///     and therefore this method always returns true.  You can override this method
        ///     in a deriving class and change it to return false when needed.  if the return
        ///     value of this method is false, the EventList collection returned by the Events
        ///     property will always return null for an event.  Events can still be added and
        ///     removed from the collection, but retrieving them through the collection's Item
        ///     property will always return null.
        /// </devdoc>
        protected virtual bool CanRaiseEvents
        {
            get
            {
                return true;
            }
        }

        /// <devdoc>
        ///     Internal API that allows the event handler list class to access the
        ///     CanRaiseEvents property.
        /// </devdoc>
        internal bool CanRaiseEventsInternal
        {
            get
            {
                return CanRaiseEvents;
            }
        }

        /// <devdoc>
        ///    <para>Adds a event handler to listen to the Disposed event on the component.</para>
        /// </devdoc>
        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Advanced)
        ]
        public event EventHandler Disposed {
            add {
                Events.AddHandler(EventDisposed, value);
            }
            remove {
                Events.RemoveHandler(EventDisposed, value);
            }
        }

        /// <devdoc>
        ///    <para>Gets the list of event handlers that are attached to this component.</para>
        /// </devdoc>
        protected EventHandlerList Events {
            get {
                if (events == null) {
                    events = new EventHandlerList(this);
                }
                return events;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the site of the <see cref='System.ComponentModel.Component'/>
        ///       .
        ///    </para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual ISite Site {
            get { return site;}
            set { site = value;}
        }

        /// <devdoc>
        ///    <para>
        ///       Disposes of the <see cref='System.ComponentModel.Component'/>
        ///       .
        ///    </para>
        /// </devdoc>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    
        /// <devdoc>
        ///    <para>
        ///    Disposes all the resources associated with this component.
        ///    If disposing is false then you must never touch any other
        ///    managed objects, as they may already be finalized. When
        ///    in this state you should dispose any native resources
        ///    that you have a reference to.
        ///    </para>
        ///    <para>
        ///    When disposing is true then you should dispose all data
        ///    and objects you have references to. The normal implementation
        ///    of this method would look something like:
        ///    </para>
        ///    <code>
        ///    public void Dispose() {
        ///        Dispose(true);
        ///        GC.SuppressFinalize(this);
        ///    }
        ///
        ///    protected virtual void Dispose(bool disposing) {
        ///        if (disposing) {
        ///            if (myobject != null) {
        ///                myobject.Dispose();
        ///                myobject = null;
        ///            }
        ///        }
        ///        if (myhandle != IntPtr.Zero) {
        ///            NativeMethods.Release(myhandle);
        ///            myhandle = IntPtr.Zero;
        ///        }
        ///    }
        ///
        ///    ~MyClass() {
        ///        Dispose(false);
        ///    }
        ///    </code>
        ///    <para>
        ///    For base classes, you should never override the Finalier (~Class in C#)
        ///    or the Dispose method that takes no arguments, rather you should
        ///    always override the Dispose method that takes a bool. 
        ///    </para>
        ///    <code>
        ///    protected override void Dispose(bool disposing) {
        ///        if (disposing) {
        ///            if (myobject != null) {
        ///                myobject.Dispose();
        ///                myobject = null;
        ///            }
        ///        }
        ///        if (myhandle != IntPtr.Zero) {
        ///            NativeMethods.Release(myhandle);
        ///            myhandle = IntPtr.Zero;
        ///        }
        ///        base.Dispose(disposing);
        ///    }
        ///    </code>
        /// </devdoc>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                lock(this) {
                    if (site != null && site.Container != null) {
                        site.Container.Remove(this);
                    }
                    if (events != null) {
                        EventHandler handler = (EventHandler)events[EventDisposed];
                        if (handler != null) handler(this, EventArgs.Empty);
                    }
                }
            }
        }

        // Returns the component's container.
        //
        /// <devdoc>
        ///    <para>
        ///       Returns the <see cref='System.ComponentModel.IContainer'/>
        ///       that contains the <see cref='System.ComponentModel.Component'/>
        ///       .
        ///    </para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public IContainer Container {
            get {
                ISite s = site;
                return s == null? null : s.Container;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Returns an object representing a service provided by
        ///       the <see cref='System.ComponentModel.Component'/>
        ///       .
        ///    </para>
        /// </devdoc>
        protected virtual object GetService(Type service) {
            ISite s = site;
            return((s== null) ? null : s.GetService(service));
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether the <see cref='System.ComponentModel.Component'/>
        ///       is currently in design mode.
        ///    </para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        protected bool DesignMode {
            get {
                ISite s = site;
                return(s == null) ? false : s.DesignMode;
            }
        }

        /// <internalonly/>
        /// <devdoc>
        ///    <para>
        ///       Returns a <see cref='System.String'/> containing the name of the <see cref='System.ComponentModel.Component'/> , if any. This method should not be
        ///       overridden. For
        ///       internal use only.
        ///    </para>
        /// </devdoc>
        public override String ToString() {
            ISite s = site;

            if (s != null)
                return s.Name + " [" + GetType().FullName + "]";
            else
                return GetType().FullName;
        }
    }
}
