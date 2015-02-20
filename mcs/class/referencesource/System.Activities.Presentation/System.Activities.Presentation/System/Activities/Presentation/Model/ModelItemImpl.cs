//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Dynamic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Markup;



    // This class provides the View facing ModelItem implementation, this works with the ModelTreeManager
    // and keeps the xaml up to date by intercepting every change to the model properties.

    class ModelItemImpl : ModelItem, IModelTreeItem, ICustomTypeDescriptor, IDynamicMetaObjectProvider
    {
        ModelProperty contentProperty;
        object instance;
        Type itemType;
        Dictionary<string, ModelItem> modelPropertyStore;
        ModelTreeManager modelTreeManager;
        ModelProperty nameProperty;
        internal ObservableCollection<ModelItem> parents;
        ReadOnlyObservableCollection<ModelItem> internalParents;
        ModelPropertyCollectionImpl properties;
        ObservableCollection<ModelProperty> sources;
        ModelTreeItemHelper helper;
        ReadOnlyObservableCollection<ModelProperty> internalSources;
        List<ModelItem> subTreeNodesThatNeedBackLinkPatching;
        DependencyObject view;
        ModelItem manuallySetParent;


        public ModelItemImpl(ModelTreeManager modelTreeManager, Type itemType, object instance, ModelItem parent)
        {
            this.itemType = itemType;
            this.instance = instance;
            this.modelTreeManager = modelTreeManager;
            this.parents = new ObservableCollection<ModelItem>();
            this.internalParents = new ReadOnlyObservableCollection<ModelItem>(parents);
            this.sources = new ObservableCollection<ModelProperty>();
            this.helper = new ModelTreeItemHelper();
            this.internalSources = new ReadOnlyObservableCollection<ModelProperty>(sources);
            if (parent != null)
            {
                this.manuallySetParent = parent;
            }
            this.modelPropertyStore = new Dictionary<string, ModelItem>();
            this.subTreeNodesThatNeedBackLinkPatching = new List<ModelItem>();
        }



        public override event PropertyChangedEventHandler PropertyChanged;

        public override global::System.ComponentModel.AttributeCollection Attributes
        {
            get
            {
                return TypeDescriptor.GetAttributes(itemType);
            }
        }

        public override ModelProperty Content
        {
            get
            {

                if (this.contentProperty == null)
                {
                    Fx.Assert(this.instance != null, "instance should not be null");

                    ContentPropertyAttribute contentAttribute = TypeDescriptor.GetAttributes(this.instance)[typeof(ContentPropertyAttribute)] as ContentPropertyAttribute;
                    if (contentAttribute != null && !String.IsNullOrEmpty(contentAttribute.Name))
                    {
                        this.contentProperty = this.Properties.Find(contentAttribute.Name);
                    }
                }
                return contentProperty;
            }
        }

        public override Type ItemType
        {
            get
            {
                return this.itemType;
            }
        }

        public ModelItem ModelItem
        {
            get
            {
                return this;
            }
        }

        public override string Name
        {
            get
            {
                string name = null;
                if ((this.NameProperty != null) && (this.NameProperty.Value != null))
                {
                    name = (string)this.NameProperty.Value.GetCurrentValue();
                }
                return name;
            }
            set
            {
                if (this.NameProperty != null)
                {
                    this.NameProperty.SetValue(value);
                }
            }
        }

        public override ModelItem Parent
        {
            get
            {
                return (this.Parents.Count() > 0) ? this.Parents.First() : null;
            }

        }

        public override ModelPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    properties = new ModelPropertyCollectionImpl(this);
                }
                return properties;
            }
        }

        public override ModelItem Root
        {
            get
            {
                return this.modelTreeManager.Root;
            }
        }


        // This holds a reference to the modelproperty that is currently holding this ModelItem.
        public override ModelProperty Source
        {
            get
            {
                return (this.sources.Count > 0) ? this.sources.First() : null;
            }
        }

        public override DependencyObject View
        {
            get
            {
                return this.view;
            }
        }

        public override IEnumerable<ModelItem> Parents
        {
            get
            {
                if (this.manuallySetParent != null)
                {
                    List<ModelItem> list = new List<ModelItem>();
                    list.Add(this.manuallySetParent);
                    return list.Concat(this.parents).Concat(
                        from source in this.sources
                        select source.Parent);
                }

                return this.parents.Concat(
                    from source in this.sources
                    select source.Parent);
            }
        }

        internal ReadOnlyObservableCollection<ModelItem> InternalParents
        {
            get
            {
                return internalParents;
            }
        }

        public override IEnumerable<ModelProperty> Sources
        {
            get
            {
                return this.sources;
            }
        }

        internal ReadOnlyObservableCollection<ModelProperty> InternalSources
        {
            get
            {
                return internalSources;
            }
        }

        protected ModelProperty NameProperty
        {
            get
            {
                if (this.nameProperty == null)
                {
                    Fx.Assert(this.instance != null, "instance should not be null");

                    RuntimeNamePropertyAttribute runtimeNamePropertyAttribute = TypeDescriptor.GetAttributes(this.instance)[typeof(RuntimeNamePropertyAttribute)] as RuntimeNamePropertyAttribute;
                    if (runtimeNamePropertyAttribute != null && !String.IsNullOrEmpty(runtimeNamePropertyAttribute.Name))
                    {
                        this.nameProperty = this.Properties.Find(runtimeNamePropertyAttribute.Name);
                    }
                }
                return nameProperty;
            }
        }


        Dictionary<string, ModelItem> IModelTreeItem.ModelPropertyStore
        {
            get
            {
                return modelPropertyStore;
            }
        }

        ModelTreeManager IModelTreeItem.ModelTreeManager
        {
            get
            {
                return modelTreeManager;
            }
        }

        void IModelTreeItem.SetCurrentView(DependencyObject view)
        {
            this.view = view;
        }

        public override ModelEditingScope BeginEdit(string description, bool shouldApplyChangesImmediately)
        {
            return ModelItemHelper.ModelItemBeginEdit(this.modelTreeManager, description, shouldApplyChangesImmediately);
        }

        public override ModelEditingScope BeginEdit(bool shouldApplyChangesImmediately)
        {
            return this.BeginEdit(null, shouldApplyChangesImmediately);
        }

        public override ModelEditingScope BeginEdit(string description)
        {
            return this.BeginEdit(description, false);
        }

        public override ModelEditingScope BeginEdit()
        {
            return this.BeginEdit(null);
        }

        public override object GetCurrentValue()
        {
            return this.instance;
        }

        void IModelTreeItem.OnPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(propertyName);
        }


        void IModelTreeItem.SetParent(ModelItem dataModelItem)
        {
            if (this.manuallySetParent == dataModelItem)
            {
                this.manuallySetParent = null;
            }

            if (dataModelItem != null && !this.parents.Contains(dataModelItem))
            {
                this.parents.Add(dataModelItem);
            }
        }

        IEnumerable<ModelItem> IModelTreeItem.ItemBackPointers
        {
            get { return this.parents; }
        }

        List<BackPointer> IModelTreeItem.ExtraPropertyBackPointers
        {
            get { return this.helper.ExtraPropertyBackPointers; }
        }

        void IModelTreeItem.SetSource(ModelProperty property)
        {
            if (!this.sources.Contains(property))
            {
                // also check if the same parent.property is in the list as a different instance of oldModelProperty
                ModelProperty foundProperty = sources.FirstOrDefault<ModelProperty>((modelProperty) =>
                    modelProperty.Name.Equals(property.Name) && property.Parent == modelProperty.Parent);
                if (foundProperty == null)
                {
                    this.sources.Add(property);
                }
            }
        }

        #region ICustomTypeDescriptor Members

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return this.Attributes;
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return TypeDescriptor.GetClassName(this);
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return ModelUtilities.GetConverter(this);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            // we dont support events;
            return null;
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return ModelUtilities.GetDefaultProperty(this);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            // we dont support editors
            return null;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            // we dont support events;
            return null;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            // we dont support events;
            return null;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return ModelUtilities.WrapProperties(this);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            // get model properties
            List<PropertyDescriptor> properties = new List<PropertyDescriptor>();


            foreach (PropertyDescriptor modelPropertyDescriptor in ModelUtilities.WrapProperties(this))
            {
                properties.Add(modelPropertyDescriptor);
            }

            // try to see if there are pseudo builtin properties for this type.
            AttachedPropertiesService AttachedPropertiesService = this.modelTreeManager.Context.Services.GetService<AttachedPropertiesService>();
            if (AttachedPropertiesService != null)
            {
                var nonBrowsableAttachedProperties = from attachedProperty in AttachedPropertiesService.GetAttachedProperties(this.itemType)
                                                     where (!attachedProperty.IsBrowsable && !attachedProperty.IsVisibleToModelItem)
                                                     select attachedProperty;

                foreach (AttachedProperty AttachedProperty in nonBrowsableAttachedProperties)
                {
                    properties.Add(new AttachedPropertyDescriptor(AttachedProperty, this));
                }
            }
            return new PropertyDescriptorCollection(properties.ToArray(), true);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion


        void IModelTreeItem.RemoveParent(ModelItem oldParent)
        {
            if (this.manuallySetParent == oldParent)
            {
                this.manuallySetParent = null;
            }

            if (this.parents.Contains(oldParent))
            {
                this.parents.Remove(oldParent);
            }
        }

        void IModelTreeItem.RemoveSource(ModelProperty oldModelProperty)
        {
            if (this.sources.Contains(oldModelProperty))
            {
                this.sources.Remove(oldModelProperty);
            }
            else
            {
                ((IModelTreeItem)this).RemoveSource(oldModelProperty.Parent, oldModelProperty.Name);
            }
        }

        void IModelTreeItem.RemoveSource(ModelItem parent, string propertyName)
        {
            // also check if the same parent.property is in the list as a different instance of oldModelProperty
            ModelProperty foundProperty = this.sources.FirstOrDefault<ModelProperty>((modelProperty) => modelProperty.Name.Equals(propertyName) && modelProperty.Parent == parent);
            if (foundProperty != null)
            {
                this.sources.Remove(foundProperty);
            }
            else
            {
                this.helper.RemoveExtraPropertyBackPointer(parent, propertyName);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(System.Linq.Expressions.Expression parameter)
        {
            return new ModelItemMetaObject(parameter, this);
        }

        public object SetPropertyValue(string propertyName, object val)
        {
            ModelProperty modelProperty = this.Properties.Find(propertyName);
            if (modelProperty != null)
            {
                modelProperty.SetValue(val);
            }
            else
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this)[propertyName];
                if (descriptor != null)
                {
                    descriptor.SetValue(this, val);
                }
            }
            return GetPropertyValue(propertyName);
        }

        public object GetPropertyValue(string propertyName)
        {
            ModelProperty modelProperty = this.Properties.Find(propertyName);
            object value = null;
            if (modelProperty != null)
            {
                ModelItem valueModelitem = modelProperty.Value;
                if (valueModelitem == null)
                {
                    value = null;
                }
                else
                {
                    Type itemType = valueModelitem.ItemType;
                    if (itemType.IsPrimitive || itemType.IsEnum || itemType.Equals(typeof(String)))
                    {
                        value = valueModelitem.GetCurrentValue();
                    }
                    else
                    {
                        value = valueModelitem;
                    }
                }

            }
            else
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this)[propertyName];
                if (descriptor != null)
                {
                    value = descriptor.GetValue(this);
                }
            }
            return value;
        }

        class ModelItemMetaObject : System.Dynamic.DynamicMetaObject
        {
            MethodInfo getPropertyValueMethodInfo = typeof(ModelItemImpl).GetMethod("GetPropertyValue");
            MethodInfo setPropertyValueMethodInfo = typeof(ModelItemImpl).GetMethod("SetPropertyValue");

            public ModelItemMetaObject(System.Linq.Expressions.Expression parameter, ModelItemImpl target)
                : base(parameter, BindingRestrictions.Empty, target)
            {
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                System.Linq.Expressions.Expression s = System.Linq.Expressions.Expression.Convert(this.Expression, typeof(ModelItemImpl));
                System.Linq.Expressions.Expression value = System.Linq.Expressions.Expression.Call(s, getPropertyValueMethodInfo, System.Linq.Expressions.Expression.Constant(binder.Name));
                return new DynamicMetaObject(value, BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType));
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
                System.Linq.Expressions.Expression s = System.Linq.Expressions.Expression.Convert(this.Expression, typeof(ModelItemImpl));
                System.Linq.Expressions.Expression objectValue = System.Linq.Expressions.Expression.Convert(value.Expression, typeof(object));
                System.Linq.Expressions.Expression valueExp = System.Linq.Expressions.Expression.Call(s, setPropertyValueMethodInfo, System.Linq.Expressions.Expression.Constant(binder.Name), objectValue);
                return new DynamicMetaObject(valueExp, BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType));
            }
        }



    }







}
