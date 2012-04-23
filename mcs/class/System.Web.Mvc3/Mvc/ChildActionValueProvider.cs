namespace System.Web.Mvc {
    using System;
    using System.Globalization;

    public sealed class ChildActionValueProvider : DictionaryValueProvider<object> {

        public ChildActionValueProvider(ControllerContext controllerContext)
            : base(controllerContext.RouteData.Values, CultureInfo.InvariantCulture) {
        }

        private static string _childActionValuesKey = Guid.NewGuid().ToString();

        internal static string ChildActionValuesKey {
            get {
                return _childActionValuesKey;
            }
        }

        public override ValueProviderResult GetValue(string key) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }

            ValueProviderResult explicitValues = base.GetValue(ChildActionValuesKey);
            if (explicitValues != null) {
                DictionaryValueProvider<object> rawExplicitValues = explicitValues.RawValue as DictionaryValueProvider<object>;
                if (rawExplicitValues != null) {
                    return rawExplicitValues.GetValue(key);
                }
            }

            return null;
        }
    }
}
