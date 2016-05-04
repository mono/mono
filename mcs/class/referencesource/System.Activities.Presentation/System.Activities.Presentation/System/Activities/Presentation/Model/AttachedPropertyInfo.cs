//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System.Activities.Presentation;

    public abstract class AttachedPropertyInfo
    {
        bool isBrowsable = true;
        bool? isVisibleToModelItem;

        public string PropertyName { get; set; }

        internal bool IsBrowsable
        {
            get { return isBrowsable; }
            set { this.isBrowsable = value; }
        }

        internal bool IsVisibleToModelItem
        {
            get { return this.isVisibleToModelItem.HasValue ? this.isVisibleToModelItem.Value : this.isBrowsable; }
            set { this.isVisibleToModelItem = value; }
        }

        internal abstract void Register(ViewStateAttachedPropertyFeature viewStateAttachedPropertyFeature);
    }

    public sealed class AttachedPropertyInfo<T> : AttachedPropertyInfo
    {
        public T DefaultValue { get; set; }

        internal override void Register(ViewStateAttachedPropertyFeature viewStateAttachedPropertyFeature)
        {
            viewStateAttachedPropertyFeature.RegisterAttachedProperty<T>(this.PropertyName, this.IsBrowsable, this.IsVisibleToModelItem, this.DefaultValue);
        }
    }
}
