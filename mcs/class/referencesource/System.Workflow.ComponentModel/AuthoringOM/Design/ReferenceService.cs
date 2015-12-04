namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Reflection;
    using System.Globalization;

    internal sealed class ReferenceService : IReferenceService, IDisposable
    {
        private static readonly Attribute[] Attributes = new Attribute[] { BrowsableAttribute.Yes };
        private IServiceProvider provider;   // service provider we use to get to other services
        private ArrayList addedComponents;   // list of newly added components
        private ArrayList removedComponents; // list of newly removed components
        private ArrayList changedComponents; // list of changed components, we will re-cylcle their references too
        private ArrayList references;        // our current list of references

        internal ReferenceService(IServiceProvider provider)
        {
            this.provider = provider;
        }

        ~ReferenceService()
        {
            Dispose(false);
        }

        private void CreateReferences(IComponent component)
        {
            CreateReferences(string.Empty, component, component);
        }

        private void CreateReferences(string trailingName, object reference, IComponent sitedComponent)
        {
            if (object.ReferenceEquals(reference, null))
                return;

            this.references.Add(new ReferenceHolder(trailingName, reference, sitedComponent));
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(reference, Attributes))
            {
                object value = null;
                try
                {
                    value = property.GetValue(reference);
                }
                catch
                {
                    // Work around!!! if an property getter throws exception then we ignore it.
                }
                if (value != null)
                {
                    BrowsableAttribute[] browsableAttrs = (BrowsableAttribute[])(value.GetType().GetCustomAttributes(typeof(BrowsableAttribute), true));
                    if (browsableAttrs.Length > 0 && browsableAttrs[0].Browsable)
                    {
                        CreateReferences(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[] { trailingName, property.Name }), property.GetValue(reference), sitedComponent);
                    }
                }
            }
        }

        private void EnsureReferences()
        {
            // If the references are null, create them for the first time and connect
            // up our events to listen to changes to the container.  Otherwise, check to
            // see if the added or removed lists contain anything for us to [....] up.
            //
            if (this.references == null)
            {
                if (this.provider == null)
                {
                    throw new ObjectDisposedException("IReferenceService");
                }

                IComponentChangeService cs = this.provider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                Debug.Assert(cs != null, "Reference service relies on IComponentChangeService");
                if (cs != null)
                {
                    cs.ComponentAdded += new ComponentEventHandler(this.OnComponentAdded);
                    cs.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
                    cs.ComponentRename += new ComponentRenameEventHandler(this.OnComponentRename);
                    cs.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
                }

                TypeDescriptor.Refreshed += new RefreshEventHandler(OnComponentRefreshed);

                IContainer container = this.provider.GetService(typeof(IContainer)) as IContainer;
                if (container == null)
                {
                    Debug.Fail("Reference service cannot operate without IContainer");
                    throw new InvalidOperationException();
                }

                this.references = new ArrayList(container.Components.Count);

                foreach (IComponent component in container.Components)
                {
                    CreateReferences(component);
                }
            }
            else
            {
                if (this.addedComponents != null && this.addedComponents.Count > 0)
                {
                    // There is a possibility that this component already exists.
                    // If it does, just remove it first and then re-add it.
                    //
                    ArrayList clonedAddedComponents = new ArrayList(this.addedComponents);
                    foreach (IComponent ic in clonedAddedComponents)
                    {
                        RemoveReferences(ic);
                        CreateReferences(ic);
                    }
                    this.addedComponents.Clear();
                }

                if (this.removedComponents != null && this.removedComponents.Count > 0)
                {
                    ArrayList clonedRemovedComponents = new ArrayList(this.removedComponents);
                    foreach (IComponent ic in clonedRemovedComponents)
                        RemoveReferences(ic);

                    this.removedComponents.Clear();
                }
                if (this.changedComponents != null && this.changedComponents.Count > 0)
                {
                    ArrayList clonedChangedComponents = new ArrayList(this.changedComponents);
                    foreach (IComponent ic in clonedChangedComponents)
                    {
                        RemoveReferences(ic);
                        CreateReferences(ic);
                    }
                    this.changedComponents.Clear();
                }
            }
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs cevent)
        {
            IComponent comp = ((IReferenceService)this).GetComponent(cevent.Component);

            if (comp != null)
            {
                if ((this.addedComponents == null || !this.addedComponents.Contains(comp)) &&
                    (this.removedComponents == null || !this.removedComponents.Contains(comp)))
                {
                    if (this.changedComponents == null)
                    {
                        this.changedComponents = new ArrayList();
                        this.changedComponents.Add(comp);
                    }
                    else if (!this.changedComponents.Contains(comp))
                    {
                        this.changedComponents.Add(comp);
                    }
                }
            }
        }

        private void OnComponentAdded(object sender, ComponentEventArgs cevent)
        {
            if (this.addedComponents == null)
                this.addedComponents = new ArrayList();
            this.addedComponents.Add(cevent.Component);
            if (this.removedComponents != null)
                this.removedComponents.Remove(cevent.Component);
            if (this.changedComponents != null)
                this.changedComponents.Remove(cevent.Component);
        }

        private void OnComponentRemoved(object sender, ComponentEventArgs cevent)
        {
            if (this.removedComponents == null)
                this.removedComponents = new ArrayList();

            this.removedComponents.Add(cevent.Component);

            if (this.addedComponents != null)
                this.addedComponents.Remove(cevent.Component);
            if (this.changedComponents != null)
                this.changedComponents.Remove(cevent.Component);
        }

        private void OnComponentRename(object sender, ComponentRenameEventArgs cevent)
        {
            foreach (ReferenceHolder reference in this.references)
            {
                if (object.ReferenceEquals(reference.SitedComponent, cevent.Component))
                {
                    reference.ResetName();
                    return;
                }
            }
        }
        private void OnComponentRefreshed(RefreshEventArgs e)
        {
            if (e.ComponentChanged != null)
                OnComponentChanged(this, new ComponentChangedEventArgs(e.ComponentChanged, null, null, null));
        }

        private void RemoveReferences(IComponent component)
        {
            if (this.references != null)
            {
                int size = this.references.Count;
                for (int i = size - 1; i >= 0; i--)
                {
                    if (object.ReferenceEquals(((ReferenceHolder)this.references[i]).SitedComponent, component))
                        this.references.RemoveAt(i);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (this.references != null && this.provider != null)
            {
                IComponentChangeService cs = this.provider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (cs != null)
                {
                    cs.ComponentAdded -= new ComponentEventHandler(this.OnComponentAdded);
                    cs.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemoved);
                    cs.ComponentRename -= new ComponentRenameEventHandler(this.OnComponentRename);
                    cs.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
                }

                TypeDescriptor.Refreshed -= new RefreshEventHandler(OnComponentRefreshed);

                this.references = null;
                this.provider = null;
            }
        }

        IComponent IReferenceService.GetComponent(object reference)
        {
            if (object.ReferenceEquals(reference, null))
                throw new ArgumentNullException("reference");

            EnsureReferences();

            foreach (ReferenceHolder holder in this.references)
            {
                if (object.ReferenceEquals(holder.Reference, reference))
                    return holder.SitedComponent;
            }
            return null;
        }
        string IReferenceService.GetName(object reference)
        {
            if (object.ReferenceEquals(reference, null))
                throw new ArgumentNullException("reference");

            EnsureReferences();

            foreach (ReferenceHolder holder in this.references)
            {
                if (object.ReferenceEquals(holder.Reference, reference))
                    return holder.Name;
            }
            return null;
        }
        object IReferenceService.GetReference(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            EnsureReferences();

            foreach (ReferenceHolder holder in this.references)
            {
                if (string.Equals(holder.Name, name, StringComparison.OrdinalIgnoreCase))
                    return holder.Reference;
            }

            return null;
        }

        object[] IReferenceService.GetReferences()
        {
            EnsureReferences();

            object[] references = new object[this.references.Count];
            for (int i = 0; i < references.Length; i++)
                references[i] = ((ReferenceHolder)this.references[i]).Reference;

            return references;
        }

        object[] IReferenceService.GetReferences(Type baseType)
        {
            if (baseType == null)
                throw new ArgumentNullException("baseType");

            EnsureReferences();

            ArrayList results = new ArrayList(this.references.Count);
            foreach (ReferenceHolder holder in this.references)
            {
                object reference = holder.Reference;
                if (baseType.IsAssignableFrom(reference.GetType()))
                    results.Add(reference);
            }

            object[] references = new object[results.Count];
            results.CopyTo(references, 0);
            return references;
        }

        private sealed class ReferenceHolder
        {
            private string trailingName;
            private object reference;
            private IComponent sitedComponent;
            private string fullName;

            internal ReferenceHolder(string trailingName, object reference, IComponent sitedComponent)
            {
                this.trailingName = trailingName;
                this.reference = reference;
                this.sitedComponent = sitedComponent;

                Debug.Assert(trailingName != null, "Expected a trailing name");
                Debug.Assert(reference != null, "Expected a reference");

#if DEBUG

                Debug.Assert(sitedComponent != null, "Expected a sited component");
                if (sitedComponent != null)
                    Debug.Assert(sitedComponent.Site != null, "Sited component is not really sited: " + sitedComponent.ToString());
                if (sitedComponent != null && sitedComponent.Site != null)
                    Debug.Assert(sitedComponent.Site.Name != null, "Sited component has no name: " + sitedComponent.ToString());

#endif // DEBUG
            }

            internal void ResetName()
            {
                this.fullName = null;
            }

            internal string Name
            {
                get
                {
                    if (this.fullName == null)
                    {
                        if (this.sitedComponent != null && this.sitedComponent.Site != null && this.sitedComponent.Site.Name != null)
                        {
                            this.fullName = string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[] { this.sitedComponent.Site.Name, this.trailingName });
                        }
                        else
                        {
#if DEBUG
                            Debug.Assert(this.sitedComponent != null, "Expected a sited component");
                            if (this.sitedComponent != null)
                                Debug.Assert(this.sitedComponent.Site != null, "Sited component is not really sited: " + this.sitedComponent.ToString());
                            if (this.sitedComponent != null && this.sitedComponent.Site != null)
                                Debug.Assert(this.sitedComponent.Site.Name != null, "Sited component has no name: " + this.sitedComponent.ToString());
#endif // DEBUG
                            this.fullName = string.Empty;
                        }
                    }
                    return this.fullName;
                }
            }

            internal object Reference
            {
                get
                {
                    return this.reference;
                }
            }

            internal IComponent SitedComponent
            {
                get
                {
                    return this.sitedComponent;
                }
            }
        }
    }
}

