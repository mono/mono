    //------------------------------------------------------------------------------
// <copyright file="Container.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    
    /// <devdoc>
    ///    <para>
    ///       Encapsulates
    ///       zero or more components.
    ///    </para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public class Container : IContainer
    {
        private ISite[] sites;
        private int siteCount;
        private ComponentCollection components;
        private ContainerFilterService filter;
        private bool checkedFilter;

        private object syncObj = new Object();

        ~Container() {
            Dispose(false);
        }

        // Adds a component to the container.
        /// <devdoc>
        ///    <para>
        ///       Adds the specified component to the <see cref='System.ComponentModel.Container'/>
        ///       . The component is unnamed.
        ///    </para>
        /// </devdoc>
        public virtual void Add(IComponent component) {
            Add(component, null);
        }

        // Adds a component to the container.
        /// <devdoc>
        ///    <para>
        ///       Adds the specified component to the <see cref='System.ComponentModel.Container'/> and assigns a name to
        ///       it.
        ///    </para>
        /// </devdoc>
        public virtual void Add(IComponent component, String name) {
            lock (syncObj) {
                if (component == null) {
                    return;
                }
                
                ISite site = component.Site;
                
                if (site != null && site.Container == this) {
                    return;
                }
                
                if (sites == null) {
                    sites = new ISite[4];
                }
                else {
                    // Validate that new components
                    // have either a null name or a unique one.
                    //
                    ValidateName(component, name);
                
                    if (sites.Length == siteCount) {
                        ISite[] newSites = new ISite[siteCount * 2];
                        Array.Copy(sites, 0, newSites, 0, siteCount);
                        sites = newSites;
                    }
                }
                
                if (site != null) {
                    site.Container.Remove(component);
                }

                ISite newSite = CreateSite(component, name);
                sites[siteCount++] = newSite;
                component.Site = newSite;
                components = null;
            }
        }

        // Creates a site for the component within the container.
        /// <devdoc>
        /// <para>Creates a Site <see cref='System.ComponentModel.ISite'/> for the given <see cref='System.ComponentModel.IComponent'/>
        /// and assigns the given name to the site.</para>
        /// </devdoc>
        protected virtual ISite CreateSite(IComponent component, string name) {
            return new Site(component, this, name);
        }

        // Disposes of the container.  A call to the Dispose method indicates that
        // the user of the container has no further need for it.
        //
        // The implementation of Dispose must:
        //
        // (1) Remove any references the container is holding to other components.
        //     This is typically accomplished by assigning null to any fields that
        //     contain references to other components.
        //
        // (2) Release any system resources that are associated with the container,
        //     such as file handles, window handles, or database connections.
        //
        // (3) Dispose of child components by calling the Dispose method of each.
        //
        // Ideally, a call to Dispose will revert a container to the state it was
        // in immediately after it was created. However, this is not a requirement.
        // Following a call to its Dispose method, a container is permitted to raise
        // exceptions for operations that cannot meaningfully be performed.
        //
        /// <devdoc>
        ///    <para>
        ///       Disposes of the <see cref='System.ComponentModel.Container'/>
        ///       .
        ///    </para>
        /// </devdoc>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                lock (syncObj) {
                    while (siteCount > 0) {
                        ISite site = sites[--siteCount];
                        site.Component.Site = null;
                        site.Component.Dispose();
                    }
                    sites = null;
                    components = null;
                }
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual object GetService(Type service) {
            return((service == typeof(IContainer)) ? this : null);
        }

        // The components in the container.
        /// <devdoc>
        ///    <para>
        ///       Gets all the components in the <see cref='System.ComponentModel.Container'/>
        ///       .
        ///    </para>
        /// </devdoc>
        public virtual ComponentCollection Components {
            get {
                lock (syncObj) {
                    if (components == null) {
                        IComponent[] result = new IComponent[siteCount];
                        for (int i = 0; i < siteCount; i++) {
                            result[i] = sites[i].Component;
                        }
                        components = new ComponentCollection(result);

                        // At each component add, if we don't yet have a filter, look for one.
                        // Components may add filters.
                        if (filter == null && checkedFilter) {
                            checkedFilter = false;
                        }
                    }

                    if (!checkedFilter) {
                        filter = GetService(typeof(ContainerFilterService)) as ContainerFilterService;
                        checkedFilter = true;
                    }

                    if (filter != null) {
                        ComponentCollection filteredComponents = filter.FilterComponents(components);
                        Debug.Assert(filteredComponents != null, "Incorrect ContainerFilterService implementation.");
                        if (filteredComponents != null) {
                            components = filteredComponents;
                        }
                    }

                    return components;
                }
            }
        }

        // Removes a component from the container.
        /// <devdoc>
        ///    <para>
        ///       Removes a component from the <see cref='System.ComponentModel.Container'/>
        ///       .
        ///    </para>
        /// </devdoc>
        public virtual void Remove(IComponent component) {
            Remove(component, false);
        }

        private void Remove(IComponent component, bool preserveSite) {
             lock (syncObj) {
                if (component == null)
                    return;
                ISite site = component.Site;
                if (site == null || site.Container != this)
                    return;
                if (!preserveSite) 
                    component.Site = null;
                for (int i = 0; i < siteCount; i++) {
                    if (sites[i] == site) {
                        siteCount--;
                        Array.Copy(sites, i + 1, sites, i, siteCount - i);
                        sites[siteCount] = null;
                        components = null;
                        break;
                    }
                }
            }
        }

        protected void RemoveWithoutUnsiting(IComponent component) {
            Remove(component, true);
        }

        /// <devdoc>
        ///     Validates that the given name is valid for a component.  The default implementation
        ///     verifies that name is either null or unique compared to the names of other
        ///     components in the container.
        /// </devdoc>
        protected virtual void ValidateName(IComponent component, string name) {

            if (component == null) {
                throw new ArgumentNullException("component");
            }

            if (name != null) {
                for (int i = 0; i < Math.Min(siteCount,sites.Length); i++) {
                    ISite s = sites[ i ];

                    if (s != null && s.Name != null 
                            && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase) 
                            && s.Component != component) {
                        InheritanceAttribute inheritanceAttribute = (InheritanceAttribute)TypeDescriptor.GetAttributes(s.Component)[typeof(InheritanceAttribute)];
                        if(inheritanceAttribute.InheritanceLevel != InheritanceLevel.InheritedReadOnly) {
                            throw new ArgumentException(SR.GetString(SR.DuplicateComponentName, name));
                        }
                    }
                }
            }
        }

        private class Site : ISite 
        {
            private IComponent component;
            private Container container;
            private String name;

            internal Site(IComponent component, Container container, String name) {
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
                    return false;
                }
            }

            // The name of the component.
            //
            public String Name 
            {
                get { return name;}
                set { 
                    if (value == null || name == null || !value.Equals(name)) {
                        // 
                        container.ValidateName(component, value);
                        name = value;
                    }
                }
            }
        }
    }
}
