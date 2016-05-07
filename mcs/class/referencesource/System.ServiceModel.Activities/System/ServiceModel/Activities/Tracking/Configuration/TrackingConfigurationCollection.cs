//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{
    using System.Configuration;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;

    // Base class for all the workflow tracking configuration collections
    [Fx.Tag.XamlVisible(false)]
    public class TrackingConfigurationCollection<TConfigurationElement> : ConfigurationElementCollection
        where TConfigurationElement : TrackingConfigurationElement, new ()
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public TConfigurationElement this[int index]
        {
            get
            {
                return (TConfigurationElement)base.BaseGet(index);
            }
            set
            {
                // Only validate input if config is not Read-Only, otherwise
                // let BaseAdd throw appropriate exception
                if (!this.IsReadOnly())
                {
                    if (value == null)
                    {
                        throw FxTrace.Exception.ArgumentNull("value");
                    }
                    if (base.BaseGet(index) != null)
                    {
                        base.BaseRemoveAt(index);
                    }
                }

                base.BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TConfigurationElement();
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationPropertyAttributeRule,
            Justification = "This property is defined by the base class to compute unique key.")]
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TrackingConfigurationElement)element).ElementKey;
        }

        public void Add(TConfigurationElement element)
        {
            // Only validate input if config is not Read-Only, otherwise
            // let BaseAdd throw appropriate exception
            if (!this.IsReadOnly())
            {
                if (element == null)
                {
                    throw FxTrace.Exception.ArgumentNull("element");
                }
            }

            base.BaseAdd(element);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        public int IndexOf(TConfigurationElement element)
        {
            if (element == null)
            {
                throw FxTrace.Exception.ArgumentNull("element");
            }

            return base.BaseIndexOf(element);
        }

        public void Remove(TConfigurationElement element)
        {
            // Only validate input if config is not Read-Only, otherwise
            // let BaseRemove throw appropriate exception
            if (!this.IsReadOnly())
            {
                if (element == null)
                {
                    throw FxTrace.Exception.ArgumentNull("element");
                }
            }

            base.BaseRemove(this.GetElementKey(element));
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }
    }
}
