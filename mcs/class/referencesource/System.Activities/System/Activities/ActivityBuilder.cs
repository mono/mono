//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities
{
    using System.Activities.Debugger;
    using System.Activities.Validation;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime;
    using System.Windows.Markup;
    using System.Xaml;

    [ContentProperty("Implementation")]
    public sealed class ActivityBuilder : IDebuggableWorkflowTree
    {
        // define attached properties that will identify PropertyReferenceExtension-based
        // object properties
        static AttachableMemberIdentifier propertyReferencePropertyID = new AttachableMemberIdentifier(typeof(ActivityBuilder), "PropertyReference");
        static AttachableMemberIdentifier propertyReferencesPropertyID = new AttachableMemberIdentifier(typeof(ActivityBuilder), "PropertyReferences");

        KeyedCollection<string, DynamicActivityProperty> properties;
        Collection<Constraint> constraints;
        Collection<Attribute> attributes;

        public ActivityBuilder()
        {
        }

        public string Name
        {
            get;
            set;
        }

        [DependsOn("Name")]
        public Collection<Attribute> Attributes
        {
            get
            {
                if (this.attributes == null)
                {
                    this.attributes = new Collection<Attribute>();
                }
                return this.attributes;
            }
        }

        [Browsable(false)]
        [DependsOn("Attributes")]
        public KeyedCollection<string, DynamicActivityProperty> Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = new ActivityPropertyCollection();
                }
                return this.properties;
            }
        }


        [DependsOn("Properties")]
        [Browsable(false)]
        public Collection<Constraint> Constraints
        {
            get
            {
                if (this.constraints == null)
                {
                    this.constraints = new Collection<Constraint>();
                }
                return this.constraints;
            }
        }

        [TypeConverter(typeof(ImplementationVersionConverter))]
        [DefaultValue(null)]
        [DependsOn("Name")]
        public Version ImplementationVersion
        {
            get;
            set;
        }

        [DefaultValue(null)]
        [Browsable(false)]
        [DependsOn("Constraints")]
        public Activity Implementation
        {
            get;
            set;
        }

        // Back-compat workaround: PropertyReference shipped in 4.0. PropertyReferences is new in 4.5.
        //
        // Requirements:
        // - Runtime compat: Get/SetPropertyReference needs to continue to work, both when set programatically
        //   and when loading a doc which contains only one PropertyReference on an object.
        // - Serialization compat: If only one PropertyReference was set, we shouldn't serialize PropertyReferences.
        //   (Only affects when ActivityBuilder is used directly with XamlServices, since ActivityXamlServices
        //   will convert ActivityPropertyReference to PropertyReferenceExtension.)
        // - Usability: To avoid the designer needing to support two separate access methods, we want
        //   the value from SetPropertyReference to also appear in the PropertyReferences collection.

        // <ActivityBuilder.PropertyReference>activity property name</ActivityBuilder.PropertyReference>
        public static ActivityPropertyReference GetPropertyReference(object target)
        {
            return GetPropertyReferenceCollection(target).SingleItem;
        }

        // <ActivityBuilder.PropertyReference>activity property name</ActivityBuilder.PropertyReference>
        public static void SetPropertyReference(object target, ActivityPropertyReference value)
        {
            GetPropertyReferenceCollection(target).SingleItem = value;
        }

        public static IList<ActivityPropertyReference> GetPropertyReferences(object target)
        {
            return GetPropertyReferenceCollection(target);
        }

        public static bool ShouldSerializePropertyReference(object target)
        {
            PropertyReferenceCollection propertyReferences = GetPropertyReferenceCollection(target);
            return propertyReferences.Count == 1 && propertyReferences.SingleItem != null;
        }

        public static bool ShouldSerializePropertyReferences(object target)
        {
            PropertyReferenceCollection propertyReferences = GetPropertyReferenceCollection(target);
            return propertyReferences.Count > 1 || propertyReferences.SingleItem == null;
        }

        internal static bool HasPropertyReferences(object target)
        {
            PropertyReferenceCollection propertyReferences;
            if (AttachablePropertyServices.TryGetProperty(target, propertyReferencesPropertyID, out propertyReferences))
            {
                return propertyReferences.Count > 0;
            }
            return false;
        }

        static PropertyReferenceCollection GetPropertyReferenceCollection(object target)
        {
            PropertyReferenceCollection propertyReferences;
            if (!AttachablePropertyServices.TryGetProperty(target, propertyReferencesPropertyID, out propertyReferences))
            {
                propertyReferences = new PropertyReferenceCollection(target);
                AttachablePropertyServices.SetProperty(target, propertyReferencesPropertyID, propertyReferences);
            }
            return propertyReferences;

        }

        Activity IDebuggableWorkflowTree.GetWorkflowRoot()
        {
            return this.Implementation;
        }

        internal static KeyedCollection<string, DynamicActivityProperty> CreateActivityPropertyCollection()
        {
            return new ActivityPropertyCollection();
        }

        class ActivityPropertyCollection : KeyedCollection<string, DynamicActivityProperty>
        {
            protected override string GetKeyForItem(DynamicActivityProperty item)
            {
                return item.Name;
            }
        }

        // See back-compat requirements in comment above. Design is:
        // - First value added to collection when it is empty becomes the single PropertyReference value
        // - If the single value is removed, then PropertyReference AP is removed
        // - If PropertyReference AP is set to null, we remove the single value.
        // - If PropertyReference is set to non-null, we replace the existing single value if there
        //    is one, or else add the new value to the collection.
        class PropertyReferenceCollection : Collection<ActivityPropertyReference>
        {
            WeakReference targetObject;
            int singleItemIndex = -1;

            public PropertyReferenceCollection(object target)
            {
                this.targetObject = new WeakReference(target);
            }

            public ActivityPropertyReference SingleItem
            {
                get
                {
                    return this.singleItemIndex >= 0 ? this[this.singleItemIndex] : null;
                }
                set
                {
                    if (this.singleItemIndex >= 0)
                    {
                        if (value != null)
                        {
                            SetItem(this.singleItemIndex, value);
                        }
                        else
                        {
                            RemoveItem(this.singleItemIndex);
                        }
                    }
                    else if (value != null)
                    {
                        Add(value);
                        if (Count > 1)
                        {
                            this.singleItemIndex = Count - 1;
                            UpdateAttachedProperty();
                        }
                    }
                }
            }

            protected override void ClearItems()
            {
                this.singleItemIndex = -1;
                UpdateAttachedProperty();
            }

            protected override void InsertItem(int index, ActivityPropertyReference item)
            {
                base.InsertItem(index, item);
                if (index <= this.singleItemIndex)
                {
                    this.singleItemIndex++;
                }
                else if (Count == 1)
                {
                    Fx.Assert(this.singleItemIndex < 0, "How did we have an index if we were empty?");
                    this.singleItemIndex = 0;
                    UpdateAttachedProperty();
                }
            }

            protected override void RemoveItem(int index)
            {
                base.RemoveItem(index);
                if (index < this.singleItemIndex)
                {
                    this.singleItemIndex--;
                }
                else if (index == this.singleItemIndex)
                {
                    this.singleItemIndex = -1;
                    UpdateAttachedProperty();
                }
            }

            protected override void SetItem(int index, ActivityPropertyReference item)
            {
                base.SetItem(index, item);
                if (index == this.singleItemIndex)
                {
                    UpdateAttachedProperty();
                }
            }

            void UpdateAttachedProperty()
            {
                object target = this.targetObject.Target;
                if (target != null)
                {
                    if (this.singleItemIndex >= 0)
                    {
                        AttachablePropertyServices.SetProperty(target, propertyReferencePropertyID, this[this.singleItemIndex]);
                    }
                    else
                    {
                        AttachablePropertyServices.RemoveProperty(target, propertyReferencePropertyID);
                    }
                }
            }
        }
    }

    [ContentProperty("Implementation")]
    public sealed class ActivityBuilder<TResult> : IDebuggableWorkflowTree
    {
        KeyedCollection<string, DynamicActivityProperty> properties;
        Collection<Constraint> constraints;
        Collection<Attribute> attributes;

        public ActivityBuilder()
        {
        }

        public string Name
        {
            get;
            set;
        }

        [DependsOn("Name")]
        public Collection<Attribute> Attributes
        {
            get
            {
                if (this.attributes == null)
                {
                    this.attributes = new Collection<Attribute>();
                }
                return this.attributes;
            }
        }

        [Browsable(false)]
        [DependsOn("Attributes")]
        public KeyedCollection<string, DynamicActivityProperty> Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = ActivityBuilder.CreateActivityPropertyCollection();
                }
                return this.properties;
            }
        }

        [DependsOn("Properties")]
        [Browsable(false)]
        public Collection<Constraint> Constraints
        {
            get
            {
                if (this.constraints == null)
                {
                    this.constraints = new Collection<Constraint>();
                }
                return this.constraints;
            }
        }

        [TypeConverter(typeof(ImplementationVersionConverter))]
        [DefaultValue(null)]
        [DependsOn("Name")]
        public Version ImplementationVersion
        {
            get;
            set;
        }

        [DefaultValue(null)]
        [Browsable(false)]
        [DependsOn("Constraints")]
        public Activity Implementation
        {
            get;
            set;
        }

        Activity IDebuggableWorkflowTree.GetWorkflowRoot()
        {
            return this.Implementation;
        }
    }

}
