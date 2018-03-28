//------------------------------------------------------------------------------
// <copyright file="ResourceDefaultValueAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Web.Resources;

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class ResourceDefaultValueAttribute : DefaultValueAttribute {

        private Type _type;
        private bool _resourceLoaded;

        internal ResourceDefaultValueAttribute(Type type, string value)
            : base(value) {
            _type = type;
        }

        internal ResourceDefaultValueAttribute(string value) : base(value) { }

        public override object TypeId {
            get {
                return typeof(DefaultValueAttribute);
            }
        }

        public override object Value {
            get {
                if (!_resourceLoaded) {
                    _resourceLoaded = true;
                    string baseValue = (string)base.Value;
                    if (!String.IsNullOrEmpty(baseValue)) {
                        object value = AtlasWeb.ResourceManager.GetString(baseValue, AtlasWeb.Culture);
                        if (_type != null) {
                            try {
                                value = TypeDescriptor.GetConverter(_type).ConvertFromInvariantString((string)value);
                            }
                            catch (NotSupportedException) {
                                value = null;
                            }
                        }
                        base.SetValue(value);
                    }
                }
                return base.Value;
            }
        }
    }
}
