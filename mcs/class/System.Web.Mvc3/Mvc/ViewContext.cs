namespace System.Web.Mvc {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Web.WebPages.Scope;

    public class ViewContext : ControllerContext {

        private const string _clientValidationScript = @"<script type=""text/javascript"">
//<![CDATA[
if (!window.mvcClientValidationMetadata) {{ window.mvcClientValidationMetadata = []; }}
window.mvcClientValidationMetadata.push({0});
//]]>
</script>";

        internal static readonly string ClientValidationKeyName = "ClientValidationEnabled";
        internal static readonly string UnobtrusiveJavaScriptKeyName = "UnobtrusiveJavaScriptEnabled";

        // Some values have to be stored in HttpContext.Items in order to be propagated between calls
        // to RenderPartial(), RenderAction(), etc.
        private static readonly object _formContextKey = new object();
        private static readonly object _lastFormNumKey = new object();

        private Func<IDictionary<object, object>> _scopeThunk;
        private IDictionary<object, object> _transientScope;

        private Func<string> _formIdGenerator;

        // parameterless constructor used for mocking
        public ViewContext() {
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "The virtual property setters are only to support mocking frameworks, in which case this constructor shouldn't be called anyway.")]
        public ViewContext(ControllerContext controllerContext, IView view, ViewDataDictionary viewData, TempDataDictionary tempData, TextWriter writer)
            : base(controllerContext) {
            if (controllerContext == null) {
                throw new ArgumentNullException("controllerContext");
            }
            if (view == null) {
                throw new ArgumentNullException("view");
            }
            if (viewData == null) {
                throw new ArgumentNullException("viewData");
            }
            if (tempData == null) {
                throw new ArgumentNullException("tempData");
            }
            if (writer == null) {
                throw new ArgumentNullException("writer");
            }

            View = view;
            ViewData = viewData;
            Writer = writer;
            TempData = tempData;
        }

        public virtual bool ClientValidationEnabled {
            get {
                return GetClientValidationEnabled(Scope, HttpContext);
            }
            set {
                SetClientValidationEnabled(value, Scope, HttpContext);
            }
        }

        public virtual FormContext FormContext {
            get {
                return HttpContext.Items[_formContextKey] as FormContext;
            }
            set {
                HttpContext.Items[_formContextKey] = value;
            }
        }

        internal Func<string> FormIdGenerator {
            get {
                if (_formIdGenerator == null) {
                    _formIdGenerator = DefaultFormIdGenerator;
                }
                return _formIdGenerator;
            }
            set {
                _formIdGenerator = value;
            }
        }

        internal static Func<IDictionary<object, object>> GlobalScopeThunk {
            get;
            set;
        }

        private IDictionary<object, object> Scope {
            get {
                if (ScopeThunk != null) {
                    return ScopeThunk();
                }
                if (_transientScope == null) {
                    _transientScope = new Dictionary<object, object>();
                }
                return _transientScope;
            }
        }

        internal Func<IDictionary<object, object>> ScopeThunk {
            get {
                return _scopeThunk ?? GlobalScopeThunk;
            }
            set {
                _scopeThunk = value;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "The property setter is only here to support mocking this type and should not be called at runtime.")]
        public virtual TempDataDictionary TempData {
            get;
            set;
        }

        public virtual bool UnobtrusiveJavaScriptEnabled {
            get {
                return GetUnobtrusiveJavaScriptEnabled(Scope, HttpContext);
            }
            set {
                SetUnobtrusiveJavaScriptEnabled(value, Scope, HttpContext);
            }
        }

        public virtual IView View {
            get;
            set;
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "The property setter is only here to support mocking this type and should not be called at runtime.")]
        public virtual ViewDataDictionary ViewData {
            get;
            set;
        }

        public virtual TextWriter Writer {
            get;
            set;
        }

        private string DefaultFormIdGenerator() {
            int formNum = IncrementFormCount(HttpContext.Items);
            return String.Format(CultureInfo.InvariantCulture, "form{0}", formNum);
        }

        internal static bool GetClientValidationEnabled(IDictionary<object, object> scope = null, HttpContextBase httpContext = null) {
            return ScopeCache.Get(scope, httpContext).ClientValidationEnabled;
        }

        internal FormContext GetFormContextForClientValidation() {
            return (ClientValidationEnabled) ? FormContext : null;
        }

        internal static bool GetUnobtrusiveJavaScriptEnabled(IDictionary<object, object> scope = null, HttpContextBase httpContext = null) {
            return ScopeCache.Get(scope, httpContext).UnobtrusiveJavaScriptEnabled;
        }

        private static int IncrementFormCount(IDictionary items) {
            object lastFormNum = items[_lastFormNumKey];
            int newFormNum = (lastFormNum != null) ? ((int)lastFormNum) + 1 : 0;
            items[_lastFormNumKey] = newFormNum;
            return newFormNum;
        }

        public void OutputClientValidation() {
            FormContext formContext = GetFormContextForClientValidation();
            if (formContext == null || UnobtrusiveJavaScriptEnabled) {
                return; // do nothing
            }

            string scriptWithCorrectNewLines = _clientValidationScript.Replace("\r\n", Environment.NewLine);
            string validationJson = formContext.GetJsonValidationMetadata();
            string formatted = String.Format(CultureInfo.InvariantCulture, scriptWithCorrectNewLines, validationJson);

            Writer.Write(formatted);
            FormContext = null;
        }

        internal static void SetClientValidationEnabled(bool enabled, IDictionary<object, object> scope = null, HttpContextBase httpContext = null) {
            ScopeCache.Get(scope, httpContext).ClientValidationEnabled = enabled;
        }

        internal static void SetUnobtrusiveJavaScriptEnabled(bool enabled, IDictionary<object, object> scope = null, HttpContextBase httpContext = null) {
            ScopeCache.Get(scope, httpContext).UnobtrusiveJavaScriptEnabled = enabled;
        }

        private static TValue ScopeGet<TValue>(IDictionary<object, object> scope, string name, TValue defaultValue = default(TValue)) {
            object result;
            if (scope.TryGetValue(name, out result)) {
                return (TValue)Convert.ChangeType(result, typeof(TValue), CultureInfo.InvariantCulture);
            }
            return defaultValue;
        }

        private sealed class ScopeCache {
            private static readonly object _cacheKey = new object();
            private bool _clientValidationEnabled;
            private IDictionary<object, object> _scope;
            private bool _unobtrusiveJavaScriptEnabled;

            private ScopeCache(IDictionary<object, object> scope) {
                _scope = scope;

                _clientValidationEnabled = ScopeGet(scope, ClientValidationKeyName, false);
                _unobtrusiveJavaScriptEnabled = ScopeGet(scope, UnobtrusiveJavaScriptKeyName, false);
            }

            public bool ClientValidationEnabled {
                get {
                    return _clientValidationEnabled;
                }
                set {
                    _clientValidationEnabled = value;
                    _scope[ClientValidationKeyName] = value;
                }
            }

            public bool UnobtrusiveJavaScriptEnabled {
                get {
                    return _unobtrusiveJavaScriptEnabled;
                }
                set {
                    _unobtrusiveJavaScriptEnabled = value;
                    _scope[UnobtrusiveJavaScriptKeyName] = value;
                }
            }

            public static ScopeCache Get(IDictionary<object, object> scope, HttpContextBase httpContext) {
                if (httpContext == null && System.Web.HttpContext.Current != null) {
                    httpContext = new HttpContextWrapper(System.Web.HttpContext.Current);
                }

                ScopeCache result = null;
                scope = scope ?? ScopeStorage.CurrentScope;

                if (httpContext != null) {
                    result = httpContext.Items[_cacheKey] as ScopeCache;
                }

                if (result == null || result._scope != scope) {
                    result = new ScopeCache(scope);

                    if (httpContext != null) {
                        httpContext.Items[_cacheKey] = result;
                    }
                }

                return result;
            }
        }
    }
}
