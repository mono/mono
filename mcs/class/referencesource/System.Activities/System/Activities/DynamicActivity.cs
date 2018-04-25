//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Activities.Validation;
    using System.Activities.XamlIntegration;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime;
    using System.Windows.Markup;
    using System.Xaml;

    [ContentProperty("Implementation")]
    public sealed class DynamicActivity : Activity, ICustomTypeDescriptor, IDynamicActivity
    {
        Activity runtimeImplementation;
        DynamicActivityTypeDescriptor typeDescriptor;
        Collection<Attribute> attributes;

        public DynamicActivity()
            : base()
        {
            this.typeDescriptor = new DynamicActivityTypeDescriptor(this);
        }

        public string Name
        {
            get
            {
                return this.typeDescriptor.Name;
            }
            set
            {
                this.typeDescriptor.Name = value;
            }
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
                return this.typeDescriptor.Properties; 
            }
        }

        [DependsOn("Properties")]
        public new Collection<Constraint> Constraints
        {
            get
            {              
                return base.Constraints;
            }
        }

        [TypeConverter(typeof(ImplementationVersionConverter))]
        [DefaultValue(null)]
        public new Version ImplementationVersion
        {
            get
            {
                return base.ImplementationVersion;
            }
            set
            {
                base.ImplementationVersion = value;
            }
        }

        [XamlDeferLoad(typeof(FuncDeferringLoader), typeof(Activity))]
        [DefaultValue(null)]
        [Browsable(false)]
        [Ambient]
        public new Func<Activity> Implementation
        {
            get
            {
                return base.Implementation;
            }
            set
            {
                base.Implementation = value;
            }
        }

        KeyedCollection<string, DynamicActivityProperty> IDynamicActivity.Properties
        {
            get
            {
                return this.Properties;
            }
        }

        internal override void InternalExecute(ActivityInstance instance, ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            if (this.runtimeImplementation != null)
            {
                executor.ScheduleActivity(this.runtimeImplementation, instance, null, null, null);
            }
        }

        sealed internal override void OnInternalCacheMetadata(bool createEmptyBindings)
        {
            Activity body = null;
            if (this.Implementation != null)
            {
                body = this.Implementation();
            }

            if (body != null)
            {
                SetImplementationChildrenCollection(new Collection<Activity> { body });
            }

            // Always cache the last body that we returned
            this.runtimeImplementation = body;

            ReflectedInformation information = new ReflectedInformation(this);

            SetImportedChildrenCollection(information.GetChildren());
            SetVariablesCollection(information.GetVariables());
            SetImportedDelegatesCollection(information.GetDelegates());
            SetArgumentsCollection(information.GetArguments(), createEmptyBindings);
        }

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return this.typeDescriptor.GetAttributes();
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return this.typeDescriptor.GetClassName();
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return this.typeDescriptor.GetComponentName();
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return this.typeDescriptor.GetConverter();
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return this.typeDescriptor.GetDefaultEvent();
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return this.typeDescriptor.GetDefaultProperty();
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return this.typeDescriptor.GetEditor(editorBaseType);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return this.typeDescriptor.GetEvents(attributes);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return this.typeDescriptor.GetEvents();
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return this.typeDescriptor.GetProperties();
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return this.typeDescriptor.GetProperties(attributes);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this.typeDescriptor.GetPropertyOwner(pd);
        }
    }

    [ContentProperty("Implementation")]
    public sealed class DynamicActivity<TResult> : Activity<TResult>, ICustomTypeDescriptor, IDynamicActivity
    {
        Activity runtimeImplementation;
        DynamicActivityTypeDescriptor typeDescriptor;
        Collection<Attribute> attributes;

        public DynamicActivity()
            : base()
        {
            this.typeDescriptor = new DynamicActivityTypeDescriptor(this);
        }

        public string Name
        {
            get
            {
                return this.typeDescriptor.Name;
            }
            set
            {
                this.typeDescriptor.Name = value;
            }
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
                return this.typeDescriptor.Properties;
            }
        }

        [DependsOn("Properties")]
        public new Collection<Constraint> Constraints
        {
            get
            {
                return base.Constraints;
            }
        }

        [TypeConverter(typeof(ImplementationVersionConverter))]
        [DefaultValue(null)]
        public new Version ImplementationVersion
        {
            get
            {
                return base.ImplementationVersion;
            }
            set
            {
                base.ImplementationVersion = value;
            }
        }

        [XamlDeferLoad(typeof(FuncDeferringLoader), typeof(Activity))]
        [DefaultValue(null)]
        [Browsable(false)]
        [Ambient]
        public new Func<Activity> Implementation
        {
            get
            {
                return base.Implementation;
            }
            set
            {
                base.Implementation = value;
            }
        }

        KeyedCollection<string, DynamicActivityProperty> IDynamicActivity.Properties
        {
            get
            {
                return this.Properties;
            }
        }

        internal override void InternalExecute(ActivityInstance instance, ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            if (this.runtimeImplementation != null)
            {
                executor.ScheduleActivity(this.runtimeImplementation, instance, null, null, null);
            }
        }

        sealed internal override void OnInternalCacheMetadataExceptResult(bool createEmptyBindings)
        {
            Activity body = null;
            if (this.Implementation != null)
            {
                body = this.Implementation();
            }

            if (body != null)
            {
                SetImplementationChildrenCollection(new Collection<Activity> { body });
            }

            // Always cache the last body that we returned
            this.runtimeImplementation = body;

            ReflectedInformation information = new ReflectedInformation(this);

            SetImportedChildrenCollection(information.GetChildren());
            SetVariablesCollection(information.GetVariables());
            SetImportedDelegatesCollection(information.GetDelegates());
            SetArgumentsCollection(information.GetArguments(), createEmptyBindings);
        }       

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return this.typeDescriptor.GetAttributes();
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return this.typeDescriptor.GetClassName();
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return this.typeDescriptor.GetComponentName();
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return this.typeDescriptor.GetConverter();
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return this.typeDescriptor.GetDefaultEvent();
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return this.typeDescriptor.GetDefaultProperty();
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return this.typeDescriptor.GetEditor(editorBaseType);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return this.typeDescriptor.GetEvents(attributes);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return this.typeDescriptor.GetEvents();
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return this.typeDescriptor.GetProperties();
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return this.typeDescriptor.GetProperties(attributes);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this.typeDescriptor.GetPropertyOwner(pd);
        }
    }
}


