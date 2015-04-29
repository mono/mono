//------------------------------------------------------------------------------
// <copyright file="DateTimeConfigurationElement.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * ImmutableCollections
 *
 * Copyright (c) 2004 Microsoft Corporation
 */

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Configuration;

//
// This file contains configuration collections that are used by multiple sections
//
#if ORCAS
namespace System.Configuration {

    class DateTimeConfigurationElement : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propValue =
            new ConfigurationProperty("value", typeof(DateTime), DateTime.MinValue, ConfigurationPropertyOptions.IsKey);

        static DateTimeConfigurationElement() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propValue);
        }

        bool _needsInit;
        DateTime _initValue;

        protected internal override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }
        //
        // Constructor
        //
        public DateTimeConfigurationElement() {
        }

        public DateTimeConfigurationElement(DateTime value) {
            _needsInit = true;
            _initValue = value;
        }

        protected internal override void Init() {
            base.Init();

            // We cannot initialize configuration properties in the constructor,
            // because Properties is an overridable virtual property that 
            // hence may not be available in the constructor.
            if (_needsInit) {
                _needsInit = false;
                Value = _initValue;
            }
        }

        //
        // Properties
        //

        //
        // ConfigurationPropertyOptions.IsKey="true"
        //
        [ConfigurationProperty("value", IsKey = true)]
        public DateTime Value {
            get {
                return (DateTime)base[_propValue];
            }
            set {
                base[_propValue] = value;
            }
        }
    }
}
#endif
