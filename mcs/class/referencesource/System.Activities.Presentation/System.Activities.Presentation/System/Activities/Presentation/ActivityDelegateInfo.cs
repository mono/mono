//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;

    internal class ActivityDelegateInfo : IActivityDelegateFactory
    {
        public ActivityDelegateInfo(ModelItem modelItem, string propertyName)
        {
            this.ModelItem = modelItem;
            this.PropertyName = propertyName;
            this.DelegateType = this.ModelItem.Properties[this.PropertyName].PropertyType;
        }

        public ModelItem ModelItem { get; private set; }

        public string PropertyName { get; private set; }
        
        public Type DelegateType { get; private set; }

        public EditingContext EditingContext
        {
            get
            {
                return this.ModelItem.GetEditingContext();
            }
        }

        public ActivityDelegate Create()
        {
            ActivityDelegate delegateObject = Activator.CreateInstance(this.DelegateType) as ActivityDelegate;
            ActivityDelegateMetadata metadata = ActivityDelegateUtilities.GetMetadata(this.DelegateType);
            ActivityDelegateUtilities.FillDelegate(delegateObject, metadata);
            return delegateObject;
        }
    }
}
