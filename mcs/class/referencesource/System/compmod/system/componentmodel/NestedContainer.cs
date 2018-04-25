//------------------------------------------------------------------------------
// <copyright file="NestedContainer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {

    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    
    /// <devdoc>
    ///     A nested container is a container that is owned by another component.  Nested
    ///     containers can be found by querying a component site's services for NestedConainter.
    ///     Nested containers are a useful tool to establish owner relationships among components.
    ///     All components within a nested container are named with the owning component's name
    ///     as a prefix.
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public class NestedContainer : Container, INestedContainer {

        private IComponent _owner;

        /// <devdoc>
        ///     Creates a new NestedContainer.
        /// </devdoc>
        public NestedContainer(IComponent owner) {
            if (owner == null) {
                throw new ArgumentNullException("owner");
            }
            _owner = owner;
            _owner.Disposed += new EventHandler(OnOwnerDisposed);
        }

        /// <devdoc>
        ///     The component that owns this nested container.
        /// </devdoc>
        public IComponent Owner {
            get {
                return _owner;
            }
        }

        /// <devdoc>
        ///     Retrieves the name of the owning component.  This may be overridden to
        ///     provide a custom owner name.  The default searches the owner's site for
        ///     INestedSite and calls FullName, or ISite.Name if there is no nested site.
        ///     If neither is available, this returns null.
        /// </devdoc>
        protected virtual string OwnerName {
            get {
                string ownerName = null;
                if (_owner != null && _owner.Site != null) {
                    INestedSite nestedOwnerSite = _owner.Site as INestedSite;
                    if (nestedOwnerSite != null) {
                        ownerName = nestedOwnerSite.FullName;
                    }
                    else {
                        ownerName = _owner.Site.Name;
                    }
                }

                return ownerName;
            }
        }

        /// <devdoc>
        ///     Creates a site for the component within the container.
        /// </devdoc>
        protected override ISite CreateSite(IComponent component, string name) {
            if (component == null) {
                throw new ArgumentNullException("component");
            }
            return new Site(component, this, name);
        }

        /// <devdoc>
        ///    Override of Container's dispose.
        /// </devdoc>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                _owner.Disposed -= new EventHandler(OnOwnerDisposed);
            }
            base.Dispose(disposing);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override object GetService(Type service) {
            if (service == typeof(INestedContainer)) {
                return this;
            }
            else {
                return base.GetService(service);
            }
        }

        /// <devdoc>
        ///     Called when our owning component is destroyed.
        /// </devdoc>
        private void OnOwnerDisposed(object sender, EventArgs e) {
            Dispose();
        }

        /// <devdoc>
        ///     Simple site implementation.  We do some special processing to name the site, but 
        ///     that's about it.
        /// </devdoc>
        private class Site : INestedSite
        {
            private IComponent component;
            private NestedContainer container;
            private string name;

            internal Site(IComponent component, NestedContainer container, string name) {
                this.component = component;
                this.container = container;
                this.name = name;
            }

            // The component sited by this component site.
            public IComponent Component {
                get {
                    return component;
                }
            }

            // The container in which the component is sited.
            public IContainer Container {
                get {
                    return container;
                }
            }

            public Object GetService(Type service) {

                return((service == typeof(ISite)) ? this : container.GetService(service));
            }

            // Indicates whether the component is in design mode.
            public bool DesignMode {
                get {
                    IComponent owner = container.Owner;
                    if (owner != null && owner.Site != null) {
                        return owner.Site.DesignMode;
                    }
                    return false;
                }
            }

            public string FullName {
                get {
                    if (name != null) {
                        string ownerName = container.OwnerName;
                        string childName = name;
                        if (ownerName != null) {
                            childName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", ownerName, childName);
                        }

                        return childName;
                    }

                    return name;
                }
            }

            // The name of the component.
            //
            public String Name 
            {
                get { 
                    return name;
                }
                set { 
                    if (value == null || name == null || !value.Equals(name)) {
                        container.ValidateName(component, value);
                        name = value;
                    }
                }
            }
        }
    }
}

