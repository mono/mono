//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System.Collections.Generic;
    using System.Activities.Presentation.Services;

    // Implementation of ModelChangedEventArgs used by the ModelServiceImpl

    class ModelChangedEventArgsImpl : ModelChangedEventArgs
    {
        List<ModelItem> itemsAdded;
        List<ModelItem> itemsRemoved;
        List<ModelProperty> propertiesChanged;
        ModelChangeInfo modelChangeInfo;

        public ModelChangedEventArgsImpl(List<ModelItem> itemsAdded, List<ModelItem> itemsRemoved, List<ModelProperty> propertiesChanged)
            : this(itemsAdded, itemsRemoved, propertiesChanged, null)
        {
        }

        public ModelChangedEventArgsImpl(List<ModelItem> itemsAdded, List<ModelItem> itemsRemoved, List<ModelProperty> propertiesChanged, ModelChangeInfo modelChangeInfo)
        {
            this.itemsAdded = itemsAdded;
            this.itemsRemoved = itemsRemoved;
            this.propertiesChanged = propertiesChanged;
            this.modelChangeInfo = modelChangeInfo;
        }

        [Obsolete("Don't use this property. Use \"ModelChangeInfo\" instead.")]
        public override IEnumerable<ModelItem> ItemsAdded
        {
            get
            {
                return itemsAdded;
            }
        }

        [Obsolete("Don't use this property. Use \"ModelChangeInfo\" instead.")]
        public override IEnumerable<ModelItem> ItemsRemoved
        {
            get
            {
                return itemsRemoved;
            }
        }

        [Obsolete("Don't use this property. Use \"ModelChangeInfo\" instead.")]
        public override IEnumerable<ModelProperty> PropertiesChanged
        {
            get
            {
                return propertiesChanged;
            }
        }

        public override ModelChangeInfo ModelChangeInfo
        {
            get
            {
                return this.modelChangeInfo;
            }
        }
    }
}
