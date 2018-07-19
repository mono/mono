#pragma warning disable 1634, 1691
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Reflection;
using System.ComponentModel.Design.Serialization;

namespace System.Workflow.ComponentModel
{

    //TBD: DharmaS, Move the collections to the LocalStore class and have DO keep a ref to LS
    [DesignerSerializer(typeof(WorkflowMarkupSerializer), typeof(WorkflowMarkupSerializer))]
    [DesignerSerializer(typeof(DependencyObjectCodeDomSerializer), typeof(CodeDomSerializer))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class DependencyObject : IComponent, IDependencyObjectAccessor, IDisposable
    {
        private static DependencyProperty SiteProperty = DependencyProperty.Register("Site", typeof(ISite), typeof(DependencyObject), new PropertyMetadata(DependencyPropertyOptions.Metadata, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        private static DependencyProperty ReadonlyProperty = DependencyProperty.Register("Readonly", typeof(bool), typeof(DependencyObject), new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        private static DependencyProperty ParentDependencyObjectProperty = DependencyProperty.Register("ParentDependencyObject", typeof(DependencyObject), typeof(DependencyObject), new PropertyMetadata(null, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));
        private static DependencyProperty UserDataProperty = DependencyProperty.Register("UserData", typeof(IDictionary), typeof(DependencyObject), new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));

        //design meta data holder
        [NonSerialized]
        private IDictionary<DependencyProperty, object> metaProperties = new Dictionary<DependencyProperty, object>();
        [NonSerialized]
        private bool readonlyPropertyValue;

        protected DependencyObject()
        {
            //
            SetReadOnlyPropertyValue(ReadonlyProperty, false);
            readonlyPropertyValue = false;
            //
            SetReadOnlyPropertyValue(UserDataProperty, Hashtable.Synchronized(new Hashtable()));
        }

        ~DependencyObject()
        {
            // In designer/build scenarios, some dependencies may be missing, which could cause a 
            // JIT failure in user overrides of Dispose. Ignore these failures so they don't
            // crash the process.
            if (this.DesignMode)
            {
                try
                {
                    Dispose(false);
                }
                // These are the most common JIT exceptions for this scenario, based on Watson data
                catch (TypeInitializationException)
                {
                }
                catch (System.IO.FileNotFoundException)
                {
                }
            }
            else
            {
                Dispose(false);
            }
        }

        internal bool Readonly
        {
            get
            {
                return (bool)GetValue(ReadonlyProperty);
            }
            set
            {
                SetReadOnlyPropertyValue(ReadonlyProperty, value);
                readonlyPropertyValue = value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDictionary UserData
        {
            get
            {
                return (IDictionary)GetValue(UserDataProperty);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected internal bool DesignMode
        {
            get
            {
                return !readonlyPropertyValue;
            }
        }
        protected DependencyObject ParentDependencyObject
        {
            get
            {
                return (DependencyObject)this.GetValue(ParentDependencyObjectProperty);
            }
        }

        #region Dynamic Property Support
        public void SetBinding(DependencyProperty dependencyProperty, ActivityBind bind)
        {
            if (dependencyProperty == null)
                throw new ArgumentNullException("dependencyProperty");

            if (dependencyProperty.DefaultMetadata != null && dependencyProperty.DefaultMetadata.IsReadOnly)
                throw new ArgumentException(SR.GetString(SR.Error_DPReadOnly), "dependencyProperty");

            if (dependencyProperty.OwnerType == null)
                throw new ArgumentException(SR.GetString(SR.Error_MissingOwnerTypeProperty), "dependencyProperty");

            if (!dependencyProperty.IsAttached && !dependencyProperty.OwnerType.IsAssignableFrom(this.GetType()))
                throw new InvalidOperationException(SR.GetString(SR.Error_InvalidDependencyProperty, this.GetType().FullName, dependencyProperty.Name, dependencyProperty.OwnerType.FullName));

            if (!this.DesignMode)
                throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));

            if (dependencyProperty.DefaultMetadata != null && dependencyProperty.DefaultMetadata.IsMetaProperty && !typeof(ActivityBind).IsAssignableFrom(dependencyProperty.PropertyType))
                throw new ArgumentException(SR.GetString(SR.Error_DPMetaPropertyBinding), "dependencyProperty");

            if (this.metaProperties.ContainsKey(dependencyProperty))
                this.metaProperties[dependencyProperty] = bind;
            else
                this.metaProperties.Add(dependencyProperty, bind);

            // Remove the instance value
            if (this.DependencyPropertyValues.ContainsKey(dependencyProperty))
                this.DependencyPropertyValues.Remove(dependencyProperty);
        }

        // 





        public ActivityBind GetBinding(DependencyProperty dependencyProperty)
        {
            if (dependencyProperty == null)
                throw new ArgumentNullException("dependencyProperty");

            //Designer needs the bind and not bound value
            return (this.metaProperties.ContainsKey(dependencyProperty) ? this.metaProperties[dependencyProperty] as ActivityBind : null);
        }
        public object GetValue(DependencyProperty dependencyProperty)
        {
            //Return the property values
            if (dependencyProperty == null)
                throw new ArgumentNullException("dependencyProperty");

            if (dependencyProperty.IsEvent)
                throw new ArgumentException(SR.GetString(SR.Error_DPGetValueHandler), "dependencyProperty");

            // Get type-specific metadata for this property
            PropertyMetadata metadata = dependencyProperty.DefaultMetadata;
            if (metadata.GetValueOverride != null)
                return metadata.GetValueOverride(this);

            return GetValueCommon(dependencyProperty, metadata);
        }

        private object GetValueCommon(DependencyProperty dependencyProperty, PropertyMetadata metadata)
        {
            //ok, the default case
            // pick either the instance bag or meta bag, instance bag is given first chance
            object value;
            if (!this.DependencyPropertyValues.TryGetValue(dependencyProperty, out value))
            {
                if (metaProperties == null || !metaProperties.TryGetValue(dependencyProperty, out value))
                    return dependencyProperty.DefaultMetadata.DefaultValue;
            }

            //Designer needs the bind and not bound value
            if (this.metaProperties != null && !this.DesignMode && value is ActivityBind && !typeof(ActivityBind).IsAssignableFrom(dependencyProperty.PropertyType))
                value = this.GetBoundValue((ActivityBind)value, dependencyProperty.PropertyType);

            if (value == null || value is ActivityBind)
                return dependencyProperty.DefaultMetadata.DefaultValue;

            if (!dependencyProperty.PropertyType.IsAssignableFrom(value.GetType()))
                throw new InvalidOperationException(SR.GetString(SR.Error_DynamicPropertyTypeValueMismatch, new object[] { dependencyProperty.PropertyType.FullName, dependencyProperty.Name, value.GetType().FullName }));

            return value;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public object GetValueBase(DependencyProperty dependencyProperty)
        {
            if (dependencyProperty == null)
                throw new ArgumentNullException("dependencyProperty");

            return this.GetValueCommon(dependencyProperty, dependencyProperty.DefaultMetadata);
        }

        protected internal void SetReadOnlyPropertyValue(DependencyProperty dependencyProperty, object value)
        {
            if (dependencyProperty == null)
                throw new ArgumentNullException("dependencyProperty");

            if (!dependencyProperty.DefaultMetadata.IsReadOnly)
                throw new InvalidOperationException(SR.GetString(SR.Error_NotReadOnlyProperty, dependencyProperty.Name, dependencyProperty.OwnerType.FullName));

            if (!dependencyProperty.IsAttached && !dependencyProperty.OwnerType.IsAssignableFrom(this.GetType()))
                throw new InvalidOperationException(SR.GetString(SR.Error_InvalidDependencyProperty, this.GetType().FullName, dependencyProperty.Name, dependencyProperty.OwnerType.FullName));

            IDictionary<DependencyProperty, object> properties = null;
            if (dependencyProperty.DefaultMetadata.IsMetaProperty)
                properties = this.metaProperties;
            else
                properties = this.DependencyPropertyValues;

            if (properties.ContainsKey(dependencyProperty))
                properties[dependencyProperty] = value;
            else
                properties.Add(dependencyProperty, value);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetValueBase(DependencyProperty dependencyProperty, object value)
        {
            if (dependencyProperty == null)
                throw new ArgumentNullException("dependencyProperty");

            PropertyMetadata metadata = dependencyProperty.DefaultMetadata;
            this.SetValueCommon(dependencyProperty, value, metadata, metadata.ShouldAlwaysCallOverride);
        }


        public void SetValue(DependencyProperty dependencyProperty, object value)
        {
            if (dependencyProperty == null)
                throw new ArgumentNullException("dependencyProperty");

            PropertyMetadata metadata = dependencyProperty.DefaultMetadata;
            this.SetValueCommon(dependencyProperty, value, metadata, true);
        }

        internal void SetValueCommon(DependencyProperty dependencyProperty, object value, PropertyMetadata metadata, bool shouldCallSetValueOverrideIfExists)
        {
            if (dependencyProperty.DefaultMetadata.IsReadOnly)
                throw new ArgumentException(SR.GetString(SR.Error_DPReadOnly), "dependencyProperty");
            if (value is ActivityBind)
                throw new ArgumentException(SR.GetString(SR.Error_DPSetValueBind), "value");
            if (dependencyProperty.IsEvent)
                throw new ArgumentException(SR.GetString(SR.Error_DPSetValueHandler), "dependencyProperty");

            if (!dependencyProperty.IsAttached && !dependencyProperty.OwnerType.IsAssignableFrom(this.GetType()))
                throw new InvalidOperationException(SR.GetString(SR.Error_InvalidDependencyProperty, this.GetType().FullName, dependencyProperty.Name, dependencyProperty.OwnerType.FullName));

            //ok, the default case
            // Work around  Declarative conditions
            if (!this.DesignMode && (dependencyProperty.DefaultMetadata.IsMetaProperty && dependencyProperty != Design.ConditionTypeConverter.DeclarativeConditionDynamicProp))
                throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));

            if (value != null && !dependencyProperty.PropertyType.IsAssignableFrom(value.GetType()))
                throw new ArgumentException(SR.GetString(SR.Error_DynamicPropertyTypeValueMismatch, new object[] { dependencyProperty.PropertyType.FullName, dependencyProperty.Name, value.GetType().FullName }), "value");

            // Set type-specific metadata for this property if the delegate was specified
            if (shouldCallSetValueOverrideIfExists && metadata.SetValueOverride != null)
            {
                metadata.SetValueOverride(this, value);
                return;
            }

            //Even if the DependencyProperty is an instance property if its value is of type bind
            //we store it in MetaProperties, We need this to make bind work
            IDictionary<DependencyProperty, object> properties = null;
            if (dependencyProperty.DefaultMetadata.IsMetaProperty)
                properties = this.metaProperties;
            else
                properties = this.DependencyPropertyValues;


            object oldMetaValue = null;
            if (this.metaProperties != null && this.metaProperties.ContainsKey(dependencyProperty))
            {
                oldMetaValue = this.metaProperties[dependencyProperty];
                if (this.DesignMode)
                    this.metaProperties.Remove(dependencyProperty);
            }
            if (!this.DesignMode && (oldMetaValue is ActivityBind))
            {
                this.SetBoundValue((ActivityBind)oldMetaValue, value);
            }
            else
            {
                if (properties.ContainsKey(dependencyProperty))
                    properties[dependencyProperty] = value;
                else
                    properties.Add(dependencyProperty, value);
            }
        }

        public bool RemoveProperty(DependencyProperty dependencyProperty)
        {
            if (dependencyProperty == null)
                throw new ArgumentNullException("dependencyProperty");

            bool removed = false;
            if (dependencyProperty.DefaultMetadata != null && dependencyProperty.DefaultMetadata.IsMetaProperty)
            {
                if (!DesignMode)
                    throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));
                removed = this.metaProperties.Remove(dependencyProperty);
            }
            else
            {
                //These statements are required as InstanceProperties which are Bind are stored as MetaProperties
                removed = this.metaProperties.Remove(dependencyProperty);
                removed |= this.DependencyPropertyValues.Remove(dependencyProperty);
            }
            return removed;
        }

        public void AddHandler(DependencyProperty dependencyEvent, object value)
        {
            if (dependencyEvent == null)
                throw new ArgumentNullException("dependencyEvent");

            if (value == null)
                throw new ArgumentNullException("value");

            if (value is ActivityBind)
                throw new ArgumentException(SR.GetString(SR.Error_DPSetValueBind), "value");

            if (dependencyEvent.DefaultMetadata != null && dependencyEvent.DefaultMetadata.IsMetaProperty)
                throw new ArgumentException(SR.GetString(SR.Error_DPAddHandlerMetaProperty), "dependencyEvent");

            if (!dependencyEvent.IsEvent)
                throw new ArgumentException(SR.GetString(SR.Error_DPAddHandlerNonEvent), "dependencyEvent");

            if (dependencyEvent.PropertyType == null)
                throw new ArgumentException(SR.GetString(SR.Error_DPPropertyTypeMissing), "dependencyEvent");

            if (dependencyEvent.OwnerType == null)
                throw new ArgumentException(SR.GetString(SR.Error_MissingOwnerTypeProperty), "dependencyEvent");

            if (!dependencyEvent.IsAttached && !dependencyEvent.OwnerType.IsAssignableFrom(this.GetType()))
                throw new InvalidOperationException(SR.GetString(SR.Error_InvalidDependencyProperty, this.GetType().FullName, dependencyEvent.Name, dependencyEvent.OwnerType.FullName));

            if (value != null && !dependencyEvent.PropertyType.IsAssignableFrom(value.GetType()))
                throw new ArgumentException(SR.GetString(SR.Error_DynamicPropertyTypeValueMismatch, new object[] { dependencyEvent.PropertyType.FullName, dependencyEvent.Name, value.GetType().FullName }), "value");

            // get appropriate meta bag or instance bag
            IDictionary<DependencyProperty, object> properties = this.DependencyPropertyValues;

            // add it to array list
            ArrayList eventListeners = null;
            if (properties.ContainsKey(dependencyEvent))
            {
                eventListeners = (ArrayList)properties[dependencyEvent];
            }
            else
            {
                eventListeners = new ArrayList();
                properties.Add(dependencyEvent, eventListeners);
            }
            eventListeners.Add(value);
            if (this.DesignMode && this.metaProperties.ContainsKey(dependencyEvent))
                this.metaProperties.Remove(dependencyEvent);
        }

        public void RemoveHandler(DependencyProperty dependencyEvent, object value)
        {
            if (dependencyEvent == null)
                throw new ArgumentNullException("dependencyEvent");
            if (value == null)
                throw new ArgumentNullException("value");
            if (value is ActivityBind)
                throw new ArgumentException(SR.GetString(SR.Error_DPRemoveHandlerBind), "value");

            if (dependencyEvent.DefaultMetadata != null && dependencyEvent.DefaultMetadata.IsMetaProperty)
                throw new ArgumentException(SR.GetString(SR.Error_DPAddHandlerMetaProperty), "dependencyEvent");

            if (!dependencyEvent.IsEvent)
                throw new ArgumentException(SR.GetString(SR.Error_DPAddHandlerNonEvent), "dependencyEvent");

            // get appropriate meta bag or instance bag
            IDictionary<DependencyProperty, object> properties = this.DependencyPropertyValues;
            if (properties.ContainsKey(dependencyEvent))
            {
                ArrayList eventListeners = (ArrayList)properties[dependencyEvent];
                if (eventListeners.Contains(value))
                    eventListeners.Remove(value);
                if (eventListeners.Count == 0)
                    properties.Remove(dependencyEvent);
            }
        }

        internal object GetHandler(DependencyProperty dependencyEvent)
        {
            if (dependencyEvent == null)
                throw new ArgumentNullException("dependencyEvent");

            if (!dependencyEvent.IsEvent)
                throw new ArgumentException("dependencyEvent");

            IDictionary<DependencyProperty, object> properties = this.DependencyPropertyValues;
            if (properties.ContainsKey(dependencyEvent))
            {
                ArrayList handlers = properties[dependencyEvent] as ArrayList;
                if (handlers != null && handlers.Count != 0)
                {
                    if (handlers.Count == 1)
                        return handlers[0];
                    else //Combine 
                    {
                        Delegate delegateHandler = handlers[0] as Delegate;
                        for (int i = 1; i < handlers.Count; ++i)
                        {
                            delegateHandler = Delegate.Combine(delegateHandler, handlers[i] as Delegate);
                        }

                        return delegateHandler;
                    }
                }
            }
            return null;
        }

        public bool IsBindingSet(DependencyProperty dependencyProperty)
        {
            if (dependencyProperty == null)
                throw new ArgumentNullException("dependencyProperty");

            return (!this.DependencyPropertyValues.ContainsKey(dependencyProperty) &&
                    this.metaProperties.ContainsKey(dependencyProperty) &&
                    this.metaProperties[dependencyProperty] is ActivityBind);
        }

        public bool MetaEquals(DependencyObject dependencyObject)
        {
#pragma warning suppress 56506
            return dependencyObject != null && dependencyObject.metaProperties == this.metaProperties;
        }

        #region IDependencyObjectAccessor

        void IDependencyObjectAccessor.InitializeInstanceForRuntime(IWorkflowCoreRuntime workflowCoreRuntime)
        {
            OnInitializeInstanceForRuntime(workflowCoreRuntime);
        }
        internal virtual void OnInitializeInstanceForRuntime(IWorkflowCoreRuntime workflowCoreRuntime)
        {
        }

        //This is invoked for every activating instance
        void IDependencyObjectAccessor.InitializeActivatingInstanceForRuntime(DependencyObject parentDependencyObject, IWorkflowCoreRuntime workflowCoreRuntime)
        {
            if (parentDependencyObject != null)
                this.DependencyPropertyValues[ParentDependencyObjectProperty] = parentDependencyObject;

            // InitializeForRuntime call allows people to put more meta properties in the bag.
            foreach (DependencyProperty dependencyProperty in this.MetaDependencyProperties)
            {
                object propValue = this.metaProperties[dependencyProperty];
                if (propValue is DependencyObject)
                {
                    ((IDependencyObjectAccessor)propValue).InitializeActivatingInstanceForRuntime(this, workflowCoreRuntime);
                    this.DependencyPropertyValues[dependencyProperty] = propValue;
                }
                else if (propValue is WorkflowParameterBindingCollection)
                {
                    IList collection = propValue as IList;
                    for (int index2 = 0; index2 < collection.Count; index2++)
                        ((IDependencyObjectAccessor)collection[index2]).InitializeActivatingInstanceForRuntime(this, workflowCoreRuntime);
                    this.DependencyPropertyValues[dependencyProperty] = propValue;
                }
            }
            OnInitializeActivatingInstanceForRuntime(workflowCoreRuntime);

            this.Readonly = true;
        }
        internal virtual void OnInitializeActivatingInstanceForRuntime(IWorkflowCoreRuntime workflowCoreRuntime)
        {
            InitializeProperties();
        }

        void IDependencyObjectAccessor.InitializeDefinitionForRuntime(DependencyObject parentDependencyObject)
        {
            if (parentDependencyObject != null)
                this.DependencyPropertyValues[ParentDependencyObjectProperty] = parentDependencyObject;

            // InitializeForRuntime call allows people to put more meta properties in the bag.
            foreach (DependencyProperty dependencyProperty in this.MetaDependencyProperties)
            {
                object propValue = this.metaProperties[dependencyProperty];
                if (propValue is DependencyObject)
                {
                    ((IDependencyObjectAccessor)propValue).InitializeDefinitionForRuntime(this);
                    this.DependencyPropertyValues[dependencyProperty] = propValue;
                }
                else if (propValue is WorkflowParameterBindingCollection)
                {
                    IList collection = propValue as IList;
                    for (int index2 = 0; index2 < collection.Count; index2++)
                        ((IDependencyObjectAccessor)collection[index2]).InitializeDefinitionForRuntime(this);
                    this.DependencyPropertyValues[dependencyProperty] = propValue;
                }
                else if (propValue is ActivityBind)
                {
                    Activity activity = ResolveOwnerActivity();
                    if (activity != null)
                        ((ActivityBind)propValue).SetContext(activity);
                }
            }
            OnInitializeDefinitionForRuntime();
            InitializeProperties();
            this.Readonly = true;
        }
        internal virtual void OnInitializeDefinitionForRuntime()
        {
        }
        protected virtual void InitializeProperties()
        {
        }
        internal virtual void FixUpMetaProperties(DependencyObject originalObject)
        {
            if (originalObject == null)
                throw new ArgumentNullException();

            this.metaProperties = originalObject.metaProperties;
            this.readonlyPropertyValue = true;       // alway readonly if fixing up
            foreach (KeyValuePair<DependencyProperty, object> keyValuePair in this.DependencyPropertyValues)
            {
                if (keyValuePair.Key != ParentDependencyObjectProperty && originalObject.DependencyPropertyValues.ContainsKey(keyValuePair.Key))
                {
                    object originalPropValue = originalObject.DependencyPropertyValues[keyValuePair.Key];
                    if (keyValuePair.Value is DependencyObject)
                    {
                        ((DependencyObject)keyValuePair.Value).FixUpMetaProperties(originalPropValue as DependencyObject);
                    }
                    else if (keyValuePair.Value is WorkflowParameterBindingCollection)
                    {
                        IList collection = keyValuePair.Value as IList;
                        IList originalCollection = originalPropValue as IList;
                        for (int index = 0; index < collection.Count; index++)
                        {
                            ((DependencyObject)collection[index]).FixUpMetaProperties(originalCollection[index] as DependencyObject);
                        }
                    }
                }
            }
        }
        T[] IDependencyObjectAccessor.GetInvocationList<T>(DependencyProperty dependencyEvent)
        {
            return this.GetInvocationList<T>(dependencyEvent);
        }
        protected T[] GetInvocationList<T>(DependencyProperty dependencyEvent)
        {
            if (dependencyEvent == null)
                throw new ArgumentNullException("dependencyEvent");

            if (!dependencyEvent.IsEvent)
                throw new ArgumentException(SR.GetString(SR.Error_DPAddHandlerNonEvent), "dependencyEvent");

            // pick the appropriate meta bag or instance bag, first priority is to instance bag
            IDictionary<DependencyProperty, object> properties = null;
            if (this.DependencyPropertyValues.ContainsKey(dependencyEvent))
                properties = this.DependencyPropertyValues;
            else
                properties = this.metaProperties;

            List<T> delegates = new List<T>();
            if (properties.ContainsKey(dependencyEvent))
            {
                if (properties[dependencyEvent] is ActivityBind)
                {
                    if (!this.DesignMode)
                    {
                        T delegateValue = default(T);
                        delegateValue = (T)this.GetBoundValue((ActivityBind)properties[dependencyEvent], typeof(T));
                        if (delegateValue is T)
                            delegates.Add(delegateValue);
                    }
                }
                else
                {
                    foreach (object value in (ArrayList)properties[dependencyEvent])
                    {
                        if (value is T)
                        {
                            delegates.Add((T)value);
                        }
                    }
                }
            }
            return delegates.ToArray();
        }
        #endregion

        internal IList<DependencyProperty> MetaDependencyProperties
        {
            get
            {
                return new List<DependencyProperty>(this.metaProperties.Keys).AsReadOnly();
            }
        }

        //
        private IDictionary<DependencyProperty, object> dependencyPropertyValues = null;
        internal IDictionary<DependencyProperty, object> DependencyPropertyValues
        {
            get
            {
                if (this.dependencyPropertyValues == null)
                    this.dependencyPropertyValues = new Dictionary<DependencyProperty, object>();
                return this.dependencyPropertyValues;
            }
        }

        #endregion

        #region Bind resolution Support

        [EditorBrowsableAttribute(EditorBrowsableState.Never)]
        protected virtual object GetBoundValue(ActivityBind bind, Type targetType)
        {
            if (bind == null)
                throw new ArgumentNullException("bind");
            if (targetType == null)
                throw new ArgumentNullException("targetType");

            object returnVal = bind;
            Activity activity = ResolveOwnerActivity();
            if (activity != null)
                returnVal = bind.GetRuntimeValue(activity, targetType);

            return returnVal;
        }
        [EditorBrowsableAttribute(EditorBrowsableState.Never)]
        protected virtual void SetBoundValue(ActivityBind bind, object value)
        {
            if (bind == null)
                throw new ArgumentNullException("bind");

            Activity activity = ResolveOwnerActivity();
            if (activity != null)
                bind.SetRuntimeValue(activity, value);
        }

        // 
        private Activity ResolveOwnerActivity()
        {
            DependencyObject activityDependencyObject = this;
            while (activityDependencyObject != null && !(activityDependencyObject is Activity))
                activityDependencyObject = activityDependencyObject.ParentDependencyObject;
            return activityDependencyObject as Activity;
        }

        #endregion

        #region IComponent Members

        private event EventHandler disposed;

        event EventHandler IComponent.Disposed
        {
            add
            {
                disposed += value;
            }
            remove
            {
                disposed -= value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsableAttribute(EditorBrowsableState.Never)]
        public ISite Site
        {
            get
            {
                return (ISite)GetValue(SiteProperty);
            }
            set
            {
                SetValue(SiteProperty, value);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Site != null && this.Site.Container != null)
                    this.Site.Container.Remove(this);

                if (disposed != null)
                    disposed(this, EventArgs.Empty);
            }
        }
        #endregion
    }
}
