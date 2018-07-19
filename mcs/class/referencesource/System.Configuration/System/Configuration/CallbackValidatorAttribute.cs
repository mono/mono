//------------------------------------------------------------------------------
// <copyright file="CallbackValidatorAttribute.cs" company="Microsoft">
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

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CallbackValidatorAttribute : ConfigurationValidatorAttribute {
        private Type _type = null;
        private String _callbackMethodName = String.Empty;
        private ValidatorCallback _callbackMethod;

        public override ConfigurationValidatorBase ValidatorInstance {
            get {
                if (_callbackMethod == null) {
                    if (_type == null) {
                        throw new ArgumentNullException("Type");
                    }
                    if (!String.IsNullOrEmpty(_callbackMethodName)) {
                        MethodInfo methodInfo = _type.GetMethod(_callbackMethodName, BindingFlags.Public | BindingFlags.Static);
                        if (methodInfo != null) {
                            ParameterInfo[] parameters = methodInfo.GetParameters();
                            if ((parameters.Length == 1) && (parameters[0].ParameterType == typeof(Object))) {
                                // The security here depends on nobody changing the type or callback method once the declaring type has been
                                // set. This currently isn't an issue since the attribute is instantiated then the declaring type is set without
                                // user code having gotten a chance to run in between. But if the behavior of PropertyInfo.GetAttributes ever
                                // changes such that it returns cached attribute instances rather than new instances every time, this assumption is void.
                                _callbackMethod = (ValidatorCallback)TypeUtil.CreateDelegateRestricted(_declaringType, typeof(ValidatorCallback), methodInfo);
                            }
                        }
                    }
                }
                if (_callbackMethod == null) {
                    throw new System.ArgumentException(SR.GetString(SR.Validator_method_not_found, _callbackMethodName));
                }

                return new CallbackValidator(_callbackMethod);
            }
        }

        public CallbackValidatorAttribute() {
        }

        public Type Type {
            get {
                return _type;
            }
            set {
                _type = value;
                _callbackMethod = null;
            }
        }

        public String CallbackMethodName {
            get {
                return _callbackMethodName;
            }
            set {
                _callbackMethodName = value;
                _callbackMethod = null;
            }
        }
    }
}
