namespace System.Web.Mvc {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Web.Mvc.Resources;

    // TODO: Unit test ModelState interaction with VDD

    public class ViewDataDictionary : IDictionary<string, object> {

        private readonly Dictionary<string, object> _innerDictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        private object _model;
        private ModelMetadata _modelMetadata;
        private readonly ModelStateDictionary _modelState = new ModelStateDictionary();
        private TemplateInfo _templateMetadata;

        public ViewDataDictionary()
            : this((object)null) {
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "See note on SetModel() method.")]
        public ViewDataDictionary(object model) {
            Model = model;
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "See note on SetModel() method.")]
        public ViewDataDictionary(ViewDataDictionary dictionary) {
            if (dictionary == null) {
                throw new ArgumentNullException("dictionary");
            }

            foreach (var entry in dictionary) {
                _innerDictionary.Add(entry.Key, entry.Value);
            }
            foreach (var entry in dictionary.ModelState) {
                ModelState.Add(entry.Key, entry.Value);
            }

            Model = dictionary.Model;
            TemplateInfo = dictionary.TemplateInfo;

            // PERF: Don't unnecessarily instantiate the model metadata
            _modelMetadata = dictionary._modelMetadata;
        }

        public int Count {
            get {
                return _innerDictionary.Count;
            }
        }

        public bool IsReadOnly {
            get {
                return ((IDictionary<string, object>)_innerDictionary).IsReadOnly;
            }
        }

        public ICollection<string> Keys {
            get {
                return _innerDictionary.Keys;
            }
        }

        public object Model {
            get {
                return _model;
            }
            set {
                _modelMetadata = null;
                SetModel(value);
            }
        }

        public virtual ModelMetadata ModelMetadata {
            get {
                if (_modelMetadata == null && _model != null) {
                    _modelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => _model, _model.GetType());
                }
                return _modelMetadata;
            }
            set {
                _modelMetadata = value;
            }
        }

        public ModelStateDictionary ModelState {
            get {
                return _modelState;
            }
        }

        public object this[string key] {
            get {
                object value;
                _innerDictionary.TryGetValue(key, out value);
                return value;
            }
            set {
                _innerDictionary[key] = value;
            }
        }

        public TemplateInfo TemplateInfo {
            get {
                if (_templateMetadata == null) {
                    _templateMetadata = new TemplateInfo();
                }
                return _templateMetadata;
            }
            set {
                _templateMetadata = value;
            }
        }

        public ICollection<object> Values {
            get {
                return _innerDictionary.Values;
            }
        }

        public void Add(KeyValuePair<string, object> item) {
            ((IDictionary<string, object>)_innerDictionary).Add(item);
        }

        public void Add(string key, object value) {
            _innerDictionary.Add(key, value);
        }

        public void Clear() {
            _innerDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item) {
            return ((IDictionary<string, object>)_innerDictionary).Contains(item);
        }

        public bool ContainsKey(string key) {
            return _innerDictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) {
            ((IDictionary<string, object>)_innerDictionary).CopyTo(array, arrayIndex);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Eval", Justification = "Commonly used shorthand for Evaluate.")]
        public object Eval(string expression) {
            ViewDataInfo info = GetViewDataInfo(expression);
            return (info != null) ? info.Value : null;
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Eval", Justification = "Commonly used shorthand for Evaluate.")]
        public string Eval(string expression, string format) {
            object value = Eval(expression);

            if (value == null) {
                return String.Empty;
            }

            if (String.IsNullOrEmpty(format)) {
                return Convert.ToString(value, CultureInfo.CurrentCulture);
            }
            else {
                return String.Format(CultureInfo.CurrentCulture, format, value);
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            return _innerDictionary.GetEnumerator();
        }

        public ViewDataInfo GetViewDataInfo(string expression) {
            if (String.IsNullOrEmpty(expression)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "expression");
            }

            return ViewDataEvaluator.Eval(this, expression);
        }

        public bool Remove(KeyValuePair<string, object> item) {
            return ((IDictionary<string, object>)_innerDictionary).Remove(item);
        }

        public bool Remove(string key) {
            return _innerDictionary.Remove(key);
        }

        // This method will execute before the derived type's instance constructor executes. Derived types must
        // be aware of this and should plan accordingly. For example, the logic in SetModel() should be simple
        // enough so as not to depend on the "this" pointer referencing a fully constructed object.
        protected virtual void SetModel(object value) {
            _model = value;
        }

        public bool TryGetValue(string key, out object value) {
            return _innerDictionary.TryGetValue(key, out value);
        }

        internal static class ViewDataEvaluator {

            public static ViewDataInfo Eval(ViewDataDictionary vdd, string expression) {
                //Given an expression "foo.bar.baz" we look up the following (pseudocode):
                //  this["foo.bar.baz.quux"]
                //  this["foo.bar.baz"]["quux"]
                //  this["foo.bar"]["baz.quux]
                //  this["foo.bar"]["baz"]["quux"]
                //  this["foo"]["bar.baz.quux"]
                //  this["foo"]["bar.baz"]["quux"]
                //  this["foo"]["bar"]["baz.quux"]
                //  this["foo"]["bar"]["baz"]["quux"]

                ViewDataInfo evaluated = EvalComplexExpression(vdd, expression);
                return evaluated;
            }

            private static ViewDataInfo EvalComplexExpression(object indexableObject, string expression) {
                foreach (ExpressionPair expressionPair in GetRightToLeftExpressions(expression)) {
                    string subExpression = expressionPair.Left;
                    string postExpression = expressionPair.Right;

                    ViewDataInfo subTargetInfo = GetPropertyValue(indexableObject, subExpression);
                    if (subTargetInfo != null) {
                        if (String.IsNullOrEmpty(postExpression)) {
                            return subTargetInfo;
                        }

                        if (subTargetInfo.Value != null) {
                            ViewDataInfo potential = EvalComplexExpression(subTargetInfo.Value, postExpression);
                            if (potential != null) {
                                return potential;
                            }
                        }
                    }
                }
                return null;
            }

            private static IEnumerable<ExpressionPair> GetRightToLeftExpressions(string expression) {
                // Produces an enumeration of all the combinations of complex property names
                // given a complex expression. See the list above for an example of the result
                // of the enumeration.

                yield return new ExpressionPair(expression, String.Empty);

                int lastDot = expression.LastIndexOf('.');

                string subExpression = expression;
                string postExpression = string.Empty;

                while (lastDot > -1) {
                    subExpression = expression.Substring(0, lastDot);
                    postExpression = expression.Substring(lastDot + 1);
                    yield return new ExpressionPair(subExpression, postExpression);

                    lastDot = subExpression.LastIndexOf('.');
                }
            }

            private static ViewDataInfo GetIndexedPropertyValue(object indexableObject, string key) {
                IDictionary<string, object> dict = indexableObject as IDictionary<string, object>;
                object value = null;
                bool success = false;

                if (dict != null) {
                    success = dict.TryGetValue(key, out value);
                }
                else {
                    TryGetValueDelegate tgvDel = TypeHelpers.CreateTryGetValueDelegate(indexableObject.GetType());
                    if (tgvDel != null) {
                        success = tgvDel(indexableObject, key, out value);
                    }
                }

                if (success) {
                    return new ViewDataInfo() {
                        Container = indexableObject,
                        Value = value
                    };
                }

                return null;
            }

            private static ViewDataInfo GetPropertyValue(object container, string propertyName) {
                // This method handles one "segment" of a complex property expression

                // First, we try to evaluate the property based on its indexer
                ViewDataInfo value = GetIndexedPropertyValue(container, propertyName);
                if (value != null) {
                    return value;
                }

                // If the indexer didn't return anything useful, continue...

                // If the container is a ViewDataDictionary then treat its Model property
                // as the container instead of the ViewDataDictionary itself.
                ViewDataDictionary vdd = container as ViewDataDictionary;
                if (vdd != null) {
                    container = vdd.Model;
                }

                // If the container is null, we're out of options
                if (container == null) {
                    return null;
                }

                // Second, we try to use PropertyDescriptors and treat the expression as a property name
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(container).Find(propertyName, true);
                if (descriptor == null) {
                    return null;
                }

                return new ViewDataInfo(() => descriptor.GetValue(container)) {
                    Container = container,
                    PropertyDescriptor = descriptor
                };
            }

            private struct ExpressionPair {
                public readonly string Left;
                public readonly string Right;

                public ExpressionPair(string left, string right) {
                    Left = left;
                    Right = right;
                }
            }
        }

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)_innerDictionary).GetEnumerator();
        }
        #endregion

    }
}
