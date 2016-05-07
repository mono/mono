//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Services
{
    using System.Activities.Presentation.Model;

    internal class ModelChangeInfoImpl : ModelChangeInfo
    {
        private ModelChangeType modelChangeType;
        private ModelItem subject;
        private string propertyName;
        private ModelItem key;
        private ModelItem oldValue;
        private ModelItem value;

        private ModelChangeInfoImpl(ModelChangeType modelChangeType, ModelItem subject, string propertyName, ModelItem key, ModelItem oldValue, ModelItem value)
        {
            this.modelChangeType = modelChangeType;
            this.subject = subject;
            this.propertyName = propertyName;
            this.key = key;
            this.oldValue = oldValue;
            this.value = value;
        }

        public override ModelChangeType ModelChangeType
        {
            get { return this.modelChangeType; }
        }

        public override ModelItem Subject
        {
            get { return this.subject; }
        }

        public override string PropertyName
        {
            get { return this.propertyName; }
        }

        public override ModelItem Key
        {
            get { return this.key; }
        }

        public override ModelItem OldValue
        {
            get { return this.oldValue; }
        }

        public override ModelItem Value
        {
            get { return this.value; }
        }

        public static ModelChangeInfoImpl CreatePropertyChanged(ModelItem subject, string propertyName, ModelItem oldValue, ModelItem newValue)
        {
            return new ModelChangeInfoImpl(ModelChangeType.PropertyChanged, subject, propertyName, null, oldValue, newValue);
        }

        public static ModelChangeInfoImpl CreateCollectionItemAdded(ModelItem subject, ModelItem item)
        {
            return new ModelChangeInfoImpl(ModelChangeType.CollectionItemAdded, subject, null, null, null, item);
        }

        public static ModelChangeInfoImpl CreateCollectionItemRemoved(ModelItem subject, ModelItem item)
        {
            return new ModelChangeInfoImpl(ModelChangeType.CollectionItemRemoved, subject, null, null, null, item);
        }

        public static ModelChangeInfoImpl CreateDictionaryKeyValueAdded(ModelItem subject, ModelItem key, ModelItem value)
        {
            return new ModelChangeInfoImpl(ModelChangeType.DictionaryKeyValueAdded, subject, null, key, null, value);
        }

        public static ModelChangeInfoImpl CreateDictionaryKeyValueRemoved(ModelItem subject, ModelItem key, ModelItem value)
        {
            return new ModelChangeInfoImpl(ModelChangeType.DictionaryKeyValueRemoved, subject, null, key, null, value);
        }

        public static ModelChangeInfoImpl CreateDictionaryValueChanged(ModelItem subject, ModelItem key, ModelItem oldValue, ModelItem newValue)
        {
            return new ModelChangeInfoImpl(ModelChangeType.DictionaryValueChanged, subject, null, key, oldValue, newValue);
        }
    }
}
