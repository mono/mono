//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System;
    using System.Activities.Presentation.Services;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Text;

    class DictionaryChange : ModelChange
    {
        public ModelItemDictionary Dictionary { get; set; }

        public OperationType Operation { get; set; }

        public ModelItem Key { get; set; }

        public ModelItem Value { get; set; }

        public ModelTreeManager ModelTreeManager { get; set; }

        public override string Description
        {
            get
            {
                return this.Operation == OperationType.Insert ? SR.DictionaryAddEditingScopeDescription : SR.DictionaryRemoveEditingScopeDescription;
            }
        }

        public override bool Apply()
        {
            switch (this.Operation)
            {
                case OperationType.Insert:
                    ApplyInsert();
                    break;
                case OperationType.Delete:
                    ApplyDelete();
                    break;
                default:
                    Fx.Assert("Operation should be Insert or Delete");
                    break;
            }
            return true;
        }

        private void ApplyDelete()
        {
            ((ModelItemDictionaryImpl)this.Dictionary).RemoveCore(this.Key);

            ModelChangeInfo changeInfo = ModelChangeInfoImpl.CreateDictionaryKeyValueRemoved(this.Dictionary, this.Key, this.Value);

            if (this.Key != null)
            {
                this.ModelTreeManager.modelService.OnModelItemRemoved(this.Key, changeInfo);
                changeInfo = null;
            }

            if (this.Value != null)
            {
                this.ModelTreeManager.modelService.OnModelItemRemoved(this.Value, changeInfo);
                changeInfo = null;
            }

            if (changeInfo != null)
            {
                this.ModelTreeManager.modelService.EmitModelChangeInfo(changeInfo);
            }
        }

        private void ApplyInsert()
        {
            ((ModelItemDictionaryImpl)this.Dictionary).AddCore(this.Key, this.Value);

            ModelChangeInfo changeInfo = ModelChangeInfoImpl.CreateDictionaryKeyValueAdded(this.Dictionary, this.Key, this.Value);

            if (this.Key != null)
            {
                this.ModelTreeManager.modelService.OnModelItemAdded(this.Key, changeInfo);
                changeInfo = null;
            }

            if (this.Value != null)
            {
                this.ModelTreeManager.modelService.OnModelItemAdded(this.Value, changeInfo);
                changeInfo = null;
            }

            if (changeInfo != null)
            {
                this.ModelTreeManager.modelService.EmitModelChangeInfo(changeInfo);
            }
        }

        public override Change GetInverse()
        {
            OperationType reverseOperation = this.Operation == OperationType.Insert ? OperationType.Delete : OperationType.Insert;
            return new DictionaryChange()
                {
                    Dictionary = this.Dictionary,
                    Operation = reverseOperation,
                    Key = this.Key,
                    Value = this.Value,
                    ModelTreeManager = this.ModelTreeManager,
                };
        }

        public enum OperationType 
        { 
            Insert, Delete 
        }
    }
}
