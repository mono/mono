namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Reflection;
    using System.Web.Mvc.Resources;

    internal class ReflectedParameterBindingInfo : ParameterBindingInfo {

        private ICollection<string> _exclude = new string[0];
        private ICollection<string> _include = new string[0];
        private readonly ParameterInfo _parameterInfo;
        private string _prefix;

        public ReflectedParameterBindingInfo(ParameterInfo parameterInfo) {
            _parameterInfo = parameterInfo;
            ReadSettingsFromBindAttribute();
        }

        public override IModelBinder Binder {
            get {
                IModelBinder binder = ModelBinders.GetBinderFromAttributes(_parameterInfo,
                    () => String.Format(CultureInfo.CurrentCulture, MvcResources.ReflectedParameterBindingInfo_MultipleConverterAttributes,
                        _parameterInfo.Name, _parameterInfo.Member));

                return binder;
            }
        }

        public override ICollection<string> Exclude {
            get {
                return _exclude;
            }
        }

        public override ICollection<string> Include {
            get {
                return _include;
            }
        }

        public override string Prefix {
            get {
                return _prefix;
            }
        }

        private void ReadSettingsFromBindAttribute() {
            BindAttribute attr = (BindAttribute)Attribute.GetCustomAttribute(_parameterInfo, typeof(BindAttribute));
            if (attr == null) {
                return;
            }

            _exclude = new ReadOnlyCollection<string>(AuthorizeAttribute.SplitString(attr.Exclude));
            _include = new ReadOnlyCollection<string>(AuthorizeAttribute.SplitString(attr.Include));
            _prefix = attr.Prefix;
        }

    }
}
