//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System.Xaml;

    internal class AttachablePropertyChange : ModelChange
    {
        public ModelItem Owner { get; set; }
        
        public AttachableMemberIdentifier AttachablePropertyIdentifier { get; set; }
        
        public object OldValue { get; set; }
        
        public object NewValue { get; set; }
        
        public string PropertyName { get; set; }

        public override string Description
        {
            get { return SR.PropertyChangeEditingScopeDescription; }
        }

        public override bool Apply()
        {
            if (this.NewValue == null)
            {
                AttachablePropertyServices.RemoveProperty(this.Owner.GetCurrentValue(), this.AttachablePropertyIdentifier);
            }
            else
            {
                AttachablePropertyServices.SetProperty(this.Owner.GetCurrentValue(), this.AttachablePropertyIdentifier, this.NewValue);
            }

            // notify observer
            if (!string.IsNullOrEmpty(this.PropertyName))
            {
                ((IModelTreeItem)this.Owner).OnPropertyChanged(this.PropertyName);
            }

            return true;
        }

        public override Change GetInverse()
        {
            return new AttachablePropertyChange()
            {
                Owner = this.Owner,
                AttachablePropertyIdentifier = this.AttachablePropertyIdentifier,
                OldValue = this.NewValue,
                NewValue = this.OldValue,
                PropertyName = this.PropertyName
            };
        }
    }
}
