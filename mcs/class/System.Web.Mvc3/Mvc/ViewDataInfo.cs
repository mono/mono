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
