//------------------------------------------------------------------------------
// <copyright file="ConfigurationElement.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Configuration.Internal;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Xml;
using System.Globalization;
using System.ComponentModel;
using System.Security;
using System.Text;

namespace System.Configuration {

    //
    // Methods that are called by the configuration system, and must be overridable
    // by derived classes that wish to implement their own serialization/deserialization
    //      IsModified()
    //      ResetModified()
    //      Reset(ConfigurationElement parentSection, object context)
    //      DeserializeSection(object context, XmlNode xmlNode)
    //      SerializeSection(ConfigurationElement parentSection, object context, string name)
    //

    public abstract class ConfigurationElement {
        private  const string LockAttributesKey = "lockAttributes";
        private  const string LockAllAttributesExceptKey = "lockAllAttributesExcept";
        private  const string LockElementsKey = "lockElements";
        private  const string LockAll = "*";
        private  const string LockAllElementsExceptKey = "lockAllElementsExcept";
        private  const string LockItemKey = "lockItem";
        internal const string DefaultCollectionPropertyName = "";

        private static string[] s_lockAttributeNames = new string[] {
            LockAttributesKey,
            LockAllAttributesExceptKey,
            LockElementsKey,
            LockAllElementsExceptKey,
            LockItemKey,
        };

        private static Hashtable s_propertyBags = new Hashtable();
        private static volatile Dictionary<Type,ConfigurationValidatorBase> s_perTypeValidators;
        internal static readonly Object s_nullPropertyValue = new Object();
        private static ConfigurationElementProperty s_ElementProperty =
            new ConfigurationElementProperty(new DefaultValidator());

        private bool                            _bDataToWrite;
        private bool                            _bModified;
        private bool                            _bReadOnly;
        private bool                            _bElementPresent; // Set to false if any part of the element is not inherited
        private bool                            _bInited;
        internal ConfigurationLockCollection    _lockedAttributesList;
        internal ConfigurationLockCollection    _lockedAllExceptAttributesList;
        internal ConfigurationLockCollection    _lockedElementsList;
        internal ConfigurationLockCollection    _lockedAllExceptElementsList;
        private readonly ConfigurationValues    _values;
        private string                          _elementTagName;
        private volatile ElementInformation     _evaluationElement;
        private ConfigurationElementProperty    _elementProperty = s_ElementProperty;
        internal ConfigurationValueFlags        _fItemLocked;
        internal ContextInformation             _evalContext;
        internal BaseConfigurationRecord        _configRecord;

        internal bool DataToWriteInternal {
            get {
                return _bDataToWrite;
            }
            set {
                _bDataToWrite = value;
            }
        }

        internal ConfigurationElement CreateElement(Type type) {
            // We use this.GetType() as the calling type since all code paths which lead to
            // CreateElement are protected methods, so inputs are provided by somebody in
            // the current type hierarchy. Since we expect that the most subclassed type
            // will be the most restricted security-wise, we'll use it as the calling type.

            ConfigurationElement element = (ConfigurationElement)TypeUtil.CreateInstanceRestricted(callingType: GetType(), targetType: type);
            element.CallInit();
            return element;
        }


        protected ConfigurationElement() {
            _values = new ConfigurationValues();

            // Set the per-type validator ( this will actually have an effect only for an attributed model elements )
            // Note that in the case where the property bag fot this.GetType() has not yet been created
            // the validator for this instance will get applied in ApplyValidatorsRecursive ( see this.get_Properties )
            ApplyValidator(this);
        }

        // Give elements that are added to a collection an opportunity to
        //
        protected internal virtual void Init() {
            // If Init is called by the derived class, we may be able
            // to set _bInited to true if the derived class properly
            // calls Init on its base.
            _bInited = true;
        }

        internal void CallInit() {
            // Ensure Init is called just once
            if (!_bInited) {
                Init();
                _bInited = true;
            }
        }

        internal bool ElementPresent {
            get {
                return _bElementPresent;
            }
            set {
                _bElementPresent = value;
            }
        }

        internal string ElementTagName {
            get {
                return _elementTagName;
            }
        }

        internal ConfigurationLockCollection LockedAttributesList {
            get {
                return _lockedAttributesList;
            }
        }

        internal ConfigurationLockCollection LockedAllExceptAttributesList {
            get {
                return _lockedAllExceptAttributesList;
            }
        }

        internal ConfigurationValueFlags ItemLocked {
            get {
                return _fItemLocked;
            }
        }

        public ConfigurationLockCollection LockAttributes {
            get {
                if (_lockedAttributesList == null) {
                    _lockedAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedAttributes);
                }
                return _lockedAttributesList;
            }
        }

        internal void MergeLocks(ConfigurationElement source) {
            if (source != null) {
                _fItemLocked = ((source._fItemLocked & ConfigurationValueFlags.Locked) != 0) ?
                    (ConfigurationValueFlags.Inherited | source._fItemLocked) : _fItemLocked;

                if (source._lockedAttributesList != null) {
                    if (_lockedAttributesList == null) {
                        _lockedAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedAttributes);
                    }
                    foreach (string key in source._lockedAttributesList)
                        _lockedAttributesList.Add(key, ConfigurationValueFlags.Inherited);  // Mark entry as from the parent - read only
                }
                if (source._lockedAllExceptAttributesList != null) {
                    if (_lockedAllExceptAttributesList == null) {
                        _lockedAllExceptAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedExceptionList, String.Empty, source._lockedAllExceptAttributesList);
                    }

                    StringCollection intersectionCollection = IntersectLockCollections(_lockedAllExceptAttributesList, source._lockedAllExceptAttributesList);

                    _lockedAllExceptAttributesList.ClearInternal(false);
                    foreach (string key in intersectionCollection) {
                        _lockedAllExceptAttributesList.Add(key, ConfigurationValueFlags.Default);
                    }

                }
                if (source._lockedElementsList != null) {
                    if (_lockedElementsList == null) {
                        _lockedElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElements);
                    }

                    ConfigurationElementCollection collection = null;
                    if (Properties.DefaultCollectionProperty != null) // this is not a collection but it may contain a default collection
                {
                        collection = this[Properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                        if (collection != null) {
                            collection.internalElementTagName = source.ElementTagName; // Default collections don't know there tag name
                            if (collection._lockedElementsList == null) {
                                collection._lockedElementsList = _lockedElementsList; //point to the same instance of the collection from parent
                            }
                        }
                    }

                    foreach (string key in source._lockedElementsList) {
                        _lockedElementsList.Add(key, ConfigurationValueFlags.Inherited);  // Mark entry as from the parent - read only
                        if (collection != null) {
                            collection._lockedElementsList.Add(key, ConfigurationValueFlags.Inherited);  // add the local copy
                        }
                    }
                }

                if (source._lockedAllExceptElementsList != null) {
                    if (_lockedAllExceptElementsList == null || _lockedAllExceptElementsList.Count == 0) {
                        _lockedAllExceptElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElementsExceptionList, source._elementTagName, source._lockedAllExceptElementsList);
                    }
                    StringCollection intersectionCollection = IntersectLockCollections(_lockedAllExceptElementsList, source._lockedAllExceptElementsList);

