//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Text;

    public abstract class ServiceModelConfigurationElementCollection<ConfigurationElementType> : ConfigurationElementCollection
        where ConfigurationElementType : ConfigurationElement, new()
    {
        ConfigurationElementCollectionType collectionType;
        string elementName;

        internal ServiceModelConfigurationElementCollection()
            : this(ConfigurationElementCollectionType.AddRemoveClearMap, null)
        { }

        internal ServiceModelConfigurationElementCollection(ConfigurationElementCollectionType collectionType,
            string elementName)
        {
            this.collectionType = collectionType;
            this.elementName = elementName;

            if (!String.IsNullOrEmpty(elementName))
            {
                this.AddElementName = elementName;
            }
        }

        internal ServiceModelConfigurationElementCollection(ConfigurationElementCollectionType collectionType,
            string elementName, IComparer comparer) : base(comparer)
        {
            this.collectionType = collectionType;
            this.elementName = elementName;
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            if (!this.IsReadOnly() && !this.ThrowOnDuplicate)
            {
                object key = this.GetElementKey(element);

                if (this.ContainsKey(key))
                {
                    this.BaseRemove(key);
                }
            }
            base.BaseAdd(element);
        }

        public void Add(ConfigurationElementType element)
        {
            if (!this.IsReadOnly())
            {
                if (element == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
                }
            }

            this.BaseAdd(element);
        }

        public void Clear()
        {
            this.BaseClear();
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return this.collectionType; }
        }

        public virtual bool ContainsKey(object key)
        {
            if (key == null)
            {
                List<string> elementKeys = new List<string>();

                ConfigurationElement dummyElement = this.CreateNewElement();
                foreach (PropertyInformation propertyInfo in dummyElement.ElementInformation.Properties)
                {
                    if (propertyInfo.IsKey)
                    {
                        elementKeys.Add(propertyInfo.Name);
                    }
                }

                if (0 == elementKeys.Count)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
                }
                else if (1 == elementKeys.Count)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                        SR.GetString(SR.ConfigElementKeyNull, elementKeys[0])));
                }
                else
                {
                    StringBuilder elementKeysString = new StringBuilder();

                    for (int i = 0; i < elementKeys.Count - 1; i++)
                    {
                        elementKeysString = elementKeysString.Append(elementKeys[i] + ", ");
                    }

                    elementKeysString = elementKeysString.Append(elementKeys[elementKeys.Count - 1]);

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                        SR.GetString(SR.ConfigElementKeysNull, elementKeys.ToString())));
                }
            }
            else
            {
                return null != this.BaseGet(key);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ConfigurationElementType();
        }

        public void CopyTo(ConfigurationElementType[] array, int start)
        {
            if (array == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("array");
            }

            if (start < 0 || start >= array.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("start", SR.GetString(SR.ConfigInvalidStartValue,
                    array.Length - 1,
                    start));
            }
            ((ICollection)this).CopyTo(array, start);
        }

        protected override string ElementName
        {
            get 
            {
                string retval = this.elementName;
                if (string.IsNullOrEmpty(retval))
                {
                    retval = base.ElementName;
                }
                return retval; 
            }
        }

        public int IndexOf(ConfigurationElementType element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            return this.BaseIndexOf(element);
        }

        public void Remove(ConfigurationElementType element) 
        {
            if (!this.IsReadOnly())
            {
                if (element == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
                }
            }

            this.BaseRemove(this.GetElementKey(element));
        }

        public void RemoveAt(object key) 
        {
            if (!this.IsReadOnly())
            {
                if (key == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
                }
            }

            this.BaseRemove(key);
        }

        public void RemoveAt(int index)
        {
            this.BaseRemoveAt(index);
        }

        public virtual ConfigurationElementType this[object key]
        {
            get
            {
                if (key == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
                }
                ConfigurationElementType retval = (ConfigurationElementType)this.BaseGet(key);
                if (retval == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new System.Collections.Generic.KeyNotFoundException(
                        SR.GetString(SR.ConfigKeyNotFoundInElementCollection,
                        key.ToString())));
                }
                return retval;
            }
            set
            {
                if (this.IsReadOnly())
                {
                    this.Add(value);
                }

                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                if (key == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
                }
                if (this.GetElementKey(value).ToString().Equals((string)key, StringComparison.Ordinal))
                {
                    if (this.BaseGet(key) != null)
                    {
                        this.BaseRemove(key);
                    }
                    this.Add(value);
                }
                else
                {
#pragma warning disable 56506 //Microsoft; Variable 'key' checked for null previously
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.ConfigKeysDoNotMatch,
                        this.GetElementKey(value).ToString(),
                        key.ToString()));
#pragma warning restore
                }
            }
        }

        public ConfigurationElementType this[int index]
        {
            get
            {
                return (ConfigurationElementType)this.BaseGet(index);
            }
            set
            {
                if (!this.IsReadOnly() && !this.ThrowOnDuplicate)
                {
                    if (this.BaseGet(index) != null)
                    {
                        this.BaseRemoveAt(index);
                    }
                }
                this.BaseAdd(index, value);
            }
        }
    }

}


