//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Globalization;
    using System.Runtime;
    using System.Activities.Presentation.Services;

    class PropertyChange : ModelChange
    {
        public ModelItem Owner { get; set; }

        public string PropertyName { get; set; }

        public ModelItem OldValue { get; set; }

        public ModelItem NewValue { get; set; }

        public ModelTreeManager ModelTreeManager { get; set; }

        public override string Description
        {
            get 
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} - {1}", SR.PropertyChangeEditingScopeDescription, this.PropertyName); 
            }
        }

        public override bool Apply()
        {
            Fx.Assert(this.ModelTreeManager != null, "Modeltreemanager cannot be null");
            Fx.Assert(this.Owner != null, "Owner modelitem cannot be null");
            Fx.Assert(!String.IsNullOrEmpty(this.PropertyName), " property name cannot be null or emptry");
            ModelPropertyImpl dataModelProperty = (ModelPropertyImpl)this.Owner.Properties[this.PropertyName];
            ModelItem oldValue = dataModelProperty.Value;
            if ((oldValue == null && this.NewValue == null) ||
                (oldValue != null && this.NewValue != null && oldValue.GetCurrentValue().Equals(this.NewValue.GetCurrentValue())))
            {
                return false;
            }
            dataModelProperty.SetValueCore(this.NewValue);
            ModelChangeInfo changeInfo = ModelChangeInfoImpl.CreatePropertyChanged(this.Owner, this.PropertyName, this.OldValue, this.NewValue);
            this.ModelTreeManager.NotifyPropertyChange(dataModelProperty, changeInfo);
            return true;
        }

        public override Change GetInverse()
        {
            return new PropertyChange()
                {
                    ModelTreeManager = this.ModelTreeManager,
                    Owner = this.Owner,
                    OldValue = this.NewValue,
                    NewValue = this.OldValue,
                    PropertyName = this.PropertyName
                };
        }
    }
}