                    ConfigurationElementCollection collection = null;
                    if (Properties.DefaultCollectionProperty != null) { // this is not a collection but it may contain a default collection
                        collection = this[Properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                        if (collection != null && collection._lockedAllExceptElementsList == null) {
                            // point default collection to the parent collection
                            collection._lockedAllExceptElementsList = _lockedAllExceptElementsList;
                        }
                    }
                    _lockedAllExceptElementsList.ClearInternal(false);
                    foreach (string key in intersectionCollection) {
                        if (!_lockedAllExceptElementsList.Contains(key) || key == ElementTagName)
                            _lockedAllExceptElementsList.Add(key, ConfigurationValueFlags.Default);  // add the local copy
                    }
                    if (_lockedAllExceptElementsList.HasParentElements) {
                        foreach (ConfigurationProperty prop in Properties) {
                            if ((!_lockedAllExceptElementsList.Contains(prop.Name)) &&
                                prop.IsConfigurationElementType) {
                                ((ConfigurationElement)this[prop]).SetLocked();
                            }
                        }
                    }
                }
            }
        }

        internal void HandleLockedAttributes(ConfigurationElement source) {
            // if there are locked attributes on this collection element
            if (source != null) {
                if (source._lockedAttributesList != null || source._lockedAllExceptAttributesList != null) {
                    // enumerate the possible locked properties
                    foreach (PropertyInformation propInfo in source.ElementInformation.Properties) {
                        if ((source._lockedAttributesList != null && (source._lockedAttributesList.Contains(propInfo.Name) ||
                            source._lockedAttributesList.Contains(LockAll))) ||
                            (source._lockedAllExceptAttributesList != null && !source._lockedAllExceptAttributesList.Contains(propInfo.Name))
                           ) {
                            // if the attribute has been locked in the source then check to see
                            // if the local config is trying to override it
                            if (propInfo.Name != LockAttributesKey && propInfo.Name != LockAllAttributesExceptKey) {

                                if (ElementInformation.Properties[propInfo.Name] == null) { // locked items are not defined

                                    ConfigurationPropertyCollection props = Properties; // so create the property based in the source item
                                    ConfigurationProperty prop = (ConfigurationProperty)source.Properties[propInfo.Name];
                                    props.Add(prop); // Add the property information to the property bag
                                    _evaluationElement = null; // flush the cached element data

                                    // Add the data from the source element but mark it as in herited
                                    // This must use setvalue in order to set the lock and inherited flags
                                    ConfigurationValueFlags flags = ConfigurationValueFlags.Inherited | ConfigurationValueFlags.Locked;
                                    _values.SetValue(propInfo.Name, propInfo.Value, flags, source.PropertyInfoInternal(propInfo.Name));

                                }
                                else { // don't error when optional attibute are not defined yet
                                    if (ElementInformation.Properties[propInfo.Name].ValueOrigin == PropertyValueOrigin.SetHere) {
                                        // Don't allow the override
                                        throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_attribute_locked, propInfo.Name));
                                    }
                                    // They did not override so we need to make sure the value comes from the locked one
                                    ElementInformation.Properties[propInfo.Name].Value = propInfo.Value;
                                }
                            }
                        }
                    }
                }
            }
        }

        // AssociateContext
        //
        // Associate a context with this element
        //
        internal virtual void AssociateContext(BaseConfigurationRecord configRecord) {
            _configRecord = configRecord;
            Values.AssociateContext(configRecord);
        }

        public /*protected internal virtual*/ ConfigurationLockCollection LockAllAttributesExcept {
            get {
                if (_lockedAllExceptAttributesList == null) {
                    _lockedAllExceptAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedExceptionList, _elementTagName);
                }
                return _lockedAllExceptAttributesList;
            }
        }

        public ConfigurationLockCollection LockElements {
            get {
                if (_lockedElementsList == null) {
                    _lockedElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElements);
                }
                return _lockedElementsList;
            }
        }

        public ConfigurationLockCollection LockAllElementsExcept {
            get {
                if (_lockedAllExceptElementsList == null) {
                    _lockedAllExceptElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElementsExceptionList, _elementTagName);
                }
                return _lockedAllExceptElementsList;
            }
        }

        public bool LockItem {
            get {
                return ((_fItemLocked & ConfigurationValueFlags.Locked) != 0);
            }
            set {
                if ((_fItemLocked & ConfigurationValueFlags.Inherited) == 0) {
                    _fItemLocked = (value == true) ? ConfigurationValueFlags.Locked : ConfigurationValueFlags.Default;
                    _fItemLocked |= ConfigurationValueFlags.Modified;
                }
                else {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_attribute_locked, LockItemKey));
                }
            }
        }

        protected internal virtual bool IsModified() {

            if (_bModified) {
                return true;
            }

            if (_lockedAttributesList != null && _lockedAttributesList.IsModified) {
                return true;
            }

            if (_lockedAllExceptAttributesList != null && _lockedAllExceptAttributesList.IsModified) {
                return true;
            }

            if (_lockedElementsList != null && _lockedElementsList.IsModified) {
                return true;
            }

            if (_lockedAllExceptElementsList != null && _lockedAllExceptElementsList.IsModified) {
                return true;
            }

            if ((_fItemLocked & ConfigurationValueFlags.Modified) != 0) {
                return true;
            }

            foreach (ConfigurationElement elem in _values.ConfigurationElements) {
                if (elem.IsModified()) {
                    return true;
                }
            }
            return false;
        }

        protected internal virtual void ResetModified() {
            _bModified = false;

            if (_lockedAttributesList != null) {
                _lockedAttributesList.ResetModified();
            }

            if (_lockedAllExceptAttributesList != null) {
                _lockedAllExceptAttributesList.ResetModified();
            }

            if (_lockedElementsList != null) {
                _lockedElementsList.ResetModified();
            }

            if (_lockedAllExceptElementsList != null) {
                _lockedAllExceptElementsList.ResetModified();
            }

            foreach (ConfigurationElement elem in _values.ConfigurationElements) {
                elem.ResetModified();
            }
        }

        public virtual bool IsReadOnly() {
            return _bReadOnly;
        }

        protected internal virtual void SetReadOnly() {
            _bReadOnly = true;
            foreach (ConfigurationElement elem in _values.ConfigurationElements) {
                elem.SetReadOnly();
            }
        }

        internal void SetLocked() {
            _fItemLocked = ConfigurationValueFlags.Locked | ConfigurationValueFlags.XMLParentInherited;

            foreach (ConfigurationProperty prop in Properties) {
                ConfigurationElement elem = this[prop] as ConfigurationElement;
                if (elem != null) {
                    if (elem.GetType() != this.GetType()) {
                        elem.SetLocked();
                    }

                    ConfigurationElementCollection collection = this[prop] as ConfigurationElementCollection;
                    if (collection != null) {
                        foreach (object obj in collection) {
                            ConfigurationElement element = obj as ConfigurationElement;
                            if (element != null) {
                                element.SetLocked();
                            }
                        }
                    }
                }
            }
        }

        // GetErrorsList
        //
        // Get the list of Errors for this location and all
        // sub locations
        //
        internal ArrayList GetErrorsList() {
            ArrayList errorList = new ArrayList();

            ListErrors(errorList);

            return errorList;
        }

        // GetErrors
        //
        // Get a ConfigurationErrorsException that contains the errors
        // for this ConfigurationElement and its children
        //
        internal ConfigurationErrorsException GetErrors() {
            ArrayList errorsList;

            errorsList = GetErrorsList();

            if (errorsList.Count == 0) {
                return null;
            }

            ConfigurationErrorsException e = new ConfigurationErrorsException(errorsList);
            return e;
        }

        protected virtual void ListErrors(IList errorList) {
            // First list errors in this element, then in subelements
            foreach (InvalidPropValue invalidValue in _values.InvalidValues) {
                errorList.Add(invalidValue.Error);
            }

            foreach (ConfigurationElement elem in _values.ConfigurationElements) {
                elem.ListErrors(errorList);
                ConfigurationElementCollection collection = elem as ConfigurationElementCollection;
                if (collection != null) {
                    foreach (ConfigurationElement item in collection) {
                        item.ListErrors(errorList);
                    }
                }
            }
        }

        protected internal virtual void InitializeDefault() {
        }

        internal void CheckLockedElement(string elementName, XmlReader reader) {
            // have to check if clear was locked!
            if(elementName != null) {
                if(((_lockedElementsList != null) &&
                     (_lockedElementsList.DefinedInParent(LockAll) || _lockedElementsList.DefinedInParent(elementName))) ||
                    ((_lockedAllExceptElementsList != null && _lockedAllExceptElementsList.Count != 0) &&
                    _lockedAllExceptElementsList.HasParentElements &&
                    !_lockedAllExceptElementsList.DefinedInParent(elementName) ||
                    (_fItemLocked & ConfigurationValueFlags.Inherited) != 0)
                   ) {

                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_element_locked, elementName), reader);
                }
            }
        }

        internal void RemoveAllInheritedLocks() {
            if (_lockedAttributesList != null) {
                _lockedAttributesList.RemoveInheritedLocks();
            }
            if (_lockedElementsList != null) {
                _lockedElementsList.RemoveInheritedLocks();
            }
            if (_lockedAllExceptAttributesList != null) {
                _lockedAllExceptAttributesList.RemoveInheritedLocks();
            }
            if (_lockedAllExceptElementsList != null) {
                _lockedAllExceptElementsList.RemoveInheritedLocks();
            }
        }

        internal void ResetLockLists(ConfigurationElement parentElement) {
            _lockedAttributesList = null;
            _lockedAllExceptAttributesList = null;
            _lockedElementsList = null;
            _lockedAllExceptElementsList = null;

            if (parentElement != null) {
                _fItemLocked = ((parentElement._fItemLocked & ConfigurationValueFlags.Locked) != 0) ?
                    (ConfigurationValueFlags.Inherited | parentElement._fItemLocked) :
                    ConfigurationValueFlags.Default;

                if (parentElement._lockedAttributesList != null) {
                    _lockedAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedAttributes);
                    foreach (string key in parentElement._lockedAttributesList)
                        _lockedAttributesList.Add(key, ConfigurationValueFlags.Inherited);  // Mark entry as from the parent - read only
                }
                if (parentElement._lockedAllExceptAttributesList != null) {
                    _lockedAllExceptAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedExceptionList, String.Empty, parentElement._lockedAllExceptAttributesList);
                }
                if (parentElement._lockedElementsList != null) {
                    _lockedElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElements);

                    ConfigurationElementCollection collection = null;
                    if (Properties.DefaultCollectionProperty != null) // this is not a collection but it may contain a default collection
                    {
                        collection = this[Properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                        if (collection != null) {
                            collection.internalElementTagName = parentElement.ElementTagName; // Default collections don't know there tag name
                            if (collection._lockedElementsList == null) {
                                collection._lockedElementsList = _lockedElementsList;
                            }
                        }
                    }

                    foreach (string key in parentElement._lockedElementsList) {
                        _lockedElementsList.Add(key, ConfigurationValueFlags.Inherited);  // Mark entry as from the parent - read only
                    }
                }

                if (parentElement._lockedAllExceptElementsList != null) {
                    _lockedAllExceptElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElementsExceptionList, parentElement._elementTagName, parentElement._lockedAllExceptElementsList);

                    ConfigurationElementCollection collection = null;
                    if (Properties.DefaultCollectionProperty != null) // this is not a collection but it may contain a default collection
                    {
                        collection = this[Properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                        if (collection != null && collection._lockedAllExceptElementsList == null) {
                            collection._lockedAllExceptElementsList = _lockedAllExceptElementsList;
                        }
                    }
                }
            }
        }

        protected internal virtual void Reset(ConfigurationElement parentElement) {
            Values.Clear();
            ResetLockLists(parentElement);
            ConfigurationPropertyCollection props = Properties; // Force the bag to be up to date
            _bElementPresent = false;
            if (parentElement == null) {
                InitializeDefault();
            }
            else {
                bool hasAnyChildElements = false;

                ConfigurationPropertyCollection collectionKeys = null;

                for (int index = 0; index < parentElement.Values.Count; index++) {
                    string key = parentElement.Values.GetKey(index);
                    ConfigurationValue ConfigValue = parentElement.Values.GetConfigValue(index);
                    object value = (ConfigValue != null) ? ConfigValue.Value : null;
                    PropertySourceInfo sourceInfo = (ConfigValue != null) ? ConfigValue.SourceInfo : null;

                    ConfigurationProperty prop = (ConfigurationProperty)parentElement.Properties[key];
                    if (prop == null || ((collectionKeys != null) && !collectionKeys.Contains(prop.Name))) {
                        continue;
                    }

                    if (prop.IsConfigurationElementType) {
                        hasAnyChildElements = true;
                    }
                    else {
                        ConfigurationValueFlags flags = ConfigurationValueFlags.Inherited |
                            (((_lockedAttributesList != null) &&
                              (_lockedAttributesList.Contains(key) ||
                               _lockedAttributesList.Contains(LockAll)) ||
                              (_lockedAllExceptAttributesList != null) &&
                              !_lockedAllExceptAttributesList.Contains(key)) ?
                              ConfigurationValueFlags.Locked : ConfigurationValueFlags.Default);

                        if (value != s_nullPropertyValue) {
                            // _values[key] = value;
                            _values.SetValue(key, value, flags, sourceInfo);
                        }
                        if (!props.Contains(key)) // this is for optional provider models keys
                        {
                            props.Add(prop);
                            _values.SetValue(key, value, flags, sourceInfo);
                        }
                    }
                }

                if (hasAnyChildElements) {
                    for (int index = 0; index < parentElement.Values.Count; index++) {
                        string key = parentElement.Values.GetKey(index);
                        object value = parentElement.Values[index];

                        ConfigurationProperty prop = (ConfigurationProperty)parentElement.Properties[key];
                        if ((prop != null) && prop.IsConfigurationElementType) {
                            //((ConfigurationElement)value).SerializeToXmlElement(writer, prop.Name);
                            ConfigurationElement childElement = (ConfigurationElement)this[prop];
                            childElement.Reset((ConfigurationElement)value);
                        }
                    }
                }
            }
        }

        public override bool Equals(object compareTo) {
            ConfigurationElement compareToElem = compareTo as ConfigurationElement;

            if (compareToElem == null ||
                (compareTo.GetType() != this.GetType()) ||
                ((compareToElem != null) && (compareToElem.Properties.Count != this.Properties.Count))) {
                return false;
            }

            foreach (ConfigurationProperty configProperty in this.Properties) {

                if (!Object.Equals(Values[configProperty.Name], compareToElem.Values[configProperty.Name])) {
                    if (!(((Values[configProperty.Name] == null ||
                            Values[configProperty.Name] == s_nullPropertyValue) &&
                           Object.Equals(compareToElem.Values[configProperty.Name], configProperty.DefaultValue)) ||
                          ((compareToElem.Values[configProperty.Name] == null ||
                            compareToElem.Values[configProperty.Name] == s_nullPropertyValue) &&
                           Object.Equals(Values[configProperty.Name], configProperty.DefaultValue))))
                        return false;
                }
            }
            return true;
        }

        public override int GetHashCode() {
            int hHashCode = 0;
            foreach (ConfigurationProperty configProperty in this.Properties) {
                object o = this[configProperty];
                if (o != null) {
                    hHashCode ^= this[configProperty].GetHashCode();
                }
            }
            return hHashCode;
        }

        protected internal Object this[ConfigurationProperty prop] {
            get {
                Object o = _values[prop.Name];
                if (o == null) {
                    if (prop.IsConfigurationElementType) {
                        lock (_values.SyncRoot) {
                            o = _values[prop.Name];
                            if (o == null) {
                                ConfigurationElement childElement = CreateElement(prop.Type);

                                if (_bReadOnly) {
                                    childElement.SetReadOnly();
                                }

                                if (typeof(ConfigurationElementCollection).IsAssignableFrom(prop.Type)) {
                                    ConfigurationElementCollection childElementCollection = childElement as ConfigurationElementCollection;
                                    if (prop.AddElementName != null)
                                        childElementCollection.AddElementName = prop.AddElementName;
                                    if (prop.RemoveElementName != null)
                                        childElementCollection.RemoveElementName = prop.RemoveElementName;
                                    if (prop.ClearElementName != null)
                                        childElementCollection.ClearElementName = prop.ClearElementName;
                                }

                                //_values[prop.Name] = childElement;
                                _values.SetValue(prop.Name, childElement, ConfigurationValueFlags.Inherited, null);
                                o = childElement;
                            }
                        }
                    }
                    else {
                        o = prop.DefaultValue;
                    }
                }
                else if (o == s_nullPropertyValue) {
                    o = null;
                }

                // If its an invalid value - throw the error now
                if (o is InvalidPropValue) {
                    throw ((InvalidPropValue)o).Error;
                }

                return o;
            }

            set {
                SetPropertyValue(prop, value,false); // Do not ignore locks!!!
            }
        }

        protected internal Object this[String propertyName] {
            get {
                ConfigurationProperty prop = Properties[propertyName];
                if (prop == null) {
                    prop = Properties[DefaultCollectionPropertyName];
                    if (prop.ProvidedName != propertyName) {
                        return null;
                    }
                }
                return this[prop];
            }
            set {
                Debug.Assert(Properties.Contains(propertyName), "Properties.Contains(propertyName)");
                SetPropertyValue(Properties[propertyName], value, false);// Do not ignore locks!!!
            }
        }

        // Note: this method is completelly redundant ( the code is duplaicated in ConfigurationProperty( PropertyInfo ) )
        // We do not remove the code now to minimize code changes for Whidbey RTM but this method and all calls leading to it should
        // be removed post-Whidbey
        private static void ApplyInstanceAttributes(object instance) {

            Debug.Assert(instance is ConfigurationElement, "instance is ConfigurationElement");
            Type type = instance.GetType();

            foreach (PropertyInfo propertyInformation in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {

                ConfigurationPropertyAttribute attribProperty =
                    Attribute.GetCustomAttribute(propertyInformation,
                                                 typeof(ConfigurationPropertyAttribute)) as ConfigurationPropertyAttribute;

                if (attribProperty != null)
                {
                    Type propertyType = propertyInformation.PropertyType;
                    // Collections need some customization when the collection attribute is present
                    if (typeof(ConfigurationElementCollection).IsAssignableFrom(propertyType)) {
                        ConfigurationCollectionAttribute attribCollection =
                            Attribute.GetCustomAttribute(propertyInformation,
                                                            typeof(ConfigurationCollectionAttribute)) as ConfigurationCollectionAttribute;

                        // If none on the property - see if there is an attribute on the collection type itself
                        if (attribCollection == null) {
                            attribCollection =
                                Attribute.GetCustomAttribute(propertyType,
                                                                typeof(ConfigurationCollectionAttribute)) as ConfigurationCollectionAttribute;
                        }

                        ConfigurationElementCollection coll = propertyInformation.GetValue(instance, null) as ConfigurationElementCollection;
                        if (coll == null) {
                            throw new ConfigurationErrorsException(SR.GetString(SR.Config_element_null_instance,
                                propertyInformation.Name, attribProperty.Name));
                        }

                        // If the attribute is found - get the collection instance and set the data from the attribute
                        if (attribCollection != null) {
                            if (attribCollection.AddItemName.IndexOf(',') == -1) {
                                coll.AddElementName = attribCollection.AddItemName;
                            }

                            coll.RemoveElementName = attribCollection.RemoveItemName;

                            coll.ClearElementName = attribCollection.ClearItemsName;
                        }
                    }
                    else if (typeof(ConfigurationElement).IsAssignableFrom(propertyType)) {
                        // Nested configuration element - handle recursively
                        object element = propertyInformation.GetValue(instance, null);
                        if (element == null) {
                            throw new ConfigurationErrorsException(SR.GetString(SR.Config_element_null_instance,
                                propertyInformation.Name,attribProperty.Name));
                        }

                        ApplyInstanceAttributes(element);
                    }
                }
            }
        }

        private static bool PropertiesFromType(Type type, out ConfigurationPropertyCollection result) {
            ConfigurationPropertyCollection properties = (ConfigurationPropertyCollection)s_propertyBags[type];
            result = null;
            bool firstTimeInit = false;
            if (properties == null) {
                lock (s_propertyBags.SyncRoot) {
                    properties = (ConfigurationPropertyCollection)s_propertyBags[type];
                    if (properties == null) {
                        properties = CreatePropertyBagFromType(type);
                        s_propertyBags[type] = properties;
                        firstTimeInit = true;
                    }
                }
            }
            result = properties;
            return firstTimeInit;
        }

        private static ConfigurationPropertyCollection CreatePropertyBagFromType(Type type) {
            Debug.Assert(type != null, "type != null");

            // For ConfigurationElement derived classes - get the per-type validator
            if (typeof(ConfigurationElement).IsAssignableFrom(type)) {
                ConfigurationValidatorAttribute attribValidator = Attribute.GetCustomAttribute(type, typeof(ConfigurationValidatorAttribute)) as ConfigurationValidatorAttribute;

                if (attribValidator != null) {
                    attribValidator.SetDeclaringType(type);
                    ConfigurationValidatorBase validator = attribValidator.ValidatorInstance;

                    if (validator != null) {
                        CachePerTypeValidator(type, validator);
                    }
                }
            }

            ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

            foreach (PropertyInfo propertyInformation in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                ConfigurationProperty newProp = CreateConfigurationPropertyFromAttributes(propertyInformation);

                if (newProp != null) {
                    properties.Add(newProp);
                }
            }

            return properties;
        }
        private static ConfigurationProperty CreateConfigurationPropertyFromAttributes(PropertyInfo propertyInformation) {
            Debug.Assert(propertyInformation != null, "propertyInformation != null");

            ConfigurationProperty result = null;

            ConfigurationPropertyAttribute attribProperty =
                Attribute.GetCustomAttribute(propertyInformation,
                                                typeof(ConfigurationPropertyAttribute)) as ConfigurationPropertyAttribute;

            // If there is no ConfigurationProperty attrib - this is not considered a property
            if (attribProperty != null) {
                result = new ConfigurationProperty(propertyInformation);
            }

            // Handle some special cases of property types
            if (result != null && typeof(ConfigurationElement).IsAssignableFrom(result.Type)) {
                ConfigurationPropertyCollection unused = null;

                PropertiesFromType(result.Type, out unused);
            }

            return result;
        }

        private static void CachePerTypeValidator( Type type, ConfigurationValidatorBase validator ) {
            Debug.Assert((type != null) && ( validator != null));
            Debug.Assert(typeof(ConfigurationElement).IsAssignableFrom(type));

            // Use the same lock as the property bag lock since in the current implementation
            // the only way to get to this method is through the code path that locks the property bag cache first ( see PropertiesFromType() )

            // NOTE[ Thread Safety ]: Non-guarded access to static variable - since this code is called only from CreatePropertyBagFromType
            // which in turn is done onle once per type and is guarded by the s_propertyBag.SyncRoot then this call is thread safe as well
            if (s_perTypeValidators == null ) {
                    s_perTypeValidators = new Dictionary<Type,ConfigurationValidatorBase>();
            }

            // A type validator should be cached only once. If it isn't then attribute parsing is done more then once which should be avoided
            Debug.Assert( !s_perTypeValidators.ContainsKey(type));

            // Make sure the supplied validator supports validating this object
            if (!validator.CanValidate(type)) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Validator_does_not_support_elem_type,
                                                       type.Name));
            }

            s_perTypeValidators.Add(type, validator);
        }

        private static void ApplyValidatorsRecursive(ConfigurationElement root) {
            Debug.Assert(root != null);

            // Apply the validator on 'root'
            ApplyValidator(root);

            // Apply validators on child elements ( note - we will do this only on already created child elements
            // The non created ones will get their validators in the ctor
            foreach (ConfigurationElement elem in root._values.ConfigurationElements) {

                ApplyValidatorsRecursive(elem);
            }
        }

        private static void ApplyValidator(ConfigurationElement elem) {
            Debug.Assert(elem != null);

            if ((s_perTypeValidators != null) && (s_perTypeValidators.ContainsKey(elem.GetType()))) {
                elem._elementProperty = new ConfigurationElementProperty(s_perTypeValidators[ elem.GetType() ]);
            }
        }

        protected void SetPropertyValue(ConfigurationProperty prop, object value, bool ignoreLocks) {
            if (IsReadOnly()) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_read_only));
            }

            if ((ignoreLocks == false) &&
                ((_lockedAllExceptAttributesList != null && _lockedAllExceptAttributesList.HasParentElements && !_lockedAllExceptAttributesList.DefinedInParent(prop.Name)) ||
                    (_lockedAttributesList != null && (_lockedAttributesList.DefinedInParent(prop.Name) || _lockedAttributesList.DefinedInParent(LockAll))) ||
                    ((_fItemLocked & ConfigurationValueFlags.Locked) != 0) &&
                    (_fItemLocked & ConfigurationValueFlags.Inherited) != 0)) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_attribute_locked, prop.Name));
            }

            _bModified = true;

            // Run the new value through the validator to make sure its ok to store it
            if (value != null) {
                prop.Validate(value);
            }

            _values[prop.Name] = (value != null) ? value : s_nullPropertyValue;
        }

        protected internal virtual ConfigurationPropertyCollection Properties {
            get {
                ConfigurationPropertyCollection result = null;

                if (PropertiesFromType(this.GetType(), out result)) {
                    ApplyInstanceAttributes(this);  // Redundant but preserved to minimize code changes for Whidbey RTM
                    ApplyValidatorsRecursive(this);
                }
                return result;
            }
        }

        internal ConfigurationValues Values {
            get {
                return _values;
            }
        }

        internal PropertySourceInfo PropertyInfoInternal(string propertyName) {
            return (PropertySourceInfo)_values.GetSourceInfo(propertyName);
        }

        internal string PropertyFileName(string propertyName) {
            PropertySourceInfo p = (PropertySourceInfo)PropertyInfoInternal(propertyName);
            if (p == null)
                p = (PropertySourceInfo)PropertyInfoInternal(String.Empty); // Get the filename of the parent if prop is not there
            if (p == null)
                return String.Empty;
            return p.FileName;
        }

        internal int PropertyLineNumber(string propertyName) {
            PropertySourceInfo p = (PropertySourceInfo)PropertyInfoInternal(propertyName);
            if (p == null)
                p = (PropertySourceInfo)PropertyInfoInternal(String.Empty);
            if (p == null)
                return 0;
            return p.LineNumber;
        }

        internal virtual void Dump(TextWriter tw) {
            tw.WriteLine("Type: " + GetType().FullName);

            foreach (PropertyInfo pi in GetType().GetProperties()) {
                tw.WriteLine("{0}: {1}", pi.Name, pi.GetValue(this, null));
            }

        }

        protected internal virtual void Unmerge(ConfigurationElement sourceElement,
                                                ConfigurationElement parentElement,
                                                ConfigurationSaveMode saveMode) {
            if (sourceElement != null) {
                bool hasAnyChildElements = false;


                _lockedAllExceptAttributesList = sourceElement._lockedAllExceptAttributesList;
                _lockedAllExceptElementsList = sourceElement._lockedAllExceptElementsList;
                _fItemLocked = sourceElement._fItemLocked;
                _lockedAttributesList = sourceElement._lockedAttributesList;
                _lockedElementsList = sourceElement._lockedElementsList;
                AssociateContext(sourceElement._configRecord);

                if (parentElement != null) {
                    if (parentElement._lockedAttributesList != null)
                        _lockedAttributesList = UnMergeLockList(sourceElement._lockedAttributesList,
                            parentElement._lockedAttributesList, saveMode);
                    if (parentElement._lockedElementsList != null)
                        _lockedElementsList = UnMergeLockList(sourceElement._lockedElementsList,
                            parentElement._lockedElementsList, saveMode);
                    if (parentElement._lockedAllExceptAttributesList != null)
                        _lockedAllExceptAttributesList = UnMergeLockList(sourceElement._lockedAllExceptAttributesList,
                            parentElement._lockedAllExceptAttributesList, saveMode);
                    if (parentElement._lockedAllExceptElementsList != null)
                        _lockedAllExceptElementsList = UnMergeLockList(sourceElement._lockedAllExceptElementsList,
                            parentElement._lockedAllExceptElementsList, saveMode);
                }

                ConfigurationPropertyCollection props = Properties;
                ConfigurationPropertyCollection collectionKeys = null;

                // check for props not in bag from source
                for (int index = 0; index < sourceElement.Values.Count; index++) {
                    string key = sourceElement.Values.GetKey(index);
                    object value = sourceElement.Values[index];
                    ConfigurationProperty prop = (ConfigurationProperty)sourceElement.Properties[key];
                    if (prop == null || (collectionKeys != null && !collectionKeys.Contains(prop.Name)))
                        continue;
                    if (prop.IsConfigurationElementType) {
                        hasAnyChildElements = true;
                    }
                    else {
                        if (value != s_nullPropertyValue) {
                            if (!props.Contains(key)) // this is for optional provider models keys
                            {
                                // _values[key] = value;
                                ConfigurationValueFlags valueFlags = sourceElement.Values.RetrieveFlags(key);
                                _values.SetValue(key, value, valueFlags, null);

                                props.Add(prop);
                            }
                        }
                    }
                }

                foreach (ConfigurationProperty prop in Properties) {
                    if (prop == null || (collectionKeys != null && !collectionKeys.Contains(prop.Name))) {
                        continue;
                    }
                    if (prop.IsConfigurationElementType) {
                        hasAnyChildElements = true;
                    }
                    else {
                        object value = sourceElement.Values[prop.Name];

                        // if the property is required or we are writing a full config make sure we have defaults
                        if ((prop.IsRequired == true || saveMode == ConfigurationSaveMode.Full) && (value == null || value == s_nullPropertyValue)) {
                            // If the default value is null, this means there wasnt a reasonable default for the value
                            // and there is nothing more we can do. Otherwise reset the value to the default

                            // Note: 'null' should be used as default for non-empty strings instead
                            // of the current practice to use String.Epmty

                            if (prop.DefaultValue != null) {
                                value = prop.DefaultValue; // need to make sure required properties are persisted
                            }
                        }

                        if (value != null && value != s_nullPropertyValue) {
                            object value2 = null;
                            if (parentElement != null)                      // Is there a parent
                                value2 = parentElement.Values[prop.Name];   // if so get it's value

                            if (value2 == null)                             // no parent use default
                                value2 = prop.DefaultValue;
                            // If changed and not same as parent write or required

                            switch (saveMode) {
                                case ConfigurationSaveMode.Minimal: {
                                        if (!Object.Equals(value, value2) || prop.IsRequired == true)
                                            _values[prop.Name] = value;
                                    }
                                    break;
                                // (value != null && value != s_nullPropertyValue) ||
                                case ConfigurationSaveMode.Modified: {
                                        bool modified = sourceElement.Values.IsModified(prop.Name);
                                        bool inherited = sourceElement.Values.IsInherited(prop.Name);

                                        // update the value if the property is required, modified or it was not inherited
                                        // Also update properties that ARE inherited when we are resetting the object
                                        // as long as the property is not the same as the default value for the property
                                        if ((prop.IsRequired || modified || !inherited) ||
                                            (parentElement == null && inherited && !Object.Equals(value, value2))) {
                                            _values[prop.Name] = value;
                                        }
                                    }
                                    break;
                                case ConfigurationSaveMode.Full: {
                                        if (value != null && value != s_nullPropertyValue)
                                            _values[prop.Name] = value;
                                        else
                                            _values[prop.Name] = value2;

                                    }
                                    break;
                            }
                        }
                    }
                }

                if (hasAnyChildElements) {
                    foreach (ConfigurationProperty prop in Properties) {
                        if (prop.IsConfigurationElementType) {
                            ConfigurationElement pElem = (ConfigurationElement)((parentElement != null) ? parentElement[prop] : null);
                            ConfigurationElement childElement = (ConfigurationElement)this[prop];
                            if ((ConfigurationElement)sourceElement[prop] != null)
                                childElement.Unmerge((ConfigurationElement)sourceElement[prop],
                                    pElem, saveMode);
                        }

                    }
                }
            }
        }

        protected internal virtual bool SerializeToXmlElement(XmlWriter writer, String elementName) {
            if (_configRecord != null && _configRecord.TargetFramework != null) {
                ConfigurationSection section = null;
                if (_configRecord.SectionsStack.Count >0) {
                    section = _configRecord.SectionsStack.Peek() as ConfigurationSection;
                }
                if (section != null && !section.ShouldSerializeElementInTargetVersion(this, elementName, _configRecord.TargetFramework)) {
                    return false;
                }
            }

            bool DataToWrite = _bDataToWrite;

            //  Don't write elements that are locked in the parent
            if ((_lockedElementsList != null && _lockedElementsList.DefinedInParent(elementName)) ||
                    (_lockedAllExceptElementsList != null && _lockedAllExceptElementsList.HasParentElements && !_lockedAllExceptElementsList.DefinedInParent(elementName))) {
                return DataToWrite;
            }

            if (SerializeElement(null, false) == true) // check if there is anything to write...
            {
                if (writer != null)
                    writer.WriteStartElement(elementName);
                DataToWrite |= SerializeElement(writer, false);
                if (writer != null)
                    writer.WriteEndElement();
            }
            return DataToWrite;
        }

        protected internal virtual bool SerializeElement(XmlWriter writer, bool serializeCollectionKey) {
            PreSerialize(writer);

            bool DataToWrite = _bDataToWrite;
            bool hasAnyChildElements = false;
            bool foundDefaultElement = false;
            ConfigurationPropertyCollection props = Properties;
            ConfigurationPropertyCollection collectionKeys = null;

            for (int index = 0; index < _values.Count; index++) {
                string key = _values.GetKey(index);
                object value = _values[index];

                ConfigurationProperty prop = (ConfigurationProperty)props[key];
                if (prop == null || (collectionKeys != null && !collectionKeys.Contains(prop.Name))) {
                    continue;
                }

                if ( prop.IsVersionCheckRequired && _configRecord != null && _configRecord.TargetFramework != null) {
                    ConfigurationSection section = null;
                    if (_configRecord.SectionsStack.Count >0) {
                        section = _configRecord.SectionsStack.Peek() as ConfigurationSection;
                    }

                    if (section != null && !section.ShouldSerializePropertyInTargetVersion(prop, prop.Name, _configRecord.TargetFramework, this)) {
                        continue;
                    }
                }


                if (prop.IsConfigurationElementType) {
                    hasAnyChildElements = true;
                }
                else {
                    if ((_lockedAllExceptAttributesList != null && _lockedAllExceptAttributesList.HasParentElements && !_lockedAllExceptAttributesList.DefinedInParent(prop.Name)) ||
                        (_lockedAttributesList != null && _lockedAttributesList.DefinedInParent(prop.Name))) {
                        if (prop.IsRequired == true)
                            throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_required_attribute_locked, prop.Name));
                        value = s_nullPropertyValue;
                    }

                    if (value != s_nullPropertyValue) {
                        if (serializeCollectionKey == false || prop.IsKey == true) {
                            string xmlValue = null;

                            // If this was an invalid string value and was cached - write it out as is
                            if (value is InvalidPropValue) {
                                xmlValue = ((InvalidPropValue)value).Value;
                            }
                            else {
                                prop.Validate(value);
                                xmlValue = prop.ConvertToString(value);
                            }

                            if ((xmlValue != null) && (writer != null)) {
                                if (prop.IsTypeStringTransformationRequired)
                                    xmlValue = GetTransformedTypeString(xmlValue);
                                if (prop.IsAssemblyStringTransformationRequired)
                                    xmlValue = GetTransformedAssemblyString(xmlValue);

                                writer.WriteAttributeString(prop.Name, xmlValue);
                            }

                            DataToWrite = DataToWrite || (xmlValue != null);
                        }
                    }
                }
            }
            if (serializeCollectionKey == false) {
                DataToWrite |= SerializeLockList(_lockedAttributesList, LockAttributesKey, writer);
                DataToWrite |= SerializeLockList(_lockedAllExceptAttributesList, LockAllAttributesExceptKey, writer);
                DataToWrite |= SerializeLockList(_lockedElementsList, LockElementsKey, writer);
                DataToWrite |= SerializeLockList(_lockedAllExceptElementsList, LockAllElementsExceptKey, writer);
                if ((_fItemLocked & ConfigurationValueFlags.Locked) != 0 &&
                    (_fItemLocked & ConfigurationValueFlags.Inherited) == 0 &&
                    (_fItemLocked & ConfigurationValueFlags.XMLParentInherited) == 0) {
                    DataToWrite = true;
                    if (writer != null)
                        writer.WriteAttributeString(LockItemKey, true.ToString().ToLower(CultureInfo.InvariantCulture));
                }
            }
            if (hasAnyChildElements) {
                for (int index = 0; index < _values.Count; index++) {
                    string key = _values.GetKey(index);
                    object value = _values[index];

                    ConfigurationProperty prop = (ConfigurationProperty)props[key];
                    // if we are writing a remove and the sub element is not part of the key don't write it.
                    if (serializeCollectionKey == false || prop.IsKey == true) {
                        if (value is ConfigurationElement) {
                            if (!((_lockedElementsList != null && _lockedElementsList.DefinedInParent(key)) ||
                                (_lockedAllExceptElementsList != null && _lockedAllExceptElementsList.HasParentElements && !_lockedAllExceptElementsList.DefinedInParent(key)))) {

                                ConfigurationElement elem = (ConfigurationElement)value;

                                if (prop.Name != ConfigurationProperty.DefaultCollectionPropertyName) {
                                    DataToWrite |= elem.SerializeToXmlElement(writer, prop.Name);
                                }
                                else if (!foundDefaultElement) {
                                    // Prevent the locks from serializing a second time since locks
                                    // on a default collection serialize with their parent node
                                    elem._lockedAttributesList = null;
                                    elem._lockedAllExceptAttributesList = null;
                                    elem._lockedElementsList = null;
                                    elem._lockedAllExceptElementsList = null;

                                    DataToWrite |= elem.SerializeElement(writer, false);

                                    foundDefaultElement = true;
                                }
                                else {
                                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_element_cannot_have_multiple_child_elements, prop.Name));
                                }
                            }
                        }
                    }
                }
            }
            return DataToWrite;
        }
        private bool SerializeLockList(ConfigurationLockCollection list, String elementKey, XmlWriter writer) {
            StringBuilder sb;

            sb = new StringBuilder();

            if (list != null) {
                foreach (string key in list) {
                    if (!list.DefinedInParent(key)) {
                        if (sb.Length != 0)
                            sb.Append(',');
                        sb.Append((string)key);
                    }
                }
            }

            if (writer != null && sb.Length != 0)
                writer.WriteAttributeString(elementKey, sb.ToString());
            return (sb.Length != 0);
        }
        internal void ReportInvalidLock(string attribToLockTrim, ConfigurationLockCollectionType lockedType, ConfigurationValue value, String collectionProperties) {
            StringBuilder sb;
            sb = new StringBuilder();

            // Add the collection properties when locking elements
            if (!String.IsNullOrEmpty(collectionProperties) &&
                    ((lockedType == ConfigurationLockCollectionType.LockedElements) || (lockedType == ConfigurationLockCollectionType.LockedElementsExceptionList))) {
                if (sb.Length != 0)
                    sb.Append(',');
                sb.Append(collectionProperties);
            }

            // construct a list of valid lockable properties
            foreach (object _prop in Properties) {
                ConfigurationProperty validProp = (ConfigurationProperty)_prop;
                if (validProp.Name != LockAttributesKey &&
                    validProp.Name != LockAllAttributesExceptKey &&
                    validProp.Name != LockElementsKey &&
                    validProp.Name != LockAllElementsExceptKey
                ) {
                    if ((lockedType == ConfigurationLockCollectionType.LockedElements) ||
                            (lockedType == ConfigurationLockCollectionType.LockedElementsExceptionList)) {
                        if (typeof(ConfigurationElement).IsAssignableFrom(validProp.Type)) {
                            if (sb.Length != 0)
                                sb.Append(", ");
                            sb.Append("'");
                            sb.Append(validProp.Name);
                            sb.Append("'");
                        }
                    }
                    else {
                        if (!typeof(ConfigurationElement).IsAssignableFrom(validProp.Type)) {
                            if (sb.Length != 0)
                                sb.Append(", ");
                            sb.Append("'");
                            sb.Append(validProp.Name);
                            sb.Append("'");
                        }
                    }
                }
            }

            string format = null;

            if ((lockedType == ConfigurationLockCollectionType.LockedElements) ||
                    (lockedType == ConfigurationLockCollectionType.LockedElementsExceptionList)) {
                if (value != null)
                    format = SR.GetString(SR.Config_base_invalid_element_to_lock);
                else
                    format = SR.GetString(SR.Config_base_invalid_element_to_lock_by_add);

            }
            else {
                if (value != null)
                    format = SR.GetString(SR.Config_base_invalid_attribute_to_lock);
                else
                    format = SR.GetString(SR.Config_base_invalid_attribute_to_lock_by_add);
            }
            if (value != null)
                throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture, format, attribToLockTrim, sb.ToString()), value.SourceInfo.FileName, value.SourceInfo.LineNumber);
            else
                throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture, format, attribToLockTrim, sb.ToString()));
        }

        private ConfigurationLockCollection ParseLockedAttributes(ConfigurationValue value, ConfigurationLockCollectionType lockType) {
            // check that only actual properties are in the lock attribute
            ConfigurationLockCollection localLockedAttributesList = new ConfigurationLockCollection(this, lockType);
            string attributeList = (string)(value.Value);

            if (string.IsNullOrEmpty(attributeList)) {
                if (lockType == ConfigurationLockCollectionType.LockedAttributes)
                    throw new ConfigurationErrorsException(SR.GetString(SR.Empty_attribute, LockAttributesKey), value.SourceInfo.FileName, value.SourceInfo.LineNumber);
                if (lockType == ConfigurationLockCollectionType.LockedElements)
                    throw new ConfigurationErrorsException(SR.GetString(SR.Empty_attribute, LockElementsKey), value.SourceInfo.FileName, value.SourceInfo.LineNumber);
                if (lockType == ConfigurationLockCollectionType.LockedExceptionList)
                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_empty_lock_attributes_except, LockAllAttributesExceptKey, LockAttributesKey), value.SourceInfo.FileName, value.SourceInfo.LineNumber);
                if (lockType == ConfigurationLockCollectionType.LockedElementsExceptionList)
                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_empty_lock_element_except, LockAllElementsExceptKey, LockElementsKey), value.SourceInfo.FileName, value.SourceInfo.LineNumber);
            }

            string[] attribsToLock = attributeList.Split(new char[] { ',', ':', ';' });
            foreach (string attribToLock in attribsToLock) {
                string attribToLockTrim = attribToLock.Trim();
                if (!String.IsNullOrEmpty(attribToLockTrim)) {
                    // validate that the locks are good
                    if (!((lockType == ConfigurationLockCollectionType.LockedElements ||
                         lockType == ConfigurationLockCollectionType.LockedAttributes) &&
                         attribToLockTrim == LockAll)) {
                        ConfigurationProperty propToLock = Properties[attribToLockTrim];

                        if (propToLock == null ||                                   // if the prop does not exist
                            attribToLockTrim == LockAttributesKey ||                // or it is the lockattributes keyword
                            attribToLockTrim == LockAllAttributesExceptKey ||       // or it is the lockattributes keyword
                            attribToLockTrim == LockElementsKey ||                  // or it is the lockelements keyword
                            (lockType != ConfigurationLockCollectionType.LockedElements && lockType != ConfigurationLockCollectionType.LockedElementsExceptionList &&
                                typeof(ConfigurationElement).IsAssignableFrom(propToLock.Type)) ||  // or if not locking elements but the property is a element
                            ((lockType == ConfigurationLockCollectionType.LockedElements || lockType == ConfigurationLockCollectionType.LockedElementsExceptionList) &&
                             !typeof(ConfigurationElement).IsAssignableFrom(propToLock.Type)) // or if locking elements but the property is not an element
                        ) {
                        // check to see if this is a collection and we are locking a collection element

                            ConfigurationElementCollection collection = this as ConfigurationElementCollection;
                            if (collection == null && Properties.DefaultCollectionProperty != null) // this is not a collection but it may contain a default collection
                            {
                                collection = this[Properties.DefaultCollectionProperty] as ConfigurationElementCollection;
                            }
                            if (collection == null ||
                                lockType == ConfigurationLockCollectionType.LockedAttributes || // If the collection type is not element then the lock is bogus
                                lockType == ConfigurationLockCollectionType.LockedExceptionList) {
                                ReportInvalidLock(attribToLockTrim, lockType, value, null);
                            }
                            else if (!collection.IsLockableElement(attribToLockTrim)) {
                                ReportInvalidLock(attribToLockTrim, lockType, value, collection.LockableElements);
                            }
                        }
                        if (propToLock != null && propToLock.IsRequired == true)
                            throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_required_attribute_lock_attempt, propToLock.Name));
                    }


                    // concatenate the new attribute.
                    localLockedAttributesList.Add(attribToLockTrim, ConfigurationValueFlags.Default); // Mark as local
                }
            }
            return localLockedAttributesList;
        }

        private StringCollection IntersectLockCollections(ConfigurationLockCollection Collection1, ConfigurationLockCollection Collection2) {
            ConfigurationLockCollection smallCollection = Collection1.Count < Collection2.Count ? Collection1 : Collection2;
            ConfigurationLockCollection largeCollection = Collection1.Count >= Collection2.Count ? Collection1 : Collection2;
            StringCollection intersectionCollection = new StringCollection();

            foreach (string key in smallCollection) {
                if (largeCollection.Contains(key) || key == ElementTagName)
                    intersectionCollection.Add(key);  // add the local copy
            }
            return intersectionCollection;
        }


        protected internal virtual void DeserializeElement(XmlReader reader, bool serializeCollectionKey) {
            ConfigurationPropertyCollection props = Properties;
            ConfigurationValue LockedAttributesList = null;
            ConfigurationValue LockedAllExceptList = null;
            ConfigurationValue LockedElementList = null;
            ConfigurationValue LockedAllElementsExceptList = null;
            bool ItemLockedLocally = false;

            _bElementPresent = true;

            ConfigurationElement defaultCollection = null;
            ConfigurationProperty defaultCollectionProperty = props != null ? props.DefaultCollectionProperty : null;
            if (defaultCollectionProperty != null) {
                defaultCollection = (ConfigurationElement)this[defaultCollectionProperty];
            }

            // Process attributes
            _elementTagName = reader.Name;
            PropertySourceInfo rootInfo = new PropertySourceInfo(reader);
            _values.SetValue(reader.Name, null, ConfigurationValueFlags.Modified, rootInfo);
            _values.SetValue(DefaultCollectionPropertyName, defaultCollection, ConfigurationValueFlags.Modified, rootInfo);

            if ((_lockedElementsList != null && (_lockedElementsList.Contains(reader.Name) ||
                    (_lockedElementsList.Contains(LockAll) && reader.Name != ElementTagName))) ||
                (_lockedAllExceptElementsList != null && _lockedAllExceptElementsList.Count != 0 && !_lockedAllExceptElementsList.Contains(reader.Name)) ||
                ((_fItemLocked & ConfigurationValueFlags.Locked) != 0 && (_fItemLocked & ConfigurationValueFlags.Inherited) != 0)
               ) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_element_locked, reader.Name), reader);
            }


            if (reader.AttributeCount > 0) {
                while (reader.MoveToNextAttribute()) {

                    String propertyName = reader.Name;
                    if ((_lockedAttributesList != null && (_lockedAttributesList.Contains(propertyName) || _lockedAttributesList.Contains(LockAll))) ||
                        (_lockedAllExceptAttributesList != null && !_lockedAllExceptAttributesList.Contains(propertyName))
                       ) {
                        if (propertyName != LockAttributesKey && propertyName != LockAllAttributesExceptKey)
                            throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_attribute_locked, propertyName), reader);
                    }

                    ConfigurationProperty prop = props != null ? props[propertyName] : null;
                    if (prop != null) {
                        if (serializeCollectionKey && !prop.IsKey) {
                            throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_unrecognized_attribute, propertyName), reader);
                        }

                        _values.SetValue(propertyName,
                                            DeserializePropertyValue(prop, reader),
                                            ConfigurationValueFlags.Modified,
                                            new PropertySourceInfo(reader));

                    }   // if (deserializing a remove OR an add that does not handle optional attributes)
                    else if (propertyName == LockItemKey) {
                        try {
                                ItemLockedLocally = bool.Parse(reader.Value);
                        }
                        catch {
                            throw new ConfigurationErrorsException(SR.GetString(SR.Config_invalid_boolean_attribute, propertyName), reader);
                        }
                    }
                    else if (propertyName == LockAttributesKey) {
                        LockedAttributesList = new ConfigurationValue(reader.Value, ConfigurationValueFlags.Default, new PropertySourceInfo(reader));
                    }
                    else if (propertyName == LockAllAttributesExceptKey) {
                        LockedAllExceptList = new ConfigurationValue(reader.Value, ConfigurationValueFlags.Default, new PropertySourceInfo(reader));
                    }
                    else if (propertyName == LockElementsKey) {
                        LockedElementList = new ConfigurationValue(reader.Value, ConfigurationValueFlags.Default, new PropertySourceInfo(reader));
                    }
                    else if (propertyName == LockAllElementsExceptKey) {
                        LockedAllElementsExceptList = new ConfigurationValue(reader.Value, ConfigurationValueFlags.Default, new PropertySourceInfo(reader));
                    }
                    else if (serializeCollectionKey || !OnDeserializeUnrecognizedAttribute(propertyName, reader.Value)) {
                        throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_unrecognized_attribute, propertyName), reader);
                    }
                }
            }

            reader.MoveToElement();

            // Check for nested elements.
            try {

                HybridDictionary nodeFound = new HybridDictionary();
                if (!reader.IsEmptyElement) {
                    while (reader.Read()) {
                        if (reader.NodeType == XmlNodeType.Element) {
                            String propertyName = reader.Name;

                            CheckLockedElement(propertyName, null);

                            ConfigurationProperty prop = props != null ? props[propertyName] : null;
                            if (prop != null) {
                                if (prop.IsConfigurationElementType) {
                                    // 
                                    if (nodeFound.Contains(propertyName))
                                        throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_element_cannot_have_multiple_child_elements, propertyName), reader);
                                    nodeFound.Add(propertyName, propertyName);
                                    ConfigurationElement childElement = (ConfigurationElement)this[prop];
                                    childElement.DeserializeElement(reader, serializeCollectionKey);

                                    // Validate the new element with the per-property Validator
                                    // Note that the per-type validator for childElement has been already executed as part of Deserialize
                                    ValidateElement(childElement, prop.Validator, false);
                                }
                                else {
                                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_property_is_not_a_configuration_element, propertyName), reader);
                                }
                            }
                            else if (!OnDeserializeUnrecognizedElement(propertyName, reader)) {
                                // Let the default collection, if there is one, handle this node.
                                if (defaultCollection == null ||
                                        !defaultCollection.OnDeserializeUnrecognizedElement(propertyName, reader)) {
                                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_unrecognized_element_name, propertyName), reader);
                                }
                            }
                        }
                        else if (reader.NodeType == XmlNodeType.EndElement) {
                            break;
                        }
                        else if ((reader.NodeType == XmlNodeType.CDATA) || (reader.NodeType == XmlNodeType.Text)) {
                            throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_section_invalid_content), reader);
                        }
                    }
                }

                EnsureRequiredProperties(serializeCollectionKey);

                // Call the per-type validator for this object
                ValidateElement(this, null, false);
            }
            catch (ConfigurationException e) {
                // Catch the generic message from deserialization and include line info if necessary
                if (e.Filename == null || e.Filename.Length == 0)
                    throw new ConfigurationErrorsException(e.Message, reader); // give it some info
                else
                    throw e;
            }

            if (ItemLockedLocally) {
                SetLocked();
                _fItemLocked = ConfigurationValueFlags.Locked;
            }

            if (LockedAttributesList != null) {
                if (_lockedAttributesList == null)
                    _lockedAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedAttributes);
                foreach (string key in ParseLockedAttributes(LockedAttributesList, ConfigurationLockCollectionType.LockedAttributes)) {
                    if (!_lockedAttributesList.Contains(key))
                        _lockedAttributesList.Add(key, ConfigurationValueFlags.Default);  // add the local copy
                    else
                        _lockedAttributesList.Add(key, ConfigurationValueFlags.Modified | ConfigurationValueFlags.Inherited);  // add the local copy
                }
            }
            if (LockedAllExceptList != null) {
                ConfigurationLockCollection newCollection = ParseLockedAttributes(LockedAllExceptList, ConfigurationLockCollectionType.LockedExceptionList);
                if (_lockedAllExceptAttributesList == null) {
                    _lockedAllExceptAttributesList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedExceptionList, String.Empty, newCollection);
                    _lockedAllExceptAttributesList.ClearSeedList(); // Prevent the list from thinking this was set by a parent.
                }
                StringCollection intersectionCollection = IntersectLockCollections(_lockedAllExceptAttributesList, newCollection);
                /*
                if (intersectionCollection.Count == 0) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_empty_lock_attributes_except_effective,
                                                                        LockAllAttributesExceptKey,
                                                                        LockedAllExceptList.Value,
                                                                        LockAttributesKey),
                                                                        LockedAllExceptList.SourceInfo.FileName,
                                                                        LockedAllExceptList.SourceInfo.LineNumber);

                }
                */
                _lockedAllExceptAttributesList.ClearInternal(false);
                foreach (string key in intersectionCollection) {
                    _lockedAllExceptAttributesList.Add(key, ConfigurationValueFlags.Default);
                }
            }
            if (LockedElementList != null) {
                if (_lockedElementsList == null)
                    _lockedElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElements);

                ConfigurationLockCollection localLockedElementList = ParseLockedAttributes(LockedElementList, ConfigurationLockCollectionType.LockedElements);

                ConfigurationElementCollection collection = null;
                if (props.DefaultCollectionProperty != null) // this is not a collection but it may contain a default collection
                {
                    collection = this[props.DefaultCollectionProperty] as ConfigurationElementCollection;
                    if (collection != null && collection._lockedElementsList == null)
                        collection._lockedElementsList = _lockedElementsList;
                }

                foreach (string key in localLockedElementList) {
                    if (!_lockedElementsList.Contains(key)) {
                        _lockedElementsList.Add(key, ConfigurationValueFlags.Default);  // add the local copy

                        ConfigurationProperty propToLock = Properties[key];
                        if (propToLock != null && typeof(ConfigurationElement).IsAssignableFrom(propToLock.Type)) {
                            ((ConfigurationElement)this[key]).SetLocked();
                        }
                        if (key == LockAll) {
                            foreach (ConfigurationProperty prop in Properties) {
                                if (!string.IsNullOrEmpty(prop.Name) &&
                                    prop.IsConfigurationElementType) {
                                    ((ConfigurationElement)this[prop]).SetLocked();
                                }
                            }
                        }

                    }
                }
            }

            if (LockedAllElementsExceptList != null) {
                ConfigurationLockCollection newCollection = ParseLockedAttributes(LockedAllElementsExceptList, ConfigurationLockCollectionType.LockedElementsExceptionList);
                if (_lockedAllExceptElementsList == null) {
                    _lockedAllExceptElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElementsExceptionList, _elementTagName, newCollection);
                    _lockedAllExceptElementsList.ClearSeedList();
                }

                StringCollection intersectionCollection = IntersectLockCollections(_lockedAllExceptElementsList, newCollection);

                ConfigurationElementCollection collection = null;
                if (props.DefaultCollectionProperty != null) // this is not a collection but it may contain a default collection
                {
                    collection = this[props.DefaultCollectionProperty] as ConfigurationElementCollection;
                    if (collection != null && collection._lockedAllExceptElementsList == null)
                        collection._lockedAllExceptElementsList = _lockedAllExceptElementsList;
                }

                _lockedAllExceptElementsList.ClearInternal(false);
                foreach (string key in intersectionCollection) {
                    if (!_lockedAllExceptElementsList.Contains(key) || key == ElementTagName)
                        _lockedAllExceptElementsList.Add(key, ConfigurationValueFlags.Default);  // add the local copy
                }

                foreach (ConfigurationProperty prop in Properties) {
                    if (!(string.IsNullOrEmpty(prop.Name) || _lockedAllExceptElementsList.Contains(prop.Name)) &&
                        prop.IsConfigurationElementType) {
                        ((ConfigurationElement)this[prop]).SetLocked();
                    }
                }

            }

            // Make sure default collections use the same lock element lists
            if (defaultCollectionProperty != null) {
                defaultCollection = (ConfigurationElement)this[defaultCollectionProperty];
                if (_lockedElementsList == null) {
                    _lockedElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElements);
                }
                defaultCollection._lockedElementsList = _lockedElementsList;
                if (_lockedAllExceptElementsList == null) {
                    _lockedAllExceptElementsList = new ConfigurationLockCollection(this, ConfigurationLockCollectionType.LockedElementsExceptionList, reader.Name);
                    _lockedAllExceptElementsList.ClearSeedList();
                }
                defaultCollection._lockedAllExceptElementsList = _lockedAllExceptElementsList;
            }

            // This has to be the last thing to execute
            PostDeserialize();
        }
        private object DeserializePropertyValue(ConfigurationProperty prop, XmlReader reader) {
            Debug.Assert(prop != null, "prop != null");
            Debug.Assert(reader != null, "reader != null");

            // By default we try to load (i.e. parse/validate ) all properties
            // If a property value is invalid ( cannot be parsed or is not valid ) we will keep the value
            // as string ( from the xml ) and will write it out unchanged if needed
            // If the property value is needed by users the actuall exception will be thrown

            string xmlValue = reader.Value;
            object propertyValue = null;

            try {
                propertyValue = prop.ConvertFromString(xmlValue);

                // Validate the loaded and converted value
                prop.Validate(propertyValue);
            }
            catch (ConfigurationException ce) {
                // If the error is incomplete - complete it :)
                if (string.IsNullOrEmpty(ce.Filename)) {
                    ce = new ConfigurationErrorsException(ce.Message, reader);
                }

                // Cannot parse/validate the value. Keep it as string
                propertyValue = new InvalidPropValue(xmlValue, ce);
            }
            catch {
                // If this is an exception related to the parsing/validating the
                // value ConfigurationErrorsException should be thrown instead.
                // If not - the exception is ok to surface out of here
                Debug.Fail("Unknown exception type thrown");
            }

            return propertyValue;
        }

        internal static void ValidateElement(ConfigurationElement elem, ConfigurationValidatorBase propValidator, bool recursive) {
            // Validate a config element with the per-type validator when a per-property ( propValidator ) is not supplied
            // or with the per-prop validator when the element ( elem ) is a child property of another configuration element

            ConfigurationValidatorBase validator = propValidator;

            if ((validator == null) &&   // Not a property - use the per-type validator
                (elem.ElementProperty != null)) {
                validator = elem.ElementProperty.Validator;

                // Since ElementProperty can be overriden by derived classes we need to make sure that
                // the validator supports the type of elem every time
                if ((validator != null) && !validator.CanValidate(elem.GetType())) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Validator_does_not_support_elem_type, elem.GetType().Name));
                }
            }

            try {
                if (validator != null) {
                    validator.Validate(elem);
                }
            }
            catch (ConfigurationException) {
                // ConfigurationElement validators are allowed to throw ConfigurationErrorsException.
                throw;
            }
            catch (Exception ex) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Validator_element_not_valid, elem._elementTagName, ex.Message));
            }

            if (recursive == true) {
                // Validate collection items:
                // Note: this is a bit of a hack - we will exploit the fact that this method is called with recursive == true only when serializing the top level section
                // At deserializtion time the per-element validator for collection items will get executed as part of their deserialization logic
                // However we dont perform validation in the serialization logic ( because at that time the object is unmerged and not all data is present )
                // so we have to do that validation here.
                if (elem is ConfigurationElementCollection) {
                    if (elem is ConfigurationElementCollection) {
                        IEnumerator it = ((ConfigurationElementCollection)elem).GetElementsEnumerator();
                        while( it.MoveNext() ) {
                            ValidateElement((ConfigurationElement)it.Current, null, true);
                        }
                    }
                }

                // Validate all child elements recursively
                for (int index = 0; index < elem.Values.Count; index++) {
                    ConfigurationElement value = elem.Values[index] as ConfigurationElement;

                    if (value != null) {
                        // Run the per-type validator on the child element and proceed with validation in subelements
                        // Note we dont run the per-property validator here since we run those when the property value is set
                        ValidateElement(value, null, true);              // per-type
                    }
                }
            }
        }

        private void EnsureRequiredProperties(bool ensureKeysOnly) {
            ConfigurationPropertyCollection props = Properties;

            // Make sure all required properties are here
            if (props != null) {
                foreach (ConfigurationProperty prop in props) {
                    // The property is required but no value was found
                    if (prop.IsRequired && !_values.Contains(prop.Name)) {
                        // Required properties can be ommited when we need only the keys to be there
                        if (!ensureKeysOnly || prop.IsKey) {
                            _values[prop.Name] = OnRequiredPropertyNotFound(prop.Name);
                        }
                    }
                }
            }
        }
        protected virtual object OnRequiredPropertyNotFound(string name) {
            // Derivied classes can override this to return a value for a required property that is missing
            // Here we treat this as an error though

            throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_required_attribute_missing, name),
                                                    PropertyFileName(name),
                                                    PropertyLineNumber(name));

        }
        protected virtual void PostDeserialize() {
            // Please try to not add code in here
        }
        protected virtual void PreSerialize(XmlWriter writer) {
            // Please try to not add code in here
        }

        protected virtual bool OnDeserializeUnrecognizedAttribute(String name, String value) {
            return false;
        }

        protected virtual bool OnDeserializeUnrecognizedElement(String elementName, XmlReader reader) {
            return false;
        }

        protected virtual string GetTransformedTypeString(string typeName)
        {
            if ( typeName == null || _configRecord == null || !_configRecord.TypeStringTransformerIsSet)
                return typeName;
            else
                return _configRecord.TypeStringTransformer(typeName);
        }

        protected virtual string GetTransformedAssemblyString(string assemblyName)
        {
            if ( assemblyName == null || _configRecord == null || !_configRecord.AssemblyStringTransformerIsSet)
                return assemblyName;
            else
                return _configRecord.AssemblyStringTransformer(assemblyName);
        }

        // Element
        //
        // Retrieve information specific to the element
        //
        public ElementInformation ElementInformation {
            get {
                if (_evaluationElement == null) {
                    _evaluationElement = new ElementInformation(this);
                }
                return _evaluationElement;
            }
        }

        // EvaluationContext
        //
        // Retrieve information specific to the context of how we are
        // being evaluated
        //
        protected ContextInformation EvaluationContext {
            get {
                if (_evalContext == null) {
                    if (_configRecord == null) {
                        // This is not associated with a context, so throw
                        // failure
                        throw new ConfigurationErrorsException(
                                    SR.GetString(
                                      SR.Config_element_no_context));
                    }

                    _evalContext = new ContextInformation(_configRecord);
                }

                return _evalContext;
            }
        }

        internal protected virtual ConfigurationElementProperty ElementProperty {
            get {
                return _elementProperty;
            }
        }

        internal ConfigurationLockCollection UnMergeLockList(
            ConfigurationLockCollection sourceLockList,
            ConfigurationLockCollection parentLockList,
            ConfigurationSaveMode saveMode) {
            if (sourceLockList.ExceptionList == false) {
                switch (saveMode) {
                    case ConfigurationSaveMode.Modified: {
                            ConfigurationLockCollection tempLockList = new ConfigurationLockCollection(this, sourceLockList.LockType);
                            foreach (string lockedAttributeName in sourceLockList)
                                if (!parentLockList.Contains(lockedAttributeName) ||
                                    sourceLockList.IsValueModified(lockedAttributeName)) {
                                    tempLockList.Add(lockedAttributeName, ConfigurationValueFlags.Default);
                                }
                            return tempLockList;
                        }
                    case ConfigurationSaveMode.Minimal: {
                            ConfigurationLockCollection tempLockList = new ConfigurationLockCollection(this, sourceLockList.LockType);
                            foreach (string lockedAttributeName in sourceLockList)
                                if (!parentLockList.Contains(lockedAttributeName)) {
                                    tempLockList.Add(lockedAttributeName, ConfigurationValueFlags.Default);
                                }
                            return tempLockList;
                        }
                }
            }
            else {
                // exception list write out the entire collection unless the entire collection
                // came from the parent.
                if (saveMode == ConfigurationSaveMode.Modified || saveMode == ConfigurationSaveMode.Minimal) {
                    bool sameAsParent = false;
                    if (sourceLockList.Count == parentLockList.Count) {
                        sameAsParent = true;
                        foreach (string lockedAttributeName in sourceLockList) {
                            if (!parentLockList.Contains(lockedAttributeName) ||
                                (sourceLockList.IsValueModified(lockedAttributeName) &&
                                 saveMode == ConfigurationSaveMode.Modified)) {
                                sameAsParent = false;
                            }
                        }
                    }
                    if (sameAsParent == true) {
                        return null;
                    }
                }
            }
            return sourceLockList;
        }

        //
        // Return true if an attribute is one of our reserved locking attributes,
        // false otherwise.
        //
        internal static bool IsLockAttributeName(string name) {
            // optimize for common case that attribute name does not start with "lock"
            if (!StringUtil.StartsWith(name, "lock")) {
                return false;
            }

            foreach (string lockAttributeName in s_lockAttributeNames) {
                if (name == lockAttributeName) {
                    return true;
                }
            }

            return false;
        }

        protected bool HasContext {
            get {
                return _configRecord != null;
            }
        }

        public Configuration CurrentConfiguration {
            get {
                return (_configRecord==null) ? null : _configRecord.CurrentConfiguration;
            }
        }
    }
}
