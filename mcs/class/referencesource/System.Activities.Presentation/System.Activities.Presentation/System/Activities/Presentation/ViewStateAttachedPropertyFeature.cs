//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.Runtime;
 
    abstract class ViewStateAttachedPropertyFeature : Feature
    {
        Type modelType;
        ViewStateService viewStateService;
        AttachedPropertiesService attachedPropertiesService;
        Dictionary<string, AttachedProperty> attachedProperties = new Dictionary<string, AttachedProperty>();

        public sealed override void Initialize(EditingContext context, Type modelType)
        {
            this.modelType = modelType;

            context.Services.Subscribe<ViewStateService>(delegate(ViewStateService viewStateService)
            {
                this.viewStateService = viewStateService;
                viewStateService.ViewStateChanged += this.OnViewStateChanged;
                if (this.attachedPropertiesService != null)
                {
                    RegisterAttachedProperties();
                }
            });
            context.Services.Subscribe<AttachedPropertiesService>(delegate(AttachedPropertiesService attachedPropertiesService)
            {
                this.attachedPropertiesService = attachedPropertiesService;
                if (this.viewStateService != null)
                {
                    RegisterAttachedProperties();
                }
            });
        }

        protected abstract IEnumerable<AttachedPropertyInfo> AttachedProperties
        {
            get;
        }

        internal void RegisterAttachedProperty<T>(string propertyName, bool isBrowsable, bool isVisibleToModelItem, T defaultValue)
        {
            AttachedProperty<T> attachedProperty = new AttachedProperty<T>
            {
                IsBrowsable = isBrowsable,
                IsVisibleToModelItem = isVisibleToModelItem,
                Name = propertyName,
                OwnerType = modelType,
                Getter = (modelItem) =>
                {
                    T result = (T)viewStateService.RetrieveViewState(modelItem, propertyName);
                    return result == null ? defaultValue : result;
                },
                Setter = (modelItem, value) =>
                {
                    if (value == null || value.Equals(defaultValue))
                    {
                        viewStateService.StoreViewStateWithUndo(modelItem, propertyName, null);
                    }
                    else
                    {
                        viewStateService.StoreViewStateWithUndo(modelItem, propertyName, value);
                    }
                }
            };
            attachedPropertiesService.AddProperty(attachedProperty);
            attachedProperties.Add(propertyName, attachedProperty);
        }

        void OnViewStateChanged(object sender, ViewStateChangedEventArgs e)
        {
            if (attachedProperties.ContainsKey(e.Key))
            {
                // Checking is required to avoid infinite loop of ViewState -> AttachedProperty -> ...
                if ((e.NewValue == null && e.OldValue != null) || !e.NewValue.Equals(e.OldValue))
                {
                    attachedProperties[e.Key].SetValue(e.ParentModelItem, e.NewValue);
                }
            }
        }

        void RegisterAttachedProperties()
        {
            foreach (AttachedPropertyInfo attachedPropertyInfo in this.AttachedProperties)
            {
                attachedPropertyInfo.Register(this);
            }
        }
    }
}
