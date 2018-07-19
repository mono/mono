//------------------------------------------------------------------------------
// <copyright file="WebSysDefaultValueAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {

    using System;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class WebSysDefaultValueAttribute : DefaultValueAttribute {

        private Type _type;
        private bool _localized;

        internal WebSysDefaultValueAttribute(Type type, string value) : base(value) {
            _type = type;
        }

        internal WebSysDefaultValueAttribute(string value) : base(value) { }

        public override object TypeId {
            get {
                return typeof(DefaultValueAttribute);
            }
        }

        public override object Value {
            get {
                if (!_localized) {
                    _localized = true;
                    string baseValue = (string)base.Value;
                    if (!String.IsNullOrEmpty(baseValue)) {
                        object value = SR.GetString(baseValue);
                        if (_type != null) {
                            try {
                                value = TypeDescriptor.GetConverter(_type).ConvertFromInvariantString((string) value);
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
