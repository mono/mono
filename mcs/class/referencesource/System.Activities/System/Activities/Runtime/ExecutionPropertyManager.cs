//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Runtime.Serialization;

    [DataContract]
    class ExecutionPropertyManager
    {
        ActivityInstance owningInstance;

        Dictionary<string, ExecutionProperty> properties;

        // Since the ExecutionProperty objects in this list
        // could exist in several places we need to make sure
        // that we clean up any booleans before more work items run
        List<ExecutionProperty> threadProperties;
        bool ownsThreadPropertiesList;

        string lastPropertyName;
        object lastProperty;
        IdSpace lastPropertyVisibility;

        // used by the root activity instance to chain parents correctly
        ExecutionPropertyManager rootPropertyManager;

        int exclusiveHandleCount;

        public ExecutionPropertyManager(ActivityInstance owningInstance)
        {
            Fx.Assert(owningInstance != null, "null instance should be using the internal host-based ctor");
            this.owningInstance = owningInstance;

            // This object is only constructed if we know we have properties to add to it
            this.properties = new Dictionary<string, ExecutionProperty>();

            if (owningInstance.HasChildren)
            {
                ActivityInstance previousOwner = owningInstance.PropertyManager != null ? owningInstance.PropertyManager.owningInstance : null;

                // we're setting a handle property. Walk the children and associate the new property manager
                // then walk our instance list, fixup parent references, and perform basic validation
                ActivityUtilities.ProcessActivityInstanceTree(owningInstance, null, (instance, executor) => AttachPropertyManager(instance, previousOwner));
            }
            else
            {
                owningInstance.PropertyManager = this;
            }
        }

        public ExecutionPropertyManager(ActivityInstance owningInstance, ExecutionPropertyManager parentPropertyManager)
            : this(owningInstance)
        {
            Fx.Assert(parentPropertyManager != null, "caller must verify");
            this.threadProperties = parentPropertyManager.threadProperties;

            // if our parent is null, capture any root properties
            if (owningInstance.Parent == null)
            {
                this.rootPropertyManager = parentPropertyManager.rootPropertyManager;
            }
        }

        internal ExecutionPropertyManager(ActivityInstance owningInstance, Dictionary<string, ExecutionProperty> properties)
        {
            Fx.Assert(properties != null, "properties should never be null");
            this.owningInstance = owningInstance;
            this.properties = properties;

            // owningInstance can be null (for host-provided root properties)
            if (owningInstance == null)
            {
                this.rootPropertyManager = this;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "properties")]
        internal Dictionary<string, ExecutionProperty> SerializedProperties
        {
            get { return this.properties; }
            set { this.properties = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "exclusiveHandleCount")]
        internal int SerializedExclusiveHandleCount
        {
            get { return this.exclusiveHandleCount; }
            set { this.exclusiveHandleCount = value; }
        }

        internal Dictionary<string, ExecutionProperty> Properties
        {
            get
            {
                return this.properties;
            }
        }

        internal bool HasExclusiveHandlesInScope
        {
            get
            {
                return this.exclusiveHandleCount > 0;
            }
        }

        bool AttachPropertyManager(ActivityInstance instance, ActivityInstance previousOwner)
        {
            if (instance.PropertyManager == null || instance.PropertyManager.owningInstance == previousOwner)
            {
                instance.PropertyManager = this;
                return true;
            }
            else
            {
                return false;
            }
        }

        public object GetProperty(string name, IdSpace currentIdSpace)
        {
            Fx.Assert(!string.IsNullOrEmpty(name), "The name should be validated by the caller.");

            if (lastPropertyName == name && (this.lastPropertyVisibility == null || this.lastPropertyVisibility == currentIdSpace))
            {
                return lastProperty;
            }

            ExecutionPropertyManager currentManager = this;

            while (currentManager != null)
            {
                ExecutionProperty property;
                if (currentManager.properties.TryGetValue(name, out property))
                {
                    if (!property.IsRemoved && (!property.HasRestrictedVisibility || property.Visibility == currentIdSpace))
                    {
                        this.lastPropertyName = name;
                        this.lastProperty = property.Property;
                        this.lastPropertyVisibility = property.Visibility;

                        return this.lastProperty;
                    }
                }

                currentManager = GetParent(currentManager);
            }

            return null;
        }

        void AddProperties(IDictionary<string, ExecutionProperty> properties, IDictionary<string, object> flattenedProperties, IdSpace currentIdSpace)
        {
            foreach (KeyValuePair<string, ExecutionProperty> item in properties)
            {
                if (!item.Value.IsRemoved && !flattenedProperties.ContainsKey(item.Key) && (!item.Value.HasRestrictedVisibility || item.Value.Visibility == currentIdSpace))
                {
                    flattenedProperties.Add(item.Key, item.Value.Property);
                }
            }
        }

        public IEnumerable<KeyValuePair<string, object>> GetFlattenedProperties(IdSpace currentIdSpace)
        {
            ExecutionPropertyManager currentManager = this;
            Dictionary<string, object> flattenedProperties = new Dictionary<string, object>();
            while (currentManager != null)
            {
                AddProperties(currentManager.Properties, flattenedProperties, currentIdSpace);
                currentManager = GetParent(currentManager);
            }
            return flattenedProperties;
        }

        //Currently this is only used for the exclusive scope processing
        internal List<T> FindAll<T>() where T : class
        {
            ExecutionPropertyManager currentManager = this;
            List<T> list = null;

            while (currentManager != null)
            {
                foreach (ExecutionProperty property in currentManager.Properties.Values)
                {
                    if (property.Property is T)
                    {
                        if (list == null)
                        {
                            list = new List<T>();
                        }
                        list.Add((T)property.Property);
                    }
                }

                currentManager = GetParent(currentManager);
            }

            return list;
        }

        static ExecutionPropertyManager GetParent(ExecutionPropertyManager currentManager)
        {
            if (currentManager.owningInstance != null)
            {
                if (currentManager.owningInstance.Parent != null)
                {
                    return currentManager.owningInstance.Parent.PropertyManager;
                }
                else
                {
                    return currentManager.rootPropertyManager;
                }
            }
            else
            {
                return null;
            }
        }

        public void Add(string name, object property, IdSpace visibility)
        {
            Fx.Assert(!string.IsNullOrEmpty(name), "The name should be validated before calling this collection.");
            Fx.Assert(property != null, "The property should be validated before caling this collection.");

            ExecutionProperty executionProperty = new ExecutionProperty(name, property, visibility);
            this.properties.Add(name, executionProperty);

            if (this.lastPropertyName == name)
            {
                this.lastProperty = property;
            }

            if (property is ExclusiveHandle)
            {
                this.exclusiveHandleCount++;

                UpdateChildExclusiveHandleCounts(1);
            }

            if (property is IExecutionProperty)
            {
                AddIExecutionProperty(executionProperty, false);
            }
        }

        void UpdateChildExclusiveHandleCounts(int amountToUpdate)
        {
            Queue<HybridCollection<ActivityInstance>> toProcess = null;

            HybridCollection<ActivityInstance> children = this.owningInstance.GetRawChildren();

            if (children != null && children.Count > 0)
            {
                ProcessChildrenForExclusiveHandles(children, amountToUpdate, ref toProcess);

                if (toProcess != null)
                {
                    while (toProcess.Count > 0)
                    {
                        children = toProcess.Dequeue();
                        ProcessChildrenForExclusiveHandles(children, amountToUpdate, ref toProcess);
                    }
                }
            }
        }

        void ProcessChildrenForExclusiveHandles(HybridCollection<ActivityInstance> children, int amountToUpdate, ref Queue<HybridCollection<ActivityInstance>> toProcess)
        {
            for (int i = 0; i < children.Count; i++)
            {
                ActivityInstance child = children[i];

                ExecutionPropertyManager childManager = child.PropertyManager;

                if (childManager.IsOwner(child))
                {
                    childManager.exclusiveHandleCount += amountToUpdate;
                }

                HybridCollection<ActivityInstance> tempChildren = child.GetRawChildren();

                if (tempChildren != null && tempChildren.Count > 0)
                {
                    if (toProcess == null)
                    {
                        toProcess = new Queue<HybridCollection<ActivityInstance>>();
                    }

                    toProcess.Enqueue(tempChildren);
                }
            }
        }

        void AddIExecutionProperty(ExecutionProperty property, bool isDeserializationFixup)
        {
            bool willCleanupBeCalled = !isDeserializationFixup;

            if (this.threadProperties == null)
            {
                this.threadProperties = new List<ExecutionProperty>(1);
                this.ownsThreadPropertiesList = true;
            }
            else if (!this.ownsThreadPropertiesList)
            {
                List<ExecutionProperty> updatedProperties = new List<ExecutionProperty>(this.threadProperties.Count);

                // We need to copy all properties to our new list and we
                // need to mark hidden properties as "to be removed" (or just
                // not copy them on the deserialization path)
                for (int i = 0; i < this.threadProperties.Count; i++)
                {
                    ExecutionProperty currentProperty = this.threadProperties[i];

                    if (currentProperty.Name == property.Name)
                    {
                        if (willCleanupBeCalled)
                        {
                            currentProperty.ShouldBeRemovedAfterCleanup = true;
                            updatedProperties.Add(currentProperty);
                        }

                        // If cleanup won't be called then we are on the
                        // deserialization path and shouldn't copy this
                        // property over to our new list
                    }
                    else
                    {
                        updatedProperties.Add(currentProperty);
                    }
                }

                this.threadProperties = updatedProperties;
                this.ownsThreadPropertiesList = true;
            }
            else
            {
                for (int i = this.threadProperties.Count - 1; i >= 0; i--)
                {
                    ExecutionProperty currentProperty = this.threadProperties[i];

                    if (currentProperty.Name == property.Name)
                    {
                        if (willCleanupBeCalled)
                        {
                            currentProperty.ShouldBeRemovedAfterCleanup = true;
                        }
                        else
                        {
                            this.threadProperties.RemoveAt(i);
                        }

                        // There will only be at most one property in this list that
                        // matches the name
                        break;
                    }
                }
            }

            property.ShouldSkipNextCleanup = willCleanupBeCalled;
            this.threadProperties.Add(property);
        }

        public void Remove(string name)
        {
            Fx.Assert(!string.IsNullOrEmpty(name), "This should have been validated by the caller.");

            ExecutionProperty executionProperty = this.properties[name];

            Fx.Assert(executionProperty != null, "This should only be called if we know the property exists");

            if (executionProperty.Property is IExecutionProperty)
            {
                Fx.Assert(this.ownsThreadPropertiesList && this.threadProperties != null, "We should definitely be the list owner if we have an IExecutionProperty");

                if (!this.threadProperties.Remove(executionProperty))
                {
                    Fx.Assert("We should have had this property in the list.");
                }
            }

            this.properties.Remove(name);

            if (executionProperty.Property is ExclusiveHandle)
            {
                this.exclusiveHandleCount--;

                UpdateChildExclusiveHandleCounts(-1);
            }

            if (this.lastPropertyName == name)
            {
                this.lastPropertyName = null;
                this.lastProperty = null;
            }
        }

        public object GetPropertyAtCurrentScope(string name)
        {
            Fx.Assert(!string.IsNullOrEmpty(name), "This should be validated elsewhere");

            ExecutionProperty property;
            if (this.properties.TryGetValue(name, out property))
            {
                return property.Property;
            }

            return null;
        }

        public bool IsOwner(ActivityInstance instance)
        {
            return this.owningInstance == instance;
        }

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "Called from Serialization")]
        internal bool ShouldSerialize(ActivityInstance instance)
        {
            return IsOwner(instance) && this.properties.Count > 0;
        }

        public void SetupWorkflowThread()
        {
            if (this.threadProperties != null)
            {
                for (int i = 0; i < this.threadProperties.Count; i++)
                {
                    ExecutionProperty executionProperty = this.threadProperties[i];
                    executionProperty.ShouldSkipNextCleanup = false;
                    IExecutionProperty property = (IExecutionProperty)executionProperty.Property;

                    property.SetupWorkflowThread();
                }
            }
        }

        // This method only throws fatal exceptions
        public void CleanupWorkflowThread(ref Exception abortException)
        {
            if (this.threadProperties != null)
            {
                for (int i = this.threadProperties.Count - 1; i >= 0; i--)
                {
                    ExecutionProperty current = this.threadProperties[i];

                    if (current.ShouldSkipNextCleanup)
                    {
                        current.ShouldSkipNextCleanup = false;
                    }
                    else
                    {
                        IExecutionProperty property = (IExecutionProperty)current.Property;

                        try
                        {
                            property.CleanupWorkflowThread();
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }

                            abortException = e;
                        }
                    }

                    if (current.ShouldBeRemovedAfterCleanup)
                    {
                        this.threadProperties.RemoveAt(i);
                        current.ShouldBeRemovedAfterCleanup = false;
                    }
                }
            }
        }

        public void UnregisterProperties(ActivityInstance completedInstance, IdSpace currentIdSpace)
        {
            UnregisterProperties(completedInstance, currentIdSpace, false);
        }

        public void UnregisterProperties(ActivityInstance completedInstance, IdSpace currentIdSpace, bool ignoreExceptions)
        {
            if (IsOwner(completedInstance))
            {
                RegistrationContext registrationContext = new RegistrationContext(this, currentIdSpace);

                foreach (ExecutionProperty property in this.properties.Values)
                {
                    // We do a soft removal because we're about to throw away this dictionary
                    // and we don't want to mess up our enumerator
                    property.IsRemoved = true;

                    IPropertyRegistrationCallback registrationCallback = property.Property as IPropertyRegistrationCallback;

                    if (registrationCallback != null)
                    {
                        try
                        {
                            registrationCallback.Unregister(registrationContext);
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e) || !ignoreExceptions)
                            {
                                throw;
                            }
                        }
                    }
                }

                Fx.Assert(completedInstance == null || completedInstance.GetRawChildren() == null || completedInstance.GetRawChildren().Count == 0, "There must not be any children at this point otherwise our exclusive handle count would be incorrect.");

                // We still need to clear this list in case any non-serializable
                // properties were being used in a no persist zone
                this.properties.Clear();
            }
        }

        public void ThrowIfAlreadyDefined(string name, ActivityInstance executingInstance)
        {
            if (executingInstance == this.owningInstance)
            {
                if (this.properties.ContainsKey(name))
                {
                    throw FxTrace.Exception.Argument("name", SR.ExecutionPropertyAlreadyDefined(name));
                }
            }
        }

        public void OnDeserialized(ActivityInstance owner, ActivityInstance parent, IdSpace visibility, ActivityExecutor executor)
        {
            this.owningInstance = owner;

            if (parent != null)
            {
                if (parent.PropertyManager != null)
                {
                    this.threadProperties = parent.PropertyManager.threadProperties;
                }
            }
            else
            {
                this.rootPropertyManager = executor.RootPropertyManager;
            }

            foreach (ExecutionProperty property in this.properties.Values)
            {
                if (property.Property is IExecutionProperty)
                {
                    AddIExecutionProperty(property, true);
                }

                if (property.HasRestrictedVisibility)
                {
                    property.Visibility = visibility;
                }
            }
        }

        [DataContract]
        internal class ExecutionProperty
        {
            string name;
            object property;
            bool hasRestrictedVisibility;

            public ExecutionProperty(string name, object property, IdSpace visibility)
            {
                this.Name = name;
                this.Property = property;

                if (visibility != null)
                {
                    this.Visibility = visibility;
                    this.HasRestrictedVisibility = true;
                }
            }
            
            public string Name 
            {
                get
                {
                    return this.name;
                }
                private set
                {
                    this.name = value;
                }
            }
            
            public object Property
            {
                get
                {
                    return this.property;
                }
                private set
                {
                    this.property = value;
                }
            }
           
            public bool HasRestrictedVisibility
            {
                get
                {
                    return this.hasRestrictedVisibility;
                }
                private set
                {
                    this.hasRestrictedVisibility = value;
                }
            }

            // This property is fixed up at deserialization time
            public IdSpace Visibility
            {
                get;
                set;
            }

            // This is always false at persistence because
            // a removed property belongs to an activity which
            // has completed and is therefore not part of the 
            // instance map anymore
            public bool IsRemoved { get; set; }

            // These don't need to be serialized because they are only
            // ever false at persistence time.  We potentially set
            // them to true when a property is added but we always
            // reset them to false after cleaning up the thread
            public bool ShouldBeRemovedAfterCleanup { get; set; }
            public bool ShouldSkipNextCleanup { get; set; }

            [DataMember(Name = "Name")]
            internal string SerializedName
            {
                get { return this.Name; }
                set { this.Name = value; }
            }

            [DataMember(Name = "Property")]
            internal object SerializedProperty
            {
                get { return this.Property; }
                set { this.Property = value; }
            }

            [DataMember(EmitDefaultValue = false, Name = "HasRestrictedVisibility")]
            internal bool SerializedHasRestrictedVisibility
            {
                get { return this.HasRestrictedVisibility; }
                set { this.HasRestrictedVisibility = value; }
            }
        }
    }
}


