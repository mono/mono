//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System.Runtime;
    using System.Activities.Presentation.Services;

    class CollectionChange : ModelChange
    {
        public ModelItemCollection Collection { get; set; }

        public int Index { get; set; }

        public ModelItem Item { get; set; }

        public OperationType Operation { get; set; }

        public ModelTreeManager ModelTreeManager { get; set; }

        public override string Description
        {
            get
            {
                return this.Operation == OperationType.Insert ? SR.CollectionAddEditingScopeDescription : SR.CollectionRemoveEditingScopeDescription;
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
            Fx.Assert(this.ModelTreeManager != null, "ModelTreeManager cannot be null.");
            Fx.Assert(this.Collection != null, "this.Collection cannot be null.");

            if (this.Index >= 0)
            {
                ((ModelItemCollectionImpl)this.Collection).RemoveAtCore(this.Index);
            }
            else
            {
                Fx.Assert(this.Index == -1, "-1 must be used to indicate Remove(item)");
                this.Index = this.Collection.IndexOf(this.Item);
                ((ModelItemCollectionImpl)this.Collection).RemoveCore(this.Item);
            }

            ModelChangeInfo changeInfo = ModelChangeInfoImpl.CreateCollectionItemRemoved(this.Collection, this.Item);
            this.ModelTreeManager.NotifyCollectionRemove(this.Item, changeInfo);
        }

        private void ApplyInsert()
        {
            Fx.Assert(this.ModelTreeManager != null, "ModelTreeManager cannot be null.");
            Fx.Assert(this.Collection != null, "this.Collection cannot be null.");

            if (this.Index >= 0)
            {
                ((ModelItemCollectionImpl)this.Collection).InsertCore(this.Index, this.Item);
            }
            else
            {
                Fx.Assert(this.Index == -1, "-1 must be used to indicate Add(item)");
                this.Index = this.Collection.Count;
                ((ModelItemCollectionImpl)this.Collection).AddCore(this.Item);
            }

            ModelChangeInfo changeInfo = ModelChangeInfoImpl.CreateCollectionItemAdded(this.Collection, this.Item);
            this.ModelTreeManager.NotifyCollectionInsert(this.Item, changeInfo);
        }

        public override Change GetInverse()
        {
            OperationType reverseOperation = this.Operation == OperationType.Insert ? OperationType.Delete : OperationType.Insert;
            return new CollectionChange()
                {
                    Collection = this.Collection,
                    Operation = reverseOperation,
                    Item = this.Item,
                    ModelTreeManager = this.ModelTreeManager,
                    Index = this.Index
                };

        }

        public enum OperationType 
        { 
            Insert, Delete
        }
    }
}
