namespace System.Web.DynamicData {
    using System;
    using System.Web.UI;

    public class DynamicValidatorEventArgs : EventArgs {

        private DynamicDataSourceOperation _operation;
        private Exception _exception;

        public DynamicValidatorEventArgs(Exception exception, DynamicDataSourceOperation operation) {
            _exception = exception;
            _operation = operation;
        }

        public Exception Exception {
            get {
                return _exception;
            }
        }

        public DynamicDataSourceOperation Operation {
            get {
                return _operation;
            }
        }

    }

}
