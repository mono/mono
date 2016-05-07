//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Activities.Presentation.Model;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    public class ViewStateChangedEventArgs : EventArgs
    {
        ModelItem parentModelItem;
        string key;
        object newValue;
        object oldValue;
        
        public ViewStateChangedEventArgs(ModelItem modelItem, string key, object newValue, object oldValue)
        {
            this.parentModelItem = modelItem;
            this.key = key;
            this.newValue = newValue;
            this.oldValue = oldValue;
        }

        public ModelItem ParentModelItem
        {
            get
            {
                return this.parentModelItem;
            }
        }
        public string Key
        {
            get
            {
                return this.key;
            }
        }

        public object NewValue
        {
            get
            {
                return this.newValue;
            }
        }

        public object OldValue
        {
            get
            {
                return this.oldValue;
            }
        }


    }
}
