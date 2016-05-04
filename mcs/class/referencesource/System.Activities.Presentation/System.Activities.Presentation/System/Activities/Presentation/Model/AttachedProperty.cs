//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Activities.Presentation.Model;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;

    public abstract class AttachedProperty
    {
        Type ownerType = typeof(object);

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "By design.")]
        public abstract Type Type
        {
            get;
        }

        public abstract bool IsReadOnly
        {
            get;
        }

        public bool IsBrowsable
        {
            get;
            set;
        }

        internal bool IsVisibleToModelItem
        {
            get;
            set;
        }

        public string Name
        { 
            get; set; 
        }

        public Type OwnerType
        {
            get
            {
                return this.ownerType;
            }
            set
            {
                this.ownerType = value;
            }
        }

        public abstract object GetValue(ModelItem modelItem);
        public abstract void SetValue(ModelItem modelItem, object value);
        public abstract void ResetValue(ModelItem modelItem);

        public void NotifyPropertyChanged(ModelItem modelItem)
        {
            if (null != modelItem)
            {
                ((IModelTreeItem)modelItem).OnPropertyChanged(this.Name);
            }
        }
    }

    
    public class AttachedProperty<T> : AttachedProperty
    {
        [Fx.Tag.KnownXamlExternal]
        public Func<ModelItem, T> Getter
        { 
            get; set; 
        }

        [Fx.Tag.KnownXamlExternal]
        public Action<ModelItem, T> Setter
        { 
            get; set; 
        }

        public override Type Type
        {
            get { return typeof(T); }
        }

        public override bool IsReadOnly
        {
            get { return (this.Setter == null); }
        }

        public override object GetValue(ModelItem modelItem)
        {
            return this.Getter(modelItem);
        }

        public override void SetValue(ModelItem modelItem, object Value)
        {
            if (this.IsReadOnly)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.PropertyIsReadOnly));
            }
            this.Setter(modelItem, (T)Value);
            this.NotifyPropertyChanged(modelItem);
        }

        public override void ResetValue(ModelItem modelItem)
        {
            SetValue(modelItem, default(T));
        }
    }
}
