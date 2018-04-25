//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing.Configuration
{
    using System;
    using System.Xml;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using SR2 = System.ServiceModel.Routing.SR;

    public class RoutingSection : ConfigurationSection
    {
        [ConfigurationProperty(ConfigurationStrings.Filters, Options = ConfigurationPropertyOptions.None)]
        public FilterElementCollection Filters
        {
            get { return (FilterElementCollection)base[ConfigurationStrings.Filters]; }
        }

        [ConfigurationProperty(ConfigurationStrings.FilterTables, Options = ConfigurationPropertyOptions.None)]
        public FilterTableCollection FilterTables
        {
            get { return (FilterTableCollection)base[ConfigurationStrings.FilterTables]; }
        }

        [ConfigurationProperty(ConfigurationStrings.BackupLists, Options = ConfigurationPropertyOptions.None)]
        public BackupListCollection BackupLists
        {
            get { return (BackupListCollection)base[ConfigurationStrings.BackupLists]; }
        }

        [ConfigurationProperty(ConfigurationStrings.NamespaceTable, Options = ConfigurationPropertyOptions.None)]
        public NamespaceElementCollection NamespaceTable
        {
            get { return (NamespaceElementCollection)base[ConfigurationStrings.NamespaceTable]; }
        }

        public static MessageFilterTable<IEnumerable<ServiceEndpoint>> CreateFilterTable(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("name");
            }

            RoutingSection routingSection = (RoutingSection)ConfigurationManager.GetSection("system.serviceModel/routing");
            if (routingSection == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.RoutingSectionNotFound));
            }

            FilterTableEntryCollection routingTableElement = routingSection.FilterTables[name];
            if (routingTableElement == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.RoutingTableNotFound(name)));
            }
            XmlNamespaceManager xmlNamespaces = new XPathMessageContext();
            foreach (NamespaceElement nsElement in routingSection.NamespaceTable)
            {
                xmlNamespaces.AddNamespace(nsElement.Prefix, nsElement.Namespace);
            }

            FilterElementCollection filterElements = routingSection.Filters;
            MessageFilterTable<IEnumerable<ServiceEndpoint>> routingTable = new MessageFilterTable<IEnumerable<ServiceEndpoint>>();
            foreach (FilterTableEntryElement entry in routingTableElement)
            {
                FilterElement filterElement = filterElements[entry.FilterName];
                if (filterElement == null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.FilterElementNotFound(entry.FilterName)));
                }
                MessageFilter filter = filterElement.CreateFilter(xmlNamespaces, filterElements);
                //retreive alternate service endpoints
                IList<ServiceEndpoint> endpoints = new List<ServiceEndpoint>();
                if (!string.IsNullOrEmpty(entry.BackupList))
                {
                    BackupEndpointCollection alternateEndpointListElement = routingSection.BackupLists[entry.BackupList];
                    if (alternateEndpointListElement == null)
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.BackupListNotFound(entry.BackupList)));
                    }
                    endpoints = alternateEndpointListElement.CreateAlternateEndpoints();
                }
                //add first endpoint to beginning of list
                endpoints.Insert(0, ClientEndpointLoader.LoadEndpoint(entry.EndpointName));
                routingTable.Add(filter, endpoints, entry.Priority);
            }

            return routingTable;
        }
    }

    [SuppressMessage(FxCop.Category.Design, FxCop.Rule.CollectionsShouldImplementGenericInterface, Justification = "generic interface not needed for config")]
    [ConfigurationCollection(typeof(FilterTableEntryCollection), AddItemName = ConfigurationStrings.FilterTable)]
    public class FilterTableCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new FilterTableEntryCollection();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FilterTableEntryCollection)element).Name;
        }

        public void Add(FilterTableEntryCollection element)
        {
            if (!this.IsReadOnly())
            {
                if (element == null)
                {
                    throw FxTrace.Exception.ArgumentNull("element");
                }
            }
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        public void Remove(FilterTableEntryCollection element)
        {
            if (!this.IsReadOnly())
            {
                if (element == null)
                {
                    throw FxTrace.Exception.ArgumentNull("element");
                }
            }
            BaseRemove(this.GetElementKey(element));
        }

        new public FilterTableEntryCollection this[string name]
        {
            get
            {
                return (FilterTableEntryCollection)BaseGet(name);
            }
        }
    }

    [SuppressMessage(FxCop.Category.Design, FxCop.Rule.CollectionsShouldImplementGenericInterface, Justification = "generic interface not needed for config")]
    [ConfigurationCollection(typeof(FilterTableEntryElement))]
    public class FilterTableEntryCollection : ConfigurationElementCollection
    {
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "fxcop didn't like [StringValidator(MinLength = 0)]")]
        [ConfigurationProperty(ConfigurationStrings.Name, DefaultValue = null, Options = ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired)]
        public string Name
        {
            get
            {
                return (string)this[ConfigurationStrings.Name];
            }
            set
            {
                this[ConfigurationStrings.Name] = value;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new FilterTableEntryElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            FilterTableEntryElement entry = (FilterTableEntryElement)element;
            return entry.FilterName + ":" + entry.EndpointName;
        }

        public void Add(FilterTableEntryElement element)
        {
            if (!this.IsReadOnly())
            {
                if (element == null)
                {
                    throw FxTrace.Exception.ArgumentNull("element");
                }
            }
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        public void Remove(FilterTableEntryElement element)
        {
            if (!this.IsReadOnly())
            {
                if (element == null)
                {
                    throw FxTrace.Exception.ArgumentNull("element");
                }
            }
            BaseRemove(this.GetElementKey(element));
        }
    }

    [SuppressMessage(FxCop.Category.Design, FxCop.Rule.CollectionsShouldImplementGenericInterface, Justification = "generic interface not needed for config")]
    [ConfigurationCollection(typeof(BackupEndpointCollection), AddItemName = ConfigurationStrings.BackupList)]
    public class BackupListCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new BackupEndpointCollection();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((BackupEndpointCollection)element).Name;
        }

        public void Add(BackupEndpointCollection element)
        {
            if (!this.IsReadOnly())
            {
                if (element == null)
                {
                    throw FxTrace.Exception.ArgumentNull("element");
                }
            }
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        public void Remove(BackupEndpointCollection element)
        {
            if (!this.IsReadOnly())
            {
                if (element == null)
                {
                    throw FxTrace.Exception.ArgumentNull("element");
                }
            }
            BaseRemove(this.GetElementKey(element));
        }

        new public BackupEndpointCollection this[string name]
        {
            get
            {
                return (BackupEndpointCollection)BaseGet(name);
            }
        }
    }

    [SuppressMessage(FxCop.Category.Design, FxCop.Rule.CollectionsShouldImplementGenericInterface, Justification = "generic interface not needed for config")]
    [ConfigurationCollection(typeof(BackupEndpointElement))]
    public class BackupEndpointCollection : ConfigurationElementCollection
    {
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "fxcop didn't like [StringValidator(MinLength = 0)]")]
        [ConfigurationProperty(ConfigurationStrings.Name, DefaultValue = null, Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
        public string Name
        {
            get
            {
                return (string)this[ConfigurationStrings.Name];
            }
            set
            {
                this[ConfigurationStrings.Name] = value;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new BackupEndpointElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            BackupEndpointElement entry = (BackupEndpointElement)element;
            return entry.Key;
        }

        public void Add(BackupEndpointElement element)
        {
            if (!this.IsReadOnly())
            {
                if (element == null)
                {
                    throw FxTrace.Exception.ArgumentNull("element");
                }
            }
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        public void Remove(BackupEndpointElement element)
        {
            if (!this.IsReadOnly())
            {
                if (element == null)
                {
                    throw FxTrace.Exception.ArgumentNull("element");
                }
            }
            BaseRemove(this.GetElementKey(element));
        }

        internal IList<ServiceEndpoint> CreateAlternateEndpoints()
        {
            IList<ServiceEndpoint> toReturn = new List<ServiceEndpoint>();
            foreach (BackupEndpointElement entryElement in this)
            {
                ServiceEndpoint serviceEnpoint = ClientEndpointLoader.LoadEndpoint(entryElement.EndpointName);
                toReturn.Add(serviceEnpoint);
            }
            return toReturn;
        }
    }

    [SuppressMessage(FxCop.Category.Design, FxCop.Rule.CollectionsShouldImplementGenericInterface, Justification = "generic interface not needed for config")]
    [ConfigurationCollection(typeof(FilterElement), AddItemName = ConfigurationStrings.Filter)]
    public class FilterElementCollection : ConfigurationElementCollection
    {
        public override bool IsReadOnly()
        {
            return false;
        }

        protected override bool IsElementRemovable(ConfigurationElement element)
        {
            return true;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new FilterElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FilterElement)element).Name;
        }

        public void Add(FilterElement element)
        {
            if (!this.IsReadOnly())
            {
                if (element == null)
                {
                    throw FxTrace.Exception.ArgumentNull("element");
                }
            }
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        public void Remove(FilterElement element)
        {
            if (!this.IsReadOnly())
            {
                if (element == null)
                {
                    throw FxTrace.Exception.ArgumentNull("element");
                }
            }
            BaseRemove(this.GetElementKey(element));
        }

        public FilterElement this[int index]
        {
            get
            {
                return (FilterElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        new public FilterElement this[string name]
        {
            get
            {
                return (FilterElement)BaseGet(name);
            }
        }
    }

    [SuppressMessage(FxCop.Category.Design, FxCop.Rule.CollectionsShouldImplementGenericInterface, Justification = "generic interface not needed for config")]
    [ConfigurationCollection(typeof(NamespaceElement))]
    public class NamespaceElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new NamespaceElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NamespaceElement)element).Prefix;
        }

        public void Add(NamespaceElement element)
        {
            if (!this.IsReadOnly())
            {
                if (element == null)
                {
                    throw FxTrace.Exception.ArgumentNull("element");
                }
            }
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        public void Remove(NamespaceElement element)
        {
            if (!this.IsReadOnly())
            {
                if (element == null)
                {
                    throw FxTrace.Exception.ArgumentNull("element");
                }
            }
            BaseRemove(this.GetElementKey(element));
        }

        public NamespaceElement this[int index]
        {
            get
            {
                return (NamespaceElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        new public NamespaceElement this[string name]
        {
            get
            {
                return (NamespaceElement)BaseGet(name);
            }
        }
    }

    public class FilterElement : ConfigurationElement
    {
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "fxcop didn't like [StringValidator(MinLength = 0)]")]
        [ConfigurationProperty(ConfigurationStrings.Name, DefaultValue = null, Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
        public string Name
        {
            get
            {
                return (string)this[ConfigurationStrings.Name];
            }
            set
            {
                this[ConfigurationStrings.Name] = value;
            }
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "fxcop didn't like validator")]
        [ConfigurationProperty(ConfigurationStrings.FilterType, DefaultValue = null, Options = ConfigurationPropertyOptions.IsRequired)]
        public FilterType FilterType
        {
            get
            {
                return (FilterType)this[ConfigurationStrings.FilterType];
            }
            set
            {
                if (value < FilterType.Action || value > FilterType.XPath)
                {
                    throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("value"));
                }
                this[ConfigurationStrings.FilterType] = value;
            }
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "fxcop didn't like [StringValidator(MinLength = 0)]")]
        [ConfigurationProperty(ConfigurationStrings.FilterData, DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
        public string FilterData
        {
            get
            {
                return (string)this[ConfigurationStrings.FilterData];
            }
            set
            {
                this[ConfigurationStrings.FilterData] = value;
            }
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "fxcop didn't like [StringValidator(MinLength = 0)]")]
        [ConfigurationProperty(ConfigurationStrings.Filter1, DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
        public string Filter1
        {
            get
            {
                return (string)this[ConfigurationStrings.Filter1];
            }
            set
            {
                this[ConfigurationStrings.Filter1] = value;
            }
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "fxcop didn't like [StringValidator(MinLength = 0)]")]
        [ConfigurationProperty(ConfigurationStrings.Filter2, DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
        public string Filter2
        {
            get
            {
                return (string)this[ConfigurationStrings.Filter2];
            }
            set
            {
                this[ConfigurationStrings.Filter2] = value;
            }
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "fxcop didn't like [StringValidator(MinLength = 0)]")]
        [ConfigurationProperty(ConfigurationStrings.CustomType, DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
        public string CustomType
        {
            get
            {
                return (string)this[ConfigurationStrings.CustomType];
            }
            set
            {
                this[ConfigurationStrings.CustomType] = value;
            }
        }

        internal MessageFilter CreateFilter(XmlNamespaceManager xmlNamespaces, FilterElementCollection filters)
        {
            MessageFilter filter;

            switch (this.FilterType)
            {
                case FilterType.Action:
                    filter = new ActionMessageFilter(this.FilterData);
                    break;
                case FilterType.EndpointAddress:
                    filter = new EndpointAddressMessageFilter(new EndpointAddress(this.FilterData), false);
                    break;
                case FilterType.PrefixEndpointAddress:
                    filter = new PrefixEndpointAddressMessageFilter(new EndpointAddress(this.FilterData), false);
                    break;
                case FilterType.And:
                    MessageFilter filter1 = filters[this.Filter1].CreateFilter(xmlNamespaces, filters);
                    MessageFilter filter2 = filters[this.Filter2].CreateFilter(xmlNamespaces, filters);
                    filter = new StrictAndMessageFilter(filter1, filter2);
                    break;
                case FilterType.EndpointName:
                    filter = new EndpointNameMessageFilter(this.FilterData);
                    break;
                case FilterType.MatchAll:
                    filter = new MatchAllMessageFilter();
                    break;
                case FilterType.Custom:
                    filter = CreateCustomFilter(this.CustomType, this.FilterData);
                    break;
                case FilterType.XPath:
                    filter = new XPathMessageFilter(this.FilterData, xmlNamespaces);
                    break;
                default:
                    // We can't really ever get here because set_FilterType performs validation.
                    throw FxTrace.Exception.AsError(new InvalidOperationException());
            }
            return filter;
        }

        static MessageFilter CreateCustomFilter(string customType, string filterData)
        {
            if (string.IsNullOrEmpty(customType))
            {
                throw FxTrace.Exception.ArgumentNullOrEmpty("customType");
            }

            Type customFilterType = Type.GetType(customType, true);
            return (MessageFilter)Activator.CreateInstance(customFilterType, filterData);
        }
    }

    public class NamespaceElement : ConfigurationElement
    {
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "fxcop didn't like [StringValidator(MinLength = 0)]")]
        [ConfigurationProperty(ConfigurationStrings.Prefix, DefaultValue = null, Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
        public string Prefix
        {
            get
            {
                return (string)this[ConfigurationStrings.Prefix];
            }
            set
            {
                this[ConfigurationStrings.Prefix] = value;
            }
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "fxcop didn't like [StringValidator(MinLength = 0)]")]
        [ConfigurationProperty(ConfigurationStrings.Namespace, DefaultValue = null, Options = ConfigurationPropertyOptions.IsRequired)]
        public string Namespace
        {
            get
            {
                return (string)this[ConfigurationStrings.Namespace];
            }
            set
            {
                this[ConfigurationStrings.Namespace] = value;
            }
        }
    }

    public class FilterTableEntryElement : ConfigurationElement
    {

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "fxcop didn't like [StringValidator(MinLength = 0)]")]
        [ConfigurationProperty(ConfigurationStrings.FilterName, DefaultValue = null, Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
        public string FilterName
        {
            get
            {
                return (string)this[ConfigurationStrings.FilterName];
            }
            set
            {
                this[ConfigurationStrings.FilterName] = value;
            }
        }


        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationPropertyNameRule, Justification = "fxcop rule throws null ref if fixed")]
        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "fxcop didn't like [StringValidator(MinLength = 0)]")]
        [ConfigurationProperty(ConfigurationStrings.EndpointName, DefaultValue = null, Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
        public string EndpointName
        {
            get
            {
                return (string)this[ConfigurationStrings.EndpointName];
            }
            set
            {
                this[ConfigurationStrings.EndpointName] = value;
            }
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "fxcop didn't like IntegerValidator")]
        [ConfigurationProperty(ConfigurationStrings.Priority, DefaultValue = 0, Options = ConfigurationPropertyOptions.None)]
        public int Priority
        {
            get
            {
                return (int)this[ConfigurationStrings.Priority];
            }
            set
            {
                this[ConfigurationStrings.Priority] = value;
            }
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "fxcop didn't like [StringValidator(MinLength = 0)]")]
        [ConfigurationProperty(ConfigurationStrings.BackupList, DefaultValue = null, Options = ConfigurationPropertyOptions.None)]
        public string BackupList
        {
            get
            {
                return (string)this[ConfigurationStrings.BackupList];
            }
            set
            {
                this[ConfigurationStrings.BackupList] = value;
            }
        }
    }

    public class BackupEndpointElement : ConfigurationElement
    {
        public BackupEndpointElement()
        {
            this.Key = new object();
        }

        //needed to allow duplicate alternate endpoints
        internal object Key
        {
            get;
            private set;
        }

        [SuppressMessage(FxCop.Category.Configuration, FxCop.Rule.ConfigurationValidatorAttributeRule, Justification = "fxcop didn't like [StringValidator(MinLength = 0)]")]
        [ConfigurationProperty(ConfigurationStrings.EndpointName, DefaultValue = null, Options = ConfigurationPropertyOptions.IsRequired)]
        public string EndpointName
        {
            get
            {
                return (string)this[ConfigurationStrings.EndpointName];
            }
            set
            {
                this[ConfigurationStrings.EndpointName] = value;
            }
        }
    }
}
