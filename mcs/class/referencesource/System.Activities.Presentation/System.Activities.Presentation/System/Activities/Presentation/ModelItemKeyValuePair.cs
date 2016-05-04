//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Runtime;

    class ModelItemKeyValuePair<TKey, TValue>
    {
        internal DictionaryItemsCollection<TKey, TValue> collection;

        TKey key;

        TValue value;

        public ModelItemKeyValuePair()
        {
        }

        public ModelItemKeyValuePair(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }

        [Fx.Tag.KnownXamlExternal]
        public TKey Key
        {
            get
            {
                return this.key;
            }
            set
            {
                if (this.collection != null)
                {
                    this.collection.PreUpdateKey(this.key, value);
                }
                this.key = value;
                if (this.collection != null)
                {
                    this.collection.PostUpdateKey();
                }
            }
        }

        [Fx.Tag.KnownXamlExternal]
        public TValue Value
        {
            get
            {
                return this.value;
            }
            set
            {
                if (this.collection != null)
                {
                    this.collection.UpdateValue(this.key, value);
                }
                this.value = value;
            }
        }
    }
}
