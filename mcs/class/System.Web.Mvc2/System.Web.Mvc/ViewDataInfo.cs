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

    public class ViewDataInfo {

        private object _value;
        private Func<object> _valueAccessor;

        public ViewDataInfo() {
        }

        public ViewDataInfo(Func<object> valueAccessor) {
            _valueAccessor = valueAccessor;
        }

        public object Container {
            get;
            set;
        }

        public PropertyDescriptor PropertyDescriptor {
            get;
            set;
        }

        public object Value {
            get {
                if (_valueAccessor != null) {
                    _value = _valueAccessor();
                    _valueAccessor = null;
                }

                return _value;
            }
            set {
                _value = value;
                _valueAccessor = null;
            }
        }

    }
}
