/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.ComponentModel;
    using System.Reflection;

    public class ReflectedParameterDescriptor : ParameterDescriptor {

        private readonly ActionDescriptor _actionDescriptor;
        private readonly ReflectedParameterBindingInfo _bindingInfo;

        public ReflectedParameterDescriptor(ParameterInfo parameterInfo, ActionDescriptor actionDescriptor) {
            if (parameterInfo == null) {
                throw new ArgumentNullException("parameterInfo");
            }
            if (actionDescriptor == null) {
                throw new ArgumentNullException("actionDescriptor");
            }

            ParameterInfo = parameterInfo;
            _actionDescriptor = actionDescriptor;
            _bindingInfo = new ReflectedParameterBindingInfo(parameterInfo);
        }

        public override ActionDescriptor ActionDescriptor {
            get {
                return _actionDescriptor;
            }
        }

        public override ParameterBindingInfo BindingInfo {
            get {
                return _bindingInfo;
            }
        }

        public override object DefaultValue {
            get {
                object value;
                if (ParameterInfoUtil.TryGetDefaultValue(ParameterInfo, out value)) {
                    return value;
                }
                else {
                    return base.DefaultValue;
                }
            }
        }

        public ParameterInfo ParameterInfo {
            get;
            private set;
        }

        public override string ParameterName {
            get {
                return ParameterInfo.Name;
            }
        }

        public override Type ParameterType {
            get {
                return ParameterInfo.ParameterType;
            }
        }

        public override object[] GetCustomAttributes(bool inherit) {
            return ParameterInfo.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
            return ParameterInfo.GetCustomAttributes(attributeType, inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit) {
            return ParameterInfo.IsDefined(attributeType, inherit);
        }

    }
}
