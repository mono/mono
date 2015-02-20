//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Activities.Presentation.Services;

    class DictionaryEditChange : ModelChange
    {
        public ModelItemDictionary Dictionary { get; set; }

        public ModelItem Key { get; set; }

        public ModelItem OldValue { get; set; }

        public ModelItem NewValue { get; set; }

        public ModelTreeManager ModelTreeManager { get; set; }


        public override string Description
        {
            get 
            {
                return SR.DictionaryEditEditingScopeDescription;
            }
        }

        public override bool Apply()
        {
            ModelItem oldValue = this.Dictionary[this.Key];
            if ((oldValue == null && this.NewValue == null) ||
               (oldValue != null && this.NewValue != null && oldValue.GetCurrentValue().Equals(this.NewValue.GetCurrentValue())))
            {
                return false;
            }

            ((ModelItemDictionaryImpl)this.Dictionary).EditCore(this.Key, this.NewValue);

            ModelChangeInfo changeInfo = ModelChangeInfoImpl.CreateDictionaryValueChanged(this.Dictionary, this.Key, this.OldValue, this.NewValue);

            if (this.OldValue != null)
            {
                this.ModelTreeManager.modelService.OnModelItemRemoved(this.OldValue, changeInfo);
                changeInfo = null;
            }
            if (this.NewValue != null)
            {
                this.ModelTreeManager.modelService.OnModelItemAdded(this.NewValue, changeInfo);
            }
            return true;
        }



        public override Change GetInverse()
        {
            return new DictionaryEditChange()
                {
                    Dictionary = this.Dictionary,
                    Key = this.Key,
                    OldValue = this.NewValue,
                    NewValue = this.OldValue,
                    ModelTreeManager = this.ModelTreeManager,
                };
        }
    }
}
