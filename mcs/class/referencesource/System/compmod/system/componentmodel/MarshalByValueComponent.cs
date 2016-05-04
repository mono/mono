//------------------------------------------------------------------------------
// <copyright file="MarshalByValueComponent.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {

    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;

    /// <devdoc>
    /// <para>Provides the base implementation for <see cref='System.ComponentModel.IComponent'/>,
    ///    which is the base class for all components in Win Forms.</para>
    /// </devdoc>
    [
        ComVisible(true),
        Designer("System.Windows.Forms.Design.ComponentDocumentDesigner, " + AssemblyRef.SystemDesign, typeof(IRootDesigner)),
        DesignerCategory("Component"),
        TypeConverter(typeof(ComponentConverter))
    ]
    public class MarshalByValueComponent : IComponent, IServiceProvider {

        /// <devdoc>
        ///    <para>Static hask key for the Disposed event. This field is read-only.</para>
        /// </devdoc>
        private static readonly object EventDisposed = new object(); 

        private ISite site;
        private EventHandlerList events;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.MarshalByValueComponent'/> class.</para>
        /// </devdoc>
        public MarshalByValueComponent() {
        }

        ~MarshalByValueComponent() {
            Dispose(false);
        }

        /// <devdoc>
        ///    <para>Adds a event handler to listen to the Disposed event on the component.</para>
        /// </devdoc>
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
                    events = new EventHandlerList();
                }
                return events;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the site of the component.</para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual ISite Site {
            get { return site;}
            set { site = value;}
        }

        /// <devdoc>
        ///    <para>Disposes of the resources (other than memory) used by the component.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
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

        /// <devdoc>
        ///    <para>Gets the container for the component.</para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IContainer Container {
            get {
                ISite s = site;
                return s == null ? null : s.Container;
            }
        }

        /// <devdoc>
        /// <para>Gets the implementer of the <see cref='System.IServiceProvider'/>.</para>
        /// </devdoc>
        public virtual object GetService(Type service) {
            return((site==null)? null : site.GetService(service));
        }


        /// <devdoc>
        ///    <para>Gets a value indicating whether the component is currently in design mode.</para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool DesignMode {
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
