//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.Xaml.Hosting.Configuration
{
    using System;
    using System.Configuration;
    using System.Collections.Generic;
    using System.Web;
    using System.Runtime;

    [ConfigurationCollection(typeof(HandlerElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMapAlternate)]
    public sealed class HandlerElementCollection : ConfigurationElementCollection
    {
        public HandlerElementCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMapAlternate;
            }
        }

        protected override bool ThrowOnDuplicate
        {
            get
            {
                return false;
            }
        }

        public HandlerElement this[int index]
        {
            get
            {
                return (HandlerElement)base.BaseGet(index);
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        public void Add(HandlerElement handlerElement)
        {
            if (!this.IsReadOnly())
            {
                if (handlerElement == null)
                {
                    throw FxTrace.Exception.ArgumentNull("handlerElement");
                }
            }

            this.BaseAdd(handlerElement, false);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        public void Remove(HandlerElement handlerElement)
        {
            if (!this.IsReadOnly())
            {
                if (handlerElement == null)
                {
                    throw FxTrace.Exception.ArgumentNull("handlerElement");
                }
            }

            this.BaseRemove(this.GetElementKey(handlerElement));
        }

        public void Remove(string xamlRootElementType)
        {
            if (!this.IsReadOnly())
            {
                if (xamlRootElementType == null)
                {
                    throw FxTrace.Exception.ArgumentNull("xamlRootElementType");
                }
            }

            this.BaseRemove(xamlRootElementType);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        internal bool TryGetHttpHandlerType(Type hostedXamlType, out Type httpHandlerType)
        {
            httpHandlerType = null;
            foreach (HandlerElement handler in this)
            {
                if (handler.LoadXamlRootElementType().IsAssignableFrom(hostedXamlType))
                {
                    httpHandlerType = handler.LoadHttpHandlerType();
                    return true;
                }
            }
            return false;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new HandlerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw FxTrace.Exception.ArgumentNull("element");
            }
            return ((HandlerElement)element).Key;
        }
    }
}

