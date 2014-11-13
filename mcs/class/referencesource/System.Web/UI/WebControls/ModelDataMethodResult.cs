namespace System.Web.UI.WebControls {
    using System.Collections.Specialized;

    /// <summary>
    /// Encapsulates the result of a Data Method Operation done by <see cref='System.Web.UI.WebControls.ModelDataSourceView' />.
    /// </summary>
    public class ModelDataMethodResult {

        private OrderedDictionary _outputParameters;

        /// <summary>
        /// The return value from data method.
        /// ReturnValue is IEnumerable for Select operation,
        /// and optionally int value for Update/Delete/Insert operation (affectedRows).
        /// </summary>
        public object ReturnValue {
            get;
            private set;
        }

        /// <summary>
        /// A read-only dictionary with the values of out and ref parameters.
        /// </summary>
        public OrderedDictionary OutputParameters {
            get {
                return _outputParameters;
            }
        }

        public ModelDataMethodResult(object returnValue, OrderedDictionary outputParameters) {
            ReturnValue = returnValue;
            outputParameters = outputParameters ?? new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
            _outputParameters = outputParameters.AsReadOnly();
        }
    }
}
